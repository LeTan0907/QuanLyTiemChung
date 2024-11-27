using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
                    NgaySinhTextBox.Text = patient.DOB.ToDateTime().ToLocalTime().ToString("dd/MM/yyyy");
                    GioiTinhComboBox.Text = patient.Gender;
                    DienThoaiTextBox.Text = patient.PhoneNumber;
                    DiaChiTextBox.Text = patient.Address;
                    GhiChuTextBox.Text = patient.Notes;

                    // Calculate age based on the patient's date of birth
                    var age = CalculateAge(patient.DOB.ToDateTime());
                    TuoiTextBox.Text = $"{age.Years} tuổi {age.Months} tháng {age.Days} ngày"; // Update TuoiTextBox with calculated age
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


        private async Task<int> GetTodayOrderCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Ensure correct query format
            Query query = _firestoreDb.Collection("orders")
                .WhereGreaterThanOrEqualTo("CreationTime", Timestamp.FromDateTime(today)) // Starting from midnight today
                .WhereLessThan("CreationTime", Timestamp.FromDateTime(tomorrow)); // Up to midnight of the next day

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

        private void NgaySinhTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Check if the entered date is valid
                if (DateTime.TryParse(NgaySinhTextBox.Text, out DateTime birthDate))
                {
                    // Calculate the age in years, months, and days
                    var age = CalculateAge(birthDate);

                    // Set the calculated age in TuoiTextBox in the desired format
                    TuoiTextBox.Text = $"{age.Years} tuổi {age.Months} tháng {age.Days} ngày";
                }
                else
                {
                    // If the date is invalid, clear the age field
                    TuoiTextBox.Clear();
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that may occur
                MessageBox.Show($"Error calculating age: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (int Years, int Months, int Days) CalculateAge(DateTime birthDate)
        {
            // Get today's date
            DateTime today = DateTime.Today;

            // Calculate the full years between birth date and today
            int years = today.Year - birthDate.Year;

            // Adjust the years if the birthday hasn't occurred yet this year
            if (today.Month < birthDate.Month || (today.Month == birthDate.Month && today.Day < birthDate.Day))
            {
                years--;
            }

            // Calculate the months
            int months = today.Month - birthDate.Month;
            if (months < 0)
            {
                months += 12; // Add 12 months if negative
            }

            // Calculate the days
            int days = today.Day - birthDate.Day;
            if (days < 0)
            {
                // If the day is negative, adjust the day and subtract a month
                months--;
                days += DateTime.DaysInMonth(today.Year, today.Month);
            }

            // Return years, months, and days as a tuple
            return (years, months, days);
        }

        
    }
}
