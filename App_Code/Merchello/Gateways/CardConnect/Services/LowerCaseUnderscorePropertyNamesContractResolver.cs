using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace Merchello.CardConnect.Services
{
    public class LowerCaseUnderscorePropertyNamesContractResolver : DefaultContractResolver
    {
        private Regex regex = new Regex("(?!(^[A-Z]))([A-Z])");

        protected override string ResolvePropertyName(string propertyName)
        {
            var result = regex.Replace(propertyName, "_$2").ToLower();
            return result;
        }
    }
}