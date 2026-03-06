using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;

namespace DoAn.Views
{
    public partial class DuyetLichHenControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        public DuyetLichHenControl()
        {
            InitializeComponent();
            LoadDanhSachLichHen();
        }

        private void LoadDanhSachLichHen()
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Kết nối 2 bảng để lấy tên và SĐT bệnh nhân
                    string query = @"
                        SELECT l.MaLichHen, b.HoTen, b.SoDienThoai, l.NgayHen, l.GioHen, l.LyDoKham, l.TrangThai 
                        FROM LichHen l
                        INNER JOIN BenhNhan b ON l.MaBenhNhan = b.MaBenhNhan
                        ORDER BY l.NgayHen DESC, l.GioHen DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgDuyetLich.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải lịch hẹn: " + ex.Message); }
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadDanhSachLichHen();
        }

        // --- HÀM CẬP NHẬT TRẠNG THÁI CHUNG ---
        private void CapNhatTrangThaiLich(string trangThaiMoi)
        {
            if (dgDuyetLich.SelectedItem is DataRowView row)
            {
                string maLich = row["MaLichHen"].ToString();
                string trangThaiHienTai = row["TrangThai"].ToString();

                if (trangThaiHienTai == "Đã hủy")
                {
                    MessageBox.Show("Lịch này đã bị hủy, không thể thay đổi trạng thái!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (SqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        string query = "UPDATE LichHen SET TrangThai = @trangthai WHERE MaLichHen = @ma";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@trangthai", trangThaiMoi);
                            cmd.Parameters.AddWithValue("@ma", maLich);
                            cmd.ExecuteNonQuery();

                            MessageBox.Show($"Đã chuyển lịch hẹn thành: {trangThaiMoi}", "Thành công");
                            LoadDanhSachLichHen(); // Tải lại bảng
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi cập nhật: " + ex.Message); }
            }
            else
            {
                MessageBox.Show("Vui lòng click chọn một lịch hẹn trong bảng trước!", "Thông báo");
            }
        }

        // Bấm nút Xác nhận
        private void BtnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            CapNhatTrangThaiLich("Đã xác nhận");
        }

        // Bấm nút Hủy
        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn HỦY lịch hẹn này không?", "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CapNhatTrangThaiLich("Đã hủy");
            }
        }
    }
}