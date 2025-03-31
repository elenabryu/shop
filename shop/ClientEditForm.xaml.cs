using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;


namespace shop
{
    public partial class ClientEditForm : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private Client _client;
        private bool _isNewClient;

        public ClientEditForm()
        {
            InitializeComponent();
            _client = new Client();
            _isNewClient = true;
            this.Title = "Добавить клиента";
        }

        public ClientEditForm(Client client)
        {
            InitializeComponent();
            _client = client;
            _isNewClient = false;
            this.Title = "Редактировать клиента";

            txtSurname.Text = _client.ClientSurname;
            txtName.Text = _client.ClientName;
            txtPatronymic.Text = _client.ClientPatronymic;
            txtEmail.Text = _client.Email;
            txtPhone.Text = _client.PhoneNumber;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSurname.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return;
            }
            if (txtPhone.Text.Length < 11)
            {
                MessageBox.Show("Номер телефона должен содержать 11 символов.");
                return;
            }

            _client.ClientSurname = txtSurname.Text;
            _client.ClientName = txtName.Text;
            _client.ClientPatronymic = txtPatronymic.Text;
            _client.Email = txtEmail.Text;
            _client.PhoneNumber = txtPhone.Text;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query;
                    if (_isNewClient)
                    {
                        if (MessageBox.Show("Вы уверены, что хотите добавить нового клиента?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        query = "INSERT INTO Client (ClientSurname, ClientName, ClientPatronymic, Email, PhoneNumber) " +
                                "VALUES (@Surname, @Name, @Patronymic, @Email, @Phone)";
                    }
                    else
                    {
                        if (MessageBox.Show("Вы уверены, что хотите изменить этого клиента?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        query = "UPDATE Client SET ClientSurname = @Surname, ClientName = @Name, ClientPatronymic = @Patronymic, " +
                                "Email = @Email, PhoneNumber = @Phone WHERE ClientID = @ClientID";
                    }

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Surname", _client.ClientSurname);
                        command.Parameters.AddWithValue("@Name", _client.ClientName);
                        command.Parameters.AddWithValue("@Patronymic", _client.ClientPatronymic);
                        command.Parameters.AddWithValue("@Email", _client.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Phone", _client.PhoneNumber);

                        if (!_isNewClient)
                        {
                            command.Parameters.AddWithValue("@ClientID", _client.ClientID);
                        }

                        command.ExecuteNonQuery();
                    }
                }


                MessageBox.Show("Данные клиента успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Ошибка при работе с базой данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void txtSurname_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HandleTextInput(txtSurname, e);
        }

        private void txtName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HandleTextInput(txtName, e);
        }

        private void txtPatronymic_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HandleTextInput(txtPatronymic, e);
        }

        private void HandleTextInput(TextBox textBox, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ]+$");

            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            if (string.IsNullOrEmpty(textBox.Text))
            {
                e.Handled = true;
                textBox.Text = e.Text.ToUpper();
                textBox.CaretIndex = textBox.Text.Length;
            }
        }

        private void txtEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[a-zZ-a-0-9.@_-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPhone.Text.Length > 11)
            {
                txtPhone.Text = txtPhone.Text.Substring(0, 11);
                txtPhone.CaretIndex = 11; 
            }
        }

        private void txtPhoneNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
