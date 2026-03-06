using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using DoAn.Models; // Gọi Model

namespace DoAn.Views
{
    public partial class QuanLyHoaDonControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        public QuanLyHoaDonControl()
        {
            InitializeComponent();
            // Mặc định load hóa đơn trong 30 ngày gần nhất
            dpTuNgay.SelectedDate = DateTime.Now.AddDays(-30);
            LoadDanhSachHoaDon();
        }

        private void LoadDanhSachHoaDon()
        {
            try
            {
                List<HoaDon> dsHoaDon = new List<HoaDon>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    // Câu query nối 3 bảng, có tìm kiếm theo ngày và từ khóa
                    string query = @"
                        SELECT h.MaHoaDon, h.MaPhieu, h.TienKham, h.TienThuoc, h.TongTien, h.NgayThanhToan, h.TrangThai, b.HoTen AS TenBenhNhan
                        FROM HoaDon h
                        INNER JOIN PhieuKham p ON h.MaPhieu = p.MaPhieu
                        INNER JOIN BenhNhan b ON p.MaBenhNhan = b.MaBenhNhan
                        WHERE (h.NgayThanhToan >= @tungay OR h.NgayThanhToan IS NULL) 
                          AND (h.NgayThanhToan <= @denngay OR h.NgayThanhToan IS NULL)
                          AND (h.MaHoaDon LIKE @tukhoa OR b.HoTen LIKE @tukhoa)
                        ORDER BY h.NgayThanhToan DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Set giờ bắt đầu của ngày (00:00:00) và giờ kết thúc (23:59:59) để lọc chính xác
                        cmd.Parameters.AddWithValue("@tungay", dpTuNgay.SelectedDate.Value.Date);
                        cmd.Parameters.AddWithValue("@denngay", dpDenNgay.SelectedDate.Value.Date.AddDays(1).AddTicks(-1));
                        cmd.Parameters.AddWithValue("@tukhoa", "%" + txtTimKiem.Text.Trim() + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                HoaDon hd = new HoaDon();
                                hd.MaHoaDon = reader["MaHoaDon"].ToString();
                                hd.MaPhieu = reader["MaPhieu"].ToString();
                                hd.TienKham = Convert.ToDecimal(reader["TienKham"]);
                                hd.TienThuoc = Convert.ToDecimal(reader["TienThuoc"]);
                                hd.TongTien = Convert.ToDecimal(reader["TongTien"]);
                                hd.TrangThai = reader["TrangThai"].ToString();

                                // Xử lý nếu ngày thanh toán bị Null (Hóa đơn chưa thu tiền)
                                if (reader["NgayThanhToan"] != DBNull.Value)
                                    hd.NgayThanhToan = Convert.ToDateTime(reader["NgayThanhToan"]);

                                hd.TenBenhNhan = reader["TenBenhNhan"].ToString();

                                dsHoaDon.Add(hd);
                            }
                        }
                    }
                }
                dgHoaDon.ItemsSource = dsHoaDon; // Hiển thị lên WPF
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách hóa đơn: " + ex.Message);
            }
        }

        private void BtnLoc_Click(object sender, RoutedEventArgs e)
        {
            if (dpTuNgay.SelectedDate > dpDenNgay.SelectedDate)
            {
                MessageBox.Show("Lỗi: 'Từ ngày' không được lớn hơn 'Đến ngày'!", "Cảnh báo");
                return;
            }
            LoadDanhSachHoaDon();
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = DateTime.Now.AddDays(-30);
            dpDenNgay.SelectedDate = DateTime.Now;
            txtTimKiem.Clear();
            LoadDanhSachHoaDon();
        }
    }
}