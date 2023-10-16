using AuthenticationService.Models;
//using AuthenticationService.RabbitMQServices;
using Helper;
using Helper.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;
using static Helper.Catalouge;
using System.Numerics;
using System.Security.Claims;
using JwtTokenManager;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System;
using Azure.Core;
using System.Security.Cryptography.Xml;
using Microsoft.Extensions.Configuration;
using static System.Net.WebRequestMethods;
using Microsoft.Identity.Client;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private HttpClient _httpClient = null;
        private HttpClient httpClient => _httpClient ?? (new HttpClient());
        private readonly TokenHandler _tokenHandler;
        public AuthenticationController(TokenHandler tokenHandler)
        {
            _tokenHandler = tokenHandler;
        }

        //private readonly IMessageProducer _producer;
        //public AuthenticationController(IMessageProducer messageProducer)
        //{
        //    _producer = messageProducer;
        //}

        private string CreateMailBody(string status, string OTP)
        {
            string bodyMsg = "";
            if (status == "activate")
            {
                bodyMsg += "<h3>Welcome to TaxiHub, a diverse and modern online booking vehicle system" +
                    ", You have successfully registered an account, please use the following OTP string to activate your account:</h3>";
            }
            else if(status == "resetPass")
            {
                bodyMsg += "<h3>Welcome to TaxiHub, a diverse and modern online booking vehicle system" +
                    ", You have just required to reset your password, please note down the following OTP string and fill it in your app to update your new password:</h3>";
            }
            bodyMsg += $"<h2>{OTP}</h2>";
            bodyMsg += "<img src=\"https://drive.google.com/uc?export=download&id=1xkcnw7uAxlJGtoNoPp_4F197TMnNsogq\" height=\"500\">";
            return bodyMsg;
        }

        [HttpGet]
        public async Task<ResponseMsg> GetMailSender()
        {
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Authentication.GetMailSender(),
                message = "Get mail sender success"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> AddMailSender(EmailSender sender)
        {
            int res = await Repository.Authentication.AddMailSender(sender);
            return new ResponseMsg
            {
                status = res > 0 ? true : false,
                data = null,
                message = res > 0? "Add mail sender success":"Add mail sender failed"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> ClearEmailSender()
        {
            int res = await Repository.Authentication.ClearEmailSender();
            return new ResponseMsg
            {
                status = res > 0 ? true : false,
                data = null,
                message = res > 0 ? "Clear mail sender success" : "Clear mail sender failed"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> GetResetPasswordOTP(object emailObj)
        {
            JObject objTemp = JObject.Parse(emailObj.ToString());
            string email = (string)objTemp["email"];
            bool isEmailExisted = await Repository.Authentication.CheckEmailExisted(email);
            if(isEmailExisted == false)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Get reset password OTP through email failed, email does not exist"
                };
            }
            string OTPString = Helper.DoStuff.RandomString(1, 6);
            await Repository.Authentication.SaveOTPStr(2, OTPString, email);
            string mailBody = CreateMailBody("resetPass", OTPString);
            EmailMessage msg = new EmailMessage
            {
                EmailTo = email,
                Subject = "Reset password mail",
                Content = mailBody
            };
            EmailSender sender = await Repository.Authentication.GetMailSender();
            string sendMailResult = Helper.DoStuff.SendEmails(sender, msg);
            if (sendMailResult == "Send mail success")
            {
                await Repository.Authentication.IncreaseMailSent(sender.usr);
                return new ResponseMsg
                {
                    status = true,
                    data = null,
                    message = "Send email success, please check OTP string in your email and use it to activate your account"
                };
            }
            return new ResponseMsg
            {
                status = true,
                data = null,
                message = "Send mail reset password OTP failed"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> ResetPassword(object resetPassObj)
        {
            JObject objTemp = JObject.Parse(resetPassObj.ToString());
            string email = (string)objTemp["email"];
            string OTP = (string)objTemp["OTP"];
            string newPassword = (string)objTemp["newPassword"];
            bool isEmailExisted = await Repository.Authentication.CheckEmailExisted(email);
            if(isEmailExisted)
            {
                int resetResult = await Repository.Authentication.ResetPassword(newPassword, email, OTP);
                if(resetResult == -2)
                {
                    return new ResponseMsg
                    {
                        status = false,
                        data = null,
                        message = "Reset password failed, your OTP is invalid"
                    };
                }
                return new ResponseMsg
                {
                    status = true,
                    data = null,
                    message = "Reset password success, please login"
                };
            }
            return new ResponseMsg
            {
                status = false,
                data = null,
                message = "Reset password failed, your email does not exist"
            };
        }

        private async Task<bool> SendMailActivateAccount(string email, string OTPstr)
        {
            string mailBody = CreateMailBody("activate", OTPstr);
            EmailMessage msg = new EmailMessage
            {
                EmailTo = email,
                Subject = "Activate account mail",
                Content = mailBody
            };
            EmailSender sender = await Repository.Authentication.GetMailSender();
            string sendMailResult = Helper.DoStuff.SendEmails(sender, msg);
            if (sendMailResult == "Send mail success")
            {
                await Repository.Authentication.IncreaseMailSent(sender.usr);
                return true;
            }
            return false;
        }

        [HttpPost]
        public async Task<ResponseMsg> SendEmailActivateAccount(object objAccount)
        {
            JObject objTemp = JObject.Parse(objAccount.ToString());
            Guid accountId = Guid.Parse((string)objTemp["accountId"]);
            string email = (string)objTemp["email"];
            bool isEmailExistedInAccount = await Repository.Authentication.CheckEmailExistedInAccount(accountId, email);
            if(isEmailExistedInAccount == false)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "The email you provided does not existed in the accountId you provided, or the accountId you provided does not exist"
                };
            }
            int isAccountNotValidated = await Repository.Authentication.CheckAccountNotValidated(accountId);
            if (isAccountNotValidated == -3)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Send mail activate account failed, your account is already activated"
                };
            }
            if (isAccountNotValidated == 1)
            {
                string OTPString = Helper.DoStuff.RandomString(1, 6);
                await Repository.Authentication.SaveOTPStr(1, OTPString, email);
                bool sendMailResult = await SendMailActivateAccount(email, OTPString);
                if (sendMailResult == true)
                {
                    return new ResponseMsg
                    {
                        status = true,
                        data = null,
                        message = "Send mail activate account success, please check your email to get the OTP"
                    };
                }
            }
            return new ResponseMsg
            {
                status = false,
                data = null,
                message = "Send mail activate account failed, please try again"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> ActivateAccount(object activateObj)
        {
            JObject objTemp = JObject.Parse(activateObj.ToString());
            string email = (string)objTemp["email"];
            string OTP = (string)objTemp["OTP"];
            bool isEmailExisted = await Repository.Authentication.CheckEmailExisted(email);
            if (isEmailExisted)
            {
                int validateResult = await Repository.Authentication.ValidateEmail(OTP, email);
                if(validateResult <= 0)
                {
                    return new ResponseMsg
                    {
                        status = false,
                        data = null,
                        message = "Exec validate account failed, OTP is invalid"
                    };
                }
                return new ResponseMsg
                {
                    status = true,
                    data = null,
                    message = "Validate account success, now you can use our services"
                };
            }
            return new ResponseMsg
            {
                status = false,
                data = null,
                message = "Validate account failed, your email does not exist"
            };
        }

        [HttpGet]
        public async Task<string> GetRandomString()
        {
            return "This is an random string";
        }

        private async Task<Token> SaveUserInfoAndCreateTokens(AuthenticationInfo usr)
        {
            //ClaimsIdentity is a class in C# that represents a collection of claims associated with a single identity. It contains the information that define a user's identity.
            //Claims are key-value pairs that describe some aspect of the user
            //This ClaimsIdentity object can then be used to create a JWT (JSON Web Token) that can be used to authenticate the user
            ClaimsIdentity claims = new ClaimsIdentity(new Claim[]
               {
                    new Claim(ClaimTypes.Name, usr.Name),
                    new Claim(ClaimTypes.Email, usr.Email),
                    new Claim(ClaimTypes.NameIdentifier, usr.AccountId.ToString()),
                    new Claim(ClaimTypes.Role, usr.Role),
               });
            Token token = new Token
            {
                AccessToken = _tokenHandler.CreateAccessToken(claims),
                RefreshToken = _tokenHandler.CreateRefreshToken()
            };
            int saveToDbResult = await Repository.Authentication.UpdateUserTokens(usr.AccountId, token.RefreshToken, DateTime.UtcNow.AddDays(7));
            return token;
        }

        [HttpPost]
        public async Task<ResponseMsg> Login(object loginInfo)
        {
            AuthenticationInfo info = await Repository.Authentication.Login(loginInfo);

            if (info is not null)
            {
                //_producer.SendMessage("info", new
                //{
                //    status = true,
                //    message = $"getdatainfo{info.Role}",
                //    data = info.AccountId
                //});
                Token token = await SaveUserInfoAndCreateTokens(info);
                return new ResponseMsg
                {
                    status = true,
                    data = new {
                        accountId = info.AccountId,
                        isEmailValidated = info.IsValidated,
                        accessToken = token.AccessToken,
                        refreshToken = token.RefreshToken
                    },
                    message = "Login success"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Login failed, please check your input again"
                };
            }
        }

        [HttpPost]
        public async Task<ResponseMsg> Register(object registerInfo)
        {
            int registerResult = await Repository.Authentication.Register(registerInfo);
            if (registerResult == -2)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Register failed, email already existed"
                };
            }
            else if (registerResult == -3)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Register failed, phone already existed"
                };
            }
            else
            {
                //_producer.SendMessage("info", new
                //{
                //    Status = true,
                //    Message = "AddDataInfo",
                //    Data = registerObj,
                //});
                return new ResponseMsg
                {
                    status = true,
                    data = null,
                    message = "Register success, please login"
                };
            }
        }

        //When an HTTP request is made to this action method, the ASP.NET Core authentication middleware will try to authenticate
        //the request using the configured authentication scheme, which in this case is the JwtBearerDefaults.AuthenticationScheme.

        //If the authentication is successful, the middleware will create a ClaimsPrincipal object based on the information in the token and set it as the User property of the HttpContext.

        //The Authorize attribute then checks if the user is authenticated and authorized to access the action method.
        //If the user is authenticated and authorized, the action method is executed and the User object is available for use inside the method.
        //[Authorize(Roles = "Admin")]

        [HttpPost, Authorize]
        public async Task<ResponseMsg> ChangePassword(object changePasswordObj)
        {
            JObject objTemp = JObject.Parse(changePasswordObj.ToString());
            string currentPassword = (string)objTemp["currentPassword"];
            string newPassword = (string)objTemp["newPassword"];

            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool validatePassword = await Repository.Authentication.ValidatePassword(UserId, currentPassword);
            if (validatePassword == true)
            {
                int result = await Repository.Authentication.ChangePassword(UserId, newPassword);
                return new ResponseMsg
                {
                    status = result > 0 ? true : false,
                    data = null,
                    message = result > 0 ? "Change user password success" : "Change user password failed"
                };
            }
            return new ResponseMsg
            {
                status = false,
                data = null,
                message = "Change user password failed"
            };
        }

        [HttpGet, Authorize]
        public async Task<ResponseMsg> Logout()
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int logoutResult = await Repository.Authentication.ClearUsrToken(UserId);
            return new ResponseMsg
            {
                status = true,
                data = null,
                message = logoutResult > 0 ? "Logout success" : "Logout failed"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> RetriveTokens(Token token)
        { 
            string userId = _tokenHandler.GetUserIdFromExpiredToken(token.AccessToken);
            if(userId is not null)
            {
                AuthenticationInfo userInfo = await Repository.Authentication.ValidateRefreshToken(token.RefreshToken, Guid.Parse(userId));
                if (userInfo is not null)
                {
                    Token newToken = await SaveUserInfoAndCreateTokens(userInfo);

                    return new ResponseMsg
                    {
                        status = true,
                        data = new
                        {
                            accountId = userInfo.AccountId,
                            accessToken = token.AccessToken,
                            refreshToken = token.RefreshToken
                        },
                        message = "Refresh access token success"
                    };
                }
                else
                {
                    return new ResponseMsg
                    {
                        status = false,
                        data = null,
                        message = "Invalidate refresh token or refresh token has been expired, please login again"
                    };
                }
            }
            else{
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Invalid access token, try again or re-login"
                };
            }
        }

        public async Task<bool> VerifyGoogleToken(string url)
        {
            try
            {
                var msg = new HttpRequestMessage(HttpMethod.Get, url);
                //msg.Headers.Add("User-Agent", "C# Program");
                var res = httpClient.Send(msg);

                string content = await res.Content.ReadAsStringAsync();
                JObject objTemp2 = JObject.Parse(content);
                string error = (string)objTemp2["error_description"];
                if(error != null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async Task<string> GetGoogleAccountInfo(string url)
        {
            try
            {
                var msg = new HttpRequestMessage(HttpMethod.Get, url);
                //msg.Headers.Add("User-Agent", "C# Program");
                var res = httpClient.Send(msg);

                string content = await res.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ResponseMsg> LoginWithGoogle(object loginToken)
        {
            JObject objTemp = JObject.Parse(loginToken.ToString());
            string googleToken = (string)objTemp["loginToken"];
            string role = (string)objTemp["role"];

            string urlVerify = $"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={googleToken}";
            bool tokenIsValidated = await VerifyGoogleToken(urlVerify);
            if(tokenIsValidated == true)
            {
                string urlGetInfo = $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={googleToken}";
                string responecontent = await GetGoogleAccountInfo(urlGetInfo);
                JObject objTemp2 = JObject.Parse(responecontent);
                string name = (string)objTemp2["name"];
                string email = (string)objTemp2["email"];
                string picture = (string)objTemp2["picture"];

                if (await Repository.Authentication.CheckEmailExisted(email) == true)
                {
                    AuthenticationInfo usr = await Repository.Authentication.LoginWithEmail(email);
                    //if (usr.Name != name)
                    //{
                    //    await Repository.Authentication.UpdateUserInfo(usr.AccountId, name);
                    //}
                    Token token = await SaveUserInfoAndCreateTokens(usr);
                    return new ResponseMsg
                    {
                        status = true,
                        data = new
                        {
                            accountId = usr.AccountId,
                            accessToken = token.AccessToken,
                            refreshToken = token.RefreshToken
                        },
                        message = "Login success"
                    };
                }
                else
                {
                    int registerResult = await Repository.Authentication.RegisterWithGoogleInfo(email, name, role);
                    if (registerResult > 0)
                    {
                        AuthenticationInfo usr = await Repository.Authentication.LoginWithEmail(email);
                        Token token = await SaveUserInfoAndCreateTokens(usr);
                        return new ResponseMsg
                        {
                            status = true,
                            data = new
                            {
                                accountId = usr.AccountId,
                                accessToken = token.AccessToken,
                                refreshToken = token.RefreshToken
                            },
                            message = "Login success"
                        };
                    }
                    else
                    {
                        return new ResponseMsg
                        {
                            status = false,
                            data = null,
                            message = "Login failed, failed to save google user info into database"
                        };
                    }
                }
            }

            return new ResponseMsg
            {
                status = false,
                data = null,
                message = "Login with google account failed, google token is invalid"
            };
        }

        //public async Task<ResponseMsg> GoogleLogin(GoogleLoginModel loginModel)
        //{
        //    var handler = new JwtSecurityTokenHandler();
        //    var jwtSecurityToken = handler.ReadJwtToken(loginModel.GoogleCredential);
        //    string email = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;
        //    string name = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
        //    string avatar = jwtSecurityToken.Claims.First(claim => claim.Type == "picture").Value;
        //    if (await Repository.Authentication.CheckEmailExisted(email) == true)
        //    {
        //        AuthenticationInfo usr = await Repository.Authentication.LoginWithEmail(email);
        //        if (usr.Name != name)
        //        {
        //            await Repository.Authentication.UpdateUserInfo(usr.AccountId, name);
        //        }
        //        Token token = await SaveUserInfoAndCreateTokens(usr);
        //        return new ResponseMsg
        //        {
        //            status = true,
        //            data = token,
        //            message = "Login success"
        //        };
        //    }
        //    else
        //    {
        //        int registerResult = await Repository.Authentication.RegisterWithGoogleInfo(email, name);
        //        if (registerResult > 0)
        //        {
        //            AuthenticationInfo usr = await Repository.Authentication.LoginWithEmail(email);
        //            Token token = await SaveUserInfoAndCreateTokens(usr);
        //            return new ResponseMsg
        //            {
        //                status = true,
        //                data = token,
        //                message = "Login success"
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseMsg
        //            {
        //                status = false,
        //                data = null,
        //                message = "Login failed"
        //            };
        //        }
        //    }
        //}

        [HttpGet, Authorize]
        public async Task<ResponseMsg> ClearDb()
        {
            await Repository.Authentication.ClearTable();
            return new ResponseMsg
            {
                status = true,
                data = null,
                message = "Executed clear Authentication services Database"
            };
        }
    }
}
