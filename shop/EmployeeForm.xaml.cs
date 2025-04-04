using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Linq;

namespace shop
{
    public partial class EmployeeForm : UserControl
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

        public class Employee
        {
            public int EmployeeID { get; set; }
            public string EmployeeSurname { get; set; }
            public string EmployeeName { get; set; }
            public string EmployeePatronymic { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }

            public string MaskedSurname { get; set; }
            public string MaskedName { get; set; }
            public string MaskedPatronymic { get; set; }
            public string MaskedEmail { get; set; }
            public string MaskedPhoneNumber { get; set; }
            public string MaskedAddress { get; set; }
        }

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        public EmployeeForm()
        {
            InitializeComponent();
            LoadEmployees();
            ProductsDataGrid.ItemsSource = Employees;
        }

        private void LoadEmployees()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT EmployeeID, EmployeeSurname, EmployeeName, EmployeePatronymic, Email, PhoneNumber, Address FROM Employee";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        Employees.Clear();

                        while (reader.Read())
                        {
                            Employee employee = new Employee
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                EmployeeSurname = reader["EmployeeSurname"].ToString(),
                                EmployeeName = reader["EmployeeName"].ToString(),
                                EmployeePatronymic = reader["EmployeePatronymic"].ToString(),
                                Email = reader["Email"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Address = reader["Address"].ToString()
                            };

                            employee.MaskedSurname = MaskName(employee.EmployeeSurname);
                            employee.MaskedName = MaskName(employee.EmployeeName);
                            employee.MaskedPatronymic = MaskName(employee.EmployeePatronymic);
                            employee.MaskedEmail = MaskEmail(employee.Email);
                            employee.MaskedPhoneNumber = MaskPhoneNumber(employee.PhoneNumber);
                            employee.MaskedAddress = MaskAddress(employee.Address);

                            Employees.Add(employee);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
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

        private string MaskAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return string.Empty;
            }

            string[] words = address.Split(' ');
            if (words.Length > 0)
            {
                return words[0] + new string('*', address.Length - words[0].Length);
            }

            return new string('*', address.Length);
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeEditForm addEditWindow = new EmployeeEditForm();
            if (addEditWindow.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Employee selectedEmployee = ProductsDataGrid.SelectedItem as Employee;

            if (selectedEmployee != null)
            {
                EmployeeEditForm addEditWindow = new EmployeeEditForm(selectedEmployee);

                if (addEditWindow.ShowDialog() == true)
                {
                    LoadEmployees();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для редактирования.");
            }
        }
    }
}