using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QuanLyTiemChung.MVVM
{
    public partial class FormDangKy : Window
    {
        private FirestoreDb _firestoreDb;
        public string PatientID { get; set; } // Use PatientID as the unique identifier for the patient

        public FormDangKy(string patientId)
        {
            InitializeComponent();
            PatientID = patientId; // Use PatientID as the unique identifier

            try
            {
                // Set Firebase credentials path
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\quanlytiemchung-f225a-firebase-adminsdk-ahz93-4982e9abc8.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

                // Initialize FirestoreDb
                _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                MessageBox.Show("Database Connected.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Load patient data and queue number
                LoadPatientData();
                LoadQueueNumberForToday();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối với Firestore: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load patient data from Firestore using PatientID
        private async void LoadPatientData()
        {
            try
            {
                // Ensure the PatientID is not null or empty
                if (string.IsNullOrEmpty(PatientID))
                {
                    MessageBox.Show("Mã bệnh nhân không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Retrieve patient data from Firestore using PatientID
                DocumentReference patientRef = _firestoreDb.Collection("patients").Document(PatientID); // Use PatientID as the document ID
                DocumentSnapshot snapshot = await patientRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    // Bind patient data to the UI
                    Patient patient = snapshot.ConvertTo<Patient>();
                    HoTenTextBox.Text = patient.Name;
                    // When loading and displaying the patient's DOB
                    NgaySinhTextBox.Text = patient.DOB.ToDateTime().ToLocalTime().ToString("dd/MM/yyyy");

                    GioiTinhComboBox.Text = patient.Gender;
                    DienThoaiTextBox.Text = patient.PhoneNumber;
                    DiaChiTextBox.Text = patient.Address;
                    GhiChuTextBox.Text = patient.Notes;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin bệnh nhân với Mã bệnh nhân: " + PatientID, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin bệnh nhân: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Get today's order count from Firestore
        private async Task<int> GetTodayOrderCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            Query query = _firestoreDb.Collection("orders")
                .WhereGreaterThanOrEqualTo("CreationTime", Timestamp.FromDateTime(today))
                .WhereLessThan("CreationTime", Timestamp.FromDateTime(tomorrow));

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Count;
        }

        // Load queue number for today
        private async void LoadQueueNumberForToday()
        {
            try
            {
                int todayOrderCount = await GetTodayOrderCountAsync();
                int nextQueueNumber = todayOrderCount + 1;
                SoThuTuTextBox.Text = nextQueueNumber.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading queue number: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save order data and associate it with patient
        private async void LuuButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create and save order data
                var order = new Order
                {
                    OrderID = Guid.NewGuid().ToString(),
                    PatientID = PatientID, // Use PatientID as the PatientID
                    Status = "Pending",
                    CreationTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                var orderRef = _firestoreDb.Collection("orders").Document(order.OrderID);
                await orderRef.SetAsync(order);

                // Fetch patient data from Firestore using PatientID
                var patientRef = _firestoreDb.Collection("patients").Document(PatientID); // Use PatientID as the document ID
                var patientSnapshot = await patientRef.GetSnapshotAsync();

                if (patientSnapshot.Exists)
                {
                    var patient = patientSnapshot.ConvertTo<Patient>();

                    // Determine if the patient is in a priority group
                    bool isPriority = DoTuongUuTienCheckBox.IsChecked == true; // Correct way to check checkbox

                    // Create and save orderpatientinfo data
                    var orderPatientInfo = new OrderPatientInfo
                    {
                        OrderID = order.OrderID,
                        PatientID = PatientID, // Use PatientID as the PatientID
                        Name = patient.Name,
                        IDNumber = patient.IDNumber,
                        Phone = patient.PhoneNumber,
                        Gender = patient.Gender,
                        Address = patient.Address,
                        Status = "Pending",
                        Number = int.Parse(SoThuTuTextBox.Text),
                        CreationTime = Timestamp.FromDateTime(DateTime.UtcNow),
                        IsPriority = isPriority // Correctly set the IsPriority value
                    };

                    var orderPatientInfoRef = _firestoreDb.Collection("orderpatientinfo").Document(order.OrderID);
                    await orderPatientInfoRef.SetAsync(orderPatientInfo);

                    MessageBox.Show("Đăng kí thành công!\n" +
                                    $"Họ tên: {patient.Name}\n" +
                                    $"Số điện thoại: {patient.PhoneNumber}\n" +
                                    $"Địa chỉ: {patient.Address}\n" +
                                    $"Giới tính: {patient.Gender}\n" +
                                    $"Số thứ tự: {orderPatientInfo.Number}",
                                    "Thông báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin bệnh nhân với Mã bệnh nhân: " + PatientID, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Close(); // Close the form after saving data
            }
        }


        // Cancel registration
        private void HuyDangKy(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
