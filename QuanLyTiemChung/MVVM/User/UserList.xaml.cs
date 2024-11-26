using Firebase.Auth;
using Google.Cloud.Firestore;
using QuanLyTiemChung.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyTiemChung.MVVM.User
{
    /// <summary>
    /// Interaction logic for UserList.xaml
    /// </summary>
    public partial class UserList : UserControl
    {
        private FirestoreDb firestoreDb;

        public ObservableCollection<Users> Users { get; set; }
        public UserList()
        {
            InitializeComponent();
            firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a"); // Replace with your Firestore project ID
            Users = new ObservableCollection<Users>();
            this.DataContext = this; // Set DataContext to this class
            LoadUsersAsync();
        }
        private async Task LoadUsersAsync()
        {
            var usersCollection = firestoreDb.Collection("Users");
            var snapshot = await usersCollection.GetSnapshotAsync();

            // Clear the collection before adding new data
            Users.Clear();

            // Add each user from Firestore to the ObservableCollection
            foreach (var document in snapshot.Documents)
            {
                var user = document.ConvertTo<Users>();
                Users.Add(user);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected user
            var selectedUser = patientsDataGrid.SelectedItem as Users; // Adjust this based on your user model

            if (selectedUser != null)
            {
                // Create and open the UpdateUser window
                var updateWindow = new UpdateUser(selectedUser);

                // Pass the selected user to the UpdateUser window (You can either use a constructor or a method for this)
                

                // Show the UpdateUser window
                updateWindow.ShowDialog();
                LoadUsersAsync();
            }
            else
            {
                MessageBox.Show("Please select a user to edit.");
            }
        }

    }
}
