using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System.IO;

namespace Merchello.CardConnect.Services
{
    public class NewtonsoftJsonSerializer : INewtonsoftJsonSerializer
    {
        private JsonSerializer _serializer;

        private NewtonsoftJsonSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
            ContentType = "application/json";
        }

        public string ContentType { get; set; }

        public string DateFormat { get; set; }

        public static NewtonsoftJsonSerializer LowerCaseUnderscoreSerializer
        {
            get
            {
                return new NewtonsoftJsonSerializer(new JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new LowerCaseUnderscorePropertyNamesContractResolver()
                });
            }
        }

        public static NewtonsoftJsonSerializer DefaultSerializer
        {
            get
            {
                return new NewtonsoftJsonSerializer(new JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }
        }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public T Deserialize<T>(IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    _serializer.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }
    }
}