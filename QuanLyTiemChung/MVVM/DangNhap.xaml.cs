using QuanLyTiemChung.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuanLyTiemChung.MVVM
{
    /// <summary>
    /// Interaction logic for DangNhap.xaml
    /// </summary>
    public partial class DangNhap : Window
    {
        private AuthService authService;

        public DangNhap()
        {
            InitializeComponent();
            authService = new AuthService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //var email = txtEmail.Text;
            //var password = passwordBox.Password;

            //if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            //{
            //    MessageBox.Show("Please enter both email and password.");
            //    return;
            //}

            //try
            //{
            //    var authLink = await authService.LoginWithEmailPassword(email, password);
            //    MessageBox.Show("Login successful!");

            //    // Open the MainWindow
            //    MainWindow mainWindow = new MainWindow();
            //    mainWindow.Show();

            //    // Close the DangNhap window
            //    this.Close();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }
    }
}
