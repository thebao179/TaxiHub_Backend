using InfoService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InfoService.Controllers
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
