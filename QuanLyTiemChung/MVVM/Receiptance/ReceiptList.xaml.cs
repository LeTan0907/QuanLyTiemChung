using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace QuanLyTiemChung.MVVM.Receiptance
{
    public partial class ReceiptList : UserControl
    {
        // ObservableCollection to hold and update the data displayed in the DataGrid
        public ObservableCollection<MedicalRecord> FilteredPatients { get; set; }

        // Full collection of medical records loaded from Firestore
        private ObservableCollection<MedicalRecord> AllMedicalRecords { get; set; }

        private FirestoreDb _firestoreDb;

        public ReceiptList()
        {
            InitializeComponent();

            // Initialize Firestore database client
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a"); // Replace with your actual Firestore project ID

            // Initialize collections
            AllMedicalRecords = new ObservableCollection<MedicalRecord>();
            FilteredPatients = new ObservableCollection<MedicalRecord>();
            DataContext = this;

            // Set today's date in the DatePicker and filter records accordingly
            DatePicker.SelectedDate = DateTime.Today;
            LoadMedicalRecordsAsync(); // Load records on initialization
        }

        private async Task LoadMedicalRecordsAsync()
        {
            try
            {
                // Fetching the MedicalRecords collection from Firestore
                var medicalRecordsCollection = _firestoreDb.Collection("MedicalRecords");
                var snapshot = await medicalRecordsCollection.GetSnapshotAsync();

                // Clear any existing records from the ObservableCollection
                AllMedicalRecords.Clear();

                // Iterate through the documents returned by the query
                foreach (var document in snapshot.Documents)
                {
                    try
                    {
                        // Convert Firestore document to MedicalRecord
                        var medicalRecord = document.ConvertTo<MedicalRecord>();

                        // Safe conversion of TotalPrice to integer (if it's a string in Firestore)
                        if (medicalRecord.TotalPrice != null)
                        {
                            int totalPrice;
                            if (!int.TryParse(medicalRecord.TotalPrice.ToString(), out totalPrice))
                            {
                                // Log and set to default value if conversion fails
                                Console.WriteLine($"Invalid TotalPrice for RecordID {medicalRecord.RecordsID}. Defaulting to 0.");
                                totalPrice = 0; // Set to a default value
                            }
                            medicalRecord.TotalPrice = totalPrice;
                        }

                        // Check if the VaccineList is not null or empty
                        if (medicalRecord.VaccineList == null)
                        {
                            medicalRecord.VaccineList = new Dictionary<string, int>();
                        }
                        else
                        {
                            // Ensure the values in VaccineList are integers
                            foreach (var key in medicalRecord.VaccineList.Keys.ToList())
                            {
                                int vaccineCount;
                                if (!int.TryParse(medicalRecord.VaccineList[key].ToString(), out vaccineCount))
                                {
                                    // Log and set to default value if conversion fails
                                    Console.WriteLine($"Invalid Vaccine Count for Vaccine {key} in RecordID {medicalRecord.RecordsID}. Defaulting to 0.");
                                    medicalRecord.VaccineList[key] = 0; // Default value
                                }
                            }
                        }

                        // Add the medical record to the AllMedicalRecords collection
                        AllMedicalRecords.Add(medicalRecord);
                    }
                    catch (Exception innerEx)
                    {
                        // Log any individual errors with each document
                        Console.WriteLine($"Error processing document {document.Id}: {innerEx.Message}");
                    }
                }

                // Log the loaded data for debugging
                string log = "Loaded Medical Records:\n";
                foreach (var record in AllMedicalRecords)
                {
                    log += $"RecordsID: {record.RecordsID}, PatientID: {record.PatientID}, TotalPrice: {record.TotalPrice}\n";
                }
                Console.WriteLine(log);

                // After loading all records, apply the filter
                FilterRecords();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching medical records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterRecords()
        {
            try
            {
                DateTime? selectedDate = DatePicker.SelectedDate;
                string nameFilter = SearchTextBox.Text?.Trim().ToLower();

                if (selectedDate == null)
                {
                    // Nếu không có ngày được chọn, xóa FilteredPatients
                    FilteredPatients.Clear();
                    return;
                }

                // Lọc các bản ghi theo ngày đã chọn
                var filtered = AllMedicalRecords.Where(record =>
                    record.CreatedAt.ToDateTime().Date == selectedDate.Value.Date);

                if (!string.IsNullOrEmpty(nameFilter))
                {
                    // Lọc thêm theo tên nếu có
                    filtered = filtered.Where(record =>
                        record.Name.ToLower().Contains(nameFilter));
                }

                // Xóa và thêm các bản ghi đã lọc vào FilteredPatients
                FilteredPatients.Clear();
                foreach (var record in filtered)
                {
                    // Sửa trực tiếp trạng thái hóa đơn
                    record.InvoiceStatus = ModifyInvoiceStatus(record.InvoiceStatus);

                    FilteredPatients.Add(record); // Thêm bản ghi vào FilteredPatients
                }

                // Kiểm tra và log các bản ghi đã lọc
                string log = "Filtered Records:\n";
                foreach (var record in FilteredPatients)
                {
                    log += $"RecordsID: {record.RecordsID}, PatientID: {record.PatientID}, TotalPrice: {record.TotalPrice}\n";
                }
                Console.WriteLine(log);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Example of direct modification method for InvoiceStatus
        private string ModifyInvoiceStatus(string invoiceStatus)
        {
            // Apply logic to modify the InvoiceStatus directly
            switch (invoiceStatus.ToLower())
            {
                case "paid":
                    return "Đã thanh toán";  // Example of a direct translation
                case "waiting":
                    return "Chưa thanh toán";
                case "cancelled":
                    return "Đã hủy";
                default:
                    return "Chưa xác định";
            }
        }

        // Event handlers
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRecords(); // Filter the records whenever the selected date changes
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterRecords(); // Filter the records whenever the search text changes
        }

        private void MembersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Kiểm tra bản ghi được chọn
            if (membersDataGrid.SelectedItem is MedicalRecord selectedRecord)
            {
                // Tạo và mở cửa sổ PaymentWindow
                var paymentWindow = new PaymentWindow(selectedRecord);
                paymentWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một bản ghi để thanh toán.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnInteractionButtonClick(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin MedicalRecord tương ứng từ dòng DataGrid
            var button = sender as Button;
            var selectedRecord = (MedicalRecord)((FrameworkElement)button).DataContext;

            if (selectedRecord == null)
            {
                MessageBox.Show("Không thể lấy thông tin hóa đơn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Mở cửa sổ PaymentWindow
            var paymentWindow = new PaymentWindow(selectedRecord);
            paymentWindow.ShowDialog();
        }
    }
}
