using Firebase.Database;
using QuanLyTiemChung.MVVM.ViewModels;
using Firebase.Database.Query;
using Google.Cloud.Firestore;
using MVVMEssentials.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
namespace QuanLyTiemChung.MVVM.ViewModels
{
    public class PatientViewModel : INotifyPropertyChanged
    {
        private Patient _patient;

        public Patient Patient
        {
            get => _patient;
            set
            {
                _patient = value;
                OnPropertyChanged(nameof(Patient));
            }
        }

        public string Name => Patient?.Name;
        
        public string Gender => Patient?.Gender;
        public string Address => Patient?.Address;
        public string PhoneNumber => Patient?.PhoneNumber;
        public string IDNumber => Patient?.IDNumber;
        public string PriorityGroup => Patient?.PriorityGroup;
        public string Notes => Patient?.Notes;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
