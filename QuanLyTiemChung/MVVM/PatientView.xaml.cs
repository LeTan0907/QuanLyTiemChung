using Google.Cloud.Firestore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyTiemChung.MVVM
{
    public partial class PatientView : UserControl
    {
        public ObservableCollection<Patient> Patients { get; set; } = new ObservableCollection<Patient>();
        private FirestoreDb _firestoreDb;

        public PatientView()
        {
            InitializeComponent();
            DataContext = this; 
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a"); 
            LoadPatientsFromFirestore(); 
        }

        private async void LoadPatientsFromFirestore()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("patients").GetSnapshotAsync();

                // Clear existing data
                Patients.Clear();

                // Loop through the fetched data and add it to the ObservableCollection
                foreach (var document in snapshot.Documents)
                {
                    var patient = document.ConvertTo<Patient>();
                    Patients.Add(patient);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Event to view patient details
        private void ViewPatientDetails(object sender, RoutedEventArgs e)
        {
            var selectedPatient = (Patient)patientsDataGrid.SelectedItem;
            if (selectedPatient != null)
            {
                PatientDetailView detailView = new PatientDetailView(selectedPatient);
                detailView.ShowDialog(); // Show patient details in a new window
            }
        }

        // Event to edit patient details
        private void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            var selectedPatient = (Patient)patientsDataGrid.SelectedItem;
            if (selectedPatient != null)
            {
                PatientEditView editView = new PatientEditView(selectedPatient, true); // true indicates edit mode
                editView.ShowDialog();

                // After editing, reload the patients list
                LoadPatientsFromFirestore();
            }
        }

        // Event to delete a patient
        private async void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            var selectedPatient = (Patient)patientsDataGrid.SelectedItem;
            if (selectedPatient != null)
            {
                var result = MessageBox.Show("Bạn chắc chắn muốn xóa thông tin bệnh nhân này?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Tạo đối tượng DeletedPatient
                        var deletedPatient = new DeletedPatient
                        {
                            PatientID = selectedPatient.PatientID,
                            Name = selectedPatient.Name,
                            DOB = selectedPatient.DOB,
                            Gender = selectedPatient.Gender,
                            PhoneNumber = selectedPatient.PhoneNumber,
                            Address = selectedPatient.Address,
                            IDNumber = selectedPatient.IDNumber,
                            PriorityGroup = selectedPatient.PriorityGroup,
                            Notes = selectedPatient.Notes
                        };

                        // Lưu vào bộ sưu tập "deletedpatient"
                        var deletedDocRef = _firestoreDb.Collection("deletedpatient").Document(selectedPatient.PatientID);
                        await deletedDocRef.SetAsync(deletedPatient);

                        // Xóa bệnh nhân khỏi bộ sưu tập "patients"
                        var docRef = _firestoreDb.Collection("patients").Document(selectedPatient.PatientID);
                        await docRef.DeleteAsync();

                        MessageBox.Show("Đã xóa thông tin bệnh nhân thành công và lưu vào cơ sở dữ liệu dự phòng", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Tải lại danh sách bệnh nhân
                        LoadPatientsFromFirestore();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim().ToLower(); // Get the text from the search box and trim it

            if (!string.IsNullOrEmpty(searchText))
            {
                try
                {
                    // Perform a Firestore query to search for patients by name
                    var snapshot = await _firestoreDb.Collection("patients")
                                                     .WhereGreaterThanOrEqualTo("Name", searchText)
                                                     .WhereLessThanOrEqualTo("Name", searchText + "\uf8ff") // Unicode trick to match the full name
                                                     .GetSnapshotAsync();

                    // Clear the existing patients list
                    Patients.Clear();

                    // Loop through the fetched data and add it to the ObservableCollection
                    foreach (var document in snapshot.Documents)
                    {
                        var patient = document.ConvertTo<Patient>();
                        Patients.Add(patient);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // If the search box is empty, reload all patients
                LoadPatientsFromFirestore();
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text=string.Empty;
            LoadPatientsFromFirestore();
        }
    }
}
