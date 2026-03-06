namespace DoAn.Models
{
    public class ChiTietDichVu
    {
        public int ID { get; set; }
        public string MaPhieu { get; set; }
        public string MaDichVu { get; set; }
        public int SoLuong { get; set; }

        // --- DTO để hiển thị lên bảng ---
        public string TenDichVu { get; set; }
        public decimal GiaDichVu { get; set; }
        public decimal ThanhTien { get; set; } // = SoLuong * GiaDichVu
    }
}