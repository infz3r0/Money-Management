using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Money_Management.Controllers
{
    public class ManagerController : Controller
    {
        // GET: Manager
        public ActionResult Index()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
    }
}