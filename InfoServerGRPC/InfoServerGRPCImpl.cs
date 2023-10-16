using Grpc.Core;
using InfoService.Models;
using InfoService.Repositories;
using InfoServiceGRPC;

namespace InfoServerGRPC
{
    public class InfoServerGRPCImpl : InfoGRPC.InfoGRPCBase
    {
        private ServiceRepository Repository = new ServiceRepository();

        public override Task<AddUserResponse> AddPassenger(AddPassengerRequest request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            var clientCertificate = httpContext.Connection.ClientCertificate;
            {
                Passenger passenger = new Passenger
                {
                    AccountId = Guid.Parse(request.AccountId),
                    Phone = request.Phone,
                    Email = request.Email,
                    Name = request.Name,
                    Gender = request.Gender,
                };
                int result = Repository.Passenger.AddPassengerInfo(passenger).Result;
                AddUserResponse response = new AddUserResponse();
                if (result > 0)
                {
                    response.Result = "Add user success";
                    return Task.FromResult(response);
                }
                else
                {
                    response.Result = "Add user failed";
                    return Task.FromResult(response);
                }
            }
        }

        public override Task<AddUserResponse> AddDriver(AddDriverRequest request, ServerCallContext context)
        {
            Driver driver = new Driver
            {
                AccountId = Guid.Parse(request.AccountId),
                IdentityNumber = request.IdentityNumber,
                Phone = request.Phone,
                Email = request.Email,
                Name = request.Name,
                Gender = request.Gender,
                HaveVehicleRegistered = false,
                Address = request.Address,
                AverageRate = 0.0,
                NumberOfRate = 0,
                NumberOfTrip = 0
            };
            int result = Repository.Driver.AddDriverInfo(driver).Result;
            AddUserResponse response = new AddUserResponse();
            if (result > 0)
            {
                response.Result = "Add user success";
                return Task.FromResult(response);
            }
            else
            {
                response.Result = "Add user failed";
                return Task.FromResult(response);
            }
        }

        public override Task<AddUserResponse> AddStaff(AddStaffRequest request, ServerCallContext context)
        {
            Staff staff = new Staff
            {
                AccountId = Guid.Parse(request.AccountId),
                IdentityNumber = request.IdentityNumber,
                Phone = request.Phone,
                Email = request.Email,
                Name = request.Name,
                Gender = request.Gender,
                Address = request.Address
            };
            int result = Repository.Staff.AddStaffInfo(staff).Result;
            AddUserResponse response = new AddUserResponse();
            if (result > 0)
            {
                response.Result = "Add user success";
                return Task.FromResult(response);
            }
            else
            {
                response.Result = "Add user failed";
                return Task.FromResult(response);
            }
        }


    }
}
