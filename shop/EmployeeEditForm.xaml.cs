using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;

namespace shop
{
    /// <summary>
    /// Interaction logic for EmployeeAddEditWindow.xaml
    /// </summary>
    public partial class EmployeeEditForm : Window
    {
        public Employee Employee { get; set; }
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private EmployeeForm.Employee _employee;
        private bool _isEditMode;

        public EmployeeEditForm()
        {
            InitializeComponent();
            _isEditMode = false;
            Title = "Добавить сотрудника";
        }

        public EmployeeEditForm(EmployeeForm.Employee employee)
        {
            InitializeComponent();
            _employee = employee;
            _isEditMode = true;
            Title = "Редактировать сотрудника";

            txtSurname.Text = employee.EmployeeSurname;
            txtName.Text = employee.EmployeeName;
            txtPatronymic.Text = employee.EmployeePatronymic;
            txtEmail.Text = employee.Email;
            txtPhoneNumber.Text = employee.PhoneNumber;
            txtAddress.Text = employee.Address;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string surname = txtSurname.Text;
            string name = txtName.Text;
            string patronymic = txtPatronymic.Text;
            string email = txtEmail.Text;
            string phoneNumber = txtPhoneNumber.Text;
            string address = txtAddress.Text;

            if (string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(patronymic) || string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля");
                return;
            }
            if (txtPhoneNumber.Text.Length < 11)
            {
                MessageBox.Show("Номер телефона должен содержать 11 символов.");
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "";
                    MySqlCommand command = null;

                    if (_isEditMode)
                    {
                        if (MessageBox.Show("Вы уверены, что хотите изменить этого сотрудника?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        {
                            return; 
                        }
                        query = "UPDATE Employee SET EmployeeSurname = @Surname, EmployeeName = @Name, EmployeePatronymic = @Patronymic, Email = @Email, PhoneNumber = @PhoneNumber, Address = @Address WHERE EmployeeID = @EmployeeID";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@EmployeeID", _employee.EmployeeID);

                    }
                    else
                    {
                        if (MessageBox.Show("Вы уверены, что хотите добавить нового сотрудника?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        {
                            return; 
                        }
                        query = "INSERT INTO Employee (EmployeeSurname, EmployeeName, EmployeePatronymic, Email, PhoneNumber, Address) VALUES (@Surname, @Name, @Patronymic, @Email, @PhoneNumber, @Address)";
                        command = new MySqlCommand(query, connection);


                    }

                    command.Parameters.AddWithValue("@Surname", surname);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Patronymic", patronymic);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    command.Parameters.AddWithValue("@Address", address);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show(_isEditMode ? "Сотрудник изменен." : "Сотрудник успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить сотрудника.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при работе с базой данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
            if (txtPhoneNumber.Text.Length > 11)
            {
                txtPhoneNumber.Text = txtPhoneNumber.Text.Substring(0, 11);
                txtPhoneNumber.CaretIndex = 11;
            }
        }

        private void txtPhoneNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void txtAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ 0-9.,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
