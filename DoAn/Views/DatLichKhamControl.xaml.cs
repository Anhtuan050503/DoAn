using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using DoAn.Helpers;

namespace DoAn.Views
{
    public partial class DatLichKhamControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        public DatLichKhamControl()
        {
            InitializeComponent();
            LoadLichHenCuaToi();
        }

        private void LoadLichHenCuaToi()
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Chỉ lấy lịch hẹn của chính bệnh nhân đang đăng nhập
                    string query = "SELECT MaLichHen, NgayHen, GioHen, LyDoKham, TrangThai FROM LichHen WHERE MaBenhNhan = @mabn ORDER BY NgayHen DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@mabn", Session.MaNhanVien);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgLichHen.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnDatLich_Click(object sender, RoutedEventArgs e)
        {
            if (dpNgayHen.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("Không thể đặt lịch cho ngày trong quá khứ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO LichHen (MaBenhNhan, NgayHen, GioHen, LyDoKham) VALUES (@mabn, @ngay, @gio, @lydo)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@mabn", Session.MaNhanVien);
                        cmd.Parameters.AddWithValue("@ngay", dpNgayHen.SelectedDate);
                        cmd.Parameters.AddWithValue("@gio", (cboGioHen.SelectedItem as ComboBoxItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@lydo", txtLyDo.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Đặt lịch thành công! Vui lòng chờ phòng khám xác nhận.", "Thành công");
                        txtLyDo.Clear();
                        LoadLichHenCuaToi(); // Tải lại bảng để thấy lịch vừa đặt
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi đặt lịch: " + ex.Message); }
        }
    }
}