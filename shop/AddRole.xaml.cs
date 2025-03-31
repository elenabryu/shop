using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xml.Linq;

namespace shop
{
    /// <summary>
    /// Interaction logic for AddRole.xaml
    /// </summary>
    public partial class AddRole : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private int? roleId; 

        public AddRole()
        {
            InitializeComponent();
            roleId = null; 
        }

        public AddRole(int roleId, string roleName)
        {
            InitializeComponent();
            this.roleId = roleId; 
            RoleNameTextBox.Text = roleName; 
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string roleName = RoleNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(roleName))
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
                        if (roleId == null) 
                        {
                        if (MessageBox.Show("Вы уверены, что хотите добавить новую роль?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            query = "INSERT INTO Role (RoleName) VALUES (@RoleName)";
                            command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@RoleName", roleName);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Роль добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            DialogResult = true;
                            Close();
                        }
                    }
                    else 
                    {
                        if (MessageBox.Show("Вы уверены, что хотите изменить эту роль?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            query = "UPDATE Role SET RoleName = @RoleName WHERE RoleID = @RoleID";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@RoleName", roleName);
                        command.Parameters.AddWithValue("@RoleID", roleId);
                            command.ExecuteNonQuery();
                            MessageBox.Show("Роль успешно изменена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);


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
        private void RoleNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ .,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
