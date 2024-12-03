using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QuanLyTiemChung.MVVM.Receiptance
{
    public partial class PaymentWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<VaccineDetail> VaccineDetails { get; private set; }
        public MedicalRecord SelectedRecord { get; }
        public decimal AmountPaid { get; set; } // Số tiền khách hàng trả

        // Các thuộc tính tổng tiền, VAT và tổng sau thuế
        public decimal TotalAmount => VaccineDetails?.Sum(v => v.Price * v.Quantity) ?? 0;
        public decimal VAT => TotalAmount * 0.1m; // Giả sử VAT là 10%
        public decimal TotalAmountAfterTax => TotalAmount + VAT;

        public PaymentWindow(MedicalRecord selectedRecord)
        {
            InitializeComponent();
            SelectedRecord = selectedRecord;
            DataContext = this;

            if (SelectedRecord == null)
            {
                MessageBox.Show("Không có hồ sơ bệnh nhân được chọn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _ = LoadVaccineDataAsync();
        }

        private async Task LoadVaccineDataAsync()
        {
            try
            {
                var vaccineDetailsMap = await LoadVaccineDetailsAsync();

                if (vaccineDetailsMap == null || !vaccineDetailsMap.Any())
                {
                    MessageBox.Show("Không tìm thấy thông tin vaccine.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                VaccineDetails = SelectedRecord.VaccineList
                    .Select(vaccine =>
                    {
                        var code = vaccine.Key;
                        var quantity = vaccine.Value;

                        if (vaccineDetailsMap.TryGetValue(code, out var detail))
                        {
                            return new VaccineDetail
                            {
                                VaccineID = code,
                                VaccineName = detail.Name,
                                Price = detail.Price,
                                Quantity = quantity
                            };
                        }

                        return null;
                    })
                    .Where(detail => detail != null)
                    .ToList();

                OnPropertyChanged(nameof(VaccineDetails));
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(VAT));
                OnPropertyChanged(nameof(TotalAmountAfterTax));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu vaccine: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<Dictionary<string, (string Name, decimal Price)>> LoadVaccineDetailsAsync()
        {
            try
            {
                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                var vaccineCollection = firestoreDb.Collection("vaccines");
                var snapshot = await vaccineCollection.GetSnapshotAsync();

                var vaccineDetails = new Dictionary<string, (string Name, decimal Price)>();

                foreach (var document in snapshot.Documents)
                {
                    var vaccine = document.ConvertTo<Vaccines>();

                    vaccineDetails[vaccine.VaccineID] = (vaccine.VaccineName, vaccine.Price);
                }

                return vaccineDetails;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải vaccine: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<string, (string Name, decimal Price)>();
            }
        }

        private async void ConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AmountPaid <= 0)
                {
                    MessageBox.Show("Vui lòng nhập số tiền hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (AmountPaid < TotalAmountAfterTax)
                {
                    MessageBox.Show("Số tiền trả không đủ để thanh toán.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                var recordRef = firestoreDb.Collection("MedicalRecords").Document(SelectedRecord.RecordsID);

                await recordRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "InvoiceStatus", "Paid" },
                    { "AmountPaid", AmountPaid },
                    { "TotalAmount", TotalAmount },
                    { "VAT", VAT },
                    { "TotalAmountAfterTax", TotalAmountAfterTax }
                });

                MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
