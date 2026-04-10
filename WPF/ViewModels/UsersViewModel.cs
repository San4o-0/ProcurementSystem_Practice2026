using Microsoft.Extensions.DependencyInjection;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.Collections.ObjectModel;

namespace ProcurementSystem.WPF.ViewModels
{
    public class UsersViewModel
    {
        public ObservableCollection<User> Users { get; }

        public UsersViewModel()
        {
            //var service = App.ServiceProvider.GetRequiredService<UserService>();
            //Users = new ObservableCollection<User>(service.GetAll());
        }
    }
}
