using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using DoAn.Models;

namespace DoAn.Views
{
    public partial class VienPhiControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();
        private string maPhieuDangChon = "";

        // Đã bổ sung biến này để hệ thống biết đang thu tiền cho hóa đơn nào
        private string maHoaDonDangChon = "";

        public VienPhiControl()
        {
            InitializeComponent();
            LoadDanhSachChoThanhToan();
        }

        // --- 1. TẢI DANH SÁCH BỆNH NHÂN ĐÃ KHÁM XONG (CHỜ ĐÓNG TIỀN) ---
        private void LoadDanhSachChoThanhToan(string tukhoa = "")
        {
            try
            {
                List<PhieuKham> dsCho = new List<PhieuKham>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT p.MaPhieu, p.NgayKham, b.HoTen
                        FROM PhieuKham p
                        INNER JOIN BenhNhan b ON p.MaBenhNhan = b.MaBenhNhan
                        WHERE p.TrangThai = N'Đã khám' AND (b.HoTen LIKE @tukhoa OR p.MaPhieu LIKE @tukhoa)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tukhoa", "%" + tukhoa + "%");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsCho.Add(new PhieuKham
                                {
                                    MaPhieu = reader["MaPhieu"].ToString(),
                                    NgayKham = Convert.ToDateTime(reader["NgayKham"]),
                                    TenBenhNhan = reader["HoTen"].ToString() // Gán đúng tên thuộc tính TenBenhNhan
                                });
                            }
                        }
                    }
                }
                dgChoThanhToan.ItemsSource = dsCho;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách chờ: " + ex.Message); }
        }

        private void BtnTim_Click(object sender, RoutedEventArgs e)
        {
            LoadDanhSachChoThanhToan(txtTimKiem.Text.Trim());
        }

        // --- 2. KHI CLICK CHỌN BỆNH NHÂN TRONG BẢNG ---
        private void DgChoThanhToan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgChoThanhToan.SelectedItem is PhieuKham pk)
            {
                maPhieuDangChon = pk.MaPhieu;
                txtTenBN.Text = pk.TenBenhNhan; // Đã sửa HoTen thành TenBenhNhan

                LoadThongTinHoaDon(maPhieuDangChon);
                LoadChiTietDichVu(maPhieuDangChon);
                LoadChiTietThuoc(maPhieuDangChon);

                btnThanhToan.IsEnabled = true;
                btnInHoaDon.IsEnabled = true;
            }
        }

        // --- 3. LẤY TỔNG TIỀN & MÃ HÓA ĐƠN CHỜ ---
        private void LoadThongTinHoaDon(string maPhieu)
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT h.MaHoaDon, h.TienKham, h.TienThuoc, h.TienDichVu, h.TongTien, p.ChanDoan 
                        FROM HoaDon h 
                        INNER JOIN PhieuKham p ON h.MaPhieu = p.MaPhieu
                        WHERE h.MaPhieu = @maphieu AND h.TrangThai = N'Chưa thanh toán'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maphieu", maPhieu);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                maHoaDonDangChon = reader["MaHoaDon"].ToString();
                                txtMaHoaDon.Text = "Mã HĐ: " + maHoaDonDangChon;
                                txtChanDoan.Text = reader["ChanDoan"].ToString();

                                decimal tienKham = Convert.ToDecimal(reader["TienKham"]);
                                decimal tienDichVu = Convert.ToDecimal(reader["TienDichVu"]);
                                decimal tienThuoc = Convert.ToDecimal(reader["TienThuoc"]);
                                decimal tongTien = Convert.ToDecimal(reader["TongTien"]);

                                txtTienKham.Text = tienKham.ToString("N0") + " VNĐ";
                                txtTienDichVu.Text = tienDichVu.ToString("N0") + " VNĐ";
                                txtTienThuoc.Text = tienThuoc.ToString("N0") + " VNĐ";
                                txtTongTien.Text = tongTien.ToString("N0") + " VNĐ";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi lấy thông tin hóa đơn: " + ex.Message); }
        }

        // --- 4. LẤY CHI TIẾT BẢNG DỊCH VỤ ---
        private void LoadChiTietDichVu(string maPhieu)
        {
            List<ChiTietDichVu> dsDichVu = new List<ChiTietDichVu>();
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT d.TenDichVu, d.GiaDichVu, (c.SoLuong * d.GiaDichVu) AS ThanhTien
                    FROM ChiTietDichVu c
                    INNER JOIN DichVu d ON c.MaDichVu = d.MaDichVu
                    WHERE c.MaPhieu = @maphieu";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@maphieu", maPhieu);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsDichVu.Add(new ChiTietDichVu
                            {
                                TenDichVu = reader["TenDichVu"].ToString(),
                                GiaDichVu = Convert.ToDecimal(reader["GiaDichVu"]),
                                ThanhTien = Convert.ToDecimal(reader["ThanhTien"])
                            });
                        }
                    }
                }
            }
            dgChiTietDichVu.ItemsSource = dsDichVu;
        }

        // --- 5. LẤY CHI TIẾT BẢNG ĐƠN THUỐC ---
        private void LoadChiTietThuoc(string maPhieu)
        {
            List<ChiTietDonThuoc> dsThuoc = new List<ChiTietDonThuoc>();
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT t.TenThuoc, c.SoLuong, t.GiaThuoc, (c.SoLuong * t.GiaThuoc) AS ThanhTien
                    FROM ChiTietDonThuoc c
                    INNER JOIN Thuoc t ON c.MaThuoc = t.MaThuoc
                    WHERE c.MaPhieu = @maphieu";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@maphieu", maPhieu);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsThuoc.Add(new ChiTietDonThuoc
                            {
                                TenThuoc = reader["TenThuoc"].ToString(),
                                SoLuong = Convert.ToInt32(reader["SoLuong"]),
                                GiaThuoc = Convert.ToDecimal(reader["GiaThuoc"]),
                                ThanhTien = Convert.ToDecimal(reader["ThanhTien"])
                            });
                        }
                    }
                }
            }
            dgChiTietThuoc.ItemsSource = dsThuoc;
        }

        // --- 6. XÁC NHẬN THU TIỀN VÀ XÓA TRẮNG MÀN HÌNH ---
        private void BtnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(maHoaDonDangChon)) return;

            if (MessageBox.Show("Xác nhận thu tiền hóa đơn này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        string query = "UPDATE HoaDon SET TrangThai = N'Đã thanh toán', NgayThanhToan = GETDATE() WHERE MaHoaDon = @mahd";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@mahd", maHoaDonDangChon);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Xóa trắng giao diện sau khi thu tiền xong
                    txtTenBN.Text = "---";
                    txtChanDoan.Text = "---";
                    txtMaHoaDon.Text = "Mã HĐ: HD-............";
                    txtTienKham.Text = "0 VNĐ";
                    txtTienDichVu.Text = "0 VNĐ";
                    txtTienThuoc.Text = "0 VNĐ";
                    txtTongTien.Text = "0 VNĐ";

                    dgChiTietDichVu.ItemsSource = null;
                    dgChiTietThuoc.ItemsSource = null;

                    btnThanhToan.IsEnabled = false;
                    LoadDanhSachChoThanhToan(); // Tải lại danh sách bệnh nhân
                }
                catch (Exception ex) { MessageBox.Show("Lỗi thanh toán: " + ex.Message); }
            }
        }

        // --- 7. LỆNH IN HÓA ĐƠN RA MÁY IN (HOẶC PDF) ---
        private void BtnInHoaDon_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                panelNutBam.Visibility = Visibility.Hidden;
                Thickness oldMargin = GridHoaDonToPrint.Margin;
                GridHoaDonToPrint.Margin = new Thickness(20);

                try
                {
                    printDialog.PrintVisual(GridHoaDonToPrint, "In Hoa Don Vien Phi");
                    MessageBox.Show("Đã gửi lệnh in thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi máy in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    panelNutBam.Visibility = Visibility.Visible;
                    GridHoaDonToPrint.Margin = oldMargin;
                }
            }
        }
    }
}