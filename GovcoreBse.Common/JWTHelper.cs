using JWT.Algorithms;
using JWT.Serializers;
using JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace GovcoreBse.Common;

public class JWTHelper
{
    

    static JsonSerializer _defaultlizer = new JsonSerializer
    {
        // All keys start with lowercase characters instead of the exact casing of the model/property, e.g. fullName
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        

        // Nice and easy to read, but you can also use Formatting.None to reduce the payload size
        Formatting = Formatting.Indented,        

        // The most appropriate datetime format.
        DateFormatHandling = DateFormatHandling.IsoDateFormat,

        // Don't add keys/values when the value is null.
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };


    public static string GenerateToken(string expirehours,IN.IAuthResult user)
    {

        var tokenInfo = new SimpleToken(expirehours);

        var payload = new Dictionary<string, object>
            {
                {"iss", tokenInfo.iss},
                {"iat", tokenInfo.iat},
                {"exp", tokenInfo.exp},
                {"aud", tokenInfo.aud},
                {"sub", tokenInfo.sub},
                {"jti", tokenInfo.jti},
                { "userName", user.UserName },
                { "userID", user.UserID },
                { "level",user.Level},
                { "post",user.Post},
                {"isadmin",user.IsAdmin },
                {"division", user.Division },
                {"email",user.Email }
            };

        IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        IJsonSerializer serializer = new JsonNetSerializer(_defaultlizer);
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

        var token = encoder.Encode(payload, CN.Setting.SecretKey);

        return token;
    }

    public static string GetDecodingToken(string strToken)
    {
        try
        {
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer(_defaultlizer);
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            var json = decoder.Decode(strToken, CN.Setting.SecretKey, verify: true);

            return json;
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static T DElize<T>(string json)
    {

        IJsonSerializer serializer = new JsonNetSerializer(_defaultlizer);

        return serializer.Deserialize<T>(json);
    }
    public static string SElize<T>(T obj)
    {
        IJsonSerializer serializer = new JsonNetSerializer(_defaultlizer);
        return serializer.Serialize(obj);
    }

}