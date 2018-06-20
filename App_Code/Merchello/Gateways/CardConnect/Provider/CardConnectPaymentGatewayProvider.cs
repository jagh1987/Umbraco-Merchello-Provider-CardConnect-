namespace Merchello.Plugin.Payments.CardConnect.Provider
{
    using Core.Gateways;
    using Core.Gateways.Payment;
    using Core.Models;
    using Core.Services;
    using Merchello.CardConnect.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core.Cache;
    using Umbraco.Core.Logging;

    /// <summary>
    /// The CardConnect payment gateway provider. {AEED8037-7E3B-48CE-84A6-8A51D7F5FDEB}
    /// </summary>
    [GatewayProviderEditor("CardConnect Payment Provider", "Configuration settings for the CardConnect Payment Provider", "~/App_Plugins/Merchello.CardConnect/views/dialogs/payment.cardconnect.providersettings.html")]
    [GatewayProviderActivation(Constants.GatewayProviderSettingsKey, "CardConnect Payment Provider", "CardConnect Payment Provider")]
    public class CardConnectPaymentGatewayProvider : PaymentGatewayProviderBase, ICardConnectPaymentGatewayProvider
    {
        #region AvailableResources

        /// <summary>
        /// The available resources.
        /// </summary>
        internal static readonly IEnumerable<IGatewayResource> AvailableResources = new List<IGatewayResource>
        {
            new GatewayResource("CardConnect", "CardConnect")
        };

        #endregion AvailableResources

        /// <summary>
        /// Initializes a new instance of the <see cref="CardConnectPaymentGatewayProvider"/> class.
        /// </summary>
        /// <param name="gatewayProviderService">
        /// The gateway provider service.
        /// </param>
        /// <param name="gatewayProviderSettings">
        /// The gateway provider settings.
        /// </param>
        /// <param name="runtimeCacheProvider">
        /// The runtime cache provider.
        /// </param>
        public CardConnectPaymentGatewayProvider(
            IGatewayProviderService gatewayProviderService,
            IGatewayProviderSettings gatewayProviderSettings,
            IRuntimeCacheProvider runtimeCacheProvider)
            : base(gatewayProviderService, gatewayProviderSettings, runtimeCacheProvider)
        {
        }

        /// <summary>
        /// The list resources offered.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{IGatewayResource}"/>.
        /// </returns>
        public override IEnumerable<IGatewayResource> ListResourcesOffered()
        {
            return AvailableResources.Where(x => PaymentMethods.All(y => y.PaymentCode != x.ServiceCode));
        }

        /// <summary>
        /// Responsible for creating a
        /// </summary>
        /// <param name="gatewayResource">
        /// The gateway resource.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <returns>
        /// The <see cref="IPaymentGatewayMethod"/>.
        /// </returns>
        public override IPaymentGatewayMethod CreatePaymentMethod(IGatewayResource gatewayResource, string name, string description)
        {
            var available = ListResourcesOffered().FirstOrDefault(x => x.ServiceCode == gatewayResource.ServiceCode);

            if (available == null)
            {
                var error = new InvalidOperationException("The GatewayResource has already been assigned.");

                LogHelper.Error<CardConnectPaymentGatewayProvider>("GatewayResource has alread been assigned", error);

                throw error;
            }

            var attempt = GatewayProviderService.CreatePaymentMethodWithKey(GatewayProviderSettings.Key, name, description, available.ServiceCode);

            if (attempt.Success)
            {
                PaymentMethods = null;

                return new CardConnectPaymentGatewayMethod(GatewayProviderService, attempt.Result, GetCardConnectService());
            }

            LogHelper.Error<CardConnectPaymentGatewayProvider>(string.Format("Failed to create a payment method name: {0}, description {1}, paymentCode {2}", name, description, available.ServiceCode), attempt.Exception);

            throw attempt.Exception;
        }

        /// <summary>
        /// Gets a <see cref="IPaymentGatewayMethod"/> by it's unique 'key'
        /// </summary>
        /// <param name="paymentMethodKey">The key of the <see cref="IPaymentMethod"/></param>
        /// <returns>A <see cref="IPaymentGatewayMethod"/></returns>
        public override IPaymentGatewayMethod GetPaymentGatewayMethodByKey(Guid paymentMethodKey)
        {
            var paymentMethod = PaymentMethods.FirstOrDefault(x => x.Key == paymentMethodKey);

            if (paymentMethod == null) throw new NullReferenceException("PaymentMethod not found");

            return new CardConnectPaymentGatewayMethod(GatewayProviderService, paymentMethod, GetCardConnectService());
        }

        /// <summary>
        /// Gets a <see cref="IPaymentGatewayMethod"/> by it's payment code
        /// </summary>
        /// <param name="paymentCode">The payment code of the <see cref="IPaymentGatewayMethod"/></param>
        /// <returns>A <see cref="IPaymentGatewayMethod"/></returns>
        public override IPaymentGatewayMethod GetPaymentGatewayMethodByPaymentCode(string paymentCode)
        {
            var paymentMethod = PaymentMethods.FirstOrDefault(x => x.PaymentCode == paymentCode);

            if (paymentMethod == null) throw new NullReferenceException("PaymentMethod not found");

            return new CardConnectPaymentGatewayMethod(GatewayProviderService, paymentMethod, GetCardConnectService());
        }

        /// <summary>
        /// Gets the <see cref="IPayPalApiService"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="IPayPalApiService"/>.
        /// </returns>
        private ICardConnectService GetCardConnectService()
        {
            return new CardConnectService(this.ExtendedData.GetProcessorSettings());
        }

    }
}