using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using DoAn.Models;

namespace DoAn.Views
{
    public partial class LichSuBenhAnWindow : Window
    {
        private DatabaseConnection db = new DatabaseConnection();
        private string maBN;

        // Hàm khởi tạo NHẬN THÊM THAM SỐ (Mã BN và Tên BN)
        public LichSuBenhAnWindow(string maBenhNhan, string tenBenhNhan)
        {
            InitializeComponent();
            this.maBN = maBenhNhan;
            txtTieuDe.Text = $"LỊCH SỬ KHÁM BỆNH CỦA: {tenBenhNhan.ToUpper()}";

            LoadLichSuKham();
        }

        private void LoadLichSuKham()
        {
            try
            {
                List<PhieuKham> dsLichSu = new List<PhieuKham>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    // Lấy các phiếu đã khám của bệnh nhân này
                    string query = @"
                        SELECT p.MaPhieu, p.NgayKham, p.TrieuChung, p.ChanDoan, n.HoTen AS TenBacSi
                        FROM PhieuKham p
                        INNER JOIN NhanVien n ON p.MaBacSi = n.MaNhanVien
                        WHERE p.MaBenhNhan = @mabn AND p.TrangThai = N'Đã khám'
                        ORDER BY p.NgayKham DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@mabn", maBN);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsLichSu.Add(new PhieuKham
                                {
                                    MaPhieu = reader["MaPhieu"].ToString(),
                                    NgayKham = Convert.ToDateTime(reader["NgayKham"]),
                                    TrieuChung = reader["TrieuChung"].ToString(),
                                    ChanDoan = reader["ChanDoan"].ToString(),
                                    TenBacSi = reader["TenBacSi"].ToString()
                                });
                            }
                        }
                    }
                }
                dgLichSu.ItemsSource = dsLichSu;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải lịch sử: " + ex.Message); }
        }

        private void DgLichSu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLichSu.SelectedItem is PhieuKham pk)
            {
                txtTrieuChung.Text = pk.TrieuChung;
                txtChanDoan.Text = pk.ChanDoan;

                LoadDichVuCu(pk.MaPhieu);
                LoadThuocCu(pk.MaPhieu);
            }
        }

        private void LoadDichVuCu(string maPhieu)
        {
            List<ChiTietDichVu> dsDV = new List<ChiTietDichVu>();
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT d.TenDichVu, c.SoLuong FROM ChiTietDichVu c INNER JOIN DichVu d ON c.MaDichVu = d.MaDichVu WHERE c.MaPhieu = @maphieu";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@maphieu", maPhieu);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsDV.Add(new ChiTietDichVu { TenDichVu = reader["TenDichVu"].ToString(), SoLuong = Convert.ToInt32(reader["SoLuong"]) });
                        }
                    }
                }
            }
            dgDichVuCu.ItemsSource = dsDV;
        }

        private void LoadThuocCu(string maPhieu)
        {
            List<ChiTietDonThuoc> dsThuoc = new List<ChiTietDonThuoc>();
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT t.TenThuoc, c.SoLuong, c.CachDung FROM ChiTietDonThuoc c INNER JOIN Thuoc t ON c.MaThuoc = t.MaThuoc WHERE c.MaPhieu = @maphieu";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@maphieu", maPhieu);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsThuoc.Add(new ChiTietDonThuoc { TenThuoc = reader["TenThuoc"].ToString(), SoLuong = Convert.ToInt32(reader["SoLuong"]), CachDung = reader["CachDung"].ToString() });
                        }
                    }
                }
            }
            dgThuocCu.ItemsSource = dsThuoc;
        }
    }
}