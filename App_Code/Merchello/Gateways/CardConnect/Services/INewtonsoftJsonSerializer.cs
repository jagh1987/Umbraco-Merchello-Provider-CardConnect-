using RestSharp.Deserializers;
using RestSharp.Serializers;


namespace Merchello.CardConnect.Services
{
    public interface INewtonsoftJsonSerializer : ISerializer, IDeserializer
    {
    }
}
