using DoAn.DataAccess;
using DoAn.DataAccess;
using DoAn.Helpers;
using DoAn.Helpers;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using DoAn.Helpers;
namespace DoAn.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void OpenRegister_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow reg = new RegisterWindow();
            reg.ShowDialog(); // Hiển thị cửa sổ đăng ký, tạm dừng cửa sổ đăng nhập
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedPass = PasswordHelper.HashPassword(pass);
            DatabaseConnection db = new DatabaseConnection();

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();

                    // LẦN TÌM 1: TÌM TRONG BẢNG NHÂN VIÊN (Bác sĩ, Lễ tân, Admin, Thu ngân)
                    string queryNV = "SELECT MaNhanVien, ChucVu, HoTen, TrangThai FROM NhanVien WHERE TenDangNhap = @username AND MatKhauHash = @password";
                    using (SqlCommand cmdNV = new SqlCommand(queryNV, conn))
                    {
                        cmdNV.Parameters.AddWithValue("@username", user);
                        cmdNV.Parameters.AddWithValue("@password", hashedPass);

                        using (SqlDataReader readerNV = cmdNV.ExecuteReader())
                        {
                            if (readerNV.Read())
                            {
                                bool trangThai = Convert.ToBoolean(readerNV["TrangThai"]);
                                if (!trangThai)
                                {
                                    MessageBox.Show("Tài khoản này đã bị khóa!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                // Lưu Session cho Nhân viên
                                Session.MaNhanVien = readerNV["MaNhanVien"].ToString();
                                Session.HoTen = readerNV["HoTen"].ToString();
                                Session.ChucVu = readerNV["ChucVu"].ToString();

                                MainWindow mainWin = new MainWindow();
                                mainWin.Show();
                                this.Close();
                                return; // Kết thúc hàm tại đây vì đã đăng nhập thành công
                            }
                        } // Tự động đóng readerNV để chuẩn bị cho lần tìm thứ 2
                    }

                    // LẦN TÌM 2: NẾU KHÔNG PHẢI NHÂN VIÊN, TÌM TRONG BẢNG BỆNH NHÂN
                    string queryBN = "SELECT MaBenhNhan, HoTen FROM BenhNhan WHERE TenDangNhap = @username AND MatKhauHash = @password";
                    using (SqlCommand cmdBN = new SqlCommand(queryBN, conn))
                    {
                        cmdBN.Parameters.AddWithValue("@username", user);
                        cmdBN.Parameters.AddWithValue("@password", hashedPass);

                        using (SqlDataReader readerBN = cmdBN.ExecuteReader())
                        {
                            if (readerBN.Read())
                            {
                                // Lưu Session cho Bệnh nhân (Gán cứng Chức vụ là "Bệnh nhân")
                                Session.MaNhanVien = readerBN["MaBenhNhan"].ToString();
                                Session.HoTen = readerBN["HoTen"].ToString();
                                Session.ChucVu = "Bệnh nhân";

                                MainWindow mainWin = new MainWindow();
                                mainWin.Show();
                                this.Close();
                                return; // Đăng nhập thành công
                            }
                        }
                    }

                    // NẾU CẢ 2 LẦN TÌM ĐỀU THẤT BẠI
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}