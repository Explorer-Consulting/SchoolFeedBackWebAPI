using FeedBackApp.Backend.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class RecursiveConverter<T> : ValueConverter<T, string>
{
    public RecursiveConverter() : base(
        v => EncryptEntity(v),
        v => DecryptEntity(v))
    { }

    private static string EncryptEntity(T entity)
    {
        if (entity == null) return null;

        var json = JsonConvert.SerializeObject(entity);
        var token = JToken.Parse(json);
        var encryptedToken = EncryptValuesRecursively(token);

        // serialize JToken back to string
        return JsonConvert.SerializeObject(encryptedToken);
    }

    private static T DecryptEntity(string encryptedJson)
    {
        if (string.IsNullOrEmpty(encryptedJson)) return default;

        var token = JToken.Parse(encryptedJson);
        var decryptedToken = DecryptValuesRecursively(token);

        // serialize decrypted token to JSON and deserialize back to T
        var plainJson = JsonConvert.SerializeObject(decryptedToken);
        return JsonConvert.DeserializeObject<T>(plainJson);
    }

    private static JToken EncryptValuesRecursively(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => new JObject(token.Children<JProperty>()
                .Select(p => new JProperty(p.Name, EncryptValuesRecursively(p.Value)))),

            JTokenType.Array => new JArray(token.Children().Select(EncryptValuesRecursively)),

            JTokenType.Null => JValue.CreateNull(),

            _ => new JValue(CryptoHelper.Encrypt(token.Value<string>()))
        };
    }

    private static JToken DecryptValuesRecursively(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => new JObject(token.Children<JProperty>()
                .Select(p => new JProperty(p.Name, DecryptValuesRecursively(p.Value)))),

            JTokenType.Array => new JArray(token.Children().Select(DecryptValuesRecursively)),

            JTokenType.Null => JValue.CreateNull(),

            _ => new JValue(CryptoHelper.Decrypt(token.Value<string>()))
        };
    }
}
