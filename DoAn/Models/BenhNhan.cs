using System;

namespace DoAn.Models
{
    public class BenhNhan : ConNguoi
    {
        public string MaBenhNhan { get; set; }
        // Các thuộc tính HoTen, NgaySinh, GioiTinh, DiaChi, SoDienThoai đã có sẵn từ lớp cha ConNguoi!

        public string NhomMau { get; set; }
        public string TienSuBenh { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhauHash { get; set; }
    }
}