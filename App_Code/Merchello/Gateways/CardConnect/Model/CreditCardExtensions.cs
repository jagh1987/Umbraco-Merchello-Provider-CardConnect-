using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Plugin.Payments.CardConnect;
namespace Merchello.CardConnect.Models
{
    public static class CreditCardExtensions
    {
        public static ProcessorArgumentCollection AsProcessorArgumentCollection(this CreditCard creditCard)
        {
            return new ProcessorArgumentCollection()
            {
                { "creditCardType", creditCard.CreditCardType },
                { "cardholderName", creditCard.CardholderName },
                { "cardNumber", creditCard.CardNumber },
                { "expireMonth", creditCard.ExpireMonth },
                { "expireYear", creditCard.ExpireYear },
                { "cardCode", creditCard.CardCode }
            };
        }
        public static CreditCard AsCreditCard(this ProcessorArgumentCollection args)
        {
            return new CreditCard()
            {
                CreditCardType = args.ArgValue("creditCardType"),
                CardholderName = args.ArgValue("cardholderName"),
                CardNumber = args.ArgValue("cardNumber"),
                ExpireMonth = args.ArgValue("expireMonth"),
                ExpireYear = args.ArgValue("expireYear"),
                CardCode = args.ArgValue("cardCode"),
            };
        }
        private static string ArgValue(this ProcessorArgumentCollection args, string key)
        {
            return args.ContainsKey(key) ? args[key] : string.Empty;
        }
        
        public static void SaveCarConnectTransactionRecord(this IPayment payment, CardConnectAuthorizationResponseModel record)
        {
            payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.AuthorizeTransactionResult, record);
        }
    }
}