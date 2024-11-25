using Google.Cloud.Firestore;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyTiemChung.MVVM
{
    public partial class Vaccine_Storage : UserControl
    {
        private FirestoreDb _firestoreDb;
        public ObservableCollection<Vaccines> Vaccines { get; set; }

        public Vaccine_Storage()
        {
            InitializeComponent();
            DataContext = this;
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
            Vaccines = new ObservableCollection<Vaccines>();
            LoadVaccinesFromFirestore();
        }

        private async void LoadVaccinesFromFirestore()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("vaccines").GetSnapshotAsync();

                // Clear existing data
                Vaccines.Clear();

                // Loop through the fetched data and add it to the ObservableCollection
                foreach (var document in snapshot.Documents)
                {
                    var vaccine = document.ConvertTo<Vaccines>();
                    Vaccines.Add(vaccine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vaccines: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Search Button Click Event Handler
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement your search logic here
        }

        // DatePicker Selected Date Changed Event Handler
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implement your date filter logic here
        }
    }
}
