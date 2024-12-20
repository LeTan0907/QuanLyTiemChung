﻿using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Class
{
    internal class Data
    {
    }
}


[FirestoreData]
public class Vaccines
{
    [FirestoreProperty]
    public string VaccineID { get; set; }

    [FirestoreProperty]
    public string VaccineName { get; set; }

    [FirestoreProperty]
    public int Dose { get; set; }

    [FirestoreProperty]
    public string Dosage { get; set; }

    [FirestoreProperty]
    public int Price { get; set; }  // Use decimal here if you need it to store decimal values

    [FirestoreProperty]
    public string Manufacturer { get; set; }

    [FirestoreProperty]
    public int InStock { get; set; }
}

public class Contraindication
{
    public int ContraindicationsID { get; set; }
    public string ContraindicationsName { get; set; }

    // Navigation properties
    public ICollection<Vaccines> Vaccines { get; set; }
}
[FirestoreData]
public class Order
{
    [FirestoreProperty]
    public string OrderID { get; set; }  // Firestore will store as a string

    [FirestoreProperty]
    public string PatientID { get; set; }  // To associate with the patient

    [FirestoreProperty]
    public string Status { get; set; }  // Status like "Pending", "Completed"

    [FirestoreProperty]
    public Timestamp CreationTime { get; set; }  // Timestamp of order creation
}


[FirestoreData]
public class OrderPatientInfo
{
    [FirestoreProperty]
    public string OrderID { get; set; }
    [FirestoreProperty]
    public string PatientID { get; set; }

    [FirestoreProperty]
    public string IDNumber { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public string Phone { get; set; }

    [FirestoreProperty]
    public string Gender { get; set; }

    [FirestoreProperty]
    public string Address { get; set; }

    [FirestoreProperty]
    public string Status { get; set; }

    [FirestoreProperty]
    public int Number { get; set; }
    [FirestoreProperty]
    public Timestamp CreationTime { get; set; }
    [FirestoreProperty]
    public bool IsPriority { get; set; }
    public Patient Patient { get; set; }
}
public class Disease
{
    public int DiseaseID { get; set; }
    public string Name { get; set; }
    public string Symptoms { get; set; }

    // Navigation properties
    public ICollection<Vaccines> Vaccines { get; set; }
}


[FirestoreData]
public class Users
{
    [FirestoreProperty]
    public string UserID { get; set; } // Doctor's unique ID

    [FirestoreProperty]
    public string Name { get; set; } // Doctor's unique ID
    [FirestoreProperty]
    public Timestamp DOB { get; set; } // Firestore-compatible date of birth

    [FirestoreProperty]
    public string Gender { get; set; } // Gender of the user

    [FirestoreProperty]
    public string PhoneNumber { get; set; } // Contact number
    [FirestoreProperty]
    public string Address { get; set; } // Contact number
    [FirestoreProperty]
    public string Role { get; set; } // Contact number
    [FirestoreProperty]
    public string Username { get; set; } // Foreign key or username
}


[FirestoreData]
public class Patient
{
    [FirestoreProperty]
    public string PatientID { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public Timestamp DOB { get; set; }

    [FirestoreProperty]
    public string Gender { get; set; }

    [FirestoreProperty]
    public string PhoneNumber { get; set; }

    [FirestoreProperty]
    public string Address { get; set; }

    [FirestoreProperty]
    public string IDNumber { get; set; }

    [FirestoreProperty]
    public string PriorityGroup { get; set; }

    [FirestoreProperty]
    public string Notes { get; set; }
}

[FirestoreData]
public class DeletedPatient
{
    [FirestoreProperty]
    public string PatientID { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public Timestamp DOB { get; set; }

    [FirestoreProperty]
    public string Gender { get; set; }

    [FirestoreProperty]
    public string PhoneNumber { get; set; }

    [FirestoreProperty]
    public string Address { get; set; }

    [FirestoreProperty]
    public string IDNumber { get; set; }

    [FirestoreProperty]
    public string PriorityGroup { get; set; }

    [FirestoreProperty]
    public string Notes { get; set; }
}
public class Service
{
    public int ServiceID { get; set; }

    // Foreign keys
    public int DoctorID { get; set; }
    public int TicketID { get; set; }

    public string HealthInfo { get; set; }
    public string Diagnosis { get; set; }

    // Navigation properties
}
[FirestoreData]
public class Account
{
    [FirestoreProperty]
    public string Username { get; set; }
    [FirestoreProperty]
    public string Password { get; set; }
    [FirestoreProperty]
    public bool Enable { get; set; }
    [FirestoreProperty]
    public string Role { get; set; }
}

public class Role
{
    public int RoleID { get; set; }
    public string RoleName { get; set; }

    // Navigation properties
    public ICollection<Account> Accounts { get; set; }
}

[FirestoreData]
public class DeletedOrder
{
    [FirestoreProperty]
    public string OrderID { get; set; }

    [FirestoreProperty]
    public string PatientID { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public Timestamp CreationTime { get; set; }

    [FirestoreProperty]
    public Timestamp DeletedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);

    [FirestoreProperty]
    public string DeletedBy { get; set; } // Add user information if applicable
}
[FirestoreData]
public class MedicalRecord
{
    
    [FirestoreProperty]
    public string RecordsID { get; set; }
    [FirestoreProperty]
    public string PatientID { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public string Gender { get; set; }

    [FirestoreProperty]
    public string Address { get; set; }

    [FirestoreProperty]
    public string DoctorID { get; set; }

    [FirestoreProperty]
    public string DoctorName { get; set; }

    [FirestoreProperty]
    public Dictionary<string, int> VaccineList { get; set; }

    [FirestoreProperty]
    public int TotalPrice { get; set; }

    [FirestoreProperty]
    public string InvoiceStatus { get; set; }

    [FirestoreProperty]
    public Timestamp CreatedAt { get; set; }
}
[FirestoreData]
public class VaccineDetail
{
    [FirestoreProperty]
    public string VaccineName { get; set; }
    [FirestoreProperty]
    public string VaccineID { get; set; }

    [FirestoreProperty]
    public int Quantity { get; set; }

    [FirestoreProperty]
    public decimal Price { get; set; }

    [FirestoreProperty]
    public string BatchNumber { get; set; }

    [FirestoreProperty]
    public string ExpiryDate { get; set; }

    [FirestoreProperty]
    public string Temperature { get; set; }

    [FirestoreProperty]
    public string Manufacturer { get; set; }
}
