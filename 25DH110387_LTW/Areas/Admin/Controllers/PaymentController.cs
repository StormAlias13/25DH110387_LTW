using _25DH110387_LTW.Models;
using _25DH110387_LTW.Models.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace _25DH110387_LTW.Controllers
{
    public class PaymentController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Trang thông tin đơn hàng
        public ActionResult Index()
        {
            // Chưa đăng nhập thì sang đăng nhập, xong quay lại đây
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account",
                    new { area = "Admin", returnUrl = Url.Action("Index", "Payment", new { area = "" }) });
            }

            var cart = new CartService(Session).GetCart();
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index2", "Cart", new { area = "" });

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                // tài khoản admin không có thông tin khách hàng
                TempData["Error"] = "Tài khoản này không phải tài khoản khách hàng.";
                return RedirectToAction("Index2", "Cart", new { area = "" });
            }

            ViewBag.Customer = customer;
            return View(cart);
        }

        // Đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string addressDelivery)
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var cart = new CartService(Session).GetCart();
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index2", "Cart", new { area = "" });

            if (string.IsNullOrWhiteSpace(addressDelivery))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng.";
                return RedirectToAction("Index");
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return RedirectToAction("Index2", "Cart", new { area = "" });

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalValue(),
                PaymentStatus = "Chưa thanh toán (COD)",
                AddressDelivery = addressDelivery.Trim()
            };

            foreach (var item in cart.Items)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            db.Orders.Add(order);
            db.SaveChanges();

            new CartService(Session).ClearCart();
            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        public ActionResult OrderSuccess(int id)
        {
            string username = Session["Username"] as string;
            var order = db.Orders.Include("OrderDetails.Product")
                .FirstOrDefault(o => o.OrderID == id && o.Customer.Username == username);
            if (order == null) return HttpNotFound();
            return View(order);
        }
    }
}