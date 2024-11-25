using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyTiemChung.MVVM
{
    public partial class PatientEditView : Window
    {
        private Dictionary<string, Dictionary<string, List<string>>> locationData;
        private readonly Patient _patient;
        private readonly bool _isEditMode;

        public PatientEditView(Patient patient, bool isEditMode = false)
        {
            InitializeComponent();
            _patient = patient;
            _isEditMode = isEditMode;

            LoadLocationData();
            LoadPatientData();
        }

        private void LoadLocationData()
        {
            try
            {
                string jsonFilePath = "D://QuanLyTiemChung//QuanLyTiemChung//vietnam-location.json";
                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show("Không tìm thấy tệp dữ liệu địa phương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string jsonData = File.ReadAllText(jsonFilePath);
                locationData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(jsonData);
                LoadCityComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu địa phương: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPatientData()
        {
            HoTenTextBox.Text = _patient.Name;
            NgaySinhTextBox.Text = _patient.DOB.ToDateTime().ToString("dd/MM/yyyy");
            if (_patient.Gender == "Nam")
            {
                GioiTinhComboBox.SelectedItem = GioiTinhComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "Nam");
            }
            else if (_patient.Gender == "Nữ")
            {
                GioiTinhComboBox.SelectedItem = GioiTinhComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "Nữ");
            }
            DienThoaiTextBox.Text = _patient.PhoneNumber;
            CCCDTextBox.Text = _patient.IDNumber;
            if (!string.IsNullOrEmpty(_patient.PriorityGroup))
            {
                NhomDoiTuongComboBox.SelectedItem = NhomDoiTuongComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _patient.PriorityGroup);
            }

            GhiChuTextBox.Text = _patient.Notes;

            if (!string.IsNullOrEmpty(_patient.Address))
            {
                var addressParts = _patient.Address.Split(',');

                if (addressParts.Length >= 3)
                {
                    CityComboBox.SelectedItem = addressParts[0].Trim();
                    DistrictComboBox.SelectedItem = addressParts[1].Trim();
                    WardComboBox.SelectedItem = addressParts[2].Trim();
                }
            }
        }

        private void LoadCityComboBox()
        {
            CityComboBox.Items.Clear();
            foreach (var city in locationData.Keys)
            {
                CityComboBox.Items.Add(city);
            }
        }

        private void LoadDistrictComboBox(string selectedCity)
        {
            DistrictComboBox.Items.Clear();
            if (locationData.ContainsKey(selectedCity))
            {
                foreach (var district in locationData[selectedCity].Keys)
                {
                    DistrictComboBox.Items.Add(district);
                }
            }
        }

        private void LoadWardComboBox(string selectedDistrict)
        {
            WardComboBox.Items.Clear();
            if (CityComboBox.SelectedItem != null && locationData.ContainsKey(CityComboBox.SelectedItem.ToString()) &&
                locationData[CityComboBox.SelectedItem.ToString()].ContainsKey(selectedDistrict))
            {
                foreach (var ward in locationData[CityComboBox.SelectedItem.ToString()][selectedDistrict])
                {
                    WardComboBox.Items.Add(ward);
                }
            }
        }

        private void CityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CityComboBox.SelectedItem == null) return;

            string selectedCity = CityComboBox.SelectedItem.ToString();
            LoadDistrictComboBox(selectedCity);

            // Clear the Ward ComboBox since the city has changed
            WardComboBox.Items.Clear();
        }

        private void DistrictComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DistrictComboBox.SelectedItem == null) return;

            string selectedDistrict = DistrictComboBox.SelectedItem.ToString();
            LoadWardComboBox(selectedDistrict);
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode) return;

            // Update patient information
            _patient.Name = HoTenTextBox.Text;
            _patient.DOB = Timestamp.FromDateTime(DateTime.Parse(NgaySinhTextBox.Text).ToUniversalTime());
            string gioiTinh = (GioiTinhComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            _patient.Gender = gioiTinh;
            _patient.PhoneNumber = DienThoaiTextBox.Text;
            _patient.IDNumber = CCCDTextBox.Text;
            _patient.PriorityGroup = NhomDoiTuongComboBox.SelectedItem.ToString();
            _patient.Notes = GhiChuTextBox.Text;

            // Get address from ComboBoxes
            string address = $"{CityComboBox.SelectedItem}, {DistrictComboBox.SelectedItem}, {WardComboBox.SelectedItem}";
            _patient.Address = address;

            try
            {
                // Save the updated patient information to Firestore
                FirestoreDb db = FirestoreDb.Create("quanlytiemchung-f225a");
                DocumentReference docRef = db.Collection("patients").Document(_patient.PatientID);
                await docRef.SetAsync(_patient, SetOptions.Overwrite);

                MessageBox.Show("Thông tin bệnh nhân đã được cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin bệnh nhân: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
