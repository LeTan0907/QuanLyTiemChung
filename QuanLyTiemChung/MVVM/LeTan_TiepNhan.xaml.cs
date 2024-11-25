using Google.Cloud.Firestore;
using MaterialDesignThemes.Wpf;
using QuanLyTiemChung.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Interaction logic for LeTan_TiepNhan.xaml
    /// </summary>
    public partial class LeTan_TiepNhan : UserControl
    {
        private FirestoreDb _firestoreDb;
        public ObservableCollection<Patient> Patients { get; set; }

        public LeTan_TiepNhan()
        {
            InitializeComponent();
            InitializeFirestore();
            this.DataContext = new PatientViewModel();
            Patients = new ObservableCollection<Patient>();
            this.DataContext = this; // Bind DataGrid to this UserControl's DataContext
            OrderView orderView = new OrderView();
            MainContent.Content = orderView;
        }

        private void InitializeFirestore()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\quanlytiemchung-f225a-firebase-adminsdk-ahz93-4982e9abc8.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
        }

        private async Task LoadTodayPatients()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                Query query = _firestoreDb.Collection("patients")
                    .WhereGreaterThanOrEqualTo("RegistrationDate", Timestamp.FromDateTime(today))
                    .WhereLessThan("RegistrationDate", Timestamp.FromDateTime(tomorrow));

                QuerySnapshot snapshot = await query.GetSnapshotAsync();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        Patient patient = document.ConvertTo<Patient>();
                        Patients.Add(patient);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPatientInfo(object sender, RoutedEventArgs e)
        {
            var dangKiWindow = new DangKiThongTin();  // Create an instance of the DangKiThongTin window
            dangKiWindow.ShowDialog();  // Show the window
        }
        private void ShowOrderToday(object sender, RoutedEventArgs e)
        {
            var dangKiWindow = new DangKiThongTin();  // Create an instance of the DangKiThongTin window
            dangKiWindow.ShowDialog();  // Show the window
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dangKiWindow = new DangKiThongTin();  // Create an instance of the DangKiThongTin window
            dangKiWindow.ShowDialog();  // Show the window
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var patientview = new   PatientView();
            MainContent.Content = patientview;
            Danhsach.BorderBrush = new SolidColorBrush(Colors.White);
            DaDangKi.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2e9da6");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OrderView orderView = new OrderView();
            MainContent.Content=orderView;
            DaDangKi.BorderBrush = new SolidColorBrush(Colors.White);
            Danhsach.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2e9da6");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var loadpatient = new LoadPatientData();
            loadpatient.ShowDialog();
        }
    }
}
