using Merchello.CardConnect.Models;

namespace Merchello.CardConnect.Services
{
    public interface ICardConnectService
    {
        string MerchId { get; }
        string SuccessUrl { get; }
        string CancelUrl { get; }
        CardConnectAuthorizationResponseModel AuthorizeTransaction(CardConnectAuthorizationRequestModel request);
    }
}