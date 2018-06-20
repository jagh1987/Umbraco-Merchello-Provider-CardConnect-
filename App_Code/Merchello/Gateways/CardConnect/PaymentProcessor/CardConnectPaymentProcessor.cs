using Merchello.CardConnect.Models;
using Merchello.CardConnect.Services;
using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Core.Services;
using System;
using Umbraco.Core;
using IPayment = Merchello.Core.Models.IPayment;
using IPaymentResult = Merchello.Core.Gateways.Payment.IPaymentResult;

namespace Merchello.CardConnect.PaymentProcessor
{
    public class CardConnectPaymentProcessor : CardConnectPaymentProcessorBase
    {
        public CardConnectPaymentProcessor(ICardConnectService cardConnectService) : base(cardConnectService)
        {
        }

        /// <summary>
        /// Processes the Authorize and AuthorizeAndCapture transactions
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice"/> to be paid</param>
        /// <param name="payment">The <see cref="Core.Models.IPayment"/> record</param>
        /// <param name="args"></param>
        /// <returns>The <see cref="Core.Gateways.Payment.IPaymentResult"/></returns>
        public IPaymentResult InitializePayment(IInvoice invoice, IPayment payment, CreditCard creditCard)
        {
            try
            {
                #region New

                //Credit Card Information
                var models = Map(invoice, payment, creditCard);
                var response = CardConnectService.AuthorizeTransaction(models);

                #endregion New

                if (response != null)
                {
                    //Get the CardConnect Response
                    switch (response.Respstat)
                    {
                        case "A":
                            {
                                payment.Collected = false;
                                payment.Authorized = true;
                                payment.SaveCarConnectTransactionRecord(response);
                                GatewayProviderService service = new GatewayProviderService();
                                service.ApplyPaymentToInvoice(
                                    payment.Key,
                                    invoice.Key, Core.AppliedPaymentType.Debit,
                                    "CardConnect: " + response.Resptext +" Authorized Amount "+ string.Format("{0:C}", invoice.Total) + " for Capture... RetRef: " + response.Retref,
                                    0
                                );

                                //If the payment was acepted, redirect the user to a thank you landing page
                                return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, true);
                            }
                        case "B":
                        case "C":
                        default:
                            {
                                payment.Collected = false;
                                payment.Authorized = false;
                                //If the payment was'nt acepted, redirect the user to a Cancel Url
                                return new PaymentResult(Attempt<IPayment>.Fail(payment, new Exception("CardConnect: " + response.Resptext)), invoice, true);
                            }
                    }
                }
                else
                {
                    payment.Collected = false;
                    payment.Authorized = false;
                    return new PaymentResult(Attempt<IPayment>.Fail(payment, new Exception("CardConnect: Null Response")), invoice, true);
                }
            }
            catch (Exception ex)
            {
                payment.Collected = false;
                payment.Authorized = false;
                return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, true);
            }
        }

        #region Private Methods

        private CardConnectAuthorizationRequestModel Map(IInvoice invoice, IPayment payment, CreditCard creditCard)
        {
            return new CardConnectAuthorizationRequestModel
            {
                Merchid = MerchId,
                Accttype = creditCard.CreditCardType,
                Account = creditCard.CardNumber,
                Name = creditCard.CardholderName,
                Expiry = creditCard.ExpireMonth + creditCard.ExpireYear,
                Cvv2 = creditCard.CardCode,
                Amount = payment.Amount,
                Currency = invoice.CurrencyCode,
                Orderid = invoice.InvoiceNumber,
                Address = invoice.BillToAddress1,
                City = invoice.BillToLocality,
                Region = invoice.BillToRegion,
                Country = invoice.BillToCountryCode,
                Postal = invoice.BillToPostalCode,
                Tokenize = "Y",
                Capture = "Y"
            };
        }

        #endregion Private Methods
    }
}