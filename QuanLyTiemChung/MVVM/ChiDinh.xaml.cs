using Google.Cloud.Firestore;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuanLyTiemChung.MVVM
{
    public partial class ChiDinh : UserControl
    {
        private OrderPatientInfo _orderPatientInfo; // Store OrderPatientInfo
        private TiepNhanTiem _parentControl; // Reference to parent control

        // Constructor to initialize with OrderPatientInfo and parent control
        public ChiDinh(OrderPatientInfo orderPatientInfo, TiepNhanTiem parentControl)
        {
            InitializeComponent();

            _parentControl = parentControl;
            _orderPatientInfo = orderPatientInfo; // Use OrderPatientInfo to get all patient details

            if (_orderPatientInfo != null)
            {
                MessageBox.Show($"Patient ID: {_orderPatientInfo.PatientID}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Console.WriteLine($"Patient ID: {_orderPatientInfo.PatientID}");
                LoadVaccinesFromJson();
            }
            else
            {
                MessageBox.Show("No patient order information provided.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Default constructor
        public ChiDinh()
        {
            InitializeComponent();
            LoadVaccines();
        }

        private async void LoadVaccines()
        {
            try
            {
                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                var vaccineCollection = await firestoreDb.Collection("vaccines").GetSnapshotAsync();

                var vaccines = new List<Vaccines>();

                foreach (var document in vaccineCollection.Documents)
                {
                    try
                    {
                        var vaccine = document.ConvertTo<Vaccines>();

                        // Convert decimal to int for Price
                        if (vaccine.Price != null && vaccine.Price > 0)
                        {
                            vaccine.Price = (int)vaccine.Price; // Casting decimal to int
                        }

                        vaccines.Add(vaccine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting document to Vaccine: {ex.Message}");
                    }
                }

                vaccineComboBox.ItemsSource = vaccines;
                vaccineComboBox.DisplayMemberPath = "VaccineName";

                if (vaccines.Count == 0)
                {
                    MessageBox.Show("No vaccines found in the Firestore collection.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vaccine data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get selected vaccine
                var selectedVaccine = (Vaccines)vaccineComboBox.SelectedItem;

                if (selectedVaccine == null)
                {
                    MessageBox.Show("Vui lòng chọn vaccine!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate the number of doses
                if (string.IsNullOrWhiteSpace(doseTextBox.Text) || !int.TryParse(doseTextBox.Text, out int dose) || dose <= 0)
                {
                    MessageBox.Show("Số mũi không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Calculate total price based on number of doses and vaccine price
                int totalPrice = dose * selectedVaccine.Price;

                // Prepare the medical record info to be saved
                var medicalRecord = new MedicalRecord
                {
                    PatientID = _orderPatientInfo?.PatientID ?? "Unknown",
                    Name = _orderPatientInfo?.Name ?? "Unknown",
                    Gender = _orderPatientInfo?.Gender ?? "Unknown",
                    Address = _orderPatientInfo?.Address ?? "Unknown",
                    TotalPrice = totalPrice,
                    InvoiceStatus = "waiting",
                    CreatedAt = Timestamp.GetCurrentTimestamp()
                };

                // Firestore reference for saving the MedicalRecord
                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                var medicalRecordRef = firestoreDb.Collection("MedicalRecords").Document(_orderPatientInfo.PatientID);

                // Save the new medical record to Firestore
                try
                {
                    await medicalRecordRef.SetAsync(medicalRecord);
                    Console.WriteLine("Medical Record saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu thông tin chỉ định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update the OrderPatientInfo status to "done"
                var orderPatientInfoRef = firestoreDb.Collection("orderpatientinfo").Document(_orderPatientInfo.OrderID);
                try
                {
                    await orderPatientInfoRef.UpdateAsync(new Dictionary<string, object>
            {
                { "Status", "done" }
            });
                    Console.WriteLine("Order status updated successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Show success message
                MessageBox.Show("Thông tin chỉ định đã được lưu và đơn đã hoàn thành!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Navigate to another page
                _parentControl.NavigateToDanhSachKham();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadVaccinesFromJson()
        {
            try
            {
                string jsonFilePath = @"D://QuanLyTiemChung//QuanLyTiemChung//vaccines.json";

                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show("Không tìm thấy tệp dữ liệu vaccine!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string jsonData = File.ReadAllText(jsonFilePath);
                Console.WriteLine($"Dữ liệu JSON đọc được: {jsonData}");

                var vaccines = JsonConvert.DeserializeObject<List<Vaccines>>(jsonData);

                if (vaccines != null && vaccines.Count > 0)
                {
                    vaccineComboBox.ItemsSource = vaccines;
                    vaccineComboBox.DisplayMemberPath = "VaccineName";
                    Console.WriteLine("Dữ liệu vaccine đã được nạp thành công.");
                }
                else
                {
                    MessageBox.Show("Không có vaccine nào trong tệp dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu từ JSON: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveVaccinesToJson()
        {
            try
            {
                string jsonFilePath = @"D://QuanLyTiemChung//QuanLyTiemChung//vaccines.json";
                string directory = Path.GetDirectoryName(jsonFilePath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Fetch vaccines from Firestore
                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                var vaccineCollection = await firestoreDb.Collection("vaccines").GetSnapshotAsync();

                var vaccines = new List<Vaccines>();
                foreach (var document in vaccineCollection.Documents)
                {
                    try
                    {
                        var vaccine = document.ConvertTo<Vaccines>();
                        vaccines.Add(vaccine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting document to Vaccine: {ex.Message}");
                    }
                }

                // Serialize to JSON
                string json = JsonConvert.SerializeObject(vaccines, Formatting.Indented);
                File.WriteAllText(jsonFilePath, json);

                MessageBox.Show("Dữ liệu đã được lưu vào file JSON!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu vào JSON: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
