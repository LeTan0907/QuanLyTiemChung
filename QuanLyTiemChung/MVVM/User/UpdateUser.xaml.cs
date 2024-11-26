using Google.Cloud.Firestore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuanLyTiemChung.MVVM.ViewModels;
using Newtonsoft.Json;
using System.IO;

namespace QuanLyTiemChung.MVVM.User
{
    public partial class UpdateUser : Window
    {
        private readonly Users _user;  // The user to be updated
        private readonly bool _isEditMode; // Flag to check if it's edit mode

        private Dictionary<string, Dictionary<string, List<string>>> locationData; // Location data for cities, districts, wards

        public UpdateUser(Users user, bool isEditMode = true)
        {
            InitializeComponent();
            _user = user;
            _isEditMode = isEditMode;

            LoadLocationData();
            LoadUserData();
        }

        // Load location data from JSON file or any data source
        private void LoadLocationData()
        {
            try
            {
                // Sample file path, adjust as needed
                string jsonFilePath = "D://QuanLyTiemChung//QuanLyTiemChung//vietnam-location.json";

                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show("Không tìm thấy tệp dữ liệu địa phương!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Read and deserialize JSON data into locationData dictionary
                string jsonData = File.ReadAllText(jsonFilePath);
                locationData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(jsonData);

                // Load the city ComboBox
                LoadCityComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu địa phương: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load cities into the ComboBox
        private void LoadCityComboBox()
        {
            CityComboBox.Items.Clear();
            foreach (var city in locationData.Keys)
            {
                CityComboBox.Items.Add(city);
            }
        }

        // Load districts based on selected city
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

        // Load wards based on selected district
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

        // Method to load user data into the form
        private void LoadUserData()
        {
            if (_user == null) return;

            // Load existing user data into the form
            NameTextBox.Text = _user.Name;
            DOBPicker.SelectedDate = _user.DOB.ToDateTime();
            GenderComboBox.SelectedItem = GenderComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == _user.Gender);
            PhoneNumberTextBox.Text = _user.PhoneNumber;
            RoleComboBox.SelectedItem = RoleComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == _user.Role);

            // Split the current address into city, district, and ward and select them
            if (!string.IsNullOrEmpty(_user.Address))
            {
                var addressParts = _user.Address.Split(',');

                if (addressParts.Length >= 3)
                {
                    string city = addressParts[0].Trim();
                    string district = addressParts[1].Trim();
                    string ward = addressParts[2].Trim();

                    // Set selected city, district, and ward
                    CityComboBox.SelectedItem = city;
                    LoadDistrictComboBox(city);
                    DistrictComboBox.SelectedItem = district;
                    LoadWardComboBox(district);
                    WardComboBox.SelectedItem = ward;
                }
            }
        }

        // Save user data changes
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode || !ValidateForm()) return;

            // Update user object with new values
            _user.Name = NameTextBox.Text;
            _user.PhoneNumber = PhoneNumberTextBox.Text;
            _user.Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            _user.Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            _user.DOB = Timestamp.FromDateTime(DOBPicker.SelectedDate?.ToUniversalTime() ?? DateTime.UtcNow);

            // Get the full address from the ComboBoxes
            string address = $"{CityComboBox.SelectedItem}, {DistrictComboBox.SelectedItem}, {WardComboBox.SelectedItem}";
            _user.Address = address;

            try
            {
                // Save the updated user data to Firestore
                FirestoreDb db = FirestoreDb.Create("quanlytiemchung-f225a");
                DocumentReference docRef = db.Collection("Users").Document(_user.UserID);
                await docRef.SetAsync(_user, SetOptions.Overwrite);

                MessageBox.Show("Thông tin người dùng đã được cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Cancel and close the window
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Event handler for city selection change
        private void CityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CityComboBox.SelectedItem == null) return;

            string selectedCity = CityComboBox.SelectedItem.ToString();
            LoadDistrictComboBox(selectedCity);

            // Clear the Ward ComboBox since the city has changed
            WardComboBox.Items.Clear();
        }

        // Event handler for district selection change
        private void DistrictComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DistrictComboBox.SelectedItem == null) return;

            string selectedDistrict = DistrictComboBox.SelectedItem.ToString();
            LoadWardComboBox(selectedDistrict);
        }

        // Method to validate the form before saving
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Tên người dùng không thể để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text))
            {
                MessageBox.Show("Số điện thoại không thể để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (GenderComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn giới tính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn vai trò.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (CityComboBox.SelectedItem == null || DistrictComboBox.SelectedItem == null || WardComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ địa chỉ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}
