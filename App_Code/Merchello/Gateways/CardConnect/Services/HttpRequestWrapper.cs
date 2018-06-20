using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Merchello.CardConnect.Services
{
    public class HttpRequestWrapper
    {
        private readonly RestClient _restClient;
        private readonly RestRequest _restRequest;

        public HttpRequestWrapper(string url)
        {
            _restRequest = new RestRequest();
            _restRequest.JsonSerializer = NewtonsoftJsonSerializer.DefaultSerializer;
            _restClient = new RestClient(url);
            _restClient.AddHandler("application/json", NewtonsoftJsonSerializer.DefaultSerializer);
            _restClient.AddHandler("text/json", NewtonsoftJsonSerializer.DefaultSerializer);
            _restClient.AddHandler("text/x-json", NewtonsoftJsonSerializer.DefaultSerializer);
            _restClient.AddHandler("text/javascript", NewtonsoftJsonSerializer.DefaultSerializer);
            _restClient.AddHandler("*+json", NewtonsoftJsonSerializer.DefaultSerializer);
        }

        public HttpRequestWrapper AddEtagHeader(string value)
        {
            _restRequest.AddHeader("If-None-Match", value);
            return this;
        }

        public HttpRequestWrapper AddHeader(string key, string value)
        {
            _restRequest.AddParameter(key, value, ParameterType.HttpHeader);
            return this;
        }

        public HttpRequestWrapper AddHeaders(IDictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                _restRequest.AddParameter(header.Key, header.Value, ParameterType.HttpHeader);
            }
            return this;
        }

        public HttpRequestWrapper AddJsonBody(object data)
        {
            _restRequest.AddHeader("Content-Type", "application/json");
            _restRequest.AddJsonBody(data);
            return this;
        }

        public HttpRequestWrapper AddParameter(string name, object value)
        {
            _restRequest.AddParameter(name, value);
            return this;
        }

        public HttpRequestWrapper AddParameters(IDictionary<string, object> parameters)
        {
            foreach (var item in parameters)
            {
                _restRequest.AddParameter(item.Key, item.Value);
            }
            return this;
        }

        public IRestResponse Execute()
        {
            return _restClient.Execute(_restRequest);
        }

        public IRestResponse<T> Execute<T>() where T : new()
        {
            return _restClient.Execute<T>(_restRequest);
        }

        public Task<IRestResponse<T>> ExecuteAsync<T>() where T : new()
        {
            return _restClient.ExecuteTaskAsync<T>(_restRequest);
        }

        public HttpRequestWrapper SetMethod(Method method)
        {
            _restRequest.Method = (Method)(int)method;
            return this;
        }

        public HttpRequestWrapper SetResourse(string resource)
        {
            _restRequest.Resource = resource;
            return this;
        }
    }
}