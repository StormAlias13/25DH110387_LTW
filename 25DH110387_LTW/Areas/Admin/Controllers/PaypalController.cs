using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using _25DH110387_LTW.Models;
using _25DH110387_LTW.Models.ViewModel;
using PayPal.Api;
using Item = PayPal.Api.Item;
using Payer = PayPal.Api.Payer;

namespace _25DH110387_LTW.Areas.Admin.Controllers
{
    public class PaypalController : Controller
    {
        private const decimal VND_PER_USD = 25000m;   // tỉ giá tạm cho bài tập

        private static string Usd(decimal vnd)
        {
            return Math.Round(vnd / VND_PER_USD, 2).ToString("0.00", CultureInfo.InvariantCulture);
        }

        private APIContext GetAPIContext()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var config = ConfigManager.Instance.GetProperties();
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            return new APIContext(accessToken) { Config = config };
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl, Cart cart)
        {
            var itemList = new ItemList() { items = new List<Item>() };
            decimal subtotal = 0;

            foreach (var item in cart.Items)
            {
                decimal unitUsd = Math.Round(item.UnitPrice / VND_PER_USD, 2);
                subtotal += unitUsd * item.Quantity;
                itemList.items.Add(new Item
                {
                    name = item.ProductName,
                    currency = "USD",
                    price = unitUsd.ToString("0.00", CultureInfo.InvariantCulture),
                    quantity = item.Quantity.ToString(),
                    sku = item.ProductID.ToString()
                });
            }

            string total = subtotal.ToString("0.00", CultureInfo.InvariantCulture);

            var payment = new Payment
            {
                intent = "sale",
                payer = new Payer { payment_method = "paypal" },
                transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        description = "Thanh toán đơn hàng MyStore",
                        invoice_number = new Random().Next(100000).ToString(),
                        amount = new Amount
                        {
                            currency = "USD",
                            total = total,
                            details = new Details { tax = "0", shipping = "0", subtotal = total }
                        },
                        item_list = itemList
                    }
                },
                redirect_urls = new RedirectUrls { cancel_url = redirectUrl, return_url = redirectUrl }
            };
            return payment.Create(apiContext);
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var payment = new Payment { id = paymentId };
            return payment.Execute(apiContext, new PaymentExecution { payer_id = payerId });
        }

        public ActionResult PaymentWithPaypal()
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account");

            var cart = new CartService(Session).GetCart();
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index2", "Cart", new { area = "" });

            try
            {
                var apiContext = GetAPIContext();
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    string guid = Guid.NewGuid().ToString("N");
                    string returnUrl = Url.Action("PaymentWithPaypal", "Paypal",
                        new { area = "Admin", guid = guid }, Request.Url.Scheme);

                    var created = CreatePayment(apiContext, returnUrl, cart);
                    string approvalUrl = created.links
                        .First(l => l.rel.ToLower().Trim() == "approval_url").href;

                    Session[guid] = created.id;
                    return Redirect(approvalUrl);
                }
                else
                {
                    string guid = Request.Params["guid"];
                    var executed = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executed.state.ToLower() != "approved")
                    {
                        TempData["Error"] = "Thanh toán PayPal không thành công.";
                        return RedirectToAction("Index2", "Cart", new { area = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi PayPal: " + ex.Message;
                return RedirectToAction("Index2", "Cart", new { area = "" });
            }

            // Thanh toán xong: tạo đơn hàng (OrderController tự đọc giỏ hàng trong Session)
            return RedirectToAction("CreateOrder", "Order");
        }
    }
}