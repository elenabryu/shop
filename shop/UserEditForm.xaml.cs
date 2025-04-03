using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace shop
{
    public partial class UserEditForm : Window
    {
        private int? _userId;
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

        public UserEditForm(int? userId = null)
        {
            InitializeComponent();
            _userId = userId;
            LoadEmployees();
            LoadRoles();

            if (_userId.HasValue)
            {
                LoadUserData(_userId.Value);
                Title = "Редактировать пользователя";
            }
            else
            {
                Title = "Добавить пользователя";
            }
        }

        private void LoadEmployees()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT EmployeeID, EmployeeSurname, EmployeeName, EmployeePatronymic FROM Employee";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            List<Employee> employees = new List<Employee>();
                            while (reader.Read())
                            {
                                employees.Add(new Employee
                                {
                                    EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                    EmployeeSurname = reader["EmployeeSurname"].ToString(),
                                    EmployeeName = reader["EmployeeName"].ToString(),
                                    EmployeePatronymic = reader["EmployeePatronymic"].ToString()
                                });
                            }
                            EmployeeComboBox.ItemsSource = employees;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}");
            }
        }

        private void LoadRoles()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT RoleID, RoleName FROM Role";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            List<Role> roles = new List<Role>();
                            while (reader.Read())
                            {
                                roles.Add(new Role
                                {
                                    RoleID = Convert.ToInt32(reader["RoleID"]),
                                    RoleName = reader["RoleName"].ToString()
                                });
                            }
                            RoleComboBox.ItemsSource = roles;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ролей: {ex.Message}");
            }
        }


        private void LoadUserData(int userId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            U.UserEmployeeID,
                            U.UserLogin,
                            U.UserPassword,
                            U.UserRole
                        FROM User AS U
                        WHERE U.UserID = @UserID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Загружаем сотрудника
                                int employeeId = Convert.ToInt32(reader["UserEmployeeID"]);
                                Employee selectedEmployee = ((List<Employee>)EmployeeComboBox.ItemsSource).FirstOrDefault(e => e.EmployeeID == employeeId);
                                EmployeeComboBox.SelectedItem = selectedEmployee;

                                // Загружаем логин
                                LoginTextBox.Text = reader["UserLogin"].ToString();

                                // Загружаем пароль
                                PasswordTextBox.Tag = reader["UserPassword"].ToString();
                                PasswordTextBox.Text = "";

                                // Загружаем роль
                                int roleId = Convert.ToInt32(reader["UserRole"]);
                                Role selectedRole = ((List<Role>)RoleComboBox.ItemsSource).FirstOrDefault(r => r.RoleID == roleId);
                                RoleComboBox.SelectedItem = selectedRole;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(LoginTextBox.Text) || RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return;
            }

            Employee selectedEmployee = (Employee)EmployeeComboBox.SelectedItem;
            Role selectedRole = (Role)RoleComboBox.SelectedItem;

            string confirmationMessage = _userId.HasValue
                ? "Вы уверены, что хотите изменить данные этого пользователя?"
                : "Вы уверены, что хотите добавить этого пользователя?";

            MessageBoxResult result = MessageBox.Show(confirmationMessage, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM User WHERE UserEmployeeID = @UserEmployeeID AND UserID != @UserID";
                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UserEmployeeID", selectedEmployee.EmployeeID);
                        checkCommand.Parameters.AddWithValue("@UserID", _userId ?? 0);

                        int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (userCount > 0)
                        {
                            MessageBox.Show("У данного сотрудника уже есть учетная запись.");
                            return;
                        }
                    }

                    string query = "";
                    string hashedPassword = "";

                    if (!string.IsNullOrWhiteSpace(PasswordTextBox.Text))
                    {
                        hashedPassword = HashPassword(PasswordTextBox.Text);
                    }
                    else
                    {
                        hashedPassword = PasswordTextBox.Tag as string;
                    }


                    if (_userId.HasValue)
                    {
                        query = @"
                    UPDATE User 
                    SET UserEmployeeID = @UserEmployeeID, 
                        UserLogin = @UserLogin, 
                        UserPassword = @UserPassword, 
                        UserRole = @UserRole 
                    WHERE UserID = @UserID";
                    }
                    else
                    {
                        query = @"
                    INSERT INTO User (UserEmployeeID, UserLogin, UserPassword, UserRole) 
                    VALUES (@UserEmployeeID, @UserLogin, @UserPassword, @UserRole)";
                    }

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserEmployeeID", selectedEmployee.EmployeeID);
                        command.Parameters.AddWithValue("@UserLogin", LoginTextBox.Text);
                        command.Parameters.AddWithValue("@UserPassword", hashedPassword);
                        command.Parameters.AddWithValue("@UserRole", selectedRole.RoleID);

                        if (_userId.HasValue)
                        {
                            command.Parameters.AddWithValue("@UserID", _userId.Value);
                        }

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(_userId.HasValue ? "Данные пользователя успешно изменены." : "Новый пользователь успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователя: {ex.Message}");
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }


    public class Employee
    {
        public int EmployeeID { get; set; }
        public string EmployeeSurname { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeePatronymic { get; set; }
        public string FullName => $"{EmployeeSurname} {EmployeeName} {EmployeePatronymic}";

        public override string ToString()
        {
            return FullName;
        }
    }

    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }

        public override string ToString()
        {
            return RoleName;
        }
    }
}
