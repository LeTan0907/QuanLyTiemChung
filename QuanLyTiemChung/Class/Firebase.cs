using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace QuanLyTiemChung.Class
{
    internal class Firebase
    {
        private readonly string _firebaseUrl = "https://quanlytiemchung-f225a.firebaseio.com"; // Replace with your Firebase Realtime Database URL

        private readonly HttpClient _httpClient = new HttpClient();


        public async Task UpdateDataAsync<T>(string path, T data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_firebaseUrl}/{path}.json", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Data updated successfully.");
            }
            else
            {
                Console.WriteLine("Error updating data: " + response.ReasonPhrase);
            }
        }
    }
    public static class FirestoreHelper
    {
        private static FirestoreDb _firestoreDb = FirestoreDb.Create("your-project-id");

        public static async Task<List<T>> GetCollection<T>(string collectionName)
        {
            var snapshot = await _firestoreDb.Collection(collectionName).GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
        }

        public static async Task SaveDocument<T>(string documentPath, T data)
        {
            var docRef = _firestoreDb.Document(documentPath);
            await docRef.SetAsync(data);
        }
    }
}
