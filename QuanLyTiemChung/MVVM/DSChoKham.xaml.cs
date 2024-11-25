using Google.Cloud.Firestore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyTiemChung.MVVM
{
    public partial class DSChoKham : UserControl
    {
        public event EventHandler ProceedButtonClicked;
        public ObservableCollection<OrderPatientInfo> FilteredPatients { get; set; } = new ObservableCollection<OrderPatientInfo>();
        private readonly FirestoreDb _firestoreDb;
        private readonly TiepNhanTiem _parentControl;

        public DSChoKham(TiepNhanTiem parentControl)
        {
            InitializeComponent();
            DataContext = this;
            _parentControl = parentControl; // Store the reference of the parent control
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
            LoadPendingOrdersForToday();
        }
        public DSChoKham()
        {
            InitializeComponent();
            DataContext = this;
           
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
            LoadPendingOrdersForToday();
        }

        private async void LoadPendingOrdersForToday()
        {
            try
            {
                // Clear the current list
                FilteredPatients.Clear();

                var today = DateTime.Now.Date;
                var todayStart = Timestamp.FromDateTime(today.ToUniversalTime());
                var tomorrowStart = Timestamp.FromDateTime(today.AddDays(1).ToUniversalTime());

                var snapshot = await _firestoreDb.Collection("orderpatientinfo")
                                                 .WhereGreaterThanOrEqualTo("CreationTime", todayStart)
                                                 .WhereLessThan("CreationTime", tomorrowStart)
                                                 .WhereEqualTo("Status", "Pending")
                                                 .GetSnapshotAsync();

                foreach (var document in snapshot.Documents)
                {
                    var order = document.ConvertTo<OrderPatientInfo>();

                    // Avoid duplicate entries
                    if (!FilteredPatients.Any(o => o.OrderID == order.OrderID))
                    {
                        FilteredPatients.Add(order);
                    }
                }
                FilteredPatients = new ObservableCollection<OrderPatientInfo>(FilteredPatients.OrderBy(o => o.Number));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            var filteredOrders = FilteredPatients.Where(order =>
                order.PatientID.ToLower().Contains(searchText) ||
                order.Name.ToLower().Contains(searchText)
            ).ToList();

            FilteredPatients.Clear();
            foreach (var order in filteredOrders)
            {
                FilteredPatients.Add(order);
            }
        }

        private async void ProceedButton_Click(object sender, RoutedEventArgs e)
        {
            var orderPatientInfo = ((Button)sender).CommandParameter as OrderPatientInfo;

            // Check if the CommandParameter is null
            if (orderPatientInfo == null)
            {
                MessageBox.Show("No patient selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if OrderID is valid
            if (string.IsNullOrWhiteSpace(orderPatientInfo.OrderID))
            {
                MessageBox.Show("Invalid OrderID!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Proceeding with OrderID: {orderPatientInfo.OrderID}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                // Instead of fetching the patient by PatientID, you can proceed with OrderPatientInfo.
                // The OrderPatientInfo contains all the necessary information about the patient.

                if (_parentControl != null)
                {
                    _parentControl.ChangeContentToKhamSoBo(orderPatientInfo); // Pass the entire OrderPatientInfo object
                }
                else
                {
                    MessageBox.Show("Parent control is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error proceeding with order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text=string.Empty;
            LoadPendingOrdersForToday();
        }
    }
}
