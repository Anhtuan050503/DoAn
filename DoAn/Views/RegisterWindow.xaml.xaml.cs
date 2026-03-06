using DoAn.DataAccess;
using DoAn.Helpers;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DoAn.Views
{
    public partial class RegisterWindow : Window
    {
        private DatabaseConnection db = new DatabaseConnection();

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close(); // Quay lại LoginWindow (vốn đang mở ẩn bên dưới)
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các thông tin có dấu *", "Thông báo");
                return;
            }

            // 2. Kiểm tra khớp mật khẩu
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi");
                return;
            }

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    // 3. Kiểm tra trùng Username
                    string checkQuery = "SELECT COUNT(*) FROM BenhNhan WHERE TenDangNhap = @user";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@user", txtUsername.Text.Trim());
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Tên đăng nhập này đã được sử dụng!", "Lỗi");
                            return;
                        }
                    }

                    // 4. Tạo mã Bệnh nhân tự động (Ví dụ: BN + ticks)
                    string maBN = "BN" + DateTime.Now.Ticks.ToString().Substring(10);

                    // 5. Lưu vào Database
                    string query = @"INSERT INTO BenhNhan (MaBenhNhan, HoTen, NgaySinh, GioiTinh, SoDienThoai, DiaChi, TenDangNhap, MatKhauHash) 
                                   VALUES (@ma, @ten, @ngay, @gioi, @sdt, @dc, @user, @pass)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma", maBN);
                        cmd.Parameters.AddWithValue("@ten", txtHoTen.Text.Trim());
                        cmd.Parameters.AddWithValue("@ngay", dpNgaySinh.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@gioi", (cboGioiTinh.SelectedItem as ComboBoxItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@sdt", txtSDT.Text.Trim());
                        cmd.Parameters.AddWithValue("@dc", txtDiaChi.Text.Trim());
                        cmd.Parameters.AddWithValue("@user", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", PasswordHelper.HashPassword(txtPassword.Password));

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Chúc mừng! Bạn đã đăng ký tài khoản thành công.\nMã bệnh nhân của bạn là: " + maBN, "Thành công");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message);
            }
        }
    }
}