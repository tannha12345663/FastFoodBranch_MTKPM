using FastFoodBranch.Entities;
using FastFoodBranch.Models;
using System;
using System.Web.Mvc;

namespace FastFoodBranch.Controllers
{
    public class PaymentController : Controller
    {
        private IPaymentStrategy _paymentStrategy;
        //Chức năng thanh toán VNPAY
        public ActionResult VNPay()
        {
            _paymentStrategy = new VNPAYPaymentStrategy();
            Random rd = new Random();
            Cart cart = Session["Cart"] as Cart;
            var mahd = "HD" + rd.Next(1, 100) + rd.Next(1, 100);
            Session["madonhang"] = mahd;

            string paymentUrl = _paymentStrategy.GeneratePaymentUrl(cart, mahd);
            return Redirect(paymentUrl);
        }

        public ActionResult VNPayValidate()
        {
            _paymentStrategy = new VNPAYPaymentStrategy();
            if (Request.QueryString.Count > 0)
            {
                var vnpayData = Request.QueryString;
                string result = _paymentStrategy.ValidatePaymentResult(vnpayData);
                if (result == VNPayConstaints.SUCCESS)
                {
                    TempData["Status"] = true;
                }
                else
                {
                    TempData["Status"] = false;
                }
                TempData["Message"] = result;

            }
            return RedirectToAction("ThongBaoTT", "KhachHang");
        }

        //chức năng thanh toán Zalo Pay
        // GET: Payment
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Suceess()
        {
            return View();
        }
        public ActionResult Faile()
        {
            return View();
        }
        // Tạo mới giỏ hàng
        public Cart GetCart()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null || Session["Cart"] == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }
    }
}