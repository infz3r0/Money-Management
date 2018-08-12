using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Money_Management.Models;
using System.Security.Cryptography;

namespace Money_Management.Controllers
{
    public class AccountController : Controller
    {
        
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            if (Session["Account"] != null)
            {
                return RedirectToAction("Index", "Manager");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            string username = form["acc_username"];
            string password = form["acc_password"];

            // Check try login
            using (var db = new money_managementEntities())
            {
                account acc = db.accounts.FirstOrDefault(x => x.acc_username.Equals(username));
                if (acc != null && MD5Cal.VerifyMd5Hash(password, acc.acc_password))
                {
                    Session["Account"] = acc;
                    return RedirectToAction("Index", "Manager");
                }                
            }

            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(FormCollection form)
        {
            string username = form["acc_username"];
            string password = form["acc_password"];
            string repassword = form["re_password"];
            string email = form["acc_email"];

            // Check password confirm
            if (!password.Equals(repassword))
            {
                ViewBag.Alert = "Password does not match.";
                return View();
            }

            // Check username exists
            using (var db = new money_managementEntities())
            {
                account acc = db.accounts.FirstOrDefault(x => x.acc_username.Equals(username));
                if (acc != null)
                {
                    ViewBag.Alert = "Username already exists.";
                    return View();
                }
            }

            // Check email exists
            using (var db = new money_managementEntities())
            {
                account acc = db.accounts.FirstOrDefault(x => x.acc_email.Equals(email));
                if (acc != null)
                {
                    ViewBag.Alert = "Email already exists.";
                    return View();
                }
            }

            //Create account
            account new_acc = new account
            {
                acc_username = username,
                acc_password = MD5Cal.GetMd5Hash(password),
                acc_email = email,
                acc_balance = 0,
                acc_create_time = DateTime.Now,
                acc_verified = false
            };

            using (var db = new money_managementEntities())
            {
                db.accounts.Add(new_acc);
                db.SaveChanges();
            }

            return RedirectToAction("Login");
        }
    }
}