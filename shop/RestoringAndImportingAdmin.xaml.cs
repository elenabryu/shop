using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace shop
{
    /// <summary>
    /// Логика взаимодействия для RestoringAndImportingAdmin.xaml
    /// </summary>
    public partial class RestoringAndImportingAdmin : UserControl
    {
        private string connectionString;
        public RestoringAndImportingAdmin()
        {
            InitializeComponent(); connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        }

        private async void RestoreDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string script = @"
DROP DATABASE IF EXISTS shop;
CREATE DATABASE shop;
USE shop;

CREATE TABLE Role (
    RoleID INT AUTO_INCREMENT PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Employee (
    EmployeeID INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeSurname VARCHAR(50) NOT NULL,
    EmployeeName VARCHAR(50) NOT NULL,
    EmployeePatronymic VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(11) NOT NULL,
    Address VARCHAR(255)
);

CREATE TABLE User (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    UserEmployeeID INT NOT NULL UNIQUE,
    UserLogin VARCHAR(50) NOT NULL UNIQUE,
    UserPassword VARCHAR(255) NOT NULL,
    UserRole INT NOT NULL,
    FOREIGN KEY (UserRole) REFERENCES Role(RoleID),
    FOREIGN KEY (UserEmployeeID) REFERENCES Employee(EmployeeID)
);

CREATE TABLE Client (
    ClientID INT AUTO_INCREMENT PRIMARY KEY,
    ClientSurname VARCHAR(50) NOT NULL,
    ClientName VARCHAR(50) NOT NULL,
    ClientPatronymic VARCHAR(50) NOT NULL,
    Email VARCHAR(100),
    PhoneNumber VARCHAR(11) NOT NULL
);

CREATE TABLE Category (
    CategoryID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255)
);

CREATE TABLE Brand (
    BrandID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255)
);

CREATE TABLE Supplier (
    SupplierID INT AUTO_INCREMENT PRIMARY KEY,
    SupplierName VARCHAR(50) NOT NULL UNIQUE
);


CREATE TABLE Product (
    ProductID INT AUTO_INCREMENT PRIMARY KEY,
    ProductName VARCHAR(50) NOT NULL,
    ProductDescription VARCHAR(255),
    Price DECIMAL(10, 2) NOT NULL,
    StockQuantity INT NOT NULL,
    CategoryID INT,
    BrandID INT,
    ProductPhoto VARCHAR(255),
    SupplierID INT,
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
    FOREIGN KEY (BrandID) REFERENCES Brand(BrandID),
    FOREIGN KEY (SupplierID) REFERENCES Supplier(SupplierID) -- Added foreign key for Supplier
);

CREATE TABLE Sale (
    SaleID INT AUTO_INCREMENT PRIMARY KEY,
    ClientID INT,
    EmployeeID INT,
    SaleDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    Discount DECIMAL(10, 2) NOT NULL,
    SaleStatus VARCHAR(20) NOT NULL,
    FOREIGN KEY (ClientID) REFERENCES Client(ClientID),
    FOREIGN KEY (EmployeeID) REFERENCES Employee(EmployeeID)
);

CREATE TABLE SaleDetail (
    SaleID INT,
    ProductID INT,
    Quantity INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (SaleID, ProductID),
    FOREIGN KEY (SaleID) REFERENCES Sale(SaleID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
);
";

                    IEnumerable<string> commands = SplitSqlStatements(script);

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        foreach (string sqlCommand in commands)
                        {
                            if (string.IsNullOrWhiteSpace(sqlCommand)) continue;

                            command.CommandText = sqlCommand;
                            await command.ExecuteNonQueryAsync();
                        }
                    }


                    StatusTextBlock.Text = "База данных успешно восстановлена (структура создана, данные отсутствуют).";
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Ошибка при восстановлении базы данных: {ex.Message}";
            }
        }
        private IEnumerable<string> SplitSqlStatements(string script)
        {
            return script.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim());
        }



        private void BrowseImportFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ImportFilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private async void ImportDataButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ImportFilePathTextBox.Text;
            string tableName = ((ComboBoxItem)TableSelectionComboBox.SelectedItem).Content.ToString();

            if (string.IsNullOrEmpty(filePath))
            {
                StatusTextBlock.Text = "Пожалуйста, выберите CSV файл.";
                return;
            }

            try
            {
                int importedRowCount = await ImportDataFromCsvAsync(filePath, tableName);
                StatusTextBlock.Text = $"Успешно импортировано {importedRowCount} записей в таблицу {tableName}.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Ошибка при импорте данных: {ex.Message}";
            }
        }

        private async System.Threading.Tasks.Task<int> ImportDataFromCsvAsync(string filePath, string tableName)
        {
            int importedRowCount = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var reader = new StreamReader(filePath))
                {
                    string headerLine = await reader.ReadLineAsync();
                    if (headerLine == null)
                    {
                        throw new Exception("CSV файл пуст.");
                    }
                    int csvColumnCount = headerLine.Split(';').Length;

                    int tableColumnCount = GetTableColumnCount(connection, tableName);

                    if (csvColumnCount != tableColumnCount)
                    {
                        throw new Exception($"Количество столбцов в CSV файле ({csvColumnCount}) не соответствует количеству столбцов в таблице {tableName} ({tableColumnCount}).");
                    }

                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        string[] values = line.Split(';');

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = MySqlHelper.EscapeString(values[i]);
                        }

                        string insertQuery = GenerateInsertQuery(tableName, values);

                        using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                        {
                            await command.ExecuteNonQueryAsync();
                            importedRowCount++;
                        }
                    }
                }
            }

            return importedRowCount;
        }

        private int GetTableColumnCount(MySqlConnection connection, string tableName)
        {
            string query = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = '{connection.Database}';";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }


        private string GenerateInsertQuery(string tableName, string[] values)
        {
            string columnNames = "";
            string valuePlaceholders = "";
            List<string> columnList = new List<string>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = '{connection.Database}' ORDER BY ORDINAL_POSITION;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columnList.Add(reader.GetString(0));
                        }
                    }
                }
            }

            columnNames = string.Join(",", columnList);
            valuePlaceholders = string.Join(",", values.Select(v => $"'{v}'"));

            return $"INSERT INTO {tableName} ({columnNames}) VALUES ({valuePlaceholders});";
        }
    }
}
