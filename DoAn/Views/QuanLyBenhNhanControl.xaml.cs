using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using System.Collections.Generic;
using DoAn.Models;
namespace DoAn.Views
{
    public partial class QuanLyBenhNhanControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        public QuanLyBenhNhanControl()
        {
            InitializeComponent();
            LoadData(); // Load danh sách ngay khi mở form
        }

        // --- HÀM TẢI DỮ LIỆU ---
        private void LoadData(string tukhoa = "")
        {
            try
            {
                List<BenhNhan> dsBenhNhan = new List<BenhNhan>();

                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT MaBenhNhan, HoTen, NgaySinh, GioiTinh, DiaChi, SoDienThoai, NhomMau, TienSuBenh FROM BenhNhan WHERE HoTen LIKE @tukhoa OR MaBenhNhan LIKE @tukhoa";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tukhoa", "%" + tukhoa + "%");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BenhNhan bn = new BenhNhan();
                                bn.MaBenhNhan = reader["MaBenhNhan"].ToString();

                                // Lấy từ thuộc tính kế thừa
                                bn.HoTen = reader["HoTen"].ToString();
                                bn.NgaySinh = Convert.ToDateTime(reader["NgaySinh"]);
                                bn.GioiTinh = reader["GioiTinh"].ToString();
                                bn.DiaChi = reader["DiaChi"].ToString();
                                bn.SoDienThoai = reader["SoDienThoai"].ToString();

                                bn.NhomMau = reader["NhomMau"].ToString();
                                bn.TienSuBenh = reader["TienSuBenh"].ToString();

                                dsBenhNhan.Add(bn);
                            }
                        }
                    }
                }
                dgBenhNhan.ItemsSource = dsBenhNhan; // Đẩy List Object lên UI
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        // --- NÚT LÀM MỚI ---
        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtMaBN.Clear();
            txtHoTen.Clear();
            txtDiaChi.Clear();
            txtSDT.Clear();
            txtTienSu.Clear();
            dpNgaySinh.SelectedDate = DateTime.Now;
            cboGioiTinh.SelectedIndex = 0;
            cboNhomMau.SelectedIndex = 0;
            txtTimKiem.Clear();
            txtMaBN.IsReadOnly = false; // Cho phép nhập lại mã
            LoadData();
        }

        // --- NÚT THÊM ---
        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaBN.Text) || string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã BN và Họ tên!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO BenhNhan (MaBenhNhan, HoTen, NgaySinh, GioiTinh, DiaChi, SoDienThoai, NhomMau, TienSuBenh) " +
                                   "VALUES (@ma, @ten, @ngay, @gioi, @diachi, @sdt, @mau, @tiensu)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma", txtMaBN.Text);
                        cmd.Parameters.AddWithValue("@ten", txtHoTen.Text);
                        cmd.Parameters.AddWithValue("@ngay", dpNgaySinh.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@gioi", (cboGioiTinh.SelectedItem as ComboBoxItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@diachi", txtDiaChi.Text);
                        cmd.Parameters.AddWithValue("@sdt", txtSDT.Text);
                        cmd.Parameters.AddWithValue("@mau", (cboNhomMau.SelectedItem as ComboBoxItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@tiensu", txtTienSu.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Thêm bệnh nhân thành công!");
                        BtnLamMoi_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: Mã bệnh nhân có thể bị trùng.\nChi tiết: " + ex.Message);
            }
        }

        // --- KHI CLICK VÀO BẢNG (HIỂN THỊ LÊN FORM) ---
        private void DgBenhNhan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBenhNhan.SelectedItem is DataRowView row)
            {
                txtMaBN.Text = row["MaBenhNhan"].ToString();
                txtMaBN.IsReadOnly = true; // Không cho sửa khóa chính
                txtHoTen.Text = row["HoTen"].ToString();
                txtDiaChi.Text = row["DiaChi"].ToString();
                txtSDT.Text = row["SoDienThoai"].ToString();
                txtTienSu.Text = row["TienSuBenh"].ToString();
                
                if (DateTime.TryParse(row["NgaySinh"].ToString(), out DateTime date))
                    dpNgaySinh.SelectedDate = date;

                cboGioiTinh.Text = row["GioiTinh"].ToString();
                cboNhomMau.Text = row["NhomMau"].ToString();
            }
        }

        // --- NÚT SỬA ---
        private void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaBN.Text)) return;

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE BenhNhan SET HoTen=@ten, NgaySinh=@ngay, GioiTinh=@gioi, DiaChi=@diachi, " +
                                   "SoDienThoai=@sdt, NhomMau=@mau, TienSuBenh=@tiensu WHERE MaBenhNhan=@ma";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma", txtMaBN.Text);
                        cmd.Parameters.AddWithValue("@ten", txtHoTen.Text);
                        cmd.Parameters.AddWithValue("@ngay", dpNgaySinh.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@gioi", cboGioiTinh.Text);
                        cmd.Parameters.AddWithValue("@diachi", txtDiaChi.Text);
                        cmd.Parameters.AddWithValue("@sdt", txtSDT.Text);
                        cmd.Parameters.AddWithValue("@mau", cboNhomMau.Text);
                        cmd.Parameters.AddWithValue("@tiensu", txtTienSu.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Cập nhật thành công!");
                        LoadData();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // --- NÚT XÓA ---
        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaBN.Text)) return;

            if (MessageBox.Show($"Xóa bệnh nhân {txtHoTen.Text}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        string query = "DELETE FROM BenhNhan WHERE MaBenhNhan=@ma";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ma", txtMaBN.Text);
                            cmd.ExecuteNonQuery();
                            BtnLamMoi_Click(null, null);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        // --- NÚT TÌM KIẾM ---
        private void BtnTim_Click(object sender, RoutedEventArgs e)
        {
            LoadData(txtTimKiem.Text.Trim());
        }

        private void BtnDuaVaoHangDoi_Click(object sender, RoutedEventArgs e)
        {
            if (dgBenhNhan.SelectedItem is BenhNhan bn)
            {
                try
                {
                    using (SqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        // Sinh mã phiếu ngẫu nhiên
                        string maPhieuMoi = $"PK-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";

                        string query = "INSERT INTO PhieuKham (MaPhieu, MaBenhNhan, NgayKham, TrangThai) VALUES (@maphieu, @mabn, GETDATE(), N'Chờ khám')";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@maphieu", maPhieuMoi);
                            cmd.Parameters.AddWithValue("@mabn", bn.MaBenhNhan);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show($"Đã chuyển bệnh nhân {bn.HoTen} vào phòng khám thành công!", "Tiếp nhận", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi tạo hàng đợi: " + ex.Message); }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn 1 Bệnh nhân trong bảng để tiếp nhận!", "Thông báo");
            }
        }
    }
}