namespace DoAn.Models
{
    public class ChiTietDonThuoc
    {
        public int ID { get; set; }
        public string MaPhieu { get; set; }
        public string MaThuoc { get; set; }
        public int SoLuong { get; set; }
        public string CachDung { get; set; }

        // --- CÁC THUỘC TÍNH PHỤ (DTO) ĐỂ HIỂN THỊ TÊN THUỐC ---
        public string TenThuoc { get; set; }
        public string DonViTinh { get; set; }
        public decimal GiaThuoc { get; set; }
        public decimal ThanhTien { get; set; } // = SoLuong * GiaThuoc
    }
}