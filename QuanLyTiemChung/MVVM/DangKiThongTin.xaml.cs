using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyTiemChung.MVVM
{
    public partial class DangKiThongTin : Window
    {
        private FirestoreDb _firestoreDb;
        public string PatientID { get; private set; }
        public string IDNumber { get; private set; }
        private Dictionary<string, Dictionary<string, List<string>>> locationData;

        public DangKiThongTin()
        {
            InitializeComponent();
            InitializeFirestore();
            LoadLocationData();
            PopulateCityComboBox();
        }

        // Initialize Firestore connection
        private void InitializeFirestore()
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\quanlytiemchung-f225a-firebase-adminsdk-ahz93-4982e9abc8.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
                _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi kết nối Firestore: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        public async Task SavePatientDataAsync()
        {
            if (!ValidateInputs()) return;

            // Ensure PriorityGroup is not null or empty
            string priorityGroup = (NhomDoiTuongComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(priorityGroup))
            {
                MessageBox.Show("Vui lòng chọn nhóm đối tượng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var patient = new Patient
            {
                PatientID = Guid.NewGuid().ToString(), // Generate internal PatientID
                Name = HoTenTextBox.Text,
                DOB = DateTime.TryParse(NgaySinhTextBox.Text, out var dob)
                    ? Timestamp.FromDateTime(dob.ToUniversalTime())
                    : Timestamp.GetCurrentTimestamp(), // Ensure UTC
                Gender = (GioiTinhComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                PhoneNumber = DienThoaiTextBox.Text.Trim(), // Make sure to capture phone number
                Address = $"{WardComboBox.Text}, {DistrictComboBox.Text}, {CityComboBox.Text}",
                IDNumber = CCCDTextBox.Text.Trim(), // Capture CCCD
                PriorityGroup = priorityGroup, // Set priority group value
                Notes = GhiChuTextBox.Text
            };

            try
            {
                // Check that the phone number and CCCD are captured correctly
                if (string.IsNullOrEmpty(patient.PhoneNumber))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(patient.IDNumber))
                {
                    MessageBox.Show("Vui lòng nhập số CCCD!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save to Firestore
                var patientRef = _firestoreDb.Collection("patients").Document(patient.PatientID);
                await patientRef.SetAsync(patient);

                PatientID = patient.PatientID; // Store the internal PatientID
                MessageBox.Show("Thông tin bệnh nhân đã được lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin bệnh nhân: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Handle registration button click
        private async void DangKi(object sender, RoutedEventArgs e)
        {
            await SavePatientDataAsync();

            if (!string.IsNullOrEmpty(PatientID))
            {
                var orderForm = new FormDangKy(PatientID);
                orderForm.ShowDialog();
                this.Close();
            }
        }

        // Load location data from JSON
        private void LoadLocationData()
        {
            try
            {
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vietnam-location.json");
                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show("Không tìm thấy tệp dữ liệu địa phương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string jsonData = File.ReadAllText(jsonFilePath);
                locationData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu địa phương: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Populate CityComboBox with city names
        private void PopulateCityComboBox()
        {
            if (locationData == null) return;

            CityComboBox.Items.Clear();
            foreach (var city in locationData.Keys)
            {
                CityComboBox.Items.Add(city);
            }
        }

        // Handle CityComboBox selection change
        private void CityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DistrictComboBox.Items.Clear();
            WardComboBox.Items.Clear();

            if (CityComboBox.SelectedItem == null) return;

            string selectedCity = CityComboBox.SelectedItem.ToString();
            if (locationData.ContainsKey(selectedCity))
            {
                foreach (var district in locationData[selectedCity].Keys)
                {
                    DistrictComboBox.Items.Add(district);
                }
            }
        }

        // Handle DistrictComboBox selection change
        private void DistrictComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WardComboBox.Items.Clear();

            if (CityComboBox.SelectedItem == null || DistrictComboBox.SelectedItem == null) return;

            string selectedCity = CityComboBox.SelectedItem.ToString();
            string selectedDistrict = DistrictComboBox.SelectedItem.ToString();

            if (locationData.ContainsKey(selectedCity) && locationData[selectedCity].ContainsKey(selectedDistrict))
            {
                foreach (var ward in locationData[selectedCity][selectedDistrict])
                {
                    WardComboBox.Items.Add(ward);
                }
            }
        }

        // Close window
        private void Dong(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Validate user inputs
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(HoTenTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(CCCDTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập số CCCD!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CityComboBox.SelectedItem == null || DistrictComboBox.SelectedItem == null || WardComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn địa chỉ đầy đủ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
