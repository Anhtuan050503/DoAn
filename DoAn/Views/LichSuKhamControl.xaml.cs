using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using DoAn.Models;
using DoAn.Helpers;

namespace DoAn.Views
{
    public partial class LichSuKhamControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        public LichSuKhamControl()
        {
            InitializeComponent();
            LoadLichSuKham();
        }

        // TẢI LỊCH SỬ BẰNG OBJECT PhieuKham
        private void LoadLichSuKham()
        {
            try
            {
                List<PhieuKham> dsPhieuKham = new List<PhieuKham>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT p.MaPhieu, p.NgayKham, n.HoTen AS TenBacSi, p.TrieuChung, p.ChanDoan 
                        FROM PhieuKham p
                        INNER JOIN NhanVien n ON p.MaBacSi = n.MaNhanVien
                        WHERE p.MaBenhNhan = @mabn
                        ORDER BY p.NgayKham DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@mabn", Session.MaNhanVien);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PhieuKham pk = new PhieuKham();
                                pk.MaPhieu = reader["MaPhieu"].ToString();
                                pk.NgayKham = Convert.ToDateTime(reader["NgayKham"]);
                                pk.TrieuChung = reader["TrieuChung"].ToString();
                                pk.ChanDoan = reader["ChanDoan"].ToString();
                                pk.TenBacSi = reader["TenBacSi"].ToString(); // Thuộc tính DTO
                                dsPhieuKham.Add(pk);
                            }
                        }
                    }
                }
                dgLichSu.ItemsSource = dsPhieuKham;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải lịch sử: " + ex.Message); }
        }

        // BẮT SỰ KIỆN CHỌN BẰNG OBJECT PhieuKham
        private void DgLichSu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLichSu.SelectedItem is PhieuKham pk)
            {
                txtMaPhieu.Text = pk.MaPhieu;
                txtTrieuChung.Text = pk.TrieuChung;
                txtChanDoan.Text = pk.ChanDoan;

                LoadChiTietDonThuoc(pk.MaPhieu);
            }
        }

        // TẢI CHI TIẾT ĐƠN THUỐC BẰNG OBJECT ChiTietDonThuoc
        private void LoadChiTietDonThuoc(string maPhieu)
        {
            try
            {
                List<ChiTietDonThuoc> dsThuoc = new List<ChiTietDonThuoc>();
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT t.TenThuoc, t.DonViTinh, c.SoLuong, c.CachDung
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
                                ChiTietDonThuoc ct = new ChiTietDonThuoc();
                                ct.SoLuong = Convert.ToInt32(reader["SoLuong"]);
                                ct.CachDung = reader["CachDung"].ToString();
                                ct.TenThuoc = reader["TenThuoc"].ToString(); // Thuộc tính DTO
                                ct.DonViTinh = reader["DonViTinh"].ToString(); // Thuộc tính DTO

                                dsThuoc.Add(ct);
                            }
                        }
                    }
                }
                dgDonThuoc.ItemsSource = dsThuoc;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải đơn thuốc: " + ex.Message); }
        }
    }
}