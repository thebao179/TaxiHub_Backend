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
    public class StaffController : BaseController
    {
        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<ResponseMsg> GetStaffs()
        {
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Staff.GetAllStaffs(),
                message = "Get all staff info success"
            };
        }

        [HttpGet]
        public async Task<ResponseMsg> GetNumOfPages(int pageSize)
        {
            return new ResponseMsg
            {
                status = true,
                data = await Repository.Staff.CalcNumOfPages(pageSize),
                message = "Get num of pages success"
            };
        }

        [HttpGet]
        [Authorize(Roles = "Staff")]
        public async Task<ResponseMsg> GetStaffsWithPagination(int pageSize, int pageNum)
        {
            if (pageNum == 0)
            {
                pageNum = 1;
            }
            if (pageSize == 0)
            {
                pageSize = 15;
            }
            int totalPage = await Repository.Staff.CalcNumOfPages(pageSize);
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
                data = await Repository.Staff.GetStaffsWithPagination(pageNum, pageSize),
                message = $"Get staffs in page {pageNum} success"
            };
        }

        [HttpPost]
        public async Task<ResponseMsg> AddInfo(Staff staff)
        {
            int result = await Repository.Staff.AddStaffInfo(staff);
            if (result > 0)
            {
                return new ResponseMsg
                {
                    status = true,
                    data = new { accountId = staff.AccountId },
                    message = "Add staff info success"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Add staff info failed"
                };
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> GetStaffInfoById(object accountObj)
        {
            JObject objTemp = JObject.Parse(accountObj.ToString());
            string id = (string)objTemp["accountId"];
            Staff staff = await Repository.Staff.GetStaffById(Guid.Parse(id));
            if(staff is null)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = "Get staff info failed, staff does not exist"
                };
            }
            else
            {
                return new ResponseMsg
                {
                    status = false,
                    data = staff,
                    message = "Get staff info success"
                };
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> UpdateInfo(Staff staff)
        {
            if (await Repository.Staff.CheckStaffExist(staff.AccountId))
            {
                int res = await Repository.Staff.UpdateStaffInfo(staff);
                if (res > 0)
                {
                    return new ResponseMsg
                    {
                        status = true,
                        data = null,
                        message = "Update staff info success"
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
                    message = "Update staff info failed, staff does not exist"
                };
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> ClearDb()
        {
            await Repository.Driver.ClearTable();
            await Repository.Staff.ClearTable();
            await Repository.Passenger.ClearTable();
            await Repository.Vehicle.ClearTable();

            return new ResponseMsg
            {
                status = true,
                data = null,
                message = "Executed clear all database of Info service"
            };
        }
    }
}
