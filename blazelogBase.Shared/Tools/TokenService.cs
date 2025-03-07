using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using blazelogBase.Shared.Models;

namespace blazelogBase.Shared.Tools
{
    public class TokenService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        public TokenService(IOptions<AuthSetting> option)
        {

            _secret = option.Value.Secret;
            _issuer = option.Value.Issuer;
            _audience = option.Value.Audience;

        }
        private const int ExpirationMinutes = Constants.Setting.JWTExpirationInMins;
        public string CreateToken(IAuthResult user)
        {
            var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
            var userClaims = CreateClaims(user);
            var token = CreateJwtToken(
                userClaims,
                CreateSigningCredentials(),
                expiration
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            
            return tokenHandler.WriteToken(token);
        }

        private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
            DateTime expiration) =>
            new(
                _issuer,
                _audience,
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

        private List<Claim> CreateClaims(IAuthResult user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "TokenForBuckleJwtAuth"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
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
                    Encoding.UTF8.GetBytes(_secret)
                ),
                SecurityAlgorithms.HmacSha256
            );
        }
    }
}
