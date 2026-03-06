using System;

namespace DoAn.Models
{
    public class LichHen
    {
        public int MaLichHen { get; set; }
        public string MaBenhNhan { get; set; }
        public DateTime NgayHen { get; set; }
        public TimeSpan GioHen { get; set; }
        public string LyDoKham { get; set; }
        public string TrangThai { get; set; }

        // Thuộc tính phụ (DTO) để hiển thị lên UI khi JOIN với bảng BenhNhan
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
    }
}