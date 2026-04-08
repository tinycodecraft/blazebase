using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace GovcoreBse.Common;

public class TokenService : IN.ITokenService
{
    private string msecret { get; }
    private string issuer { get; }
    private string audience { get; }
    private int expirationInHrs { get; }
    private ILogger<IN.ITokenService> mlog { get; }

    public TokenService(IOptions<AuthSetting> setting, ILogger<IN.ITokenService> itlogger)
    {
        msecret = CN.Setting.SecretKey;
        msecret = msecret + string.Join("", msecret.Reverse());
        msecret = msecret + msecret;
        expirationInHrs= HelperT.TryValue(setting.Value.ExpireInHrs, 4);
        issuer = string.IsNullOrEmpty(setting.Value.Issuer) ? setting.Value.Issuer : CN.Setting.Issuer;
        audience = string.IsNullOrEmpty(setting.Value.Audience) ? setting.Value.Audience : CN.Setting.Audience;

        mlog = itlogger;

    }
    
    public string CreateToken(IN.IAuthResult user)
    {
        var expiration = DateTime.UtcNow.AddHours(expirationInHrs);
        var userClaims = CreateClaims(user);
        var token = CreateJwtToken(
            userClaims,
            CreateSigningCredentials(),
            expiration
        );

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    public IN.IAuthResult? DecodeTokenToUser(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(msecret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,

                // set clockskew to zero so token expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            // Logging Purpose
            Console.WriteLine("Cookie was issued at " + jwtToken.IssuedAt);
            Console.WriteLine("Cookie was valid to " + jwtToken.ValidTo);

            string userid = jwtToken.Claims.First(x => x.Type == nameof(IN.IAuthResult.UserID)).Value;
            string username = jwtToken.Claims.First(x => x.Type == nameof(IN.IAuthResult.UserName)).Value;
            string email = jwtToken.Claims.First(x => x.Type == nameof(IN.IAuthResult.Email)).Value;

            string level = jwtToken.Claims.First(x => x.Type == nameof(IN.IAuthResult.Level)).Value;
            string post = jwtToken.Claims.First(x => x.Type == nameof(IN.IAuthResult.Post)).Value;
            bool isadmin = bool.Parse(jwtToken.Claims.First(x => x.Type == nameof(IN.IAuthResult.IsAdmin)).Value);
            string division = jwtToken.Claims.First(x => x.Type == nameof(IN.IAuthResult.Division)).Value;

            return new UserState
            {
                UserID = userid,
                UserName = username,
                Email = email,
                Level = int.Parse(level),
                Division = division,
                Post = post,
                IsAdmin = isadmin,


            };
        }
        catch (Exception ex)
        {
            mlog.LogDebug(ex, ex.Message);
            return null;
        }
    }


    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
        DateTime expiration) =>
        new(
            issuer,
            audience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );
    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private List<Claim> CreateClaims(IN.IAuthResult user)
    {
        try
        {
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Iss, issuer),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                    new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds().ToString()),
                    new Claim(JwtRegisteredClaimNames.Aud, audience),
                    new Claim(JwtRegisteredClaimNames.Sub, CN.Setting.Subject),
                    new Claim(JwtRegisteredClaimNames.Jti, "HYD." + DateTime.Now.ToString("yyyyMMddhhmmss")),
                    new Claim(nameof(IN.IAuthResult.UserName), user.UserName),
                    new Claim(nameof(IN.IAuthResult.UserID), user.UserID),
                    new Claim(nameof(IN.IAuthResult.Level),user.Level.ToString()),
                    new Claim(nameof(IN.IAuthResult.Post), user.Post),
                    new Claim(nameof(IN.IAuthResult.IsAdmin), user.IsAdmin.ToString(), ClaimValueTypes.Boolean),
                    new Claim(nameof(IN.IAuthResult.Division), user.Division ?? ""),
                    new Claim(nameof(IN.IAuthResult.Email), user.Email ?? ""),
                    
                    //new Claim(AuthClaims.DivisionAdminEnabled, user.IsDivisionAdmin.ToString()),
                    //new Claim(AuthClaims.DataAdminEnabled, user.IsDataAdmin.ToString()),
                    //new Claim(AuthClaims.ControlAdminEnabled, user.IsControlAdmin.ToString()),
                };
            return claims;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private SigningCredentials CreateSigningCredentials()
    {
        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(msecret)
            ),
            SecurityAlgorithms.HmacSha256
        );
    }
}