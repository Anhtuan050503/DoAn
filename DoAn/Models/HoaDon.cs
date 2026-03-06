using System;

namespace DoAn.Models
{
    public class HoaDon
    {
        public string MaHoaDon { get; set; }
        public string MaPhieu { get; set; }
        public decimal TienKham { get; set; }
        public decimal TienThuoc { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayThanhToan { get; set; } // Dấu ? vì có thể hóa đơn chưa thanh toán (NULL)

        // --- Thuộc tính DTO (Lấy từ bảng BenhNhan thông qua PhieuKham) ---
        public string TenBenhNhan { get; set; }
    }
}