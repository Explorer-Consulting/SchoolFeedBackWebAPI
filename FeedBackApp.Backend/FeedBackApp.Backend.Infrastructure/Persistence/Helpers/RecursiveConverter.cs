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

    private static string EncryptEntity(T? entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null when encrypting.");
        }

        var json = JsonConvert.SerializeObject(entity);
        var token = JToken.Parse(json);
        var encryptedToken = EncryptValuesRecursively(token);

        // serialize JToken back to string
        return JsonConvert.SerializeObject(encryptedToken);
    }

    private static T DecryptEntity(string encryptedJson)
    {
        if (string.IsNullOrEmpty(encryptedJson))
        {
            throw new ArgumentNullException(nameof(encryptedJson), "Encrypted JSON cannot be null or empty.");
        }

        var token = JToken.Parse(encryptedJson);
        var decryptedToken = DecryptValuesRecursively(token);

        if (typeof(T) == typeof(bool))
        {
            var decryptedString = decryptedToken.Type == JTokenType.String
                ? decryptedToken.Value<string>()
                : decryptedToken.ToString();

            return (T)(object)(decryptedString?.ToLower() == "true");
        }

        // serialize decrypted token to JSON and deserialize back to T
        var plainJson = JsonConvert.SerializeObject(decryptedToken);
        var result = JsonConvert.DeserializeObject<T>(plainJson);
        if (result is null)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON into type {typeof(T).FullName}.");
        }

        return result;
    }

    private static JToken EncryptValuesRecursively(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => new JObject(token.Children<JProperty>()
                .Select(p => new JProperty(p.Name, EncryptValuesRecursively(p.Value)))),

            JTokenType.Array => new JArray(token.Children().Select(EncryptValuesRecursively)),

            JTokenType.Null => JValue.CreateNull(),

            JTokenType.String => new JValue(
                CryptoHelper.Encrypt(token.Value<string>() ?? string.Empty)),

            JTokenType.Boolean => new JValue(
                CryptoHelper.Encrypt(token.Value<bool>().ToString().ToLower())),

            _ => token.DeepClone()
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

            JTokenType.String => new JValue(
                CryptoHelper.Decrypt(token.Value<string>() ?? string.Empty)),

            _ => token.DeepClone()
        };
    }
}
