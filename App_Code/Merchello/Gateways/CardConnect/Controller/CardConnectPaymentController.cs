namespace Merchello.CardConnect.Controllers
{
    using Core.Gateways;
    using Core.Gateways.Payment;
    using Merchello.CardConnect.Models;
    using System;
    using System.Web.Mvc;
    using Umbraco.Core;
    using Umbraco.Web.Mvc;
    using Umbraco.Core.Logging;
    using Web.Controllers;
    using Merchello.Plugin.Payments.CardConnect;
    using Merchello.CardConnect.Services;
    using Constants = Merchello.Plugin.Payments.CardConnect.Constants;
    using Plugin.Payments.CardConnect.Provider;
    using Core.Models;
    using Core;

    [PluginController("FastTrack")]
    [GatewayMethodUi("CardConnect.CardConnect")]
    public class CardConnectPaymentController : CheckoutPaymentControllerBase<CardConnectPaymentModel>
    {
        private ICardConnectService _cardConnectServices;

        /// <summary>
        /// The URL for a Success return.
        /// </summary>
        private string _successUrl;

        /// <summary>
        /// The URL for a Cancel return.
        /// </summary>
        private string _cancelUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardConnectPaymentController"/> class.
        /// </summary>
        public CardConnectPaymentController()
        {
            this.Initialize();
        }
        /// <summary>
        /// Initializes the controller.
        /// </summary>
        private void Initialize()
        {
            var provider = GatewayContext.Payment.GetProviderByKey(Guid.Parse(Constants.GatewayProviderSettingsKey)) as CardConnectPaymentGatewayProvider;
            if (provider == null)
            {
                var nullRef = new NullReferenceException("CardConnectPaymentGatewayProvider is not activated or has not been resolved.");
                LogHelper.Error<CardConnectPaymentController>("Failed to find active CardConnectPaymentGatewayProvider", nullRef);

                throw nullRef;
            }

            // instantiate the service
            _cardConnectServices = new CardConnectService(provider.ExtendedData.GetProcessorSettings());

            CardConnectProcessorSettings settings = provider.ExtendedData.GetProcessorSettings();
            _successUrl = settings.SuccessUrl;
            _cancelUrl = settings.CancelUrl;
            //_deleteInvoiceOnCancel = settings.DeleteInvoiceOnCancel;
        }


        /// <summary>
        /// Handles the redirection for the receipt.
        /// </summary>
        /// <param name="model">
        /// The <see cref="FastTrackPaymentModel"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        protected override ActionResult HandlePaymentSuccess(CardConnectPaymentModel model)
        {
            // Set the invoice key in the customer context (cookie)
            if (model.ViewData.Success)
            {
                CustomerContext.SetValue("invoiceKey", model.ViewData.InvoiceKey.ToString());
                model.SuccessRedirectUrl = _successUrl;
            }
            else
            {
                CustomerContext.SetValue("errorMessage", model.ViewData.Exception.Message);
            }

            return model.ViewData.Success && !model.SuccessRedirectUrl.IsNullOrWhiteSpace() ?
                Redirect(model.SuccessRedirectUrl) :
                base.HandlePaymentSuccess(model);
        }

        /// <summary>
        /// Processes the PO payment.
        /// </summary>
        /// <param name="model">
        /// The <see cref="ICheckoutPaymentModel"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Process(CardConnectPaymentModel model)
        {
            try
            {
                var paymentMethod = this.CheckoutManager.Payment.GetPaymentMethod();

                // Create the processor argument collection, where we'll pass in the purchase order
                var args = new ProcessorArgumentCollection
                {
                    { Constants.CardholderName, model.CardHolder.ToUpper()},
                    { Constants.CardNumber, model.CardNumber},
                    { Constants.ExpireMonth, model.ExpiresMonth},
                    { Constants.ExpireYear, model.ExpiresYear.ToString()},
                    { Constants.CardCode, model.Cvv},
                };

                // For PO payments we can only perform an authorize
                var attempt = this.CheckoutManager.Payment.AuthorizePayment(paymentMethod.Key, args);

                var resultModel = this.CheckoutPaymentModelFactory.Create(CurrentCustomer, paymentMethod, attempt);

                // merge the models so we can be assured that any hidden values are passed on
                model.ViewData = resultModel.ViewData;
                model.ErrorMessage = (resultModel.ViewData != null && resultModel.ViewData.Exception != null) ? resultModel.ViewData.Exception.Message : null;

                // Send the notification
                HandleNotificiation(model, attempt);

                return this.HandlePaymentSuccess(model);
            }
            catch (Exception ex)
            {
                return this.HandlePaymentException(model, ex);
            }
        }

        /// <summary>
        /// Renders the Purchase Order payment form.
        /// </summary>
        /// <param name="view">
        /// The optional view.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [ChildActionOnly]
        [GatewayMethodUi("CardConnect.CardConnect")]
        public override ActionResult PaymentForm(string view = "")
        {
            var paymentMethod = this.CheckoutManager.Payment.GetPaymentMethod();
            if (paymentMethod == null) return this.InvalidCheckoutStagePartial();

            var model = this.CheckoutPaymentModelFactory.Create(CurrentCustomer, paymentMethod);
            if (!string.IsNullOrEmpty(CustomerContext.GetValue("errorMessage")))
            {
                model.ErrorMessage = CustomerContext.GetValue("errorMessage");
                CustomerContext.SetValue("errorMessage", "");
            }

            return view.IsNullOrWhiteSpace() ? this.PartialView(model) : this.PartialView(view, model);
        }
    }
}