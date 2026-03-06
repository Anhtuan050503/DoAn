using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using DoAn.Helpers; // Gọi thư viện chứa hàm băm mật khẩu
using System.Collections.Generic; // Dùng cho List<>
using DoAn.Models; // Gọi thư mục Models của bạn
namespace DoAn.Views
{
    public partial class QuanLyNhanSuControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        public QuanLyNhanSuControl()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // 1. Tạo một List rỗng chứa các đối tượng Nhân Viên
                List<NhanVien> danhSachNhanVien = new List<NhanVien>();

                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT MaNhanVien, HoTen, TenDangNhap, ChucVu, TrangThai FROM NhanVien";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // 2. Dùng DataReader thay vì DataAdapter
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // 3. Khởi tạo Object và Mapping (Gán giá trị)
                                NhanVien nv = new NhanVien();

                                nv.MaNhanVien = reader["MaNhanVien"].ToString();
                                // Thuộc tính HoTen được KẾ THỪA từ lớp cha ConNguoi
                                nv.HoTen = reader["HoTen"].ToString();
                                nv.TenDangNhap = reader["TenDangNhap"].ToString();
                                nv.ChucVu = reader["ChucVu"].ToString();
                                nv.TrangThai = Convert.ToBoolean(reader["TrangThai"]);

                                // 4. Thêm vào List
                                danhSachNhanVien.Add(nv);
                            }
                        }
                    }
                }
                dgNhanSu.ItemsSource = danhSachNhanVien;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtMaNV.Clear(); txtMaNV.IsReadOnly = false;
            txtHoTen.Clear();
            txtTenDangNhap.Clear(); txtTenDangNhap.IsReadOnly = false;
            txtMatKhau.Clear();
            cboChucVu.SelectedIndex = 0;
            cboTrangThai.SelectedIndex = 0;
            LoadData();
        }

        // TẠO TÀI KHOẢN MỚI
        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNV.Text) || string.IsNullOrWhiteSpace(txtTenDangNhap.Text) || string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Mã, Tên đăng nhập và Mật khẩu!"); return;
            }

            // Băm mật khẩu admin nhập vào
            string hashedPass = PasswordHelper.HashPassword(txtMatKhau.Text);
            bool trangThai = cboTrangThai.SelectedIndex == 0; // 0 là Đang hoạt động

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO NhanVien (MaNhanVien, HoTen, TenDangNhap, MatKhauHash, ChucVu, TrangThai) " +
                                   "VALUES (@ma, @ten, @user, @pass, @chucvu, @trangthai)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma", txtMaNV.Text);
                        cmd.Parameters.AddWithValue("@ten", txtHoTen.Text);
                        cmd.Parameters.AddWithValue("@user", txtTenDangNhap.Text);
                        cmd.Parameters.AddWithValue("@pass", hashedPass);
                        cmd.Parameters.AddWithValue("@chucvu", (cboChucVu.SelectedItem as ComboBoxItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@trangthai", trangThai);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Đã cấp tài khoản thành công!");
                        BtnLamMoi_Click(null, null);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: Mã NV hoặc Tên đăng nhập đã tồn tại.\n" + ex.Message); }
        }

        // Đổ dữ liệu khi click vào bảng
        private void DgNhanSu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ép kiểu dòng đang chọn về đúng class NhanVien
            if (dgNhanSu.SelectedItem is NhanVien nv)
            {
                // Code tự động gợi ý (IntelliSense) cực mượt, không sợ sai chính tả!
                txtMaNV.Text = nv.MaNhanVien;
                txtMaNV.IsReadOnly = true;

                txtHoTen.Text = nv.HoTen;
                txtTenDangNhap.Text = nv.TenDangNhap;
                txtTenDangNhap.IsReadOnly = true;

                cboChucVu.Text = nv.ChucVu;
                cboTrangThai.SelectedIndex = nv.TrangThai ? 0 : 1;

                txtMatKhau.Clear();
            }
        }

        // Cập nhật tài khoản (Gắn cả logic đổi mật khẩu)
        private void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNV.Text)) return;
            bool trangThai = cboTrangThai.SelectedIndex == 0;

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE NhanVien SET HoTen=@ten, ChucVu=@chucvu, TrangThai=@trangthai ";

                    // Nếu Admin có gõ mật khẩu mới vào ô textbox, thì cập nhật cả mật khẩu
                    if (!string.IsNullOrWhiteSpace(txtMatKhau.Text))
                    {
                        query += ", MatKhauHash=@pass ";
                    }
                    query += "WHERE MaNhanVien=@ma";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma", txtMaNV.Text);
                        cmd.Parameters.AddWithValue("@ten", txtHoTen.Text);
                        cmd.Parameters.AddWithValue("@chucvu", cboChucVu.Text);
                        cmd.Parameters.AddWithValue("@trangthai", trangThai);

                        if (!string.IsNullOrWhiteSpace(txtMatKhau.Text))
                        {
                            cmd.Parameters.AddWithValue("@pass", PasswordHelper.HashPassword(txtMatKhau.Text));
                        }

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Cập nhật thông tin thành công!");
                        LoadData();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // Khóa tài khoản thay vì xóa (để giữ lịch sử phiếu khám)
        private void BtnKhoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNV.Text)) return;

            // Logic: Đổi trạng thái = 0 (false)
            cboTrangThai.SelectedIndex = 1;
            BtnSua_Click(sender, e);
        }
    }
}