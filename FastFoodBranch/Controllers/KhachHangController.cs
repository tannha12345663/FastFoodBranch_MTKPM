using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using FastFoodBranch;
using FastFoodBranch.App_Start;
using FastFoodBranch.Models;
using FastFoodBranch.Entities;
using static FastFoodBranch.Controllers.AdminController;

namespace FastFoodBranch.Controllers
{
    public class KhachHangController : Controller
    {
        QuanLyFastFoodEntities1 db = ConnectSingleton.GetInstance();
        
        // GET: KhachHang
        public ActionResult TrangChu()
        {
            return View();
        }

        //Set up chọn chi nhánh
        [HttpPost]
        public ActionResult chonChiNhanh(string id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var check = db.ChiNhanhs.Where(s=>s.LocationID == id).FirstOrDefault();
                    Session["locationCN"] = check.LocationID;
                    if (check != null)
                    {
                        GetCart().XoaSauKhiDat();
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message}, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DSThucDon()
        {
            return View();
        }

        public ActionResult VeChungToi()
        {
            return View();
        }

        public ActionResult DatBan()
        {
            return View();
        }

        public ActionResult ThanhToan()
        {
            if (Session["Cart"] == null)
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
                return View();
            Cart cart = Session["Cart"] as Cart;
            return View(cart);
        }
        public ActionResult ThongBaoTT()
        {
            Boolean check = (Boolean) TempData["Status"];
            if (check == true)
            {
                ObjectParameter MaHoaDonTrongOutput = new ObjectParameter("MaHoaDonTrongOutput", typeof(String));
                //Tiến hành thêm dữ liệu ra hóa đơn cho khách hàng
                var checkCN = Session["locationCN"];
                var kh = (KhachHang)Session["KHVangLai"];
                var check1 = (KhachHang)Session["userKH"];
                Cart cart = Session["Cart"] as Cart;
                try
                {
                    if (check1 != null)
                    {
                        
                        //Khởi tạo hóa đơn
                        db.sp_ThemHD(null, cart.TongTien(), cart.MaVoucher, null, check1.MaKH, (string)checkCN ,MaHoaDonTrongOutput);
                        foreach (var items in cart.Items)
                        {
                            db.sp_ThemChiTietHD((string)MaHoaDonTrongOutput.Value, items.idMA.ID, items.soLuong, items.idMA.GiaGoc);
                        }
                        db.SaveChanges();

                    }
                    else
                    {
                        //Lưu thông tin khách hàng vãng lai
                        var checkEmail = db.KhachHangs.Where(s => s.Email == kh.Email).FirstOrDefault();
                        if(checkEmail == null)
                        {
                            db.KhachHangs.Add(kh);
                            db.SaveChanges();
                            //Khởi tạo hóa đơn
                            db.sp_ThemHD(true, cart.TongTien(), cart.MaVoucher, null, kh.MaKH, (string)checkCN,MaHoaDonTrongOutput);
                        }
                        else
                        {
                            //Khởi tạo hóa đơn
                            db.sp_ThemHD(true, cart.TongTien(), cart.MaVoucher, null, checkEmail.MaKH, (string)checkCN, MaHoaDonTrongOutput);
                        }
                        
                        //Khởi tạo hóa đơn
                        db.sp_ThemHD(true, cart.TongTien(), cart.MaVoucher, null, kh.MaKH, (string)checkCN, MaHoaDonTrongOutput);
                        foreach (var items in cart.Items)
                        {
                            db.sp_ThemChiTietHD((string)MaHoaDonTrongOutput.Value, items.idMA.ID, items.soLuong, items.idMA.GiaGoc);
                        }
                        db.SaveChanges();

                    }
                    //Kiểm tra lại kho và thực hiện trừ số lượng nguyên liệu theo công thức 
                    db.sp_CheckTonKho((string)MaHoaDonTrongOutput.Value, (string)checkCN);

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["mess"] = e.Message;
                    return View();
                }
                TempData["messageAlert"] = "success0";
                GetCart().XoaSauKhiDat();
            }
            else
            {
                TempData["messageAlert"] = "error0";

            }
            return View();
        }

        [AuthenticationKH]
        [HttpGet]
        //Trang thông tin sản phẩm
        public ActionResult TTSanPham(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var location = (string)Session["locationCN"];

            if(location == null)
            {
                return RedirectToAction("DSThucDon");
            }
            else
            {
                var ma = db.MonAns.Find(id);
                if (ma == null)
                {
                    return RedirectToAction("DSThucDon");
                }
                return View(ma);
            }

            
        }
        //Đăng nhập và đăng ký thông tin tài khoản
        public ActionResult LRAccount()
        {
            return View();
        }
        //Patial View Menu 
        public ActionResult Menu()
        {
            return View();
        }
        //Patial View Swiper cho phần đơn đặt hàng của bạn
        public ActionResult SwiperOrder()
        {
            return View();
        }
        //Patial View hiển thị lịch sử đặt hàng
        public ActionResult OrderHistory()
        {
            return View();
        }
        //Hàm lấy thông tin chi tiết hóa đơn
        public class chitietHoaDon
        {
            public string ImageMA { get; set; }
            public string TenMonAn { get; set; }
            public int SoLuong { get; set; }
            public double Gia { get; set; }
        }
        [HttpPost]
        public JsonResult getDataChiTietHD(string MaHD)
        {
            var dsCTHoaDon = db.ChiTietHDs.Where(s => s.IDHoaDon == MaHD).ToList();
            var mavoucher = db.HoaDons.Where(s=>s.ID == MaHD).FirstOrDefault();
            var discout = db.Vouchers.Where(s=> s.ID == mavoucher.IDVoucher).FirstOrDefault();
            List<chitietHoaDon> result = dsCTHoaDon.Select(ma => new chitietHoaDon
            {
                ImageMA = ma.MonAn.HinhAnh,
                TenMonAn = ma.MonAn.TenMA,
                SoLuong = (int)ma.SoLuong,
                Gia = (double)ma.GiaBan
            }).ToList();
            if (dsCTHoaDon != null && discout != null)
            {
                return Json(new { success = true, data = result,vouCher = discout.Discount  }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, data = result, vouCher = "" }, JsonRequestBehavior.AllowGet);
            }
            //return Json(new { success = false, message = "Khong co du lieu tim thay" }, JsonRequestBehavior.AllowGet);
        }

        //Trang thông tin khách hàng
        [AuthenticationKH]
        public ActionResult ThongTinKH()
        {
            return View();
        }
        //Cập nhật thông tin KH
        [HttpPost]
        public ActionResult CapNhatTTKH([Bind(Include = "TenKH,SDT,DiaChi,Email,NgaySinh,CCCD")] KhachHang kh)
        {
            if (ModelState.IsValid)
            {
                var user = (KhachHang)Session["userKH"];
                try
                {
                    db.sp_CapNhatTTKH(user.MaKH,kh.TenKH,kh.SDT,kh.DiaChi,kh.Email,kh.NgaySinh,kh.CCCD);
                    db.SaveChanges();
                    var capnhatS = db.KhachHangs.Where(s=>s.MaKH == user.MaKH).FirstOrDefault();
                    Session["userKH"] = capnhatS;
                    TempData["messageAlert"] = "success0";

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("ThongTinKH");
            }
            return HttpNotFound();
        }
        //Cập nhật Mật khẩu
        [HttpPost]
        public ActionResult CapNhatMKKH([Bind(Include = "Password")] KhachHang kh,string oldPassword, string newPassword, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                var user = (KhachHang)Session["userKH"];
                if(oldPassword == user.Password && newPassword == confirmPassword)
                {
                    try
                    {
                        db.sp_CapNhatMKKH(user.MaKH, newPassword);
                        db.SaveChanges();
                        var capnhatS = db.KhachHangs.Where(s => s.MaKH == user.MaKH).FirstOrDefault();
                        Session["userKH"] = capnhatS;
                        TempData["messageAlert"] = "success0";

                    }
                    catch (Exception e)
                    {
                        TempData["messageAlert"] = "error0";
                        TempData["mess"] = e.InnerException.Message;
                    }
                }
                else
                {
                    TempData["messageAlert"] = "error0";
                    TempData["mess"] = "Mat khau khong dung || mat khau moi va xac nhan khong dung!";
                }
                return RedirectToAction("ThongTinKH");
            }
            return HttpNotFound();
        }
        //Cập nhật avartar
        [HttpPost]
        public ActionResult CapNhatAvartarKH(HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                var user = (KhachHang)Session["userKH"];
                try
                {
                    if(HinhAnh != null)
                    {
                        LuuAnh(user, HinhAnh);
                    }
                    else
                    {
                        user.HinhAnh = null;
                    }
                    db.sp_CapNhatAvartarKH(user.MaKH,user.HinhAnh);
                    db.SaveChanges();
                    var capnhatS = db.KhachHangs.Where(s => s.MaKH == user.MaKH).FirstOrDefault();
                    Session["userKH"] = capnhatS;
                    TempData["messageAlert"] = "success0";

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("ThongTinKH");
            }
            return HttpNotFound();
        }
        //Phần xử lý khi hình ảnh  được gửi lên
        public void LuuAnh(KhachHang sp, HttpPostedFileBase HinhAnh)
        {
            #region Hình ảnh
            //Xác định đường dẫn lưu file : Url tương đói => tuyệt đói
            var urlTuongdoi = "/Content/AvartarKH/";
            var urlTuyetDoi = Server.MapPath(urlTuongdoi);// Lấy đường dẫn lưu file trên server

            //Check trùng tên file => Đổi tên file  = tên file cũ (ko kèm đuôi)
            //Ảnh.jpg = > ảnh + "-" + 1 + ".jpg" => ảnh-1.jpg

            string fullDuongDan = urlTuyetDoi + HinhAnh.FileName;
            int i = 1;
            while (System.IO.File.Exists(fullDuongDan) == true)
            {
                // 1. Tách tên và đuôi 
                var ten = Path.GetFileNameWithoutExtension(HinhAnh.FileName);
                var duoi = Path.GetExtension(HinhAnh.FileName);
                // 2. Sử dụng biến i để chạy và cộng vào tên file mới
                fullDuongDan = urlTuyetDoi + ten + "-" + i + duoi;
                i++;
                // 3. Check lại 
            }
            #endregion
            //Lưu file (Kiểm tra trùng file)
            HinhAnh.SaveAs(fullDuongDan);
            sp.HinhAnh = urlTuongdoi + Path.GetFileName(fullDuongDan);
        }

        [AuthenticationKH]
        public ActionResult MangXaHoi()
        {
            return View();
        }
        [AuthenticationKH]
        public ActionResult DiaChiGH()
        {
            return View();
        }
        [AuthenticationKH]
        public ActionResult DonHangCuaBan()
        {
            return View();
        }
        [AuthenticationKH]
        public ActionResult Voucher()
        {
            return View();
        }



        [AuthenticationKH]
        public ActionResult MenuMobile()
        {
            return View();
        }

        //Giỏ hàng
        //Trang giỏ hàng
        public ActionResult GioHang()
        {
            if (Session["Cart"] == null)
                return View();
            Cart cart = Session["Cart"] as Cart;
            return View(cart);
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
        //Thêm vào giỏ hàng
        public ActionResult ThemVaoGH(string id,int sl)
        {
            var sp = db.MonAns.SingleOrDefault(s => s.ID == id);

            //if (sp.TongTon == 0 || sp.TongTon == null)
            //{
            //    TempData["hethang"] = "Sản phẩm đã hết hàng!!!";
            //}
            //else if (sp.TongTon > 0)
            //{
            //    TempData["Themvaogiohang"] = "thanhcong";
                
            //}

            GetCart().Add(sp,sl);
            var slht = GetCart().TongSLTrongGio();
            return Json(new { success = true, sl = slht }, JsonRequestBehavior.AllowGet);
        }
        //Cập nhật số lượng 
        [HttpPost]
        public ActionResult CapNhatGH(string id, int sl)
        {
            if (id != null && sl > 0)
            {
                GetCart().CapNhatSL(id, sl);
                var slht = GetCart().TongSLTrongGio();
                var tamtinh = GetCart().TongTienBefore();
                var tongcong = GetCart().TongTien();
                return Json(new { success = true, sl = slht,tt = tamtinh,tc = tongcong }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // Lấy tổng số lượng đang có trong giỏ hàng
        [HttpPost]
        public ActionResult LaySLCart()
        {
            int sl = 0;
            Cart cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                sl = 0;
            }
            else
            {
                sl = cart.TongSLTrongGio();
            }
            return Json(new { success = true, sl = sl }, JsonRequestBehavior.AllowGet);
        }

        //Kiểm tra mã khuyến mãi
        [HttpPost]
        public ActionResult CheckVoucher(string id)
        {
            double tongcong = 0;
            if (id != null)
            {
                var check = db.Vouchers.Where(s => s.ID == id).FirstOrDefault();
                var location = (string)Session["locationCN"];
                
                if (check == null)
                {
                    GetCart().setmaVC(id);
                    //var tongcong = GetCart().TongTien() - (GetCart().TongTien() * check.Discount);
                    tongcong = GetCart().TongTien();
                    return Json(new { success = 0, vc = 0, tc = tongcong }, JsonRequestBehavior.AllowGet);
                }
                else if (check!= null)
                {
                    //kiểm tra mã voucher này có phù hợp với chi nhánh này không
                    
                    if(check.LocationID == null) // Mã dành cho toàn hệ thống
                    {
                        GetCart().setmaVC(id);
                        //var tongcong = GetCart().TongTien() - (GetCart().TongTien() * check.Discount);
                        tongcong = GetCart().TongTien();
                        return Json(new { success = 1, vc = check.Discount, tc = tongcong }, JsonRequestBehavior.AllowGet);
                    }
                    else if (check.LocationID == location)
                    {
                        //Mã chỉ áp dụng cho từng chi nhánh
                        GetCart().setmaVC(id);
                        //var tongcong = GetCart().TongTien() - (GetCart().TongTien() * check.Discount);
                        tongcong = GetCart().TongTien();
                        return Json(new { success = 1, vc = check.Discount, tc = tongcong }, JsonRequestBehavior.AllowGet);
                    }
                    else if (check.LocationID != location)
                    {
                        //Mã này không có có ở chi nhánh này
                        GetCart().setmaVC(null);
                        tongcong = GetCart().TongTien();
                        return Json(new { success = 2, tc = tongcong }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    GetCart().setmaVC(null);
                    tongcong = GetCart().TongTien();
                    return Json(new { success = 0, tc = tongcong }, JsonRequestBehavior.AllowGet);
                }
            }
            GetCart().setmaVC(null);
            tongcong = GetCart().TongTien();
            return Json(new { success = false, tc = tongcong }, JsonRequestBehavior.AllowGet);

        }
        //Xóa sản phẩm ra khỏi giỏ hàng
        [HttpPost]
        public ActionResult XoaMA(string id)
        {
            try
            {
                GetCart().XoaSP(id);
                var tamtinh = GetCart().TongTienBefore();
                var tongcong = GetCart().TongTien();
                var sl = GetCart().TongSLTrongGio();
                return Json(new { success = true, tt = tamtinh, tc = tongcong, sl=sl }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false}, JsonRequestBehavior.AllowGet);
            }
        }

        //Tiến hành thanh toán
        [HttpPost]
        public ActionResult XacNhanThanhToan([Bind(Include = "TenKH,SDT,DiaChi,Email,Password,CCCD")] Customer kh, string LoaiTT)
        {
            Random rd = new Random();
            
            try
            {

                KhachHang khach = new KhachHang();
                khach.MaKH = "KH" + rd.Next(0, 100);
                khach.UserName = khach.MaKH;
                khach.Password = kh.Password;
                khach.TenKH = kh.TenKH;
                khach.SDT = kh.SDT;
                khach.DiaChi = kh.DiaChi;
                khach.Email = kh.Email;
                Session["KHVangLai"] = khach;
                //db.KhachHangs.Add(khach);
                //db.SaveChanges();
                if(LoaiTT == "1")
                {
                    return RedirectToAction("VNPay", "Payment");
                }
                else if(LoaiTT == "2")
                {
                    TempData["Message"] = "Thanh toán thành công hóa đơn ";
                    TempData["Status"] = true;
                    return RedirectToAction("ThongBaoTT", "KhachHang");
                }
            }
            catch
            {

            }
            return View();
        }
        //Danh sách yêu thích của khách hàng
        [AuthenticationKH]
        public ActionResult DanhSachYeuThich()
        {
            return View();
        }
        [HttpPost]
        public JsonResult getDanhSachYeuThich(string MaKH)
        {
            var dsYeuThich = db.DSYTs.Where(yt => yt.MaKH == MaKH).ToList();
            List<YeuThichViewModel> result = new List<YeuThichViewModel>();

            foreach (var yeuThich in dsYeuThich)
            {
                // Lấy thông tin từ bảng ChiNhanh và MonAn
                var chiNhanh = db.ChiNhanhs.FirstOrDefault(cn => cn.LocationID == yeuThich.LocationID);
                var monAn = db.MonAns.FirstOrDefault(ma => ma.ID == yeuThich.IDMonAn);

                // Kiểm tra xem có thông tin từ cả hai bảng hay không
                if (chiNhanh != null && monAn != null)
                {
                    result.Add(new YeuThichViewModel
                    {
                        IDMonAn = yeuThich.IDMonAn,
                        TenCN = chiNhanh.TenCN,
                        TenMA = monAn.TenMA,
                        IDCN = yeuThich.LocationID,
                        HinhAnh = monAn.HinhAnh


                    });
                }
            }
            if (result.Any())
            {
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, data = new List<YeuThichViewModel>() }, JsonRequestBehavior.AllowGet);
            }
        }
        public class YeuThichViewModel
        {
            public string IDMonAn { get; set; }
            public string TenCN { get; set; }
            public string TenMA { get; set; }
            public string IDCN { get; set; }

            public string HinhAnh { get; set; }
        }
        [HttpPost]
        public JsonResult ThemDSYT(string id, string user)
        {
            var checkCN = (string)Session["locationCN"];
            try
            {
                int result = db.sp_ThemDSYT(user, id, checkCN);
                db.SaveChanges();
                return Json(new { success = true, message = "Successfully added." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.InnerException.Message });

            }

        }
        [HttpPost]
        public JsonResult XoaDSYT(string id, string user, string idCN)
        {
            try
            {
                int result = db.sp_XoaDSYT(user, id, idCN);
                db.SaveChanges();
                return Json(new { success = true, message = "Removed Successfully." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.InnerException.Message });

            }
        }
        [HttpPost]
        public JsonResult TrangTTMA(string idCN)
        {
            try
            {
                var location =(string)Session["locationCN"];
                if (location == idCN)
                {
                    return Json(new { success = true});
                }
                else
                {
                    return Json(new { success = false});
                }
               
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.InnerException.Message });

            }
        }
        //Hủy đơn hàng
        //Xét duyệt hóa đơn
        [HttpPost]
        public ActionResult XetDuyetHD(string MaHD, bool TT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XetDuyetHD(MaHD, TT);
                    if (TT == true)
                    {
                        var check = db.HoaDons.Where(s => s.ID == MaHD).FirstOrDefault();
                        db.sp_CheckTonKho(MaHD, check.LocationID);
                    }
                    db.SaveChanges();
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}