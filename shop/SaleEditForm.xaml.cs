using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Office.Interop.Word;

namespace shop
{
    public partial class SaleEditForm : UserControl, INotifyPropertyChanged
    {
        private string connectionString;
        private ObservableCollection<ProductViewModel> products;
        private ObservableCollection<ClientViewModel> clients;
        private ObservableCollection<EmployeeViewModel> employees;
        private ObservableCollection<SaleDetailViewModel> saleDetails;
        private decimal totalAmount;
        private decimal discount;
        private int? saleID;
        private int currentEmployeeID;
        private int availableStock;

        private Microsoft.Office.Interop.Word.Application wordApp = null;
        private Document wordDoc = null;
        private object missing = System.Reflection.Missing.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SaleDetailViewModel> SaleDetails
        {
            get { return saleDetails; }
            set
            {
                saleDetails = value;
                OnPropertyChanged(nameof(SaleDetails));
            }
        }

        public decimal TotalAmount
        {
            get { return totalAmount; }
            set
            {
                totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public decimal Discount
        {
            get { return discount; }
            set
            {
                discount = value;
                OnPropertyChanged(nameof(Discount));
            }
        }

        public SaleEditForm(int? employeeID = null)
        {
            InitializeComponent();
            DataContext = this;

            connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            this.saleID = null;

            if (employeeID.HasValue)
            {
                currentEmployeeID = employeeID.Value;
            }
            else
            {
                MessageBox.Show("ID сотрудника не передан!");
            }

            SaleDetails = new ObservableCollection<SaleDetailViewModel>();
            LoadClients();
            LoadEmployees();
            LoadProducts();

            EmployeeComboBox.SelectedValue = currentEmployeeID;
            SaleDatePicker.SelectedDate = DateTime.Now;

            System.Windows.Application.Current.Exit += Application_Exit;
        }

        private void LoadClients()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ClientID, ClientSurname, ClientName, ClientPatronymic FROM Client";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            System.Data.DataTable dataTable = new System.Data.DataTable(); 
                            adapter.Fill(dataTable);

                            clients = new ObservableCollection<ClientViewModel>();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                clients.Add(new ClientViewModel
                                {
                                    ClientID = Convert.ToInt32(row["ClientID"]),
                                    ClientName = $"{row["ClientSurname"]} {row["ClientName"]} {row["ClientPatronymic"]}"
                                });
                            }

                            ClientComboBox.ItemsSource = clients;
                            ClientComboBox.DisplayMemberPath = "ClientName";
                            ClientComboBox.SelectedValuePath = "ClientID";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}");
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
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            System.Data.DataTable dataTable = new System.Data.DataTable();
                            adapter.Fill(dataTable);

                            employees = new ObservableCollection<EmployeeViewModel>();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                employees.Add(new EmployeeViewModel
                                {
                                    EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                                    EmployeeName = $"{row["EmployeeSurname"]} {row["EmployeeName"]} {row["EmployeePatronymic"]}"
                                });
                            }

                            EmployeeComboBox.ItemsSource = employees;
                            EmployeeComboBox.DisplayMemberPath = "EmployeeName";
                            EmployeeComboBox.SelectedValuePath = "EmployeeID";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ProductID, ProductName, Price, StockQuantity FROM Product";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            System.Data.DataTable dataTable = new System.Data.DataTable(); 
                            adapter.Fill(dataTable);

                            products = new ObservableCollection<ProductViewModel>();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                products.Add(new ProductViewModel
                                {
                                    ProductID = Convert.ToInt32(row["ProductID"]),
                                    ProductName = row["ProductName"].ToString(),
                                    Price = Convert.ToDecimal(row["Price"]),
                                    StockQuantity = Convert.ToInt32(row["StockQuantity"])
                                });
                            }

                            ProductComboBox.ItemsSource = products;
                            ProductComboBox.DisplayMemberPath = "ProductName";
                            ProductComboBox.SelectedValuePath = "ProductID";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке продуктов: {ex.Message}");
            }
        }


        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null || string.IsNullOrEmpty(QuantityTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, выберите продукт и укажите количество.");
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное количество.");
                return;
            }

            ProductViewModel selectedProduct = (ProductViewModel)ProductComboBox.SelectedItem;

            if (quantity > availableStock)
            {
                MessageBox.Show($"На складе недостаточно товара. Доступно: {availableStock}");
                return;
            }


            SaleDetailViewModel existingDetail = SaleDetails.FirstOrDefault(d => d.ProductID == selectedProduct.ProductID);

            if (existingDetail != null)
            {
                existingDetail.Quantity += quantity;
            }
            else
            {
                SaleDetails.Add(new SaleDetailViewModel
                {
                    ProductID = selectedProduct.ProductID,
                    ProductName = selectedProduct.ProductName,
                    Quantity = quantity,
                    Price = selectedProduct.Price
                });
            }

            UpdateTotalAmount();
            SaleDetailsDataGrid.ItemsSource = SaleDetails;
            SaleDetailsDataGrid.Items.Refresh();

            ProductComboBox.SelectedItem = null;
            QuantityTextBox.Text = string.Empty;
            AddProductButton.IsEnabled = false;
        }

        private void UpdateTotalAmount()
        {
            decimal subtotal = SaleDetails.Sum(d => d.Price * d.Quantity);

            Discount = subtotal >= 5000 ? subtotal * 0.10m : 0;

            TotalAmount = subtotal - Discount;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientComboBox.SelectedItem == null ||
                SaleDatePicker.SelectedDate == null || SaleDetails.Count == 0)
            {
                MessageBox.Show("Пожалуйста, заполните все поля и добавьте хотя бы один товар.");
                return;
            }

            // **Уведомление о сохранении**
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите сохранить эту продажу?", "Подтверждение сохранения", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return; // Отмена сохранения, если пользователь нажал "Нет"
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        int clientId = (int)ClientComboBox.SelectedValue;
                        DateTime saleDate = SaleDatePicker.SelectedDate.Value;
                        decimal totalAmount = TotalAmount;
                        decimal discount = Discount;
                        string saleStatus = "Завершен";


                        string insertSaleQuery = @"
                                INSERT INTO Sale (ClientID, EmployeeID, SaleDate, TotalAmount, Discount, SaleStatus)
                                VALUES (@ClientID, @EmployeeID, @SaleDate, @TotalAmount, @Discount, @SaleStatus);
                                SELECT LAST_INSERT_ID();";

                        using (MySqlCommand insertSaleCommand = new MySqlCommand(insertSaleQuery, connection, transaction))
                        {
                            insertSaleCommand.Parameters.AddWithValue("@ClientID", clientId);
                            insertSaleCommand.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);
                            insertSaleCommand.Parameters.AddWithValue("@SaleDate", saleDate);
                            insertSaleCommand.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            insertSaleCommand.Parameters.AddWithValue("@Discount", discount);
                            insertSaleCommand.Parameters.AddWithValue("@SaleStatus", saleStatus);

                            saleID = Convert.ToInt32(insertSaleCommand.ExecuteScalar());
                        }


                        foreach (var detail in SaleDetails)
                        {
                            string insertDetailQuery = @"
                                INSERT INTO SaleDetail (SaleID, ProductID, Quantity, Price)
                                VALUES (@SaleID, @ProductID, @Quantity, @Price)";

                            using (MySqlCommand insertDetailCommand = new MySqlCommand(insertDetailQuery, connection, transaction))
                            {
                                insertDetailCommand.Parameters.AddWithValue("@SaleID", saleID);
                                insertDetailCommand.Parameters.AddWithValue("@ProductID", detail.ProductID);
                                insertDetailCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                insertDetailCommand.Parameters.AddWithValue("@Price", detail.Price);

                                insertDetailCommand.ExecuteNonQuery();
                            }

                            string updateStockQuery = "UPDATE Product SET StockQuantity = StockQuantity - @Quantity WHERE ProductID = @ProductID";
                            using (MySqlCommand updateStockCommand = new MySqlCommand(updateStockQuery, connection, transaction))
                            {
                                updateStockCommand.Parameters.AddWithValue("@ProductID", detail.ProductID);
                                updateStockCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                updateStockCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Продажа успешно сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information); // Уведомление об успешном сохранении

                        if (PrintCheckCheckBox.IsChecked == true)
                        {
                            GenerateWordDocument(saleID.Value);
                        }

                        if (Parent is System.Windows.Window window) // Явно указано пространство имен
                        {
                            window.Close();
                        }


                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при сохранении продажи: {ex.Message}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is System.Windows.Window window) // Явно указано пространство имен
            {
                window.Close();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void RemoveProductButton_Click(object sender, RoutedEventArgs e)
        {
            SaleDetailViewModel selectedDetail = (SaleDetailViewModel)((Button)sender).DataContext;

            if (selectedDetail != null)
            {
                SaleDetails.Remove(selectedDetail);
                UpdateTotalAmount();
                SaleDetailsDataGrid.Items.Refresh();
            }
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductComboBox.SelectedItem != null)
            {
                ProductViewModel selectedProduct = (ProductViewModel)ProductComboBox.SelectedItem;
                availableStock = selectedProduct.StockQuantity;
                AddProductButton.IsEnabled = true;
            }
            else
            {
                AddProductButton.IsEnabled = false;
            }
        }

        private void GenerateWordDocument(int saleId)
        {
            SaleViewModel sale = LoadSaleData(saleId);
            ObservableCollection<SaleDetailViewModel> saleDetails = LoadSaleDetails(saleId);

            try
            {
                wordApp = new Microsoft.Office.Interop.Word.Application();
                wordDoc = wordApp.Documents.Add(ref missing, ref missing, ref missing, ref missing);

                Microsoft.Office.Interop.Word.Paragraph titleParagraphMain = wordDoc.Content.Paragraphs.Add(ref missing);
                titleParagraphMain.Range.Text = "Магазин косметики и парфюмерии";
                titleParagraphMain.Range.Font.Size = 18;
                titleParagraphMain.Range.Font.Bold = 1;
                titleParagraphMain.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                titleParagraphMain.Range.InsertParagraphAfter();


                Microsoft.Office.Interop.Word.Paragraph titleParagraph = wordDoc.Content.Paragraphs.Add(ref missing);
                titleParagraph.Range.Text = "Чек продажи";
                titleParagraph.Range.Font.Size = 16;
                titleParagraph.Range.Font.Bold = 1;
                titleParagraph.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                titleParagraph.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph saleInfoParagraph = wordDoc.Content.Paragraphs.Add(ref missing);
                saleInfoParagraph.Range.Text = $"Номер продажи: {sale.SaleID}\n" +
                                                 $"Дата: {sale.SaleDate}\n" +
                                                 $"Клиент: {sale.FullClientName}\n" +
                                                 $"Сотрудник: {sale.FullEmployeeName}\n";
                saleInfoParagraph.Range.Font.Size = 12;
                saleInfoParagraph.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Table saleDetailsTable = wordDoc.Tables.Add(saleInfoParagraph.Range, saleDetails.Count + 1, 3, ref missing, ref missing);
                saleDetailsTable.Borders.Enable = 1;

                saleDetailsTable.Cell(1, 1).Range.Text = "Продукт";
                saleDetailsTable.Cell(1, 2).Range.Text = "Количество";
                saleDetailsTable.Cell(1, 3).Range.Text = "Цена";

                for (int i = 0; i < saleDetails.Count; i++)
                {
                    saleDetailsTable.Cell(i + 2, 1).Range.Text = saleDetails[i].ProductName;
                    saleDetailsTable.Cell(i + 2, 2).Range.Text = saleDetails[i].Quantity.ToString();
                    saleDetailsTable.Cell(i + 2, 3).Range.Text = saleDetails[i].Price.ToString("F2");
                }

                // Итоги
                Microsoft.Office.Interop.Word.Paragraph totalParagraph = wordDoc.Content.Paragraphs.Add(ref missing);
                totalParagraph.Range.Text = $"Скидка: {sale.Discount:F2}\n" +
                                             $"Итоговая сумма: {sale.TotalAmount:F2}";
                totalParagraph.Range.Font.Size = 12;
                totalParagraph.Range.Font.Bold = 1;
                totalParagraph.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                totalParagraph.Range.InsertParagraphAfter();


                wordApp.Visible = true;
                wordApp.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании чека: {ex.Message}");
            }
        }

        private SaleViewModel LoadSaleData(int saleId)
        {
            SaleViewModel sale = null;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                           SELECT 
                               s.SaleID,
                               CONCAT(c.ClientSurname, ' ', c.ClientName, ' ', c.ClientPatronymic) AS FullClientName,
                               CONCAT(e.EmployeeSurname, ' ', e.EmployeeName, ' ', e.EmployeePatronymic) AS FullEmployeeName,
                               s.SaleDate,
                               s.TotalAmount,
                               s.Discount,
                               s.SaleStatus
                           FROM Sale s
                           INNER JOIN Client c ON s.ClientID = c.ClientID
                           INNER JOIN Employee e ON s.EmployeeID = e.EmployeeID
                           WHERE s.SaleID = @SaleID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SaleID", saleId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sale = new SaleViewModel
                                {
                                    SaleID = Convert.ToInt32(reader["SaleID"]),
                                    FullClientName = reader["FullClientName"].ToString(),
                                    FullEmployeeName = reader["FullEmployeeName"].ToString(),
                                    SaleDate = Convert.ToDateTime(reader["SaleDate"]),
                                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                    Discount = Convert.ToDecimal(reader["Discount"]),
                                    SaleStatus = reader["SaleStatus"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о продаже: {ex.Message}");
            }
            return sale;
        }

        private ObservableCollection<SaleDetailViewModel> LoadSaleDetails(int saleId)
        {
            ObservableCollection<SaleDetailViewModel> saleDetails = new ObservableCollection<SaleDetailViewModel>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            p.ProductName,
                            sd.Quantity,
                            sd.Price
                        FROM SaleDetail sd
                        INNER JOIN Product p ON sd.ProductID = p.ProductID
                        WHERE sd.SaleID = @SaleID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SaleID", saleId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                saleDetails.Add(new SaleDetailViewModel
                                {
                                    ProductName = reader["ProductName"].ToString(),
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    Price = Convert.ToDecimal(reader["Price"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей продажи: {ex.Message}");
            }
            return saleDetails;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (wordDoc != null)
            {
                wordDoc.Close(ref missing, ref missing, ref missing);
            }
            if (wordApp != null)
            {
                wordApp.Quit(ref missing, ref missing, ref missing);
            }
        }
    }


    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }

    public class ClientViewModel
    {
        public int ClientID { get; set; }
        public string ClientName { get; set; }
    }

    public class EmployeeViewModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
    }

    public class SaleDetailViewModel : INotifyPropertyChanged
    {
        private int quantity;

        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }

        public int Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
