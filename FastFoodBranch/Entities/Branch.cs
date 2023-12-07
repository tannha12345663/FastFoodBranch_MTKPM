using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FastFoodBranch.Entities
{
    public class Branch : ChiNhanh, IBranch
    {
        public void fatoryFromChiNhanh(ChiNhanh chiNhanh)
        {
            this.LocationID = chiNhanh.LocationID;
            this.TenCN = chiNhanh.TenCN;
            this.DiaChi = chiNhanh.DiaChi;
            this.NVQL = chiNhanh.NVQL;
            this.Lock = chiNhanh.Lock;
        }

        QuanLyFastFoodEntities1 db = ConnectSingleton.GetInstance();
        public List<HoaDon> getListIncome(int year,string option="")
        {
            
            var invoiceList = db.HoaDons.Where(hd =>hd.NgayTao.Value.Year == year && hd.LocationID==LocationID &&hd.TrangThai == true).ToList();
            List<HoaDon> result = invoiceList.Select(ma => new HoaDon
            {
                ID = ma.ID,
                NgayTao = ma.NgayTao,
                LocationID = ma.LocationID,
                TongCong = ma.TongCong,
                IDVoucher = ma.IDVoucher,
                MaNVLap = ma.MaNVLap,
                MaKH = ma.MaKH,
                TrangThai = ma.TrangThai
            }).ToList();
            return result;
        }
    }
}