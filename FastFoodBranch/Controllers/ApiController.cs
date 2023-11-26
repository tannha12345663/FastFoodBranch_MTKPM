using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FastFoodBranch.Controllers.AdminController;

namespace FastFoodBranch.Controllers
{
    public class ApiController : Controller
    {
        QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
        // GET: Api
        [HttpGet]
        public JsonResult getDataMonAnCN(string MaCN, bool Lock01)
        {
            var dsMonAnCN = db.MonAnChiNhanhs.Where(s => s.LocationID == MaCN && s.Lock == Lock01).ToList();
            List<chitietMonAnCN> result = dsMonAnCN.Select(ma => new chitietMonAnCN
            {
                ID = ma.ID,
                TenMonAn = ma.MonAn.TenMA,
                trangThai = (int)ma.TrangThai,
                image = ma.MonAn.HinhAnh
            }).ToList();
            if (dsMonAnCN != null)
            {
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Khong co du lieu tim thay" }, JsonRequestBehavior.AllowGet);
        }

    }
}