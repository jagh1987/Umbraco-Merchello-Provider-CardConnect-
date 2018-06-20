namespace Merchello.Plugin.Payments.CardConnect
{
    public static class Constants
    {
        /// <summary>
        /// Gets the gateway provider settings key.
        /// </summary>
        public const string GatewayProviderSettingsKey = "AEED8037-7E3B-48CE-84A6-8A51D7F5FDEB";

        public const string TransactionType = "AUTHORIZE";

        public static string CardType = "creditCardType";
        public static string CardholderName = "cardholderName";
        public static string CardNumber = "cardNumber";
        public static string ExpireMonth = "expireMonth";
        public static string ExpireYear = "expireYear";
        public static string CardCode = "cardCode";

        public static class ExtendedDataKeys
        {
            public static string AuthorizeTransactionResult = "CardConnectAuthorizeTransactionResult";
            public static string ProcessorSettings = "CardConnectProviderSettings";

            // Stores the CardConnect redirect URL once the transaction post has been registered
            public static string Endpoint = "CardConnectPaymentUrl";

            // Stores the receipt page URL to show confirmation of payment 
            public static string ReturnUrl = "ReturnUrl";

            // Stores the URL to return to if the customer aborts payment on CardConnect
            public static string CancelUrl = "CancelUrl";

            // Flag keys
            public static string Authorization = "auth";
            public static string Capture = "capture";
            public static string Void = "void";
            public static string Refund = "refund";
            public static string Inquire = "inquire";
            public static string Settlement_Status = "settlestat";
            public static string Profile = "profile";
            public static string Signature_Capture = "sigcap";
            public static string Open_Batch = "openBatch";
            public static string Close_Batch = "closeBatch";
            public static string Bin = "bin";

            private static class RespStat
            {
                public const string Approved = "A";
                public const string Retry = "B";
                public const string Declined = "C";
            }

            private static class CaptureRequest
            {
                public const string Yes = "Y";
                public const string No = "N";
            }

            // Our generated unique ID for the transaction , e.g. babypotz-1424717223480-230356
            public static string VendorTransactionCode = "VendorTransactionCode";

            // CardConnect unique ID for the transaction 
            public static string CardConnectTransactionCode = "CardConnectTransactionCode";

            // CardConnect security key for the transaction, used as a key for confirming the MD5 hash signature in the notification POST
            public static string CardConnectSecurityKey = "CardConnectSecurityKey";

            // Stores the invoice key with the customer context so it can be retrieved on a receipt page 
            public static string InvoiceKey = "invoiceKey";

            // Stores the 3DSecure url
            public static string ThreeDSecureUrl = "ThreeDSecureUrl";

        }
    }
}