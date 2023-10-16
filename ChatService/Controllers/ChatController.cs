using ChatService.DTOs;
using Helper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{
    [Route("api/Chat/[controller]/[action]")]
    [ApiController]
    public class ChatController : BaseController
    {
        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> GetChats(GetChatsDTO getChatsDTO)
        {
            ChatResponseDTO chatResponseDTO = await Repository.Chat.GetChat(getChatsDTO.TripId);
            bool result = true;
            if (chatResponseDTO.PassengerId == Guid.Empty || chatResponseDTO.DriverId == Guid.Empty || chatResponseDTO.TripId == Guid.Empty)
            {
                result = false;
            }
            return new ResponseMsg
            {
                status = result ? true : false,
                data = result ? chatResponseDTO : null,
                message = result ? "Get chat successfully" : "Failed to get chats"
            };

        }

        [HttpGet]
        public async Task<ResponseMsg> ClearDb()
        {
            await Repository.Chat.ClearTable();
            await Repository.ChatMessage.ClearTable();
            return new ResponseMsg
            {
                status = true,
                data = null,
                message = "Executed clear Chat services Db"
            };
        }
    }
}
