using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace QuanLyTiemChung.MVVM.User
{
    /// <summary>
    /// Interaction logic for UpdateUser.xaml
    /// </summary>
    
    public partial class UpdateUser : Window
    {
        private Dictionary<string, Dictionary<string, List<string>>> locationData;
        public UpdateUser()
        {
            InitializeComponent();
        }
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
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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

    }
}
