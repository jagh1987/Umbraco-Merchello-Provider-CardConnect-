(function () {
    //Authorize Capture
    angular.module('merchello.providers.cardconnect.controllers')
        .controller('Merchello.Provider.Payment.CardConnectdAuthorizeCapturePaymentController', CardConnectdAuthorizeCapturePaymentController);
    CardConnectdAuthorizeCapturePaymentController.$inject = ['$scope', 'assetsService', 'invoiceHelper', 'cardConnectProviderSettingsBuilder', 'gatewayProviderResource', 'gatewayProviderDisplayBuilder'];

    function CardConnectdAuthorizeCapturePaymentController($scope, assetsService, invoiceHelper, cardConnectProviderSettingsBuilder, gatewayProviderResource, gatewayProviderDisplayBuilder) {
        $scope.loaded = false;
        $scope.wasFormSubmitted = false;
        $scope.cardholderName = '';
        $scope.cardNumber = '';
        $scope.expirationYear = '';
        $scope.cvv = '';
        $scope.postalCode = '';
        $scope.months = [
            { id: '01', monthName: 'January' },     { id: '02', monthName: 'February' },{ id: '03', monthName: 'March' },    { id: '04', monthName: 'April' },
            { id: '05', monthName: 'May' },         { id: '06', monthName: 'June' },    { id: '07', monthName: 'July' },     { id: '08', monthName: 'August' },
            { id: '09', monthName: 'September' },   { id: '10', monthName: 'October' }, { id: '11', monthName: 'November' }, { id: '12', monthName: 'December' }];
        $scope.selectedMonth = $scope.months[0];
        $scope.years = [];
        $scope.cardConnectClient = {};

        // Exposed methods
        $scope.save = save;

        //Fill Year dropdown
        var year = new Date().getFullYear() - 2000;
        $scope.years.push({ value: year });
        for (var i = 1; i < 5; i++) {
            $scope.years.push({ value: year + i });
        }
        $scope.selectedYear = $scope.years[0];

        $scope.providerSettings = {};

        gatewayProviderResource.getGatewayProvider($scope.$parent.dialogData.paymentMethod.providerKey).then(function (provider) {
            $scope.dialogData.provider = gatewayProviderDisplayBuilder.transform(provider);
            init(cardConnectProviderSettingsBuilder);
        });

        function init(cardConnectProviderSettingsBuilder) {
            //Load CardConnect Settings
            var cardConnectSetting = $scope.dialogData.provider.extendedData.getValue('CardConnectProviderSettings');
            if (cardConnectSetting.length !== 0) {
                var json = JSON.parse(cardConnectSetting);
                $scope.providerSettings = cardConnectProviderSettingsBuilder.transform(json);
            }


            $scope.dialogData.warning = 'All credit card information is tokenized. No values are passed to the server.';
            var billingAddress = $scope.dialogData.invoice.getBillToAddress();
            //$scope.cardholderName = billingAddress.name;
            $scope.postalCode = billingAddress.postalCode;

            $scope.dialogData.amount = invoiceHelper.round($scope.dialogData.invoiceBalance, 2);
            $scope.loaded = true;
        }

        function save(nonce) {
            $scope.wasFormSubmitted = true;
            if (invoiceHelper.valueIsInRage($scope.dialogData.amount, 0, $scope.dialogData.invoiceBalance)) {
                $scope.dialogData.showSpinner();
                $scope.dialogData.processorArgs.setValue('nonce-from-the-client', nonce);
                $scope.submit($scope.dialogData);

            } else {
                if (!invoiceHelper.valueIsInRage($scope.dialogData.amount, 0, $scope.dialogData.invoiceBalance)) {
                    $scope.cardForm.amount.$setValidity('amount', false);
                }
            }
        }
        // Initialize the controller
        //init();
    }//]);

    //Refund Payment
    angular.module('merchello.providers').controller('Merchello.Providers.Dialogs.CardConnectRefundPaymentController',
        ['$scope', 'invoiceHelper',
            function ($scope, invoiceHelper) {

                $scope.wasFormSubmitted = false;
                $scope.save = save;

                function init() {
                    $scope.dialogData.amount = invoiceHelper.round($scope.dialogData.appliedAmount, 2);
                }

                function save() {
                    $scope.wasFormSubmitted = true;
                    if (invoiceHelper.valueIsInRage($scope.dialogData.amount, 0, $scope.dialogData.appliedAmount)) {
                        $scope.submit($scope.dialogData);
                    } else {
                        $scope.refundForm.amount.$setValidity('amount', false);
                    }
                }
                // initializes the controller
                init();
            }]);

    //Provider Settings
    angular.module('merchello.providers.cardconnect.controllers').controller('Merchello.Provider.Payment.CardConnectPaymentProviderController',
        ['$scope', 'cardConnectProviderSettingsBuilder',
        function ($scope, cardConnectProviderSettingsBuilder) {
            $scope.providerSettings = {};

            function init() {

                var cardConnectSetting = $scope.dialogData.provider.extendedData.getValue('CardConnectProviderSettings');

                if (cardConnectSetting.length !== 0) {
                    var json = JSON.parse(cardConnectSetting);
                    $scope.providerSettings = cardConnectProviderSettingsBuilder.transform(json);
                }

                $scope.$watch(function () {
                    return $scope.providerSettings;
                }, function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        $scope.dialogData.provider.extendedData.setValue('CardConnectProviderSettings', angular.toJson(newValue));
                    }
                }, true);

            }

            // initialize the controller
            init();
        }]);
})();