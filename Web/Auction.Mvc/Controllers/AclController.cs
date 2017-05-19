using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Auction.Mvc.Controllers
{
    public class AclController : Controller
    {
        // GET: Acl
        public ActionResult Index()
        {
            var u = User.Identity.Name;
            ViewBag.Username = u;
            return View();
        }
    }
}