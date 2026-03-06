using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DoAn.DataAccess;
using DoAn.Models; // Gọi Models
using DoAn.Helpers; // Gọi Session

namespace DoAn.Views
{
    public partial class KhamBenhControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();
        private string maBNDangChon = "";

    
        private ObservableCollection<ChiTietDonThuoc> danhSachThuocKeDon = new ObservableCollection<ChiTietDonThuoc>();
        private ObservableCollection<ChiTietDichVu> danhSachDichVuChiDinh = new ObservableCollection<ChiTietDichVu>();
        public KhamBenhControl()
        {
            InitializeComponent();
            LoadDanhSachBenhNhan();
            LoadKhoThuocVaoComboBox();
            LoadDichVuVaoComboBox();
            dgDonThuoc.ItemsSource = danhSachThuocKeDon;
            dgDichVuChiDinh.ItemsSource = danhSachDichVuChiDinh;
        }

        // TẢI DANH SÁCH BỆNH NHÂN (Chuẩn OOP)
        private void LoadDanhSachBenhNhan(string tuKhoa = "")
        {
            try
            {
                List<PhieuKham> dsHangDoi = new List<PhieuKham>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Lấy từ bảng PhieuKham nối với BenhNhan
                    string query = @"
                SELECT p.MaPhieu, p.MaBenhNhan, b.HoTen, b.GioiTinh 
                FROM PhieuKham p
                INNER JOIN BenhNhan b ON p.MaBenhNhan = b.MaBenhNhan
                WHERE p.TrangThai = N'Chờ khám' 
                  AND CAST(p.NgayKham AS DATE) = CAST(GETDATE() AS DATE)
                  AND b.HoTen LIKE @tukhoa";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tukhoa", "%" + tuKhoa + "%");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PhieuKham pk = new PhieuKham();
                                pk.MaPhieu = reader["MaPhieu"].ToString();
                                pk.MaBenhNhan = reader["MaBenhNhan"].ToString();
                                pk.TenBenhNhan = reader["HoTen"].ToString(); // Lưu ý biến phụ
                                pk.GioiTinh = reader["GioiTinh"].ToString();
                                dsHangDoi.Add(pk);
                            }
                        }
                    }
                }
                dgDanhSachBN.ItemsSource = dsHangDoi; // Đổ hàng đợi lên DataGrid
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải hàng đợi: " + ex.Message); }
        }

        // TẢI KHO THUỐC VÀO COMBOBOX (Chuẩn OOP)
        private void LoadKhoThuocVaoComboBox()
        {
            try
            {
                List<Thuoc> dsThuoc = new List<Thuoc>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT MaThuoc, TenThuoc FROM Thuoc WHERE SoLuongTon > 0";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Thuoc t = new Thuoc();
                                t.MaThuoc = reader["MaThuoc"].ToString();
                                t.TenThuoc = reader["TenThuoc"].ToString();
                                dsThuoc.Add(t);
                            }
                        }
                    }
                }
                cboThuoc.ItemsSource = dsThuoc;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải kho thuốc: " + ex.Message); }
        }

        private void TaoMaPhieuTuDong()
        {
            txtMaPhieu.Text = $"PK-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        private void BtnTimBN_Click(object sender, RoutedEventArgs e)
        {
            LoadDanhSachBenhNhan(txtTimKiemBN.Text.Trim());
        }

        // CHỌN BỆNH NHÂN TỪ BẢNG (Chuẩn OOP)
        private void DgDanhSachBN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgDanhSachBN.SelectedItem is PhieuKham pk)
            {
                maBNDangChon = pk.MaBenhNhan;
                txtMaPhieu.Text = pk.MaPhieu;
                txtTenBNDangKham.Text = $"{pk.TenBenhNhan} ({pk.GioiTinh})";
                txtTenBNDangKham.Foreground = new SolidColorBrush(Colors.Green);

                // [MỚI] Bật nút Xem bệnh án khi đã chọn bệnh nhân
                btnXemBenhAn.IsEnabled = true;
            }
        }
        private void LoadDichVuVaoComboBox()
        {
            try
            {
                List<DichVu> dsDichVu = new List<DichVu>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT MaDichVu, TenDichVu, GiaDichVu FROM DichVu";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsDichVu.Add(new DichVu
                                {
                                    MaDichVu = reader["MaDichVu"].ToString(),
                                    TenDichVu = reader["TenDichVu"].ToString(),
                                    GiaDichVu = Convert.ToDecimal(reader["GiaDichVu"])
                                });
                            }
                        }
                    }
                }
                cboDichVu.ItemsSource = dsDichVu;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dịch vụ: " + ex.Message); }
        }
        private void BtnThemDichVu_Click(object sender, RoutedEventArgs e)
        {
            if (cboDichVu.SelectedItem is DichVu dvChon)
            {
                // Kiểm tra xem dịch vụ này đã thêm chưa (tránh thêm trùng 2 lần siêu âm)
                foreach (var item in danhSachDichVuChiDinh)
                {
                    if (item.MaDichVu == dvChon.MaDichVu)
                    {
                        MessageBox.Show("Dịch vụ này đã được chỉ định rồi!", "Thông báo");
                        return;
                    }
                }

                danhSachDichVuChiDinh.Add(new ChiTietDichVu
                {
                    MaDichVu = dvChon.MaDichVu,
                    TenDichVu = dvChon.TenDichVu,
                    SoLuong = 1,
                    GiaDichVu = dvChon.GiaDichVu,
                    ThanhTien = dvChon.GiaDichVu // 1 * Giá
                });
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại dịch vụ!");
            }
        }
        // THÊM THUỐC VÀO ĐƠN (Chuẩn OOP)
        private void BtnThemThuoc_Click(object sender, RoutedEventArgs e)
        {
            if (cboThuoc.SelectedItem is Thuoc thuocChon)
            {
                if (!int.TryParse(txtSoLuongThuoc.Text, out int sl) || sl <= 0)
                {
                    MessageBox.Show("Số lượng phải là số lớn hơn 0!"); return;
                }

                // Thêm một Object ChiTietDonThuoc vào danh sách
                danhSachThuocKeDon.Add(new ChiTietDonThuoc
                {
                    MaThuoc = thuocChon.MaThuoc,
                    TenThuoc = thuocChon.TenThuoc, // DTO để hiển thị
                    SoLuong = sl,
                    CachDung = txtCachDung.Text
                });

                txtSoLuongThuoc.Text = "1";
                txtCachDung.Clear();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại thuốc!");
            }
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtMaPhieu.Clear();
            txtTrieuChung.Clear();
            txtChanDoan.Clear();
            maBNDangChon = "";
            txtTenBNDangKham.Text = "--- Chưa chọn ---";
            txtTenBNDangKham.Foreground = new SolidColorBrush(Color.FromRgb(211, 84, 0));
            btnXemBenhAn.IsEnabled = false; // Tắt nút xem bệnh án

            dgDanhSachBN.SelectedItem = null;
            danhSachThuocKeDon.Clear();
            danhSachDichVuChiDinh.Clear(); // [MỚI] Xóa danh sách dịch vụ đang chọn

            LoadKhoThuocVaoComboBox();  
        }
        private void BtnXemBenhAn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(maBNDangChon))
            {
                // Khởi tạo cửa sổ Lịch Sử và truyền Mã, Tên BN sang
                LichSuBenhAnWindow windowLichSu = new LichSuBenhAnWindow(maBNDangChon, txtTenBNDangKham.Text);

                // Mở dưới dạng ShowDialog() - bắt buộc bác sĩ phải tắt popup này mới được bấm tiếp vào màn hình sau
                windowLichSu.ShowDialog();
            }
        }
        // LƯU DB BẰNG TRANSACTION (Giữ nguyên logic SQL an toàn)
        private void BtnLuuPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(maBNDangChon) || string.IsNullOrWhiteSpace(txtChanDoan.Text))
            {
                MessageBox.Show("Vui lòng chọn Bệnh nhân và nhập Chẩn đoán!"); return;
            }

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. UPDATE lại Phiếu khám
                        string queryPhieu = @"UPDATE PhieuKham SET MaBacSi = @mabs, TrieuChung = @trieuchung, ChanDoan = @chandoan, TrangThai = N'Đã khám' WHERE MaPhieu = @maphieu";
                        using (SqlCommand cmd = new SqlCommand(queryPhieu, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@mabs", Session.MaNhanVien);
                            cmd.Parameters.AddWithValue("@trieuchung", txtTrieuChung.Text);
                            cmd.Parameters.AddWithValue("@chandoan", txtChanDoan.Text);
                            cmd.Parameters.AddWithValue("@maphieu", txtMaPhieu.Text);
                            cmd.ExecuteNonQuery();
                        }

                        decimal tongTienThuoc = 0;
                        // 2. LƯU THUỐC & TRỪ TỒN KHO 
                        foreach (ChiTietDonThuoc thuoc in danhSachThuocKeDon)
                        {
                            string queryThuoc = "INSERT INTO ChiTietDonThuoc (MaPhieu, MaThuoc, SoLuong, CachDung) VALUES (@maphieu, @mathuoc, @soluong, @cachdung)";
                            using (SqlCommand cmdThuoc = new SqlCommand(queryThuoc, conn, trans))
                            {
                                cmdThuoc.Parameters.AddWithValue("@maphieu", txtMaPhieu.Text);
                                cmdThuoc.Parameters.AddWithValue("@mathuoc", thuoc.MaThuoc);
                                cmdThuoc.Parameters.AddWithValue("@soluong", thuoc.SoLuong);
                                cmdThuoc.Parameters.AddWithValue("@cachdung", thuoc.CachDung);
                                cmdThuoc.ExecuteNonQuery();
                            }

                            // Cập nhật tồn kho và lấy giá thuốc để cộng vào tổng tiền thuốc
                            string queryTruKho = "UPDATE Thuoc SET SoLuongTon = SoLuongTon - @soluong WHERE MaThuoc = @mathuoc; SELECT GiaThuoc FROM Thuoc WHERE MaThuoc = @mathuoc;";
                            using (SqlCommand cmdTruKho = new SqlCommand(queryTruKho, conn, trans))
                            {
                                cmdTruKho.Parameters.AddWithValue("@soluong", thuoc.SoLuong);
                                cmdTruKho.Parameters.AddWithValue("@mathuoc", thuoc.MaThuoc);
                                decimal gia = Convert.ToDecimal(cmdTruKho.ExecuteScalar());
                                tongTienThuoc += (gia * thuoc.SoLuong);
                            }
                        }

                        decimal tongTienDichVu = 0;
                        // 3. [MỚI] LƯU DỊCH VỤ CẬN LÂM SÀNG
                        foreach (ChiTietDichVu dv in danhSachDichVuChiDinh)
                        {
                            string queryDichVu = "INSERT INTO ChiTietDichVu (MaPhieu, MaDichVu, SoLuong) VALUES (@maphieu, @madv, @soluong)";
                            using (SqlCommand cmdDV = new SqlCommand(queryDichVu, conn, trans))
                            {
                                cmdDV.Parameters.AddWithValue("@maphieu", txtMaPhieu.Text);
                                cmdDV.Parameters.AddWithValue("@madv", dv.MaDichVu);
                                cmdDV.Parameters.AddWithValue("@soluong", dv.SoLuong);
                                cmdDV.ExecuteNonQuery();
                            }
                            tongTienDichVu += dv.ThanhTien;
                        }

                        // 4. [MỚI] TẠO HÓA ĐƠN CHỜ THANH TOÁN CHO THU NGÂN
                        string maHoaDon = $"HD-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
                        decimal tienKham = 100000; // Tiền công khám mặc định
                        decimal tongTienHoaDon = tienKham + tongTienThuoc + tongTienDichVu;

                        string queryHoaDon = @"INSERT INTO HoaDon (MaHoaDon, MaPhieu, TienKham, TienThuoc, TienDichVu, TongTien, TrangThai) 
                                               VALUES (@mahd, @maphieu, @tienkham, @tienthuoc, @tiendv, @tongtien, N'Chưa thanh toán')";
                        using (SqlCommand cmdHD = new SqlCommand(queryHoaDon, conn, trans))
                        {
                            cmdHD.Parameters.AddWithValue("@mahd", maHoaDon);
                            cmdHD.Parameters.AddWithValue("@maphieu", txtMaPhieu.Text);
                            cmdHD.Parameters.AddWithValue("@tienkham", tienKham);
                            cmdHD.Parameters.AddWithValue("@tienthuoc", tongTienThuoc);
                            cmdHD.Parameters.AddWithValue("@tiendv", tongTienDichVu);
                            cmdHD.Parameters.AddWithValue("@tongtien", tongTienHoaDon);
                            cmdHD.ExecuteNonQuery();
                        }

                        trans.Commit();
                        MessageBox.Show("Khám bệnh hoàn tất! Dữ liệu đã chuyển sang Thu ngân.", "Thành công");
                        BtnLamMoi_Click(null, null);
                        LoadDanhSachBenhNhan();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
                    }
                }
            }
        }
    }
}