using Merchello.Core;
using System;
using System.Linq;

namespace Merchello.Plugin.Payments.CardConnect.Provider
{
    using Merchello.CardConnect.Models;
    using Merchello.CardConnect.PaymentProcessor;
    using Merchello.CardConnect.Services;
    using Merchello.Core.Gateways;
    using Merchello.Core.Gateways.Payment;
    using Merchello.Core.Models;
    using Merchello.Core.Services;
    using Umbraco.Core;

    /// <summary>
    /// Represents an CardConnectPayment Method
    /// </summary>
    [GatewayMethodUi("CardConnect.CardConnect")]
    [GatewayMethodEditor("CardConnect IFrame Method Editor", "~/App_Plugins/Merchello/Backoffice/Merchello/Dialogs/payment.paymentmethod.addedit.html")]
    [PaymentGatewayMethod("CardConnect Editors",
        "~/App_Plugins/Merchello.CardConnect/views/dialogs/payment.cardconnect.authorizepayment.html",
        "~/App_Plugins/Merchello.CardConnect/views/dialogs/payment.cardconnect.authorizecapturepayment.html",
        "~/App_Plugins/Merchello.CardConnect/views/dialogs/payment.cardconnect.voidpayment.html")]
    public class CardConnectPaymentGatewayMethod : CardConnectPaymentGatewayMethodBase, ICardConnectPaymentGatewayMethod
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CardConnectPaymentGatewayMethod"/> class.
        /// </summary>
        /// <param name="gatewayProviderService">
        /// The gateway provider service.
        /// </param>
        /// <param name="paymentMethod">
        /// The payment method.
        /// </param>
        public CardConnectPaymentGatewayMethod(IGatewayProviderService gatewayProviderService, IPaymentMethod paymentMethod, ICardConnectService cardConnectService)
            : base(gatewayProviderService, paymentMethod)
        {
            // New instance of the CardConnect payment processor
            _processor = new CardConnectPaymentProcessor(cardConnectService);
        }

        /// <summary>
        /// Does the actual work of creating and processing the payment
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice"/></param>
        /// <param name="args">Any arguments required to process the payment.</param>
        /// <returns>The <see cref="IPaymentResult"/></returns>
        protected override IPaymentResult PerformAuthorizePayment(IInvoice invoice, ProcessorArgumentCollection args)
        {
            var payment = GatewayProviderService.CreatePayment(PaymentMethodType.CreditCard, invoice.Total, PaymentMethod.Key);
            payment.CustomerKey = invoice.CustomerKey;
            payment.PaymentMethodName = PaymentMethod.Name;
            payment.ReferenceNumber = PaymentMethod.PaymentCode + "-" + invoice.PrefixedInvoiceNumber();
            payment.Authorized = false;
            payment.Collected = false;

            //Setting the payment Amount == 0.00 to let CardConnect validate the authorization
            payment.Amount = Decimal.Parse("0.00");

            // Have to save here to generate the payment key
            GatewayProviderService.Save(payment);

            var result = ((CardConnectPaymentProcessor)_processor).InitializePayment(invoice, payment, args.AsCreditCard());

            if (!result.Payment.Success)
            {
                GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Denied, "CardConnect: request initialization error: " + result.Payment.Exception.Message, 0);
            }
            else
            {
                // Have to save here to persist the record so it can be used in later processing.
                GatewayProviderService.Save(payment);

                // In this case, we want to do our own Apply Payment operation as the amount has not been collected -
                // so we create an applied payment with a 0 amount.  Once the payment has been "collected", another Applied Payment record will
                // be created showing the full amount and the invoice status will be set to Paid.
                GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, "CardConnect Perform Authorize Payment", 0);
            }
            return result;
        }

        /// <summary>
        /// Does the actual work of authorizing and capturing a payment
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice"/></param>
        /// <param name="amount">The amount to capture</param>
        /// <param name="args">Any arguments required to process the payment.</param>
        /// <returns>The <see cref="IPaymentResult"/></returns>
        protected override IPaymentResult PerformAuthorizeCapturePayment(IInvoice invoice, decimal amount, ProcessorArgumentCollection args)
        {
            var payment = GatewayProviderService.CreatePayment(PaymentMethodType.CreditCard, amount, PaymentMethod.Key);
            payment.CustomerKey = invoice.CustomerKey;
            payment.PaymentMethodName = PaymentMethod.Name;
            payment.ReferenceNumber = PaymentMethod.PaymentCode + "-" + invoice.PrefixedInvoiceNumber();
            payment.Collected = true;
            payment.Authorized = true;

            var po = args.AsCreditCard();

            if (string.IsNullOrEmpty(po.CardNumber))
            {
                return new PaymentResult(Attempt<IPayment>.Fail(payment, new Exception("Error Card Number is empty")), invoice, false);
            }

            invoice.PoNumber = invoice.PrefixedInvoiceNumber();
            MerchelloContext.Current.Services.InvoiceService.Save(invoice);

            GatewayProviderService.Save(payment);

            GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, "CardConnect Perform Authorize Capture Payment", amount);

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, CalculateTotalOwed(invoice).CompareTo(amount) <= 0);
        }

        /// <summary>
        /// Does the actual work capturing a payment
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice"/></param>
        /// <param name="payment">The previously Authorize payment to be captured</param>
        /// <param name="amount">The amount to capture</param>
        /// <param name="args">Any arguments required to process the payment.</param>
        /// <returns>The <see cref="IPaymentResult"/></returns>
        protected override IPaymentResult PerformCapturePayment(IInvoice invoice, IPayment payment, decimal amount, ProcessorArgumentCollection args)
        {
            // We need to determine if the entire amount authorized has been collected before marking
            // the payment collected.
            var appliedPayments = GatewayProviderService.GetAppliedPaymentsByPaymentKey(payment.Key);
            var applied = appliedPayments.Sum(x => x.Amount);

            payment.Collected = (amount + applied) == payment.Amount;
            payment.Authorized = true;

            GatewayProviderService.Save(payment);

            GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, "CardConnect Perform Capture Payment", amount);

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, CalculateTotalOwed(invoice).CompareTo(amount) <= 0);
        }

        /// <summary>
        /// Does the actual work of refunding a payment
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice"/></param>
        /// <param name="payment">The previously Authorize payment to be captured</param>
        /// <param name="amount">The amount to be refunded</param>
        /// <param name="args">Any arguments required to process the payment.</param>
        /// <returns>The <see cref="IPaymentResult"/></returns>
        protected override IPaymentResult PerformRefundPayment(IInvoice invoice, IPayment payment, decimal amount, ProcessorArgumentCollection args)
        {
            foreach (var applied in payment.AppliedPayments())
            {
                applied.TransactionType = AppliedPaymentType.Refund;
                applied.Amount = 0;
                applied.Description += " - Refunded";
                GatewayProviderService.Save(applied);
            }

            payment.Amount = payment.Amount - amount;

            if (payment.Amount != 0)
            {
                GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, "CardConnect Perform Refund Payment", payment.Amount);
            }

            GatewayProviderService.Save(payment);

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, false);
        }

        /// <summary>
        /// Does the actual work of voiding a payment
        /// </summary>
        /// <param name="invoice">The invoice to which the payment is associated</param>
        /// <param name="payment">The payment to be voided</param>
        /// <param name="args">Additional arguments required by the payment processor</param>
        /// <returns>A <see cref="IPaymentResult"/></returns>
        protected override IPaymentResult PerformVoidPayment(IInvoice invoice, IPayment payment, ProcessorArgumentCollection args)
        {
            foreach (var applied in payment.AppliedPayments())
            {
                applied.TransactionType = AppliedPaymentType.Void;
                applied.Amount = 0;
                applied.Description += " - **Void**";
                GatewayProviderService.Save(applied);
            }

            payment.Voided = true;
            GatewayProviderService.Save(payment);

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, false);
        }
    }
}