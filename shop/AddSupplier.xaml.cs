using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Text.RegularExpressions; // Добавлено для проверки на цифры
using System.Windows;
using System.Windows.Input;

namespace shop
{
    public partial class AddSupplier : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private int? supplierId;

        public AddSupplier()
        {
            InitializeComponent();
            supplierId = null;
        }

        public AddSupplier(int supplierId, string name)
        {
            InitializeComponent();
            this.supplierId = supplierId;
            txtName.Text = name;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                txtName.Focus();
                return;
            }


            string message = supplierId == null ? "Вы уверены, что хотите добавить нового поставщика?" : "Вы уверены, что хотите сохранить изменения?";
            if (MessageBox.Show(message, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string query;
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            command.Connection = connection;
                            if (supplierId == null)
                            {
                                query = "INSERT INTO Supplier (SupplierName) VALUES (@Name)";
                                command.CommandText = query;
                                command.Parameters.AddWithValue("@Name", name);
                            }
                            else
                            {
                                query = "UPDATE Supplier SET SupplierName = @Name WHERE SupplierID = @SupplierID";
                                command.CommandText = query;
                                command.Parameters.AddWithValue("@Name", name);
                                command.Parameters.AddWithValue("@SupplierID", supplierId);
                            }

                            command.ExecuteNonQuery();
                        }
                        MessageBox.Show(supplierId == null ? "Поставщик добавлен." : "Поставщик успешно изменен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ .,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
