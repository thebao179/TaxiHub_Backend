using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtTokenManager
{
    public class TokenHandler
    {
        public const string JWT_SECURITY_KEY = "12312dasdasd2312ffdasfsdfsdfadfIKDASKDndasdqweasdnA";
        public string CreateAccessToken(ClaimsIdentity claims)
        {
            byte[] key = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);

            //JwtSecurityTokenHandler object used to create and validate JWT tokens
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            //Descriptor object describes the contents of the JWT token.
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                //Subject contains the claims that identify the authenticated user
                Subject = claims,
                Expires = DateTime.UtcNow.AddDays(3),

                //Specifies the cryptographic key used to sign the JWT token created using the SymmetricSecurityKey class, with the secret key derived from the AuthenticationKey
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            //Generate Jwt token
            var token = handler.CreateToken(descriptor);

            //Returns the JWT token in string format
            return handler.WriteToken(token);
        }

        public string CreateRefreshToken()
        {
            var RefreshToken = Helper.DoStuff.RandomString(2, 32);
            return RefreshToken;
        }

        public string GetUserIdFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECURITY_KEY)),
                ValidateLifetime = false //don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
