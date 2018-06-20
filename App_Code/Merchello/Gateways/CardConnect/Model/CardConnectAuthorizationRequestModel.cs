namespace Merchello.CardConnect.Models
{
    public class CardConnectAuthorizationRequestModel
    {
        public string Merchid { get; set; }
        public string Accttype { get; set; }
        public int Orderid { get; set; }
        public string Account { get; set; }
        public string Expiry { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Postal { get; set; }
        public string Ecomind { get; set; }
        public string Cvv2 { get; set; }
        public string Track { get; set; }
        public string Tokenize { get; set; }
        public string Capture { get; set; }
    }
}