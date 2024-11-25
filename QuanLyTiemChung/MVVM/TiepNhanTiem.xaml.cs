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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyTiemChung.MVVM
{

    public partial class TiepNhanTiem : UserControl
    {
        public TiepNhanTiem()
        {
            InitializeComponent();
            ContentControl.Content = new DSChoKham(this);
        }

        private void ShowKham(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new KhamSoBo();
            btnBenhNhan.BorderBrush = Brushes.White;
            btnKhamSoBo.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2e9da6");
            btnChiDinh.BorderBrush = Brushes.White;
        }
        private void ShowChiDinh(object sender, RoutedEventArgs e)
        {
            btnBenhNhan.BorderBrush = Brushes.White;
            btnKhamSoBo.BorderBrush = Brushes.White;
            btnChiDinh.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2e9da6");
            ContentControl.Content = new ChiDinh(); 
        }

        


        private void ShowBenhNhan(object sender, RoutedEventArgs e)
        {
            btnBenhNhan.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2e9da6");
            btnKhamSoBo.BorderBrush = Brushes.White;
            btnChiDinh.BorderBrush = Brushes.White;
            // Create the DSChoKham UserControl
            var dsChoKhamUserControl = new DSChoKham();

            // Subscribe to the ProceedButtonClicked event
            dsChoKhamUserControl.ProceedButtonClicked += DSChoKham_ProceedButtonClicked;

            // Set the ContentControl to show the DSChoKham UserControl
            ContentControl.Content = dsChoKhamUserControl;
        }
        private void DSChoKham_ProceedButtonClicked(object sender, EventArgs e)
        {
            // Debug: Check if this is triggered
            Console.WriteLine("Proceed button event received.");

            // Switch the content to the "Kham So Bo" UserControl
            var khamSoBoUserControl = new KhamSoBo();
            ContentControl.Content = khamSoBoUserControl;
        }
        public void ChangeContentToChiDinh(OrderPatientInfo orderPatientInfo)
        {
            btnBenhNhan.BorderBrush = Brushes.White;
            btnKhamSoBo.BorderBrush = Brushes.White;
            btnChiDinh.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2e9da6");
            var chiDinhControl = new ChiDinh(orderPatientInfo,this); // Pass the patient record ID
            ContentControl.Content = chiDinhControl; // Set new control to ContentHost
        }
        public void ChangeContentToKhamSoBo(OrderPatientInfo orderPatientInfo)
        {
            btnBenhNhan.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2e9da6");
            btnKhamSoBo.BorderBrush = Brushes.White;
            btnChiDinh.BorderBrush = Brushes.White;
            // Tạo KhamSoBo control và truyền tham chiếu của lớp cha (this)
            var khamSoBoControl = new KhamSoBo(orderPatientInfo,this);

            // Set content của ContentControl
            ContentControl.Content = khamSoBoControl;
        }
        public void NavigateToDanhSachKham()
        {

            this.Content = new DSChoKham(); // assuming DanhSachKham is a UserControl
        }
    }
}

