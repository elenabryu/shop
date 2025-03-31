using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace shop
{
    public partial class AddCategory : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private int? categoryId; 

        public AddCategory()
        {
            InitializeComponent();
            categoryId = null; 
        }

        public AddCategory(int categoryId, string name, string description)
        {
            InitializeComponent();
            this.categoryId = categoryId; 
            txtName.Text = name;         
            txtDescription.Text = description; 
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            string description = txtDescription.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query;
                    MySqlCommand command = null;

                    if (categoryId == null) 
                    {
                        if (MessageBox.Show("Вы уверены, что хотите добавить новую категорию?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            query = "INSERT INTO Category (Name, Description) VALUES (@Name, @Description)";
                            command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Description", description);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Категория добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            DialogResult = true;
                            Close();
                        }
                    }
                    else 
                    {
                        if (MessageBox.Show("Вы уверены, что хотите изменить эту категорию?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            query = "UPDATE Category SET Name = @Name, Description = @Description WHERE CategoryID = @CategoryID";
                            command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@CategoryID", categoryId); 

                            command.ExecuteNonQuery();
                            MessageBox.Show("Категория успешно изменена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);


                            DialogResult = true;
                            Close();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TxtName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ .,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TxtDescription_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ .,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
