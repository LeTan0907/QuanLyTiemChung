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
    /// Interaction logic for PatientDetailView.xaml
    /// </summary>
    public partial class PatientDetailView : Window
    {
        public PatientDetailView(Patient selectedPatient)
        {
            InitializeComponent();
            this.DataContext = selectedPatient;
            // Kiểm tra xem DOB có phải là null không
            if (selectedPatient.DOB != null)
            {
                // Chuyển Timestamp thành DateTime và hiển thị trong TextBox với định dạng dd/MM/yyyy
                var dob = selectedPatient.DOB.ToDateTime();  // Chuyển Timestamp thành DateTime
                DOBTextBox.Text = dob.ToString("dd/MM/yyyy"); // Định dạng DateTime thành dd/MM/yyyy
            }
            else
            {
                DOBTextBox.Text = "Không có thông tin"; // Hoặc bạn có thể để rỗng
            }
        }
        private void Dong(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
