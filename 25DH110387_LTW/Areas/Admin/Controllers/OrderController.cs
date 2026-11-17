using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH110387_LTW.Models;
using _25DH110387_LTW.Models.ViewModel;

namespace _25DH110387_LTW.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            // Kiểm tra giỏ hàng trong session
            // Nếu giỏ hàng rỗng hoặc không có sản phẩm thì chuyển hướng về trang chủ
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            // Xác thực người dùng đã đăng nhập chưa, nếu chưa thì chuyển hướng tới trang Đăng nhập
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) { return RedirectToAction("Login", "Account"); }

            // Lấy thông tin khách hàng từ CSDL, nếu chưa có thì chuyển hướng tới trang Đăng nhập
            // Nếu có rồi thì lấy địa chỉ của khách hàng và gán vào ShippingAddress của CheckoutVM
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            var model = new CheckoutVM
            {
                CartItems = cart, // Lấy danh sách các sản phẩm trong giỏ hàng
                TotalAmount = cart.Sum(item => item.TotalPrice), // Tổng giá trị của các mặt hàng trong giỏ
                OrderDate = DateTime.Now, // Mặc định lấy thời điểm đặt hàng
                ShippingAddress = customer.CustomerAddress, // Lấy địa chỉ mặc định từ bảng Customer
                CustomerID = customer.CustomerID, // Lấy mã khách hàng từ bảng Customer
                Username = customer.Username // Lấy tên đăng nhập từ bảng Customer
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid)
            {
                var cart = Session["Cart"] as List<CartItem>;
                // Nếu giỏ hàng rỗng sẽ điều hướng tới trang Home
                if (cart == null || !cart.Any()) { return RedirectToAction("Index", "Home"); }

                var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                if (user == null) { return RedirectToAction("Login", "Account"); }

                var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
                if (customer == null) { return RedirectToAction("Login", "Account"); }

                // Nếu người dùng chọn thanh toán bằng Paypal, sẽ điều hướng tới trang PaymentWithPaypal
                if (model.PaymentMethod == "Paypal")
                {
                    return RedirectToAction("PaymentWithPaypal", "Paypal", model);
                }

                // Thiết lập PaymentStatus dựa trên PaymentMethod
                string paymentStatus = "Chưa thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt": paymentStatus = "Thanh toán tiền mặt"; break;
                    case "Paypal": paymentStatus = "Thanh toán paypal"; break;
                    case "Mua trước trả sau": paymentStatus = "Trả góp"; break;
                    default: paymentStatus = "Chưa thanh toán"; break;
                }

                // Tạo đơn hàng và chi tiết đơn hàng liên quan
                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = model.OrderDate,
                    TotalAmount = model.TotalAmount,
                    PaymentStatus = paymentStatus,
                    PaymentMethod = model.PaymentMethod,
                    ShippingAddress = model.ShippingAddress,
                    OrderDetails = cart.Select(item => new OrderDetail
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                };

                // Lưu đơn hàng vào CSDL
                db.Orders.Add(order);
                db.SaveChanges();

                // Xóa giỏ hàng sau khi đặt hàng thành công
                Session["Cart"] = null;

                // Điều hướng tới trang Xác nhận đơn hàng
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            return View(model);
        }

        private static readonly string[] OrderStatuses =
            { "Chờ thanh toán", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };

        // GET: Order/MyOrder
        public ActionResult MyOrder(string tab = "all", string q = null, string more = null)
        {
            var username = Session["Username"] as string;
            if (username == null)
                return RedirectToAction("Login", "Account",
                    new { area = "Admin", returnUrl = Url.Action("MyOrder", "Order", new { area = "" }) });

            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return RedirectToAction("Index", "Home", new { area = "" });

            var query = db.Orders.Include("OrderDetails.Product")
                                 .Where(o => o.CustomerID == customer.CustomerID);

            // Tìm theo Mã đơn hàng hoặc Tên sản phẩm
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                int orderId;
                if (int.TryParse(q, out orderId))
                    query = query.Where(o => o.OrderID == orderId ||
                                             o.OrderDetails.Any(d => d.Product.ProductName.Contains(q)));
                else
                    query = query.Where(o => o.OrderDetails.Any(d => d.Product.ProductName.Contains(q)));
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            // Đơn có trạng thái null hoặc lạ thì coi như "Đang xử lý"
            Func<Order, string> statusOf = o =>
                OrderStatuses.Contains(o.OrderStatus) ? o.OrderStatus : "Đang xử lý";

            // Tab "all": hiện cả 5 trạng thái. Tab khác: chỉ hiện trạng thái đó
            var statusesToShow = (tab == "all" || !OrderStatuses.Contains(tab))
                ? OrderStatuses
                : new[] { tab };

            var groups = new List<OrderGroupVM>();
            foreach (var st in statusesToShow)
            {
                var inStatus = orders.Where(o => statusOf(o) == st).ToList();
                if (!inStatus.Any()) continue;

                bool expanded = (more == st);
                groups.Add(new OrderGroupVM
                {
                    Status = st,
                    Total = inStatus.Count,
                    Expanded = expanded,
                    // Mặc định 2 đơn gần nhất; bấm "Xem thêm" thì tối đa 50 đơn
                    Orders = inStatus.Take(expanded ? 50 : 2).ToList()
                });
            }

            return View(new MyOrderVM
            {
                Tab = OrderStatuses.Contains(tab) ? tab : "all",
                Query = q,
                Statuses = OrderStatuses.ToList(),
                Groups = groups
            });
        }

        
        // POST: Order/BuyAgain/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyAgain(int id)
        {
            var username = Session["Username"] as string;
            if (username == null)
                return RedirectToAction("Login", "Account",
                    new { area = "Admin", returnUrl = Url.Action("MyOrder", "Order", new { area = "" }) });

            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null) { return HttpNotFound(); }

            var order = db.Orders.Include("OrderDetails.Product")
                          .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null) { return HttpNotFound(); }

            var cart = new CartService(Session).GetCart();
            foreach (var d in order.OrderDetails)
            {
                // Thêm mỗi sản phẩm của đơn cũ vào giỏ với số lượng = 1
                cart.AddItem(
                    d.ProductID,
                    d.Product.ProductImage,
                    d.Product.ProductName,
                    d.UnitPrice,
                    1,
                    ""   // category: xem ghi chú bên dưới
                );
            }

            return RedirectToAction("Index2", "Cart", new { area = "" });
        }
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
    }
}