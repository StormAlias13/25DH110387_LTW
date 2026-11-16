using _25DH110387_LTW.Models;
using _25DH110387_LTW.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _25DH110387_LTW.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Admin/Account/Register
        [HttpPost]
        public ActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                UserRole = "C"               // NCHAR(1): C = customer, A = admin
            };

            var customer = new Customer
            {
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                CustomerEmail = model.CustomerEmail,
                CustomerAddress = model.CustomerAddress,
                Username = model.Username
            };

            try
            {
                db.Users.Add(user);
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Login");   // đăng ký xong sang đăng nhập
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        ModelState.AddModelError("", eve.Entry.Entity.GetType().Name + "." + ve.PropertyName + ": " + ve.ErrorMessage);
                return View(model);
            }
        }

        // GET: Admin/Account/Login
        //public ActionResult Login()
        //{
        //    return View();
        //}

        //// POST: Admin/Account/Login
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Login(LoginVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = db.Users.SingleOrDefault(u => u.Username == model.Username
        //                                              && u.Password == model.Password);
        //        if (user != null)
        //        {
        //            Session["Username"] = user.Username;
        //            Session["UserRole"] = user.UserRole.Trim();

        //            FormsAuthentication.SetAuthCookie(user.Username, false);

        //            // Admin về trang quản trị, khách về trang chủ
        //            if (user.UserRole.Trim() == "A")
        //                return RedirectToAction("Index", "Home", new { area = "Admin" });

        //            return RedirectToAction("Index", "Home", new { area = "" });
        //        }
        //        ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
        //    }
        //    return View(model);
        //}
        // GET
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: thêm tham số returnUrl
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                      && u.Password == model.Password);
                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole.Trim();
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    if (user.UserRole.Trim() == "A")
                        return RedirectToAction("Index", "Home", new { area = "Admin" });

                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        // GET: Admin/Account/ProfileInfo
        public ActionResult ProfileInfo()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", new
                {
                    returnUrl = Url.Action("ProfileInfo", "Account", new { area = "Admin" })
                });
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                // tài khoản admin không có thông tin khách hàng
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View(customer);
        }

        // POST: Admin/Account/ProfileInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileInfo(Customer model)
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login");

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return RedirectToAction("Index", "Home", new { area = "" });

            if (!ModelState.IsValid)
            {
                model.Username = username;
                return View(model);
            }

            // Chỉ cập nhật các trường cho phép sửa
            customer.CustomerName = model.CustomerName;
            customer.CustomerPhone = model.CustomerPhone;
            customer.CustomerEmail = model.CustomerEmail;
            customer.CustomerAddress = model.CustomerAddress;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin thành công.";
            return RedirectToAction("ProfileInfo");
        }
    }
}