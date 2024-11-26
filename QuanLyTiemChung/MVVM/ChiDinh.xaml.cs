using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuanLyTiemChung.MVVM
{
    public partial class ChiDinh : UserControl
    {
        private OrderPatientInfo _orderPatientInfo; 
        private TiepNhanTiem _parentControl;
        private readonly FirestoreDb _firestoreDb;
        public ObservableCollection<Vaccines> vaccinesList { get; set; } = new ObservableCollection<Vaccines>();
        // Constructor to initialize with OrderPatientInfo and parent control
        public ChiDinh(OrderPatientInfo orderPatientInfo, TiepNhanTiem parentControl)
        {
            InitializeComponent();
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
            _parentControl = parentControl;
            _orderPatientInfo = orderPatientInfo; // Use OrderPatientInfo to get all patient details

            if (_orderPatientInfo != null)
            {
                MessageBox.Show($"Patient ID: {_orderPatientInfo.PatientID}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Console.WriteLine($"Patient ID: {_orderPatientInfo.PatientID}");
                LoadVaccinesAsync();
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
            LoadVaccinesAsync();
        }

        private async Task LoadVaccinesAsync()
        {
            try
            {
                var vaccinesCollection = _firestoreDb.Collection("vaccines");
                var snapshot = await vaccinesCollection.GetSnapshotAsync();

                vaccinesList.Clear();
                foreach (var document in snapshot.Documents)
                {
                    var vaccine = document.ConvertTo<Vaccines>();
                    vaccinesList.Add(vaccine);
                }
                string log = "Vaccines List Content:\n";
                foreach (var vaccine in vaccinesList)
                {
                    log += $"ID: {vaccine.VaccineID}, Name: {vaccine.VaccineName}\n";
                }
                MessageBox.Show(log, "Vaccine List");
                vaccineComboBox.ItemsSource = vaccinesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching vaccines: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnVaccineSelected(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra ComboBox đã có lựa chọn chưa
            if (vaccineComboBox.SelectedItem != null)
            {
                // Lấy vaccine đã chọn từ ComboBox
                Vaccines selectedVaccine = vaccineComboBox.SelectedItem as Vaccines;

                // Kiểm tra nếu vaccine có tồn kho > 0, nếu có thì hiển thị "Còn hàng", nếu không thì "Hết hàng"
                if (selectedVaccine != null)
                {
                    // Cập nhật giá trị cho dose (số mũi) mặc định hoặc tùy thuộc vào vaccine
                    doseTextBox.Text = selectedVaccine.Dosage; // Hoặc bạn có thể điều chỉnh giá trị này tùy theo vaccine

                    // Hiển thị trạng thái còn hàng hay hết hàng
                    string stockStatus = selectedVaccine.InStock > 0 ? "Còn hàng" : "Hết hàng";
                    stockStatusTextBlock.Text = stockStatus;  // stockStatusTextBlock là TextBlock hiển thị trạng thái
                }
            }
        }

        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedVaccine = (Vaccines)vaccineComboBox.SelectedItem;

                if (selectedVaccine == null)
                {
                    MessageBox.Show("Vui lòng chọn vaccine!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Get quantity value
                int quantity = string.IsNullOrWhiteSpace(quantityTextBox.Text) || !int.TryParse(quantityTextBox.Text, out quantity) ? 1 : quantity;

                // Calculate the price of the selected vaccine (assuming one vaccine per dose)
                int totalVaccinePrice = quantity * selectedVaccine.Price;

                // Prepare the medical record info to be saved
                var medicalRecord = new MedicalRecord
                {
                    PatientID = _orderPatientInfo?.PatientID ?? "Unknown",
                    Name = _orderPatientInfo?.Name ?? "Unknown",
                    Gender = _orderPatientInfo?.Gender ?? "Unknown",
                    Address = _orderPatientInfo?.Address ?? "Unknown",
                    TotalPrice = totalVaccinePrice, // Calculating the total price here
                    InvoiceStatus = "waiting",
                    CreatedAt = Timestamp.GetCurrentTimestamp(),
                    VaccineList = new List<string> { selectedVaccine.VaccineID } // Add the selected vaccine ID to the list
                };

                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");

                try
                {
                    var medicalRecordRef = firestoreDb.Collection("MedicalRecords").Document(_orderPatientInfo.PatientID);
                    await medicalRecordRef.SetAsync(medicalRecord); // Save or overwrite the document
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

        private void AddVaccineRow_Click(object sender, RoutedEventArgs e)
        {
            // Tạo một Grid mới để chứa các điều khiển cho vaccine mới
            Grid newRow = new Grid();
            newRow.Margin = new Thickness(0, 10, 0, 0);

            // Định nghĩa các cột trong Grid
            newRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            newRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            newRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            newRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            newRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

            // Tạo ComboBox cho Tên Vaccine
            ComboBox vaccineComboBox = new ComboBox();
            vaccineComboBox.Margin = new Thickness(5);
            vaccineComboBox.Height = 30;
            vaccineComboBox.ItemsSource = vaccinesList; // vaccinesList chứa dữ liệu vaccine
            vaccineComboBox.SelectedValuePath = "VaccineName";
            vaccineComboBox.DisplayMemberPath = "VaccineName";
            vaccineComboBox.SelectionChanged += OnVaccineSelected;
            Grid.SetColumn(vaccineComboBox, 0);

            // Tạo TextBox cho Liều lượng (sẽ được cập nhật khi chọn Vaccine)
            TextBox doseTextBox = new TextBox();
            doseTextBox.Margin = new Thickness(5);
            doseTextBox.Height = 30;
            doseTextBox.IsReadOnly = true; // Read-only để tự động cập nhật từ vaccine
            Grid.SetColumn(doseTextBox, 1);

            // Tạo TextBox cho Số lượng
            TextBox quantityTextBox = new TextBox();
            quantityTextBox.Margin = new Thickness(5);
            quantityTextBox.Height = 30;
            quantityTextBox.Text = "1"; // Giá trị mặc định
            Grid.SetColumn(quantityTextBox, 2);

            // Tạo TextBlock cho Tình trạng (Sẽ được thay đổi khi vaccine thay đổi)
            TextBlock stockStatusTextBlock = new TextBlock();
            stockStatusTextBlock.Margin = new Thickness(5);
            stockStatusTextBlock.Height = 30;
            stockStatusTextBlock.FontWeight = FontWeights.Bold;
            stockStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green); // Mặc định màu xanh
            Grid.SetColumn(stockStatusTextBlock, 3);

            // Tạo Button Xóa
            Button deleteButton = new Button();
            deleteButton.Width = 30;
            deleteButton.Height = 30;
            deleteButton.Margin = new Thickness(5);
            deleteButton.Background = new SolidColorBrush(Color.FromRgb(235, 64, 52));
            deleteButton.Click += (s, args) =>
            {
                vaccinesStackPanel.Children.Remove(newRow); // Xóa dòng khi click
            };
            Grid.SetColumn(deleteButton, 4);

            // Thêm các điều khiển vào dòng mới
            newRow.Children.Add(vaccineComboBox);
            newRow.Children.Add(doseTextBox);
            newRow.Children.Add(quantityTextBox);
            newRow.Children.Add(stockStatusTextBlock);
            newRow.Children.Add(deleteButton);

            // Thêm dòng mới vào StackPanel
            vaccinesStackPanel.Children.Add(newRow);
        }
    }
}
