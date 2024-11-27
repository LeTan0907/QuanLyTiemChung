using Google.Cloud.Firestore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyTiemChung.MVVM
{
    public partial class OrderView : UserControl
    {
        // ObservableCollection to hold filtered orders
        public ObservableCollection<OrderPatientInfo> FilteredPatients { get; set; } = new ObservableCollection<OrderPatientInfo>();
        private FirestoreDb _firestoreDb;

        public OrderView()
        {
            InitializeComponent();
            DataContext = this;
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
            DatePicker.SelectedDate = DateTime.Now.Date;
            DatePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
            LoadOrdersByDate(DatePicker.SelectedDate.Value);
        }

        // Load orders from Firestore based on the selected date
        private async void LoadOrdersByDate(DateTime selectedDate)
        {
            try
            {
                // Clear the filtered list before loading new data
                FilteredPatients.Clear();

                // Query Firestore to get orders for the selected date
                var snapshot = await _firestoreDb.Collection("orderpatientinfo")
                    .WhereGreaterThanOrEqualTo("CreationTime", Timestamp.FromDateTime(selectedDate.ToUniversalTime())) // Convert to UTC
                    .WhereLessThan("CreationTime", Timestamp.FromDateTime(selectedDate.AddDays(1).ToUniversalTime())) // Convert to UTC
                    .GetSnapshotAsync();

                // If no orders are found for the selected date
                if (snapshot.Documents.Count == 0)
                {
                    MessageBox.Show("No orders found for the selected date.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Add each order to FilteredPatients
                foreach (var document in snapshot.Documents)
                {
                    var order = document.ConvertTo<OrderPatientInfo>();

                    // Prevent adding duplicates to the list
                    if (!FilteredPatients.Any(o => o.OrderID == order.OrderID))
                    {
                        FilteredPatients.Add(order);
                    }
                }

                FilteredPatients = new ObservableCollection<OrderPatientInfo>(
            FilteredPatients.OrderByDescending(o => o.IsPriority)  // True (priority) first
                            .ThenBy(o => o.Number)               // Then sort by Number in ascending order
        );

                Console.WriteLine($"Loaded {FilteredPatients.Count} orders for {selectedDate.ToShortDateString()}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Event handler when the DatePicker value is changed
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePicker.SelectedDate.HasValue)
            {
                LoadOrdersByDate(DatePicker.SelectedDate.Value); // Load orders for the selected date
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Lấy đối tượng order từ nút Delete
            var button = sender as Button;
            var order = button?.DataContext as OrderPatientInfo;

            if (order == null)
            {
                MessageBox.Show("Không tìm thấy thông tin đơn hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Hiển thị hộp thoại xác nhận
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa đơn hàng này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return; // Thoát nếu người dùng chọn "No"
                }

                // Tham chiếu tới tài liệu của đơn hàng trong "orderpatientinfo"
                var orderDocRef = _firestoreDb.Collection("orderpatientinfo").Document(order.OrderID.ToString());

                // Sao chép dữ liệu order sang bộ sưu tập dự phòng "deletedorder"
                await _firestoreDb.Collection("deletedorder").Document(order.OrderID.ToString()).SetAsync(order);

                // Xóa dữ liệu order khỏi bộ sưu tập "orderpatientinfo"
                await orderDocRef.DeleteAsync();

                // Xóa order khỏi ObservableCollection để cập nhật giao diện người dùng
                FilteredPatients.Remove(order);
                LoadOrdersByDate(DatePicker.SelectedDate.Value);
                MessageBox.Show("Đơn hàng đã được xóa và lưu dự phòng thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Vui lòng nhập tên để tìm kiếm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Lọc danh sách bệnh nhân đã tải theo tên
            var filteredOrders = FilteredPatients.Where(order =>
                !string.IsNullOrEmpty(order.Name) && order.Name.ToLower().Contains(searchText)
            ).ToList();

            // Cập nhật lại danh sách FilteredPatients sau khi tìm kiếm
            FilteredPatients.Clear();
            foreach (var order in filteredOrders)
            {
                FilteredPatients.Add(order);
            }

            Console.WriteLine($"Found {filteredOrders.Count} orders matching '{searchText}'.");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadOrdersByDate(DatePicker.SelectedDate.Value);
        }
    }
}
