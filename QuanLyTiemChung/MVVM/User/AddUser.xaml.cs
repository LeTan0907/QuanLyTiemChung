using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace QuanLyTiemChung.MVVM.User
{
    public partial class AddUser : Window
    {
        private Dictionary<string, Dictionary<string, List<string>>> locationData;
        private FirestoreDb firestoreDb;

        public AddUser()
        {
            InitializeComponent();
            firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a"); // Initialize Firestore connection
            LoadLocationData(); // Load the location data
            PopulateCityComboBox(); // Populate City ComboBox
        }

        // Load location data from JSON
        private void LoadLocationData()
        {
            try
            {
                string jsonFilePath = "././vietnam-location.json";
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
        private void CityComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
        private void DistrictComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

        // Method to create a new user and account
        private async Task CreateUserAndAccount(Users newUser)
        {
            try
            {
                // Ensure user ID is set (it can be generated or passed in)
                newUser.UserID = "US" + new Random().Next(1000, 9999); // Example: Generate a unique user ID

                // Create the User document
                var userCollection = firestoreDb.Collection("Users");
                await userCollection.Document(newUser.UserID).SetAsync(newUser); // Save the User data

                // Create the Account document
                Account newAccount = new Account
                {
                    Username = newUser.UserID, // Link Account to User via UserID
                    Password = "1", // Default password (consider hashing it)
                    Enable = true, // Account is enabled by default
                    Role = newUser.Role // Use the Role from the User
                };

                var accountCollection = firestoreDb.Collection("Accounts");
                await accountCollection.Document(newUser.UserID).SetAsync(newAccount); // Save the Account data

                MessageBox.Show("User and Account created successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Button click handler to add a user
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            Users newUser = new Users
            {
                Name = UsernameTextBox.Text,
                Gender = GenderComboBox.SelectedItem.ToString(),
                PhoneNumber = PhoneNumberTextBox.Text,
                Address = $"{CityComboBox.SelectedItem}, {DistrictComboBox.SelectedItem}, {WardComboBox.SelectedItem}",
                Role = RoleComboBox.SelectedItem.ToString(),
                DOB = Timestamp.GetCurrentTimestamp() // Set the date of birth (can be a real value)
            };

            await CreateUserAndAccount(newUser); // Create both User and Account
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
