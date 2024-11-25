using QuanLyTiemChung.MVVM.ViewModels;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MVVMEssentials.ViewModels;
using GalaSoft.MvvmLight.Command;
using System.Windows;

namespace QuanLyTiemChung.MVVM.ViewModels
{
    internal class OrderViewModel : ViewModelBase
    {
        private FirestoreDb _firestoreDb;
        private ObservableCollection<OrderPatientInfo> _orderPatientInfos;
        public ObservableCollection<OrderPatientInfo> Orders { get; set; } = new ObservableCollection<OrderPatientInfo>();
        public ObservableCollection<Order> TodayPatients { get; set; } = new ObservableCollection<Order>();
        public ObservableCollection<OrderPatientInfo> OrderPatientInfos
        {
            get => _orderPatientInfos;
            set
            {
                _orderPatientInfos = value;
                OnPropertyChanged(nameof(OrderPatientInfos));
            }
        }


        public OrderViewModel()
        {
            // Initialize Firestore
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\cloudfire.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");

            // Load data
            _ = LoadTodayOrdersAsync();
        }

        public async Task LoadOrdersAsync()
        {
            try
            {
                // Clear existing orders
                Orders.Clear();

                // Fetch orders from Firestore
                Query allOrdersQuery = _firestoreDb.Collection("orderpatientinfo");
                QuerySnapshot snapshot = await allOrdersQuery.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var order = document.ConvertTo<OrderPatientInfo>();
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                
            }
        }
        private (Timestamp startOfDay, Timestamp endOfDay) GetYesterdayDateRange()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var startOfDay = Timestamp.FromDateTime(yesterday);
            var endOfDay = Timestamp.FromDateTime(yesterday.AddDays(1));

            return (startOfDay, endOfDay);
        }

        private async Task LoadTodayOrdersAsync()
        {
            try
            {
                TodayPatients.Clear(); // Clear any existing data

                // Get today's date range
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                // Query Firestore for orders created today
                Query query = _firestoreDb.Collection("orders")
                    .WhereGreaterThanOrEqualTo("orderDate", Timestamp.FromDateTime(today.ToUniversalTime()))
                    .WhereLessThan("orderDate", Timestamp.FromDateTime(tomorrow.ToUniversalTime()));

                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var order = document.ConvertTo<Order>();
                    TodayPatients.Add(order);
                }
            }
            catch (Exception ex)
            {
                // Handle errors
                Console.WriteLine($"Error loading orders: {ex.Message}");
            }
        }


        private async Task<Patient> GetPatientInfoAsync(string patientID)
        {
            var patientRef = _firestoreDb.Collection("patients").Document(patientID);
            var patientSnapshot = await patientRef.GetSnapshotAsync();

            if (patientSnapshot.Exists)
            {
                return patientSnapshot.ConvertTo<Patient>();
            }

            return null;
        }

        private void UpdateOrderPatientInfos(List<OrderPatientInfo> orderPatientInfosList)
        {
            OrderPatientInfos.Clear();
            foreach (var orderPatientInfo in orderPatientInfosList)
            {
                OrderPatientInfos.Add(orderPatientInfo);
            }
        }
       

    }
}
