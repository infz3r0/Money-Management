using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Money_Management.Models;
using PagedList;

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

            account acc = (account)Session["Account"];

            List<account_money> account_Money = null;
            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    account_Money = db.account_money.Include("base_money").Where(x => x.acc_id == acc.acc_id).ToList();
                    ViewBag.Balance = db.accounts.FirstOrDefault(x => x.acc_id == acc.acc_id).acc_balance;
                }
            }

             

            return View(account_Money);
        }

        public ActionResult AllTransaction(int? page)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int aid = ((account)Session["Account"]).acc_id;
            List<tran> trans = null;
            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    trans = db.trans.Include("trans_type").Include("category").Where(x => x.acc_id == aid).OrderByDescending(x=>x.t_date).ToList();
                }
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(trans.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Transaction()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            // use for script
            List<base_money> bm = null;
            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    bm = db.base_money.ToList();
                }
            }

            return View(bm);
        }

        [HttpPost]
        public ActionResult Transaction(FormCollection f)
        {
            DateTime date = DateTime.Now;
            string info = f["info"];
            int acc_id = ((account)Session["Account"]).acc_id;
            int type_id = Convert.ToInt32(f["trans_type"]);
            int cate_id = Convert.ToInt32(f["category"]);

            tran t = new tran
            {
                t_date = date,
                t_info = info,
                acc_id = acc_id,
                type_id = type_id,
                c_id = cate_id
            };

            //check quantity validity
            if (type_id == 2)
            {
                if (ModelState.IsValid)
                {
                    using (var db = new money_managementEntities())
                    {
                        List<base_money> bm = db.base_money.ToList();
                        foreach (base_money m in bm)
                        {
                            string name = "Q_" + m.bm_value.ToString();
                            int tquantity = Convert.ToInt32(f[name]);

                            if (tquantity <= 0)
                            {
                                continue;
                            }

                            int aquantity = Convert.ToInt32(db.account_money.Where(x => x.acc_id == acc_id).FirstOrDefault(x => x.bm_id == m.bm_id).quantity);
                            if (aquantity - tquantity < 0)
                            {
                                ViewBag.Alert = "You don't have enough money.";
                                return View();
                            }
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    //trans
                    db.trans.Add(t);
                    db.SaveChanges();

                    //detail
                    List<base_money> bm = db.base_money.ToList();
                    foreach (base_money m in bm)
                    {
                        string name = "Q_" + m.bm_value.ToString();
                        int quantity = Convert.ToInt32(f[name]);

                        if (quantity <= 0)
                        {
                            continue;
                        }

                        trans_detail detail = new trans_detail
                        {
                            t_id = t.t_id,
                            bm_id = m.bm_id,
                            t_quantity = quantity
                        };

                        db.trans_detail.Add(detail);
                    }
                    db.SaveChanges();

                    //update quantity
                    db.P_UpdateQuantityAfterTrans();
                }
            }

            return RedirectToAction("Index");
        }

        #region Partial View
        public ActionResult PV_TransType()
        {
            List<trans_type> types = null;
            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    types = db.trans_type.ToList();
                }
            }

            return PartialView(types);
        }

        public ActionResult PV_Category()
        {
            List<category> categories = null;
            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    categories = db.categories.ToList();
                }
            }

            return PartialView(categories);
        }

        public ActionResult PV_BaseMoney()
        {
            List<base_money> bm = null;
            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    bm = db.base_money.ToList();
                }
            }

            return PartialView(bm);
        }

        public ActionResult PV_TableTrans()
        {
            int aid = ((account)Session["Account"]).acc_id;
            List<tran> trans = null;

            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    trans = db.trans.Include("trans_type").Include("category").Take(10).OrderByDescending(x=>x.t_date).OrderByDescending(x=>x.t_id).ToList();
                }
            }

            return PartialView(trans);
        }

        [HttpPost]
        public ActionResult PV_Detail(int tid)
        {
            int aid = ((account)Session["Account"]).acc_id;
            List<trans_detail> details = null;

            if (ModelState.IsValid)
            {
                using (var db = new money_managementEntities())
                {
                    details = db.trans_detail.Include("base_money").Where(x => x.t_id == tid && x.tran.acc_id == aid).ToList();
                }
            }

            return PartialView(details);
        }
        #endregion

        //end class
    }
}