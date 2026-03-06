using System;

namespace DoAn.Models
{
    // Kế thừa từ ConNguoi
    public class NhanVien : ConNguoi
    {
        public string MaNhanVien { get; set; }
        public string ChucVu { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhauHash { get; set; }

        // Dùng kiểu bool để map tự động với DataGridCheckBoxColumn trên giao diện
        public bool TrangThai { get; set; }
    }
}