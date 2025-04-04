using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace shop
{
    public partial class SellerMainForm : Window
    {
        private bool _isDefaultAdmin;
        public int LoggedInEmployeeID { get; set; }
        private DispatcherTimer _inactivityTimer;
        private DateTime _lastActivityTime;
        private int _inactivityTimeoutSeconds;

        public SellerMainForm(bool isDefaultAdmin = false)
        {
            InitializeComponent();
            _isDefaultAdmin = isDefaultAdmin;
            InitializeInactivityTimer();
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
