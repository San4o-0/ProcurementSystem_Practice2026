using ProcurementSystem.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProcurementSystem.Services
{
    public static class UserSession
    {
        private static User? _currentUser;

        public static User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CurrentUser)));
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsAdmin)));
            }
        }

        public static bool IsAdmin => CurrentUser?.Role?.Name == "Admin";
        public static bool IsEmployee => CurrentUser?.Role?.Name == "Employee";
        public static bool IsManager => CurrentUser?.Role?.Name == "Manager";
        public static bool IsPurchaser => CurrentUser?.Role?.Name == "Procurement";


        public static event PropertyChangedEventHandler? PropertyChanged;
    }
}
