using Microsoft.AspNetCore.Mvc;

namespace TMS.Web.Controllers
{
    public class DiscussionController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
