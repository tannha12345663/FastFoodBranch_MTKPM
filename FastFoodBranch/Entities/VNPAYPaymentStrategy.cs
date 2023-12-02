using FastFoodBranch.Controllers;
using FastFoodBranch.Models;
using FastFoodBranch.Models.VNPay;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Mvc;

namespace FastFoodBranch.Entities
{
    public class VNPAYPaymentStrategy : IPaymentStrategy
    {
        private string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
        public string GeneratePaymentUrl(Cart cart,string mahd)
        {

            var tt = cart.TongTien().ToString();

            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];

            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", tt + "00"); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán hóa đơn "); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", mahd.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return paymentUrl;
        }

        public string ValidatePaymentResult(NameValueCollection data)
        {
            PayLib pay = new PayLib();

            //lấy toàn bộ dữ liệu được trả về
            foreach (string s in data)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                {
                    pay.AddResponseData(s, data[s]);
                }
            }

            string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
            string vnp_SecureHash = data["vnp_SecureHash"];

            bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret);

            if (checkSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    return VNPayConstaints.SUCCESS;
                }
                else
                {
                    return VNPayConstaints.FAIL_IN_PROCESS;
                }
            }
            else
            {
                return VNPayConstaints.INVALID_SIGNATURE;
            }
        }
    }
}