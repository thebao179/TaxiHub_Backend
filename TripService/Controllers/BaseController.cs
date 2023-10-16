using Microsoft.AspNetCore.Mvc;
using TripService.Repositories;

namespace TripService.Controllers
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
