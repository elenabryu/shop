using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using System.Windows; 

namespace shop
{
    /// <summary>
    /// Interaction logic for ClientForm.xaml
    /// </summary>
    public partial class ClientForm : UserControl
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        public ClientForm()
        {
            InitializeComponent();
            LoadData();
            ProductsDataGrid.ItemsSource = Clients;
        }

        private void LoadData()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT ClientID, ClientSurname, ClientName, ClientPatronymic, Email, PhoneNumber FROM Client";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Clients.Add(new Client
                                {
                                    ClientID = reader.GetInt32("ClientID"),
                                    ClientSurname = reader.GetString("ClientSurname"),
                                    ClientName = reader.GetString("ClientName"),
                                    ClientPatronymic = reader.GetString("ClientPatronymic"),
                                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                                    PhoneNumber = reader.GetString("PhoneNumber")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }
        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            ClientEditForm clientEditForm = new ClientEditForm();
            bool? result = clientEditForm.ShowDialog(); 

            if (result == true) 
            {
                Clients.Clear(); 
                LoadData();   
            }
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            Client selectedClient = ProductsDataGrid.SelectedItem as Client;

            if (selectedClient == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.");
                return;
            }

            ClientEditForm clientEditForm = new ClientEditForm(selectedClient);
            bool? result = clientEditForm.ShowDialog();

            if (result == true)
            {
                Clients.Clear();
                LoadData();
            }
        }
    }

    public class Client
    {
        public int ClientID { get; set; }
        public string ClientSurname { get; set; }
        public string ClientName { get; set; }
        public string ClientPatronymic { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
