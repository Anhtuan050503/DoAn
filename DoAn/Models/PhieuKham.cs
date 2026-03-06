using System;

namespace DoAn.Models
{
    public class PhieuKham
    {
        public string MaPhieu { get; set; }
        public string MaBenhNhan { get; set; }
        public string MaBacSi { get; set; }
        public DateTime NgayKham { get; set; }
        public string TrieuChung { get; set; }
        public string ChanDoan { get; set; }
        public string TrangThai { get; set; } // THÊM DÒNG NÀY

        // --- CÁC THUỘC TÍNH PHỤ (DTO) ĐỂ HIỂN THỊ LÊN GIAO DIỆN ---
        public string TenBacSi { get; set; }
        public string TenBenhNhan { get; set; }
        public string GioiTinh { get; set; } // THÊM DÒNG NÀY ĐỂ BÁC SĨ XEM
    }
}