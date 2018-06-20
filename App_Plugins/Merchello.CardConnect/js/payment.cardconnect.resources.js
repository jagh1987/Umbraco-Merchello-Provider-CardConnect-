/*! MerchelloPaymentProviders
 * https://github.com/Merchello/Merchello
 * Copyright (c) 2017 Across the Pond, LLC.
 * Licensed MIT
 */

(function () {

    angular.module('merchello.providers.cardconnect.resources').factory('cardconnectResource', ['$http', 'umbRequestHelper',
            function ($http, umbRequestHelper) {
                var baseUrl = Umbraco.Sys.ServerVariables["merchelloPaymentsUrls"]["merchelloCardConnectApiBaseUrl"];

                return {
                    getClientRequestToken: function (customerKey){
                        return umbRequestHelper.resourcePromise(
                            $http({
                                url: baseUrl + 'GetClientRequestToken',
                                method: "GET",
                                params: { customerKey: customerKey }
                            }), 'Failed to retreive customer request token');
                    }
                };
            }
    ]);
})();