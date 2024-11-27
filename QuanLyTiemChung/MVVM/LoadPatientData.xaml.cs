using Google.Cloud.Firestore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLyTiemChung.MVVM
{
    public partial class LoadPatientData : Window
    {
        public ObservableCollection<Patient> Patients { get; set; } = new ObservableCollection<Patient>();
        public ObservableCollection<Patient> FilteredPatients { get; set; } = new ObservableCollection<Patient>();
        private FirestoreDb _firestoreDb;

        public LoadPatientData()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext for binding
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");

            // Load all patients on window open
            LoadPatientsFromFirestore();
        }

        // Load all patients from Firestore
        private async void LoadPatientsFromFirestore()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("patients").GetSnapshotAsync();

                // Clear existing collections
                Patients.Clear();
                FilteredPatients.Clear();

                foreach (var document in snapshot.Documents)
                {
                    var patient = document.ConvertTo<Patient>();
                    Patients.Add(patient);
                    FilteredPatients.Add(patient);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Sự kiện khi nhấn nút Select
        private void SelectPatient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy thông tin bệnh nhân được chọn từ DataGrid
                var selectedPatient = (Patient)patientsDataGrid.SelectedItem;

                if (selectedPatient == null)
                {
                    MessageBox.Show("Vui lòng chọn một bệnh nhân!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mở FormDangKy và truyền thông tin bệnh nhân
                var formDangKy = new FormDangKy(selectedPatient.PatientID); // Sử dụng CCCD làm PatientID
                formDangKy.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn bệnh nhân: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Search function
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;

            // Lọc bệnh nhân chỉ dựa trên tên
            var filtered = Patients.Where(p =>
                !string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(searchText)
            ).ToList();

            // Cập nhật danh sách FilteredPatients
            FilteredPatients.Clear();
            foreach (var patient in filtered)
            {
                FilteredPatients.Add(patient);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text="";
            LoadPatientsFromFirestore();
        }
    }

}
