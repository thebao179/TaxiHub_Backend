using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtTokenManager
{
    public static class JwtAuthExtension
    {
        public static void AddJwtAuthExtension(this IServiceCollection services)
        {
            byte[] key = Encoding.ASCII.GetBytes(TokenHandler.JWT_SECURITY_KEY);
            services.AddAuthentication(option =>
            {
                //configuring the default authentication scheme for the application to use JWT-based authentication.
                //This means that the Authorize attribute or any other authentication-related attribute that is applied to
                //a controller action or endpoint in the application will use JWT-based authentication by default,
                //unless a different authentication scheme is specified explicitly.
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                //When a user attempts to access a resource that requires authentication, and the user is not authenticated,
                //the application will redirect the user to a login page or prompt the user to authenticate. This process is called challenge.
                //By setting the default challenge scheme to JWT-based authentication, the application will use this scheme to challenge
                //unauthenticated users to authenticate using a JWT token.
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                //This option specifies whether to require HTTPS metadata for token validation
                option.RequireHttpsMetadata = false;

                //This option specifies whether to save the token in the authentication properties
                option.SaveToken = true;

                //This option specifies the parameters for validating the JWT token
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    //This parameter specifies whether to validate the issuer signing key
                    ValidateIssuerSigningKey = true,

                    //This parameter specifies the issuer signing key to use for token validation. In this case, it's set to the symmetric security key created earlier

                    //A SymmetricSecurityKey is a type of key where the same secret key is used both for signing the token on the server and for verifying the token on the client side.
                    //This is in contrast to an AsymmetricSecurityKey, where a public key is used for verifying the token and a private key is used for signing the token.

                    //Using a SymmetricSecurityKey is a good choice for scenarios where the client and server are both trusted entities and share a secret key.
                    //This is because it is computationally faster and requires fewer resources than using an AsymmetricSecurityKey.
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    //This parameter specifies whether to validate the audience
                    ValidateAudience = false,

                    //This parameter specifies whether to validate the issuer. In this case
                    ValidateIssuer = false,

                    //This parameter specifies the amount of clock skew to allow for token validation
                    ClockSkew = TimeSpan.Zero
                };
            });
        }
    }
}
