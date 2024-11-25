using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using QuanLyTiemChung.MVVM;
using MaterialDesignThemes.Wpf;

namespace QuanLyTiemChung
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

    {
        
        public MainWindow()
        {
            InitializeComponent();
            LeTan_TiepNhan tn =new LeTan_TiepNhan();
            MainContentControl.Content = tn;
        }

        private bool IsMaximize = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    IsMaximize = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximize = true;
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        
        private void TiepNhanButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new LeTan_TiepNhan();
        }
        private void ShowLichKham(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new TiepNhanTiem();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Logout(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
               "Bạn có chắc chắn muốn đăng xuất?",
               "Xác nhận đăng xuất",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DangNhap dangnhap = new DangNhap();
                dangnhap.Show();
                this.Close();
            }
        }

        //UI//
      
    }
}