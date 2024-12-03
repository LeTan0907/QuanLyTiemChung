using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyTiemChung.MVVM.ViewModels
{
    public class MedicalRecordViewModel
    {
        public ObservableCollection<MedicalRecord> MedicalRecords { get; set; }

        public MedicalRecordViewModel()
        {
            MedicalRecords = new ObservableCollection<MedicalRecord>();
            LoadDataFromFirestore();
        }

        private async void LoadDataFromFirestore()
        {
            FirestoreDb db = FirestoreDb.Create("quanlytiemchung-f225a");
            CollectionReference colRef = db.Collection("MedicalRecords");
            QuerySnapshot snapshot = await colRef.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    var record = doc.ConvertTo<MedicalRecord>();
                    MedicalRecords.Add(record);
                }
            }
        }
    }

}
