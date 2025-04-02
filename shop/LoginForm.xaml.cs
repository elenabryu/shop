using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Input;
using System.Threading.Tasks;

namespace shop
{
    public partial class LoginForm : Window
    {
        private string _captchaCode;
        private bool _isPasswordVisible = false;
        private int _loginAttempts = 0;
        private const int MaxLoginAttempts = 3;
        private DateTime _accountLockoutTime;
        private const int AccountLockoutDuration = 10;
        private bool _isAccountLocked = false;
        private readonly object _lock = new object();

        public LoginForm()
        {
            InitializeComponent();
        }

        private void GenerateCaptcha()
        {
            _captchaCode = CaptchaGenerator.GenerateRandomCode(4);
            captchaImage.Source = CaptchaGenerator.GenerateImage(_captchaCode);
            textBoxCaptcha.Clear();
            textBoxCaptcha.Focus();
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            lock (_lock)
            {
                if (_isAccountLocked)
                {
                    TimeSpan remaining = _accountLockoutTime - DateTime.Now;
                    MessageBox.Show($"Аккаунт заблокирован. Пожалуйста, подождите {remaining.Seconds} секунд.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            string login = textBoxLogin.Text.Trim();
            string password = _isPasswordVisible ? textBoxVisiblePassword.Text : passwordBoxPassword.Password;
            string hashedPassword = HashPassword(password);

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (captchaPanel.Visibility == Visibility.Visible)
            {
                string enteredCaptcha = textBoxCaptcha.Text.Trim();

                if (string.IsNullOrEmpty(enteredCaptcha))
                {
                    MessageBox.Show("Пожалуйста, введите CAPTCHA.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (enteredCaptcha.ToUpper() != _captchaCode.ToUpper())
                {
                    MessageBox.Show("Неверный код CAPTCHA.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    GenerateCaptcha();
                    return;
                }
            }
            if(login == "admin" && password == "admin")
            {
                RestoringAndImportingForm restoringAndImportingForm = new RestoringAndImportingForm();
                restoringAndImportingForm.Show();
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
                        lock (_lock)
                        {
                            _loginAttempts = 0;
                        }
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
                        lock (_lock)
                        {
                            _loginAttempts++;

                            if (_loginAttempts >= MaxLoginAttempts && !_isAccountLocked)
                            {
                                _isAccountLocked = true;
                                _accountLockoutTime = DateTime.Now.AddSeconds(AccountLockoutDuration);
                                MessageBox.Show($"Превышено количество попыток входа. Аккаунт заблокирован на {AccountLockoutDuration} секунд.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                ClearInputFields(); 
                                captchaPanel.Visibility = Visibility.Collapsed;

                                Task.Run(async () =>
                                {
                                    await Task.Delay(AccountLockoutDuration * 1000);
                                    lock (_lock)
                                    {
                                        _isAccountLocked = false;
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            MessageBox.Show("Аккаунт разблокирован. Попробуйте снова.", "Разблокировка", MessageBoxButton.OK, MessageBoxImage.Information);
                                        });
                                    }
                                });
                                return;
                            }
                        }

                        MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                        if (captchaPanel.Visibility != Visibility.Visible)
                        {
                            GenerateCaptcha();
                            captchaPanel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            GenerateCaptcha();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonRefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
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
                textBoxVisiblePassword.Text = passwordBoxPassword.Password;
                passwordBoxPassword.Visibility = Visibility.Visible;
                textBoxVisiblePassword.Visibility = Visibility.Collapsed;
                ShowPasswordButton.Content = "👁‍🗨";
                passwordBoxPassword.Focus();
            }
        }

        private void ClearInputFields()
        {
            textBoxLogin.Text = "";
            passwordBoxPassword.Password = "";
            textBoxVisiblePassword.Text = "";
            textBoxCaptcha.Text = "";
        }
    }
}