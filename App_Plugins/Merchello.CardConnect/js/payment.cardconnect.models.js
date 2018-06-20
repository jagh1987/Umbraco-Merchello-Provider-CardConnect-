/*! MerchelloPaymentProviders
 * https://github.com/Merchello/Merchello
 * Copyright (c) 2017 Across the Pond, LLC.
 * Licensed MIT
 */
(function () {

    var CardConnectProviderSettings = function () {
        var self = this;
        self.merchid = '';
        self.merchantDescriptor = '';
        self.endpoint = '';
        self.userName = '';
        self.password = '';
        self.defaultTransactionOption = '';
    };

    angular.module('merchello.providers.cardconnect.models').constant('CardConnectProviderSettings', CardConnectProviderSettings);

    angular.module('merchello.providers.cardconnect.models').factory('cardConnectProviderSettingsBuilder', ['genericModelBuilder', 'merchantDescriptorBuilder', 'CardConnectProviderSettings',
        function (genericModelBuilder, merchantDescriptorBuilder, CardConnectProviderSettings) {
            var Constructor = CardConnectProviderSettings;

            return {
                createDefault: function () {
                    var settings = new Constructor();
                    settings.merchantDescriptor = merchantDescriptorBuilder.createDefault();
                    settings.defaultTransactionOption = 'Authorize';
                    return settings;
                },
                transform: function (jsonResult) {
                    var settings = genericModelBuilder.transform(jsonResult, Constructor);
                    //settings = merchantDescriptorBuilder.transform(settings);
                    return settings;
                }
            };
        }]);
})
();