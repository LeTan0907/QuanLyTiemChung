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

            // Gán đối tượng bệnh nhân vào DataContext để sử dụng trong XAML
            this.DataContext = selectedPatient;
        }
        private void Dong(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
