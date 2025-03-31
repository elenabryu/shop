using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

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
