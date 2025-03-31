using System.Windows;
using System.Windows.Controls;

namespace shop
{
    public partial class SellerMainForm : Window
    {
        public int LoggedInEmployeeID { get; set; }

        public SellerMainForm()
        {
            InitializeComponent();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ProductsFormSeller();
        }

        private void ClientButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClientForm();
        }

        private void SalesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SaleFormdSeller();
        }

        private void СheckButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CheckForm();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginForm loginWindow = new LoginForm();
            loginWindow.Show();
            this.Close();
        }
    }
}
