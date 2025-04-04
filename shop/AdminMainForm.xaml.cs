using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace shop
{
    public partial class AdminMainForm : Window
    {
        private bool _isDefaultAdmin;

        private DispatcherTimer _inactivityTimer;
        private DateTime _lastActivityTime;
        private int _inactivityTimeoutSeconds;

        public AdminMainForm(bool isDefaultAdmin = false)
        {
            InitializeComponent();
            _isDefaultAdmin = isDefaultAdmin;
            InitializeInactivityTimer();
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
            ShowLoginForm();
        }

        private void ReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ReferenceForm();
        }

        private void RestoringAndImporting_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new RestoringAndImportingAdmin();
        }

        private void InitializeInactivityTimer()
        {
            try
            {
                _inactivityTimeoutSeconds = int.Parse(ConfigurationManager.AppSettings["InactivityTimeoutSeconds"]);
            }
            catch (Exception ex)
            {
                _inactivityTimeoutSeconds = 30;
                MessageBox.Show($"Не удалось прочитать 'InactivityTimeoutSeconds' из конфигурации. Используется значение по умолчанию: 30 секунд. Ошибка: {ex.Message}", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            _inactivityTimer = new DispatcherTimer(DispatcherPriority.Background);
            _inactivityTimer.Interval = TimeSpan.FromSeconds(1);
            _inactivityTimer.Tick += InactivityTimer_Tick;

            EventManager.RegisterClassHandler(typeof(Window), Keyboard.KeyDownEvent, new KeyEventHandler(UserActivity));
            EventManager.RegisterClassHandler(typeof(Window), Mouse.MouseMoveEvent, new MouseEventHandler(UserActivity));
            EventManager.RegisterClassHandler(typeof(Window), Mouse.MouseDownEvent, new MouseButtonEventHandler(UserActivity));
            EventManager.RegisterClassHandler(typeof(Window), UIElement.TouchMoveEvent, new EventHandler<TouchEventArgs>(UserActivity));
            EventManager.RegisterClassHandler(typeof(Window), UIElement.TouchDownEvent, new EventHandler<TouchEventArgs>(UserActivity));


            ResetInactivityTimer();
            _inactivityTimer.Start();
        }

        private void UserActivity(object sender, EventArgs e)
        {
            ResetInactivityTimer();
        }

        private void ResetInactivityTimer()
        {
            _lastActivityTime = DateTime.Now;
        }

        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan idleTime = DateTime.Now - _lastActivityTime;

            if (idleTime.TotalSeconds >= _inactivityTimeoutSeconds)
            {
                _inactivityTimer.Stop();
                LockSystem();
            }
        }

        private void LockSystem()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowLoginForm();
            });
        }

        private void ShowLoginForm()
        {
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close(); 
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_inactivityTimer != null)
            {
                _inactivityTimer.Stop();
            }
        }
    }
}