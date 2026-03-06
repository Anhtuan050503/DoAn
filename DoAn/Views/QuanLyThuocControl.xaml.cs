using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;
using DoAn.Models;
using System.Collections.Generic;
namespace DoAn.Views
{
    public partial class QuanLyThuocControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();

        public QuanLyThuocControl()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData(string tukhoa = "")
        {
            try
            {
                List<Thuoc> dsThuoc = new List<Thuoc>();

                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM Thuoc WHERE TenThuoc LIKE @tukhoa OR MaThuoc LIKE @tukhoa";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tukhoa", "%" + tukhoa + "%");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Thuoc t = new Thuoc();
                                t.MaThuoc = reader["MaThuoc"].ToString();
                                t.TenThuoc = reader["TenThuoc"].ToString();
                                t.DonViTinh = reader["DonViTinh"].ToString();
                                t.SoLuongTon = Convert.ToInt32(reader["SoLuongTon"]);
                                t.HanSuDung = Convert.ToDateTime(reader["HanSuDung"]);
                                t.GiaThuoc = Convert.ToDecimal(reader["GiaThuoc"]);

                                dsThuoc.Add(t);
                            }
                        }
                    }
                }
                dgThuoc.ItemsSource = dsThuoc;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtMaThuoc.Clear();
            txtTenThuoc.Clear();
            txtSoLuong.Text = "0";
            txtGiaThuoc.Text = "0";
            dpHanSuDung.SelectedDate = DateTime.Now;
            cboDonViTinh.SelectedIndex = 0;
            txtTimKiem.Clear();
            txtMaThuoc.IsReadOnly = false;
            LoadData();
        }

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaThuoc.Text) || string.IsNullOrWhiteSpace(txtTenThuoc.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã và Tên thuốc!"); return;
            }

            int soLuong = 0; decimal giaThuoc = 0;
            int.TryParse(txtSoLuong.Text, out soLuong);
            decimal.TryParse(txtGiaThuoc.Text, out giaThuoc);

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Thuoc (MaThuoc, TenThuoc, DonViTinh, SoLuongTon, HanSuDung, GiaThuoc) " +
                                   "VALUES (@ma, @ten, @dvt, @sl, @hsd, @gia)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma", txtMaThuoc.Text);
                        cmd.Parameters.AddWithValue("@ten", txtTenThuoc.Text);
                        cmd.Parameters.AddWithValue("@dvt", (cboDonViTinh.SelectedItem as ComboBoxItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@sl", soLuong);
                        cmd.Parameters.AddWithValue("@hsd", dpHanSuDung.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@gia", giaThuoc);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Thêm thuốc thành công!");
                        BtnLamMoi_Click(null, null);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void DgThuoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgThuoc.SelectedItem is Thuoc t) // Ép kiểu về Thuoc
            {
                txtMaThuoc.Text = t.MaThuoc;
                txtMaThuoc.IsReadOnly = true;
                txtTenThuoc.Text = t.TenThuoc;
                cboDonViTinh.Text = t.DonViTinh;
                txtSoLuong.Text = t.SoLuongTon.ToString();
                txtGiaThuoc.Text = Math.Round(t.GiaThuoc, 0).ToString();
                dpHanSuDung.SelectedDate = t.HanSuDung;
            }
        }

        private void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaThuoc.Text)) return;

            int soLuong = 0; decimal giaThuoc = 0;
            int.TryParse(txtSoLuong.Text, out soLuong);
            decimal.TryParse(txtGiaThuoc.Text, out giaThuoc);

            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Thuoc SET TenThuoc=@ten, DonViTinh=@dvt, SoLuongTon=@sl, HanSuDung=@hsd, GiaThuoc=@gia WHERE MaThuoc=@ma";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ma", txtMaThuoc.Text);
                        cmd.Parameters.AddWithValue("@ten", txtTenThuoc.Text);
                        cmd.Parameters.AddWithValue("@dvt", cboDonViTinh.Text);
                        cmd.Parameters.AddWithValue("@sl", soLuong);
                        cmd.Parameters.AddWithValue("@hsd", dpHanSuDung.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@gia", giaThuoc);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Cập nhật thành công!");
                        LoadData();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaThuoc.Text)) return;

            if (MessageBox.Show($"Xóa thuốc {txtTenThuoc.Text}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        string query = "DELETE FROM Thuoc WHERE MaThuoc=@ma";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ma", txtMaThuoc.Text);
                            cmd.ExecuteNonQuery();
                            BtnLamMoi_Click(null, null);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: Có thể thuốc này đang nằm trong đơn thuốc của bệnh nhân."); }
            }
        }

        private void BtnTim_Click(object sender, RoutedEventArgs e)
        {
            LoadData(txtTimKiem.Text.Trim());
        }
    }
}