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
                                Client client = new Client
                                {
                                    ClientID = reader.GetInt32("ClientID"),
                                    ClientSurname = reader.GetString("ClientSurname"),
                                    ClientName = reader.GetString("ClientName"),
                                    ClientPatronymic = reader.GetString("ClientPatronymic"),
                                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                                    PhoneNumber = reader.GetString("PhoneNumber")
                                };
                                client.MaskedSurname = MaskName(client.ClientSurname);
                                client.MaskedName = MaskName(client.ClientName);
                                client.MaskedPatronymic = MaskName(client.ClientPatronymic);
                                client.MaskedEmail = MaskEmail(client.Email);
                                client.MaskedPhoneNumber = MaskPhoneNumber(client.PhoneNumber);

                                Clients.Add(client);
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
        public static string MaskFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return fullName;
            }

            string[] names = fullName.Split(' ');
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = MaskName(names[i]);
            }
            return string.Join(" ", names);
        }

        public static string MaskName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length <= 2)
            {
                return new string('*', name?.Length ?? 0);
            }

            return name.Substring(0, name.Length - 2) + "**";
        }

        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return email;
            }

            var parts = email.Split('@');
            if (parts.Length != 2)
            {
                return email;
            }

            string userName = parts[0];
            string domain = parts[1];

            if (userName.Length <= 3)
            {
                userName = new string('*', userName.Length);
            }
            else
            {
                userName = userName.Substring(0, 1) + new string('*', userName.Length - 3) + userName.Substring(userName.Length - 2);
            }

            var domainParts = domain.Split('.');
            if (domainParts.Length > 1)
            {
                domainParts[0] = domainParts[0].Length > 1 ? domainParts[0].Substring(0, 1) + new string('*', domainParts[0].Length - 1) : "*";
                domain = string.Join(".", domainParts);
            }
            else
            {
                domain = new string('*', domain.Length);
            }

            return $"{userName}@{domain}";
        }


        private string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length <= 4)
            {
                return new string('*', phoneNumber?.Length ?? 0);
            }

            return phoneNumber.Substring(0, phoneNumber.Length - 4) + "****";
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

        public string MaskedSurname { get; set; }
        public string MaskedName { get; set; }
        public string MaskedPatronymic { get; set; }
        public string MaskedEmail { get; set; }
        public string MaskedPhoneNumber { get; set; }
        public string MaskedAddress { get; set; }
    }
}
