using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using MySql.Data.MySqlClient;

namespace shop
{
    public partial class LoginForm : Window
    {
        private string _captchaCode;
        private bool _isPasswordVisible = false;
        private int _loginAttempts = 0;
        private const int MaxLoginAttempts = 2;

        public LoginForm()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            _captchaCode = CaptchaGenerator.GenerateRandomCode(4);
            captchaImage.Source = CaptchaGenerator.GenerateImage(_captchaCode);
        }
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = textBoxLogin.Text.Trim();
            string password = passwordBoxPassword.Password;
            string hashedPassword = HashPassword(password);

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (login == "admin" && password == "admin")
            {
                RestoringAndImportingForm restoringandimportingForm = new RestoringAndImportingForm();
                restoringandimportingForm.Show();
                this.Close();
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT UserRole FROM User WHERE UserLogin = @Login AND UserPassword = @Password";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", hashedPassword);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        int roleId = Convert.ToInt32(result);

                        switch (roleId)
                        {
                            case 1:
                                AdminMainForm adminForm = new AdminMainForm();
                                adminForm.Show();
                                break;
                            case 2:
                                string employeeIdQuery = "SELECT UserEmployeeID FROM User WHERE UserLogin = @Login";
                                using (MySqlCommand employeeIdCommand = new MySqlCommand(employeeIdQuery, connection))
                                {
                                    employeeIdCommand.Parameters.AddWithValue("@Login", login);
                                    int loggedInEmployeeID = Convert.ToInt32(employeeIdCommand.ExecuteScalar());

                                    SellerMainForm sellerForm = new SellerMainForm();
                                    sellerForm.LoggedInEmployeeID = loggedInEmployeeID;
                                    sellerForm.Show();
                                }
                                break;
                            default:
                                MessageBox.Show("Неизвестная роль пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                        }

                        this.Close();
                    }
                    else
                    {
                        _loginAttempts++;
                        if (_loginAttempts >= MaxLoginAttempts)
                        {
                            MessageBox.Show("Превышено количество попыток входа. Пожалуйста, заполните поля логина и пароля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            textBoxLogin.Text = "";
                            passwordBoxPassword.Password = "";

                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                textBoxVisiblePassword.Text = passwordBoxPassword.Password;
                passwordBoxPassword.Visibility = Visibility.Collapsed;
                textBoxVisiblePassword.Visibility = Visibility.Visible;
                ShowPasswordButton.Content = "👁";

            }
            else
            {
                passwordBoxPassword.Password = textBoxVisiblePassword.Text;
                textBoxVisiblePassword.Visibility = Visibility.Collapsed;
                passwordBoxPassword.Visibility = Visibility.Visible;
                ShowPasswordButton.Content = "👁‍🗨";
            }
        }
    }
}

