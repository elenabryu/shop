using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace shop
{
    public partial class UserForm : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<UserViewModel> _users;

        public ObservableCollection<UserViewModel> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public UserForm()
        {
            InitializeComponent();
            DataContext = this;
            LoadUsers();
        }

        private void LoadUsers()
        {
            Users = new ObservableCollection<UserViewModel>();

            string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Строка подключения 'MySQLConnection' не найдена в файле конфигурации.");
                return; 
            }

            string query = @"
                SELECT 
                    U.UserID,
                    E.EmployeeSurname,
                    E.EmployeeName,
                    E.EmployeePatronymic,
                    U.UserLogin,
                    U.UserPassword,
                    R.RoleName
                FROM User AS U
                INNER JOIN Employee AS E ON U.UserEmployeeID = E.EmployeeID
                INNER JOIN Role AS R ON U.UserRole = R.RoleID;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Users.Add(new UserViewModel
                                {
                                    ID = Convert.ToInt32(reader["UserID"]),
                                    EmployeeFullName = $"{reader["EmployeeSurname"]} {reader["EmployeeName"]} {reader["EmployeePatronymic"]}",
                                    Login = reader["UserLogin"].ToString(),
                                    Password = reader["UserPassword"].ToString(),
                                    Role = reader["RoleName"].ToString()
                                });
                            }
                        }
                    }
                }

                ProductsDataGrid.ItemsSource = Users;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var userEditForm = new UserEditForm();
            if (userEditForm.ShowDialog() == true)
            {
                LoadUsers(); 
            }
        }


        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is UserViewModel selectedUser)
            {
                var userEditForm = new UserEditForm(selectedUser.ID);
                if (userEditForm.ShowDialog() == true)
                {
                    LoadUsers(); 
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для редактирования.");
            }
        }
    }

    public class UserViewModel
    {
        public int ID { get; set; }
        public string EmployeeFullName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
