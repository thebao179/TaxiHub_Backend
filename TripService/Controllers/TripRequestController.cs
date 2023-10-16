using Helper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using TripService.DataAccess;
using TripService.Models;
using TripService.Repositories;

namespace TripService.Controllers
{
    [Route("api/Trip/[controller]/[action]")]
    [ApiController]
    public class TripRequestController : BaseController
    {
        private readonly TripRequestDataAccess _dataAccess;

        public TripRequestController()
        {
            _dataAccess = new TripRequestDataAccess();
        }

        [Authorize]
        [HttpPost]
        public async Task<ResponseMsg> SendRequest(TripRequest request)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            request.PassengerId = UserId;
            request.RequestId = Guid.NewGuid();
            request.CreatedTime = DateTime.Now;
            int result = await _dataAccess.CreateRequest(UserId.ToString(), request);
            //int result = await Repository.TripRequest.CreateRequest(request);
            return new ResponseMsg
            {
                status = result > 0 ? true : false,
                data =  result > 0 ? request.RequestId : null,
                message = result > 0 ? "Send request successfully":"Failed to send request",
            };
        }

        [HttpGet]
        public async Task<ResponseMsg> CalculatePrice(double distance)
        {
            return new ResponseMsg
            {
                status = true,
                data = Repository.TripRequest.CalcPrice(distance),
                message = "Price base on distance and vehicle type",
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> CancelRequest([FromBody]object requestIdJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(requestIdJson.ToString());
            string requestId = (string)objTemp["requestId"];
            int result = await _dataAccess.CancelRequest(UserId.ToString(), Guid.Parse(requestId));
            //int result = await Repository.TripRequest.CancelRequest(Guid.Parse(requestId));
            return new ResponseMsg
            {
                status = result > 0 ? true : false,
                data = null,
                message = result > 0 ? "Cancel request successfully" : "Failed to cancel request",
            };
        }
    }
}
