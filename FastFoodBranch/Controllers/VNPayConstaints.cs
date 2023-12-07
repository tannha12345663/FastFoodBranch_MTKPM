using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FastFoodBranch.Controllers
{
    public class VNPayConstaints
    {
        public const string SUCCESS = "Thanh toán thành công!";
        public const string FAIL_IN_PROCESS = "Đã có lỗi trong quá trình xử lí!";
        public const string INVALID_SIGNATURE = "Sai chữ ký!";
    }
}