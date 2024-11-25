using Firebase.Database;
using QuanLyTiemChung.MVVM.ViewModels;
using Firebase.Database.Query;
using Google.Cloud.Firestore;
using MVVMEssentials.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
namespace QuanLyTiemChung.MVVM.ViewModels
{
    public class PatientViewModel : ViewModelBase
    {
        private readonly FirestoreDb _firestoreDb;
        
        public ObservableCollection<Patient> TodayPatients { get; set; }
        public PatientViewModel()
        {
            _firestoreDb = FirestoreDb.Create("quanlytiemchung-f225a");
           
            TodayPatients = new ObservableCollection<Patient>();
            
        }

    }
}
