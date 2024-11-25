using Google.Cloud.Firestore;
using QuanLyTiemChung.Class;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyTiemChung.MVVM
{
    public partial class KhamSoBo : UserControl
    {
        private OrderPatientInfo _orderPatientInfo; // Use OrderPatientInfo
        private TiepNhanTiem _parentControl;

        // Constructor that accepts OrderPatientInfo and TiepNhanTiem
        public KhamSoBo(OrderPatientInfo orderPatientInfo, TiepNhanTiem parentControl)
        {
            InitializeComponent();
            _orderPatientInfo = orderPatientInfo; // Initialize the OrderPatientInfo
            _parentControl = parentControl;
        }

        // Default constructor (if needed)
        public KhamSoBo()
        {
            InitializeComponent();
        }

        // Save button click handler
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_orderPatientInfo == null)
                {
                    MessageBox.Show("Patient data is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
                var patientRef = firestoreDb.Collection("patients").Document(_orderPatientInfo.PatientID);

                if (string.IsNullOrWhiteSpace(_orderPatientInfo.PatientID))
                {
                    MessageBox.Show("Patient ID is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Assuming OrderPatientInfo has the Patient data
                var patient = new Patient
                {
                    PatientID = _orderPatientInfo.PatientID,
                    Name = _orderPatientInfo.Name,
                    Gender = _orderPatientInfo.Gender,
                    Address = _orderPatientInfo.Address
                    // Map other necessary fields from OrderPatientInfo to Patient
                };

                // Save patient data to Firestore
                await patientRef.SetAsync(patient);
                MessageBox.Show("Patient data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Pass the OrderPatientInfo object to the next screen (Chỉ định)
                if (_parentControl != null)
                {
                    _parentControl.ChangeContentToChiDinh(_orderPatientInfo); // Pass OrderPatientInfo
                }
                else
                {
                    MessageBox.Show("Parent control is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
