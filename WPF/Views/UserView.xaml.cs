using ProcurementSystem.WPF.ViewModels;

namespace ProcurementSystem.WPF.Views
{
    public partial class UsersView
    {
        public UsersView()
        {
            InitializeComponent();
            DataContext = new UsersViewModel();
        }
    }
}
