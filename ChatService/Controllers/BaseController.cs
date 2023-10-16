using ChatService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{
    public class BaseController : ControllerBase, IDisposable
    {
        protected ServiceRepository Repository = new ServiceRepository();
        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}
