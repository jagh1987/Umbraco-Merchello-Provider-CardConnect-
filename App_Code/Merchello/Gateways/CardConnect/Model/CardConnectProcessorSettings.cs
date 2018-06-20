namespace Merchello.CardConnect.Models
{
    public class CardConnectProcessorSettings : CardConnectProviderSetings
    {
        public CardConnectProcessorSettings() { }

        public string MerchantDescriptor { get; set; }
        public string DefaultTransactionOption { get; set; }
    }
}