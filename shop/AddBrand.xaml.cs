using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace shop
{
    public partial class AddBrand : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private int? brandId; 

        public AddBrand()
        {
            InitializeComponent();
            brandId = null; 
        }
        public AddBrand(int brandId, string name, string description)
        {
            InitializeComponent();
            this.brandId = brandId;
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
                        if (brandId == null) 
                        {
                        if (MessageBox.Show("Вы уверены, что хотите добавить новый бренд?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            query = "INSERT INTO Brand (Name, Description) VALUES (@Name, @Description)";
                            command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Description", description);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Бренд добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            DialogResult = true;
                            Close();
                        }
                    }
                    else 
                    {

                        if (MessageBox.Show("Вы уверены, что хотите изменить этот бренд?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            query = "UPDATE Brand SET Name = @Name, Description = @Description WHERE BrandID = @BrandID";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@BrandID", brandId);

                        command.ExecuteNonQuery();
                            MessageBox.Show("Бренд успешно изменен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);


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
            Close();
        }
        private void TxtName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ a-zA-Z .,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void TxtDescription_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ .,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
