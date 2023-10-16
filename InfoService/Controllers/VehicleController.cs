using Helper.Models;
using InfoService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace InfoService.Controllers
{
    [Route("api/Info/[controller]/[action]")]
    [ApiController]
    public class VehicleController : BaseController
    {
        [HttpGet]
        public async Task<ResponseMsg> GetNumOfPages(int pageSize)
        {
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Vehicle.CalcNumOfPages(pageSize),
                message = "Get num of pages success"
            };
        }

        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<ResponseMsg> GetVehiclesWithPagination(int pageSize, int pageNum)
        {
            int totalPage = await Repository.Vehicle.CalcNumOfPages(pageSize);
            if (pageNum > totalPage)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = $"The pageNum you input is greater than the max number of pages, try another pageSize or smaller pageNum"
                };
            }
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Vehicle.GetVehiclesWithPagination(pageNum, pageSize),
                message = $"Get vehicles in page {pageNum} success"
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> RegisterVehicle(Vehicle vehicle)
        {
            if(await Repository.Vehicle.CheckDriverHasVehicleAlready(vehicle.DriverId) == true)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Register vehicle failed, driver already registered a vehicle"
                };
            }
            else
            {
                int result = await Repository.Vehicle.RegisterVehicle(vehicle);
                if(result > 0)
                {
                    await Repository.Driver.MarkAlreadyRegisVehicle(vehicle.DriverId);
                    return new ResponseMsg
                    {
                        status = true,
                        data = null,
                        message = "Register vehicle success"
                    };
                }
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Register vehicle failed"
                };
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> GetDriverVehicle(object accountId)
        {
            JObject objTemp = JObject.Parse(accountId.ToString());
            string id = (string)objTemp["accountId"];
            Vehicle vehicle = await Repository.Vehicle.GetDriverVehicle(Guid.Parse(id));
            if(vehicle == null)
            {
                return new ResponseMsg
                {
                    status = true,
                    data = null,
                    message = "Driver did not registered any vehicle"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = true,
                    data = vehicle,
                    message = "Get driver vehicle success"
                };
            }
        }
    }
}
