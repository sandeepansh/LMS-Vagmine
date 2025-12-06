using Microsoft.AspNetCore.Mvc;

namespace TMS.Web.Controllers
{
    public class ChatController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
