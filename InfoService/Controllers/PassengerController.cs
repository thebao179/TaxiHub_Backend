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
    public class PassengerController : BaseController
    {
        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<ResponseMsg> GetPassengers()
        {
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Passenger.GetAllPassengers(),
                message = "Get all passenger info success"
            };
        }

        [HttpGet]
        public async Task<ResponseMsg> GetNumOfPages(int pageSize)
        {
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Passenger.CalcNumOfPages(pageSize),
                message = "Get num of pages success"
            };
        }

        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<ResponseMsg> GetPassengersWithPagination(int pageSize, int pageNum)
        {
            if (pageNum == 0)
            {
                pageNum = 1;
            }
            if (pageSize == 0)
            {
                pageSize = 15;
            }
            int totalPage = await Repository.Passenger.CalcNumOfPages(pageSize);
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
                data = await Repository.Passenger.GetPassengersWithPagination(pageNum, pageSize),
                message = $"Get passengers in page {pageNum} success"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> AddInfo(Passenger passenger)
        {
            int result = await Repository.Passenger.AddPassengerInfo(passenger);
            if (result > 0)
            {
                return new ResponseMsg
                {
                    status = true,
                    data = new { accountId = passenger.AccountId },
                    message = "Add passenger info success"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Add passenger info failed"
                };
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> GetPassengerInfoById(object accountObj)
        {
            JObject objTemp = JObject.Parse(accountObj.ToString());
            string id = (string)objTemp["accountId"];
            Passenger passenger = await Repository.Passenger.GetPassengerById(Guid.Parse(id));
            if (passenger is null)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Get passenger info failed, passenger does not exist"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = true,
                    data = passenger,
                    message = "Get passenger info success"
                };
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> UpdateInfo(Passenger passenger)
        {
            if (await Repository.Passenger.CheckPassengerExist(passenger.AccountId))
            {
                int res = await Repository.Passenger.UpdatePassengerInfo(passenger);
                if (res > 0)
                {
                    return new ResponseMsg
                    {
                        status = true,
                        data = null,
                        message = "Update passenger info success"
                    };
                }
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = res == -3 ? "Update failed, phone already existed" : res == -4 ? "Update failed, email already existed" : "Update failed, nothing changed"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Update passenger info failed, passenger does not exist"
                };
            }
        }
    }


}
