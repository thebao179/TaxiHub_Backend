using AuthenticationService.Models;
using Grpc.Net.Client;
using Helper.Models;
using InfoServiceGRPC;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace AuthenticationService.Repositories
{
    public class AuthenticationRepository : BaseRepository
    {
        public AuthenticationRepository(AuthenticationDbContext context) : base(context)
        {
        }

        // -2: Email already exist, -3: Phone already exist
        public async Task<int> Register(object registerInfo)
        {
            JObject objTemp = JObject.Parse(registerInfo.ToString());
            AuthenticationInfo registerData = new AuthenticationInfo
            {
                AccountId = new Guid(),
                Name = (string)objTemp["name"],
                Email = (string)objTemp["email"],
                Phone = (string)objTemp["phone"],
                Role = (string)objTemp["role"],
                IsValidated = false
            };
            string registerPassword = (string)objTemp["password"];
            bool existed1 = await context.AuthenticationInfo.AnyAsync(p => p.Email == registerData.Email);
            if (existed1 == true)
            {
                return -2;
            }
            bool existed2 = await context.AuthenticationInfo.AnyAsync(p => p.Phone == registerData.Phone);
            if (existed2 == true)
            {
                return -3;
            }
            string pwdSalt = Helper.DoStuff.RandomString(2, 16);
            registerData.PasswordSalt = pwdSalt;
            registerData.Password = Helper.DoStuff.HashString(registerData.Email + pwdSalt + registerPassword);
            await context.AddAsync(registerData);

            int mode = int.Parse((string)objTemp["mode"]);
            if(mode == 2)
            {
                string grpcServerURL = "http://host.docker.internal:8003";
                if (registerData.Role == Helper.Catalouge.Role.Passenger)
                {
                    //var channel = new Grpc.Core.Channel("localhost", 65147, ChannelCredentials.Insecure);
                    //var channel = GrpcChannel.ForAddress("http://host.docker.internal:51194", new GrpcChannelOptions
                    //{
                    //    HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, handler),
                    //});

                    var channel = GrpcChannel.ForAddress(grpcServerURL);
                    var client = new InfoServiceGRPC.InfoGRPC.InfoGRPCClient(channel);
                    var request = new AddPassengerRequest
                    {
                        AccountId = registerData.AccountId.ToString(),
                        Email = registerData.Email,
                        Phone = registerData.Phone,
                        Name = registerData.Name,
                        Gender = (bool)objTemp["gender"],
                    };
                    var response = await client.AddPassengerAsync(request);
                }
                else if(registerData.Role == Helper.Catalouge.Role.Staff)
                {
                    var channel = GrpcChannel.ForAddress(grpcServerURL);
                    var client = new InfoServiceGRPC.InfoGRPC.InfoGRPCClient(channel);
                    var request = new AddStaffRequest
                    {
                        AccountId = registerData.AccountId.ToString(),
                        IdentityNumber = (string)objTemp["identityNumber"],
                        Phone = registerData.Phone,
                        Email = registerData.Email,
                        Name = registerData.Name,
                        Gender = (bool)objTemp["gender"],
                        Address = (string)objTemp["address"]
                    };
                    var response = await client.AddStaffAsync(request);
                }
                else if (registerData.Role == Helper.Catalouge.Role.Driver)
                {
                    var channel = GrpcChannel.ForAddress(grpcServerURL);
                    var client = new InfoServiceGRPC.InfoGRPC.InfoGRPCClient(channel);
                    var request = new AddDriverRequest
                    {
                        AccountId = registerData.AccountId.ToString(),
                        IdentityNumber = (string)objTemp["identityNumber"],
                        Phone = registerData.Phone,
                        Email = registerData.Email,
                        Name = registerData.Name,
                        Gender = (bool)objTemp["gender"],
                        Address = (string)objTemp["address"]
                    };
                    var response = await client.AddDriverAsync(request);
                }
            }

            return await context.SaveChangesAsync();
        }

        public async Task<AuthenticationInfo> Login(object loginInfo)
        {
            JObject objTemp = JObject.Parse(loginInfo.ToString());
            string loginEmail = (string)objTemp["email"];
            string loginPhone = (string)objTemp["phone"];
            string loginPassword = (string)objTemp["password"];

            var user = context.AuthenticationInfo.Where(p => p.Email == loginEmail).FirstOrDefault();
            if(user == null)
            {
                return null;
            }
            
            var encryptedPassword = Helper.DoStuff.HashString( loginEmail + user.PasswordSalt + loginPassword);
            if (encryptedPassword.SequenceEqual(user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task<AuthenticationInfo> GetUserById(Guid UserId)
        {
            AuthenticationInfo usr = await context.AuthenticationInfo.FindAsync(UserId);
            return usr;
        }


        public async Task<int> UpdateUserTokens(Guid UserId, string RefreshToken, DateTime expiredTime)
        {
            AuthenticationInfo usr = await GetUserById(UserId);
            if (usr != null)
            {
                usr.RefreshToken = RefreshToken;
                usr.RefreshTokenExpiredDate = expiredTime;
                try
                {
                    return await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return 0;
        }

        public async Task<bool> ValidatePassword(Guid userId, string password)
        {
            AuthenticationInfo usr = await context.AuthenticationInfo.FindAsync(userId);
            if (usr == null)
            {
                return false;
            }

            var encryptedPassword = Helper.DoStuff.HashString(usr.Email + usr.PasswordSalt + password);
            if (encryptedPassword.SequenceEqual(usr.Password))
            {
                return true;
            }
            return false;
        }
        public async Task<int> ChangePassword(Guid userId, string newPassword)
        {
            AuthenticationInfo usr = await context.AuthenticationInfo.FindAsync(userId);
            usr.PasswordSalt = Helper.DoStuff.RandomString(2, 16);
            usr.Password = Helper.DoStuff.HashString(usr.Email + usr.PasswordSalt + newPassword);
            return await context.SaveChangesAsync();
        }

        public async Task<int> ClearUsrToken(Guid UserId)
        {
            AuthenticationInfo usr = context.AuthenticationInfo.Where<AuthenticationInfo>(p => p.AccountId == UserId).SingleOrDefault();
            if (usr != null)
            {
                usr.RefreshToken = String.Empty;
                usr.RefreshTokenExpiredDate = DateTime.UtcNow;
            }
            return await context.SaveChangesAsync();
        }
        public async Task<AuthenticationInfo> ValidateRefreshToken(string RefreshToken, Guid UserId)
        {
            AuthenticationInfo user = await context.AuthenticationInfo.Where(p => p.AccountId == UserId && p.RefreshToken == RefreshToken && p.RefreshTokenExpiredDate > DateTime.UtcNow).SingleOrDefaultAsync();
            return user;
        }

        public async Task<int> RegisterWithGoogleInfo(string email, string name, string role)
        {
            string pwdSalt = Helper.DoStuff.RandomString(2, 16);
            AuthenticationInfo usr = new AuthenticationInfo
            {
                Email = email,
                Role = role,
                PasswordSalt = pwdSalt,
                Name = name,
                Password = Helper.DoStuff.HashString(email + pwdSalt + "gggggggg"),
                IsValidated = true
            };
            await context.AuthenticationInfo.AddAsync(usr);
            return await context.SaveChangesAsync();
        }

        public async Task<bool> CheckEmailExisted(string email)
        {
            bool existed = await context.AuthenticationInfo.AnyAsync(p => p.Email == email);
            return existed;
        }

        public async Task<bool> CheckEmailExistedInAccount(Guid userId, string email)
        {
            bool existed = await context.AuthenticationInfo.AnyAsync(p => p.AccountId == userId && p.Email == email);
            return existed;
        }

        //-2: account not existed, -3: account already activated
        public async Task<int> CheckAccountNotValidated(Guid userId)
        {
            AuthenticationInfo user = await context.AuthenticationInfo.FindAsync(userId);
            if (user != null)
            {
                if(user.IsValidated == true)
                {
                    return -3;
                }
                return 1;
            }
            return -2;
        }

        public async Task<int> UpdateUserInfo(Guid userId, string name)
        {
            AuthenticationInfo res = await GetUserById(userId);
            res.Name = name;
            return await context.SaveChangesAsync();
        }

        public async Task<AuthenticationInfo> LoginWithEmail(string email)
        {
            var usr = from user in context.AuthenticationInfo
                      where user.Email == email
                      select user;
            return await usr.SingleOrDefaultAsync();
        }

        public async Task<EmailSender> GetMailSender()
        {
            //string sql = "Select * from dbo.EmailSender";
            EmailSender sender = await context.EmailSender.SingleOrDefaultAsync();
            return sender;
        }

        public async Task<int> AddMailSender(EmailSender sender)
        {
            await context.EmailSender.AddAsync(sender);
            return await context.SaveChangesAsync();
        }

        public async Task<int> IncreaseMailSent(string account)
        {
            EmailSender sender = await context.EmailSender.Where(p => p.usr == account).SingleOrDefaultAsync();
            sender.EmailSended += 1;
            return await context.SaveChangesAsync();
        }

        // 1: Validate email string, 2: Reset password string
        public async Task<int> SaveOTPStr(int type, string OTP, string email)
        {
            AuthenticationInfo user = await context.AuthenticationInfo.Where(p => p.Email == email).SingleOrDefaultAsync();
            if (type == 1)
            {
                user.ValidateEmailString = OTP;
            }
            else if(type == 2)
            {
                user.ResetPasswordString = OTP;
            }
            return await context.SaveChangesAsync();
        }

        //-2: OTP invalid
        public async Task<int> ValidateEmail(string OTP, string email)
        {
            AuthenticationInfo user = await context.AuthenticationInfo.Where(p => p.Email == email).SingleOrDefaultAsync();
            if (OTP.IsNullOrEmpty() || user.ValidateEmailString.IsNullOrEmpty())
            {
                return -2;
            }
            if(user.ValidateEmailString == OTP)
            {
                user.IsValidated = true;
                user.ValidateEmailString = "";
                return await context.SaveChangesAsync();
            }
            return -2;
        }

        //-2: OTP invalid
        public async Task<int> ResetPassword(string newPassword, string email, string OTP)
        {
            AuthenticationInfo user = await context.AuthenticationInfo.Where(p => p.Email == email).SingleOrDefaultAsync();
            string pwdSalt = Helper.DoStuff.RandomString(2, 16);
            if (OTP.IsNullOrEmpty() || user.ResetPasswordString.IsNullOrEmpty())
            {
                return -2;
            }
            if (user.ResetPasswordString == OTP)
            {
                user.PasswordSalt = pwdSalt;
                user.ResetPasswordString = "";
                user.Password = Helper.DoStuff.HashString(email + pwdSalt + newPassword);
                return await context.SaveChangesAsync();
            }
            return -2;
        }

        public async Task<int> ClearEmailSender()
        {
            context.RemoveRange(context.EmailSender);
            return await context.SaveChangesAsync();
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.AuthenticationInfo);
            return await context.SaveChangesAsync();
        }
    }
}
