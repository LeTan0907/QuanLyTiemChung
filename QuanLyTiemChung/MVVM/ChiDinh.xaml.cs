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
                vaccineComboBox1.ItemsSource = vaccinesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching vaccines: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnVaccineSelected(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                // Kiểm tra vaccine đã chọn từ ComboBox
                Vaccines selectedVaccine = comboBox.SelectedItem as Vaccines;

                if (selectedVaccine != null)
                {
                    // Cập nhật liều lượng (dose) cho TextBox tương ứng
                    if (comboBox == vaccineComboBox)
                    {
                        doseTextBox.Text = selectedVaccine.Dosage; // Liều lượng cho vaccineComboBox
                        stockStatusTextBlock.Text = selectedVaccine.InStock > 0 ? "Còn hàng" : "Hết hàng";
                    }
                    else if (comboBox == vaccineComboBox1)
                    {
                        doseTextBox1.Text = selectedVaccine.Dosage; // Liều lượng cho vaccineComboBox1
                        stockStatusTextBlock1.Text = selectedVaccine.InStock > 0 ? "Còn hàng" : "Hết hàng";
                    }
                }
            }
        }
        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedVaccines = new List<(Vaccines vaccine, int quantity)>();

                // Kiểm tra vaccine đầu tiên (vaccineComboBox)
                var selectedVaccine1 = vaccineComboBox.SelectedItem as Vaccines;
                if (selectedVaccine1 != null)
                {
                    int quantity1 = string.IsNullOrWhiteSpace(quantityTextBox.Text) || !int.TryParse(quantityTextBox.Text, out quantity1) ? 1 : quantity1;
                    selectedVaccines.Add((selectedVaccine1, quantity1));
                }

                // Kiểm tra vaccine thứ hai (vaccineComboBox1)
                var selectedVaccine2 = vaccineComboBox1.SelectedItem as Vaccines;
                if (selectedVaccine2 != null)
                {
                    int quantity2 = string.IsNullOrWhiteSpace(quantityTextBox1.Text) || !int.TryParse(quantityTextBox1.Text, out quantity2) ? 1 : quantity2;
                    selectedVaccines.Add((selectedVaccine2, quantity2));
                }

                if (selectedVaccines.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một loại vaccine!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Tính tổng giá tiền và kiểm tra số lượng vaccine có đủ hay không
                int totalVaccinePrice = 0;
                var vaccineMap = new Dictionary<string, int>();

                foreach (var (vaccine, quantity) in selectedVaccines)
                {
                    totalVaccinePrice += vaccine.Price * quantity;
                    vaccineMap[vaccine.VaccineID] = quantity;

                    // Kiểm tra số lượng vaccine có trong kho
                    var vaccineRef = _firestoreDb.Collection("vaccines").Document(vaccine.VaccineID);
                    var vaccineDoc = await vaccineRef.GetSnapshotAsync();

                    if (!vaccineDoc.Exists)
                    {
                        MessageBox.Show("Vaccine không tồn tại trong kho!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var vaccineInStock = vaccineDoc.ConvertTo<Vaccines>();

                    // Kiểm tra số lượng vaccine có đủ hay không
                    if (vaccineInStock.InStock < quantity)
                    {
                        MessageBox.Show($"Số lượng vaccine {vaccine.VaccineName} không đủ trong kho. Còn lại {vaccineInStock.InStock}!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Giảm số lượng vaccine trong kho
                    int newStock = vaccineInStock.InStock - quantity;
                    await vaccineRef.UpdateAsync("InStock", newStock);
                }

                // Tạo RecordsID mới theo ngày tháng + số thứ tự
                string newRecordsID = await GenerateRecordsIDAsync();

                // Chuẩn bị thông tin MedicalRecord để lưu
                var medicalRecord = new MedicalRecord
                {
                    RecordsID = newRecordsID,
                    PatientID = _orderPatientInfo?.PatientID ?? "Unknown",
                    Name = _orderPatientInfo?.Name ?? "Unknown",
                    Gender = _orderPatientInfo?.Gender ?? "Unknown",
                    Address = _orderPatientInfo?.Address ?? "Unknown",
                    TotalPrice = totalVaccinePrice,
                    InvoiceStatus = "waiting",
                    CreatedAt = Timestamp.GetCurrentTimestamp(),
                    VaccineList = vaccineMap // Lưu Map vaccine với số lượng
                };

                // Lưu MedicalRecord vào Firestore
                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");

                try
                {
                    var medicalRecordRef = firestoreDb.Collection("MedicalRecords").Document(newRecordsID);
                    await medicalRecordRef.SetAsync(medicalRecord);
                    Console.WriteLine("Medical Record saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu thông tin chỉ định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Cập nhật trạng thái đơn hàng
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

                // Hiển thị thông báo thành công
                MessageBox.Show("Thông tin chỉ định đã được lưu và đơn đã hoàn thành!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Điều hướng đến trang khác
                _parentControl.Content = new TiepNhanTiem();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> GenerateRecordsIDAsync()
        {
            try
            {
                // Lấy ngày hiện tại (yyyyMMdd)
                string currentDate = DateTime.Now.ToString("yyyyMMdd");

                // Truy vấn Firestore để lấy tất cả các MedicalRecord trong ngày hôm nay
                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                var medicalRecordsRef = firestoreDb.Collection("MedicalRecords");
                var snapshot = await medicalRecordsRef.WhereGreaterThanOrEqualTo("RecordsID", currentDate).GetSnapshotAsync();

                // Đếm số bản ghi hiện có trong ngày
                int recordCount = snapshot.Documents.Count;

                // Tạo số thứ tự (STT) tiếp theo (STT bắt đầu từ 001)
                string recordNumber = (recordCount + 1).ToString("D3");  // Format "001", "002", ...

                // Tạo ID theo dạng: yyyyMMdd-STT (ví dụ: 20241127-001)
                string newRecordsID = $"{currentDate}-{recordNumber}";

                return newRecordsID;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo RecordsID: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        private void AddVaccineRow_Click(object sender, RoutedEventArgs e)
        {
            vaccineRowTemplate1.Visibility = (vaccineRowTemplate1.Visibility == Visibility.Collapsed)
                                    ? Visibility.Visible
                                    : Visibility.Collapsed;
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Tìm StackPanel cha chứa các hàng vaccine
            var button = sender as Button;
            if (button != null)
            {
                // Tìm StackPanel hoặc Grid chứa hàng này
                var gridRow = button.Parent as Grid;
                if (gridRow != null)
                {
                    // Tìm StackPanel hoặc Grid của một hàng cụ thể trong GridRow
                    var stackPanel = gridRow.Parent as StackPanel;
                    if (stackPanel != null)
                    {
                        // Ẩn hàng
                        stackPanel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
