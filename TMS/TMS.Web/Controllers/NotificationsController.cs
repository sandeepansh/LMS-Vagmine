using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TMS.Web.Controllers
{
 
    public class NotificationsController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
