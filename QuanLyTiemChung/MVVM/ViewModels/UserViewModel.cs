using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
namespace QuanLyTiemChung.MVVM.ViewModels
{


    public class UserViewModel
    {
        public ObservableCollection<Users> Users { get; set; }

        private FirestoreDb _firestoreDb;

        public UserViewModel()
        {
            Users = new ObservableCollection<Users>();
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a"); // Replace with your Firestore project ID
            LoadUsers();
        }

        public async void LoadUsers()
        {
            try
            {
                var usersCollection = _firestoreDb.Collection("users");
                var snapshot = await usersCollection.GetSnapshotAsync();

                Users.Clear();
                foreach (var document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        var user = document.ConvertTo<Users>();
                        Users.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
            }
        }
    }
}

