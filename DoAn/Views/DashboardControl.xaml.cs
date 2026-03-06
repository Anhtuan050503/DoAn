using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using DoAn.DataAccess;

namespace DoAn.Views
{
    public partial class DashboardControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        // 1. Khai báo các biến dữ liệu cho Biểu đồ (Phải có thuộc tính get/set)
        public SeriesCollection DoanhThuSeries { get; set; }
        public string[] ThangLabels { get; set; }
        public Func<double, string> FormatterTien { get; set; }
        public SeriesCollection LichHenSeries { get; set; }

        public DashboardControl()
        {
            InitializeComponent();

            // Lệnh cực kỳ quan trọng để Giao diện nhận dữ liệu từ các biến ở trên
            DataContext = this;

            // Cấu hình định dạng tiền tệ cho trục Y (VD: 1,500,000)
            FormatterTien = value => value.ToString("N0");

            LoadThongKeNhanh();
            LoadBieuDoDoanhThu();
            LoadBieuDoLichHen();
        }

        // --- TẢI 3 CON SỐ TỔNG QUAN ---
        private void LoadThongKeNhanh()
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Tổng doanh thu
                    using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(SUM(TongTien), 0) FROM HoaDon WHERE TrangThai = N'Đã thanh toán'", conn))
                        txtDoanhThu.Text = Convert.ToDecimal(cmd.ExecuteScalar()).ToString("N0") + " VNĐ";

                    // Tổng bệnh nhân
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM BenhNhan", conn))
                        txtBenhNhan.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString() + " Người";

                    // Tổng nhân sự hoạt động
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM NhanVien WHERE TrangThai = 1", conn))
                        txtNhanSu.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString() + " Người";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thống kê: " + ex.Message); }
        }

        // --- VẼ BIỂU ĐỒ CỘT (DOANH THU 12 THÁNG) ---
        private void LoadBieuDoDoanhThu()
        {
            DoanhThuSeries = new SeriesCollection();
            ChartValues<decimal> values = new ChartValues<decimal>();
            ThangLabels = new string[12];

            // Mặc định cho 12 tháng bằng 0 trước
            decimal[] doanhThuThang = new decimal[13];

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Câu lệnh SQL nhóm tổng tiền theo Từng Tháng của năm hiện tại
                    string query = @"
                        SELECT MONTH(NgayThanhToan) AS Thang, SUM(TongTien) AS TongDoanhThu
                        FROM HoaDon 
                        WHERE YEAR(NgayThanhToan) = YEAR(GETDATE()) AND TrangThai = N'Đã thanh toán'
                        GROUP BY MONTH(NgayThanhToan)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int thang = Convert.ToInt32(reader["Thang"]);
                                decimal tien = Convert.ToDecimal(reader["TongDoanhThu"]);
                                doanhThuThang[thang] = tien; // Gán tiền vào đúng tháng
                            }
                        }
                    }
                }

                // Đổ dữ liệu mảng vào ChartValues để vẽ
                for (int i = 1; i <= 12; i++)
                {
                    values.Add(doanhThuThang[i]);
                    ThangLabels[i - 1] = "T" + i; // Tạo nhãn T1, T2... T12
                }

                // Khởi tạo cột biểu đồ
                DoanhThuSeries.Add(new ColumnSeries
                {
                    Title = "Doanh thu",
                    Values = values,
                    Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#2980B9") // Màu xanh dương
                });
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải biểu đồ doanh thu: " + ex.Message); }
        }

        // --- VẼ BIỂU ĐỒ TRÒN (TỈ LỆ TRẠNG THÁI LỊCH HẸN) ---
        private void LoadBieuDoLichHen()
        {
            LichHenSeries = new SeriesCollection();

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Câu lệnh đếm số lượng lịch hẹn theo từng trạng thái
                    string query = "SELECT TrangThai, COUNT(*) AS SoLuong FROM LichHen GROUP BY TrangThai";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tenTrangThai = reader["TrangThai"].ToString();
                                int soLuong = Convert.ToInt32(reader["SoLuong"]);

                                // Thêm từng "miếng bánh" vào biểu đồ tròn
                                LichHenSeries.Add(new PieSeries
                                {
                                    Title = tenTrangThai,
                                    Values = new ChartValues<int> { soLuong },
                                    DataLabels = true // Hiển thị con số lên miếng bánh
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải biểu đồ tròn: " + ex.Message); }
        }
    }
}