using System.Windows;
using System.Windows.Controls;

namespace shop
{
    public partial class AdminMainForm : Window
    {
        private bool _isDefaultAdmin;
        public AdminMainForm(bool isDefaultAdmin = false) 
        {
            InitializeComponent();
            _isDefaultAdmin = isDefaultAdmin;
        }
        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ProductsForm();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UserForm();
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new EmployeeForm();
        }

        private void SalesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SaleFormdAdmin();
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ReportsForm();
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginForm loginWindow = new LoginForm();
            loginWindow.Show();
            this.Close();
        }

        private void ReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ReferenceForm();
        }
    }
}