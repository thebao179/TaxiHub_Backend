using AuthenticationService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers
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
