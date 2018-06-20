using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.IO;

namespace Merchello.Plugin.Payments.CardConnect.Services
{
    public class CardConnectRestClient
    {
        private String url;
        private String userpass;
        private String username;
        private String password;

        // Endpoint names
        private static String ENDPOINT_AUTH = "auth";
        private static String ENDPOINT_CAPTURE = "capture";
        private static String ENDPOINT_VOID = "void";
        private static String ENDPOINT_REFUND = "refund";
        private static String ENDPOINT_INQUIRE = "inquire";
        private static String ENDPOINT_SETTLESTAT = "settlestat";
        private static String ENDPOINT_DEPOSIT = "deposit";
        private static String ENDPOINT_PROFILE = "profile";

        private enum OPERATIONS { GET, PUT, POST, DELETE };

        private static String USER_AGENT = "CardConnectRestClient-Csharp";
        private static String CLIENT_VERSION = "1.0";

        public CardConnectRestClient(String url, String username, String password)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("url parameter is required");
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("username parameter is required");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("password parameter is required");

            if (!url.EndsWith("/")) url = url + "/";
            this.url = url;
            this.username = username;
            this.password = password;
            this.userpass = username + ":" + password;
        }

        /**
        * Authorize trasaction
        * @param request JObject representing an Authorization transaction request
        * @return JObject representing an Authorization transaction response
        */
        public JObject authorizeTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_AUTH, OPERATIONS.PUT, request);
        }
        //public CardConnectAuthorizationResponseModel AuthorizeTransaction(JObject request)
        //{
        //    return send<CardConnectAuthorizationResponseModel>(ENDPOINT_AUTH, OPERATIONS.PUT, request);
        //}

        /**
        * Capture transaction
        * @param request JObject representing a Capture transaction request
        * @return JObject representing a Capture transaction response
        */
        public JObject captureTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_CAPTURE, OPERATIONS.PUT, request);
        }

        /**
         * Void transaction
         * @param request JObject representing a Void transaction request
         * @return JObject representing a Void transaction response
         */
        public JObject voidTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_VOID, OPERATIONS.PUT, request);
        }

        /**
         * Refund Transaction
         * @param request JObject representing a Refund transaction request
         * @return JObject represeting a Refund transactino response
         */
        public JObject refundTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_REFUND, OPERATIONS.PUT, request);
        }

        /**
         * Inquire Transaction
         * @param merchid Merchant ID
         * @param retref RetRef to inquire
         * @return JObject representing the request transaction
         * @throws IllegalArgumentException
         */
        public JObject inquireTransaction(String merchid, String retref)
        {
            if (string.IsNullOrEmpty(merchid)) throw new ArgumentException("Missing required parameter: merchid");
            if (string.IsNullOrEmpty(retref)) throw new ArgumentException("Missing required parameter: retref");

            String url = ENDPOINT_INQUIRE + "/" + retref + "/" + merchid;
            return (JObject)send(url, OPERATIONS.GET, null);
        }

        /**
         * Gets the settlement status for transactions
         * @param merchid Mechant ID
         * @param date Date in MMDD format
         * @return JArray of JObjects representing Settlement batches, each batch containing a JArray of
         * JObjects representing the settlement status of each transaction
         * @throws IllegalArgumentException
         */
        public JArray settlementStatus(String merchid, String date)
        {
            if ((!string.IsNullOrEmpty(merchid) && string.IsNullOrEmpty(date)) || (string.IsNullOrEmpty(merchid) && !string.IsNullOrEmpty(date)))
                throw new ArgumentException("Both merchid and date parameters are required, or neither");

            String url = null;
            if (string.IsNullOrEmpty(merchid) || string.IsNullOrEmpty(date))
            {
                url = ENDPOINT_SETTLESTAT;
            }
            else
            {
                url = ENDPOINT_SETTLESTAT + "?merchid=" + merchid + "&date=" + date;
            }

            return (JArray)send(url, OPERATIONS.GET, null);
        }

        /**
         * Retrieves deposit status information for the given merchant and date
         * @param merchid Merchant ID
         * @param date in MMDD format
         * @return
         * @throws IllegalArgumentException
         */
        public JObject depositStatus(String merchid, String date)
        {
            if ((!string.IsNullOrEmpty(merchid) && string.IsNullOrEmpty(date)) || (string.IsNullOrEmpty(merchid) && !string.IsNullOrEmpty(date)))
                throw new ArgumentException("Both merchid and date parameters are required, or neither");

            String url = null;
            if (string.IsNullOrEmpty(merchid) || string.IsNullOrEmpty(date))
            {
                url = ENDPOINT_DEPOSIT;
            }
            else
            {
                url = ENDPOINT_DEPOSIT + "?merchid=" + merchid + "&date=" + date;
            }
            return (JObject)send(url, OPERATIONS.GET, null);
        }

        /**
         * Retrieves a profile
         * @param profileid ProfileID to retrieve
         * @param accountid Optional account id within profile
         * @param merchid Merchant ID
         * @return JArray of JObjects each represeting a profile
         * @throws IllegalArgumentException
         */
        public JArray profileGet(String profileid, String accountid, String merchid)
        {
            if (string.IsNullOrEmpty(profileid)) throw new ArgumentException("Missing required parameter: profileid");
            if (string.IsNullOrEmpty(merchid)) throw new ArgumentException("Missing required parameter: merchid");
            if (accountid == null) accountid = "";

            String url = ENDPOINT_PROFILE + "/" + profileid + "/" + accountid + "/" + merchid;
            return (JArray)send(url, OPERATIONS.GET, null);
        }

        /**
         * Deletes a profile
         * @param profileid ProfileID to delete
         * @param accountid Optional accountID within the profile
         * @param merchid Merchant ID
         * @return
         * @throws IllegalArgumentException
         */
        public JObject profileDelete(String profileid, String accountid, String merchid)
        {
            if (string.IsNullOrEmpty(profileid)) throw new ArgumentException("Missing required parameter: profileid");
            if (string.IsNullOrEmpty(merchid)) throw new ArgumentException("Missing required parameter: merchid");
            if (accountid == null) accountid = "";

            String url = ENDPOINT_PROFILE + "/" + profileid + "/" + accountid + "/" + merchid;
            return (JObject)send(url, OPERATIONS.DELETE, null);
        }

        /**
         * Creates a new profile
         * @param request JObject representing the Profile creation request
         * @return JSONObejct representing the newly created profile
         * @throws IllegalArgumentException
         */
        public JObject profileCreate(JObject request)
        {
            return (JObject)send(ENDPOINT_PROFILE, OPERATIONS.PUT, request);
        }

        /**
         * Updates an existing profile
         * @param request JObject representing the Profile Update request
         * @return JObject representing the updated Profile
         */
        public JObject profileUpdate(JObject request)
        {
            return profileCreate(request);
        }

        /**
         * @Encode text to Base64
         *
         */
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        private Object send(String endpoint, OPERATIONS operation, JObject request)
        {
            // Create REST client
            RestClient client = new RestClient(url);

            // Set authentication credentials
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            //Encript Credentials to base64
            string autorice = Base64Encode(username + ":" + password);

            // Create REST request
            RestRequest rest = null;
            switch (operation)
            {
                case OPERATIONS.PUT: rest = new RestRequest(endpoint, Method.PUT); break;
                case OPERATIONS.GET: rest = new RestRequest(endpoint, Method.GET); break;
                case OPERATIONS.POST: rest = new RestRequest(endpoint, Method.POST); break;
                case OPERATIONS.DELETE: rest = new RestRequest(endpoint, Method.DELETE); break;
            }

            rest.RequestFormat = DataFormat.Json;
            rest.AddHeader("Content-Type", "application/json");
            rest.AddHeader("Authorization", "Basic " + autorice);

            String data = (request != null) ? request.ToString() : "";
            rest.AddParameter("application/json", data, ParameterType.RequestBody);
            IRestResponse response = client.Execute(rest);
            JsonTextReader jsreader = new JsonTextReader(new StringReader(response.Content));

            try
            {
                return new JsonSerializer().Deserialize(jsreader);
            }
            catch (JsonReaderException jx)
            {
                return null;
            }
        }

        private T send<T>(string endpoint, OPERATIONS operation, JObject request) where T : new()
        {
            // Create REST client
            RestClient client = new RestClient(url);

            // Set authentication credentials
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            //Encript Credentials to base64
            string autorice = Base64Encode(username + ":" + password);

            // Create REST request
            RestRequest rest = null;
            switch (operation)
            {
                case OPERATIONS.PUT: rest = new RestRequest(endpoint, Method.PUT); break;
                case OPERATIONS.GET: rest = new RestRequest(endpoint, Method.GET); break;
                case OPERATIONS.POST: rest = new RestRequest(endpoint, Method.POST); break;
                case OPERATIONS.DELETE: rest = new RestRequest(endpoint, Method.DELETE); break;
            }

            rest.RequestFormat = DataFormat.Json;
            rest.AddHeader("Content-Type", "application/json");
            rest.AddHeader("Authorization", "Basic " + autorice);

            String data = (request != null) ? request.ToString() : "";
            rest.AddParameter("application/json", data, ParameterType.RequestBody);
            IRestResponse response = client.Execute(rest);
            var result = client.Execute<T>(rest);
            return result.Data;
        }
    }
}