using Helper.Models;
using InfoService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace InfoService.Controllers
{
    [Route("api/Info/[controller]/[action]")]
    [ApiController]
    public class DriverController : BaseController
    {
        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<ResponseMsg> GetDrivers()
        {
            return new ResponseMsg {
                status = true,
                data = await Repository.Driver.GetAllDrivers(),
                message = "Get all driver info success"
            };
        }

        [HttpGet]
        public async Task<ResponseMsg> GetNumOfPages(int pageSize)
        {
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Driver.CalcNumOfPages(pageSize),
                message = "Get num of pages success"
            };
        }

        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<ResponseMsg> GetDriversWithPagination(int pageSize, int pageNum)
        {
            if (pageNum == 0)
            {
                pageNum = 1;
            }
            if (pageSize == 0)
            {
                pageSize = 15;
            }
            int totalPage = await Repository.Driver.CalcNumOfPages(pageSize);
            if(pageNum > totalPage) {
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
                data = await Repository.Driver.GetDriversWithPagination(pageNum, pageSize),
                message = $"Get drivers in page {pageNum} success"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> AddInfo(Driver driver)
        {
            int result = await Repository.Driver.AddDriverInfo(driver);
            if(result > 0)
            {
                return new ResponseMsg
                {
                    status = true,
                    data = new {accountId = driver.AccountId},
                    message = "Add driver info success"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Add driver info failed"
                };
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> GetDriverInfoById(object accountObj)
        {
            JObject objTemp = JObject.Parse(accountObj.ToString());
            string id = (string)objTemp["accountId"];
            Driver driver = await Repository.Driver.GetDriverById(Guid.Parse(id));
            if (driver is null)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Get driver info failed, driver does not exist"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = driver,
                    message = "Get driver info success"
                };
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> UpdateInfo(Driver driver)
        {
            if(await Repository.Driver.CheckDriverExist(driver.AccountId))
            {
                int res = await Repository.Driver.UpdateDriverInfo(driver);
                if(res > 0)
                {
                    return new ResponseMsg
                    {
                        status = true,
                        data = null,
                        message = "Update driver info success"
                    };
                }
                // -4 email already exist
                // -3 phone already exist
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = res == -3 ? "Update failed, phone already existed": res == -4 ? "Update failed, email already existed" : "Update failed, nothing changed"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Update driver info failed, driver does not exist"
                };
            }
        }

    }
}
