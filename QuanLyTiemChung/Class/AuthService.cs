using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;

namespace QuanLyTiemChung.Class
{
    internal class AuthService
    {
        private FirebaseAuthProvider authProvider;
        private const string FirebaseApiKey = "AIzaSyDF-j6Ee1_esiYU1l-NX8VfwSr4vKM1gfc"; // Replace with your actual Firebase API key

        //public AuthService()
        //{
        //    // Initialize FirebaseAuthProvider with FirebaseAuthConfig
        //    authProvider = new FirebaseAuthProvider(new FirebaseAuthConfig
        //    {
        //        ApiKey = FirebaseApiKey,
                
        //    });
        //}

        //public async Task<FirebaseAuthLinkConflictException> LoginWithEmailPassword(string email, string password)
        //{
        //    try
        //    {
        //        // Authenticate the user with email and password
        //        var auth = await authProvider.SignInWithEmailAndPasswordAsync(email, password);
        //        return auth; // Contains user token and information
        //    }
        //    catch (FirebaseAuthException ex)
        //    {
        //        throw new Exception("Invalid login credentials", ex);
        //    }
        //}

    }
}
