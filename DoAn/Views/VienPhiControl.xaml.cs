using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using DoAn.DataAccess;

namespace DoAn.Views
{
    public partial class VienPhiControl : UserControl
    {
        private DatabaseConnection db = new DatabaseConnection();
        private string maPhieuDangChon = "";
        private decimal tongTienThuoc = 0;
        private decimal tienKham = 100000; // Tiền khám mặc định 100k

        public VienPhiControl()
        {
            InitializeComponent();
            LoadDanhSachCho();
        }

        // Tải danh sách các Phiếu Khám CHƯA có Hóa đơn hoặc Hóa đơn chưa thanh toán
        private void LoadDanhSachCho(string tuKhoa = "")
        {
            try
            {
                using (SqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT p.MaPhieu, b.HoTen, p.NgayKham, p.ChanDoan 
                        FROM PhieuKham p
                        INNER JOIN BenhNhan b ON p.MaBenhNhan = b.MaBenhNhan
                        LEFT JOIN HoaDon h ON p.MaPhieu = h.MaPhieu
                        WHERE (h.TrangThai IS NULL OR h.TrangThai = N'Chưa thanh toán')
                        AND (b.HoTen LIKE @tukhoa OR p.MaPhieu LIKE @tukhoa)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tukhoa", "%" + tuKhoa + "%");
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgChoThanhToan.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách: " + ex.Message); }
        }

        private void BtnTim_Click(object sender, RoutedEventArgs e)
        {
            LoadDanhSachCho(txtTimKiem.Text.Trim());
        }

        // Khi Thu ngân click vào 1 phiếu khám để tính tiền
        private void DgChoThanhToan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgChoThanhToan.SelectedItem is DataRowView row)
            {
                maPhieuDangChon = row["MaPhieu"].ToString();
                txtTenBN.Text = row["HoTen"].ToString();
                txtChanDoan.Text = row["ChanDoan"].ToString();
                txtMaHoaDon.Text = $"Mã HĐ: HD-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";

                LoadChiTietThuoc(maPhieuDangChon);

                btnThanhToan.IsEnabled = true;
                btnInHoaDon.IsEnabled = false; // Chỉ in được sau khi đã thanh toán
            }
        }

        // Truy vấn chi tiết thuốc và tính tổng tiền
        private void LoadChiTietThuoc(string maPhieu)
        {
            tongTienThuoc = 0;
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
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgChiTietThuoc.ItemsSource = dt.DefaultView;

                    // Tính tổng tiền thuốc bằng cách cộng dồn cột ThanhTien
                    foreach (DataRow r in dt.Rows)
                    {
                        tongTienThuoc += Convert.ToDecimal(r["ThanhTien"]);
                    }
                }
            }

            // Cập nhật lên Giao diện
            txtTienThuoc.Text = tongTienThuoc.ToString("N0") + " VNĐ";
            txtTienKham.Text = tienKham.ToString("N0") + " VNĐ";
            txtTongTien.Text = (tienKham + tongTienThuoc).ToString("N0") + " VNĐ";
        }

        // Lưu hóa đơn và chuyển trạng thái thành "Đã thanh toán"
        private void BtnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(maPhieuDangChon)) return;

            if (MessageBox.Show("Xác nhận thu tiền cho phiếu khám này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        // Dùng tính năng MERGE hoặc IF EXISTS để tạo/cập nhật hóa đơn
                        string query = @"
                            INSERT INTO HoaDon (MaHoaDon, MaPhieu, TienKham, TienThuoc, TongTien, TrangThai, NgayThanhToan)
                            VALUES (@mahd, @maphieu, @tienkham, @tienthuoc, @tong, N'Đã thanh toán', GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@mahd", txtMaHoaDon.Text.Replace("Mã HĐ: ", ""));
                            cmd.Parameters.AddWithValue("@maphieu", maPhieuDangChon);
                            cmd.Parameters.AddWithValue("@tienkham", tienKham);
                            cmd.Parameters.AddWithValue("@tienthuoc", tongTienThuoc);
                            cmd.Parameters.AddWithValue("@tong", tienKham + tongTienThuoc);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDanhSachCho(); // Load lại danh sách (BN này sẽ biến mất khỏi hàng đợi thu ngân)

                    btnThanhToan.IsEnabled = false;
                    btnInHoaDon.IsEnabled = true; // Cho phép in hóa đơn
                }
                catch (Exception ex) { MessageBox.Show("Lỗi thanh toán: Có thể hóa đơn cho phiếu này đã tồn tại.\n" + ex.Message); }
            }
        }

        // Nâng cao: In hóa đơn
       
           private void BtnInHoaDon_Click(object sender, RoutedEventArgs e)
        {
            // Mở hộp thoại chọn máy in của Windows
            PrintDialog printDialog = new PrintDialog();

            // Nếu người dùng bấm OK (Print) trong hộp thoại
            if (printDialog.ShowDialog() == true)
            {
                // 1. Giấu cái thanh chứa 2 nút bấm đi để không in ra giấy
                panelNutBam.Visibility = Visibility.Hidden;

                // 2. Chỉnh lại lề một chút cho đẹp khi in (tùy chọn)
                Thickness oldMargin = GridHoaDonToPrint.Margin;
                GridHoaDonToPrint.Margin = new Thickness(20);

                try
                {
                    // 3. Thực hiện lệnh in cái khung GridHoaDonToPrint
                    printDialog.PrintVisual(GridHoaDonToPrint, "In Hoa Don Vien Phi");
                    MessageBox.Show("Đã gửi lệnh in thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi máy in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // 4. In xong thì trả lại giao diện như cũ
                    panelNutBam.Visibility = Visibility.Visible;
                    GridHoaDonToPrint.Margin = oldMargin;
                }
            }
        }
    }
    }
