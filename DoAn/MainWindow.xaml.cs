using System;
using System.Windows;
using System.Windows.Controls;
using DoAn.Views; // Đảm bảo gọi thư mục Views
using System.Windows;
using DoAn.Helpers;
using DoAn.Helpers;
namespace DoAn
{
    public partial class MainWindow : Window
    {
        private string _chucVu;

        // Constructor nhận dữ liệu từ form Login
        public MainWindow()
        {
            InitializeComponent();
            

            // Hiển thị tên người dùng lên Sidebar
            txtXinChao.Text = $"Xin chào, {Session.HoTen}";
            txtQuyenHan.Text = $"Quyền: {Session.ChucVu}";

            PhanQuyenMenu();
        }

        // Logic ẩn/hiện menu theo chức vụ
        private void PhanQuyenMenu()
        {

            // Ẩn tất cả đi trước cho an toàn
            btnTiepNhan.Visibility = Visibility.Collapsed;
            btnKhamBenh.Visibility = Visibility.Collapsed;
            btnKhoThuoc.Visibility = Visibility.Collapsed;
            btnVienPhi.Visibility = Visibility.Collapsed;
            btnNhanVien.Visibility = Visibility.Collapsed;
            btnLichSuKham.Visibility = Visibility.Collapsed;
            btnDatLich.Visibility = Visibility.Collapsed;
            btnDuyetLich.Visibility = Visibility.Collapsed;
            btnTongQuan.Visibility = Visibility.Collapsed;
            btnQuanLyHoaDon.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(Session.ChucVu)) return;
            string chucVuChuan = Session.ChucVu.Trim();

            // Bật lại tùy theo Role
            switch (chucVuChuan)
            {
                case "Admin":
                    btnTongQuan.Visibility = Visibility.Visible;
                    btnKhoThuoc.Visibility = Visibility.Visible;
                    btnNhanVien.Visibility = Visibility.Visible;
                    btnQuanLyHoaDon.Visibility = Visibility.Visible;
                    break;
                case "Lễ tân":
                    btnTiepNhan.Visibility = Visibility.Visible;
                    btnVienPhi.Visibility = Visibility.Visible; // Lễ tân có thể thu tiền
                    btnDuyetLich.Visibility = Visibility.Visible;
                    break;
                case "Bác sĩ":
                    btnKhamBenh.Visibility = Visibility.Visible;
                    break;
                case "Thu ngân":
                    btnVienPhi.Visibility = Visibility.Visible;
                    break;

                case "Bệnh nhân":
                    btnDatLich.Visibility = Visibility.Visible;
                    btnLichSuKham.Visibility = Visibility.Visible;
                    break;
            }
        }

        // Xử lý đăng xuất
        private void BtnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        // CÁC HÀM ĐIỀU HƯỚNG SẼ VIẾT VÀO ĐÂY
        private void BtnTiepNhan_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DoAn.Views.QuanLyBenhNhanControl();
        }

        private void BtnKhamBenh_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DoAn.Views.KhamBenhControl();
        }

        private void btnTongQuan_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnKhoThuoc_Click(object sender, RoutedEventArgs e)
        {
            // Gọi màn hình Quản lý Kho thuốc
            MainContent.Content = new DoAn.Views.QuanLyThuocControl();
        }

        private void BtnVienPhi_Click(object sender, RoutedEventArgs e)
        {
            // Gọi màn hình Quản lý Viện phí & Hóa đơn
            MainContent.Content = new DoAn.Views.VienPhiControl();
        }

        private void BtnNhanVien_Click(object sender, RoutedEventArgs e)
        {
            // Gọi màn hình Quản lý Nhân sự
            MainContent.Content = new DoAn.Views.QuanLyNhanSuControl();
        }

        private void BtnDatLich_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DoAn.Views.DatLichKhamControl();
        }

        private void BtnLichSuKham_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DoAn.Views.LichSuKhamControl();
        }

        private void BtnDuyetLich_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DoAn.Views.DuyetLichHenControl();
        }

        private void btnTongQuan_Click_1(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DoAn.Views.DashboardControl();
        }

        private void BtnQuanLyHoaDon_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DoAn.Views.QuanLyHoaDonControl();
        }
    }
}