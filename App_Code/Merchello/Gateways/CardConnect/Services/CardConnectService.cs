using Merchello.CardConnect.Models;
using System.Net;
using Umbraco.Core.Logging;
namespace Merchello.CardConnect.Services
{
    public class CardConnectService : ICardConnectService
    {
        #region Private Members

        private readonly CardConnectProviderSetings _settings;
        // Endpoint names
        private const string ENDPOINT_AUTH = "auth";
        private const string ENDPOINT_CAPTURE = "capture";
        private const string ENDPOINT_VOID = "void";
        private const string ENDPOINT_REFUND = "refund";
        private const string ENDPOINT_INQUIRE = "inquire";
        private const string ENDPOINT_SETTLESTAT = "settlestat";
        private const string ENDPOINT_DEPOSIT = "deposit";
        private const string ENDPOINT_PROFILE = "profile";
        private readonly HttpRequestWrapper _httpRequestWrapper;
        private readonly string _autorice;

        public string MerchId
        {
            get
            {
                return _settings.MerchId;
            }
        }
        public string SuccessUrl
        {
            get
            {
                return _settings.SuccessUrl;
            }
        }
        public string CancelUrl
        {
            get
            {
                return _settings.CancelUrl;
            }
        }
        #endregion Private Members

        public CardConnectService(CardConnectProviderSetings settings)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            _settings = settings;
            _autorice = "Basic " + Base64Encode(settings.UserName + ":" + settings.PassWord);
            _httpRequestWrapper = new HttpRequestWrapper(settings.EndPoint);
        }

        public CardConnectAuthorizationResponseModel AuthorizeTransaction(CardConnectAuthorizationRequestModel request)
        {
            try
            {
                var result = _httpRequestWrapper
                    .SetMethod(RestSharp.Method.PUT)
                    .SetResourse(ENDPOINT_AUTH)
                    .AddHeader("Authorization", _autorice)
                    .AddJsonBody(request)
                    .Execute<CardConnectAuthorizationResponseModel>();

                return result.Data;
            }
            catch (System.Exception ex)
            {
                LogHelper.Error<CardConnectAuthorizationResponseModel>("Error: CardConnectServices", ex);
                return null;
            }
        }

        #region PrivateMethods

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        #endregion PrivateMethods
    }
}