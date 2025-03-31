using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace shop
{
    public partial class SaleFormdSeller : UserControl, INotifyPropertyChanged
    {
        private string connectionString;
        private ObservableCollection<SaleViewModel> salesData;
        private ObservableCollection<SaleDetailViewModel> saleDetails;
        private ObservableCollection<string> saleStatuses;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SaleViewModel> SalesData
        {
            get { return salesData; }
            set
            {
                salesData = value;
                OnPropertyChanged(nameof(SalesData));
            }
        }

        public ObservableCollection<SaleDetailViewModel> SaleDetails
        {
            get { return saleDetails; }
            set
            {
                saleDetails = value;
                OnPropertyChanged(nameof(SaleDetails));
            }
        }

        public ObservableCollection<string> SaleStatuses
        {
            get { return saleStatuses; }
            set
            {
                saleStatuses = value;
                OnPropertyChanged(nameof(SaleStatuses));
            }
        }
        public SaleFormdSeller()
        {
            InitializeComponent();
            DataContext = this;

            connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

            LoadSalesData();
            LoadSaleStatuses();

            // Подписываемся на событие RowEditEnding DataGrid
            SalesDataGrid.RowEditEnding += SalesDataGrid_RowEditEnding;
        }

        private async Task LoadSalesData(string filterStatus = null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
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
                               INNER JOIN Employee e ON s.EmployeeID = e.EmployeeID";

                    if (!string.IsNullOrEmpty(filterStatus))
                    {
                        query += $" WHERE s.SaleStatus = '{filterStatus}'";
                    }

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            SalesData = new ObservableCollection<SaleViewModel>(
                                dataTable.AsEnumerable()
                                    .Select(row => new SaleViewModel
                                    {
                                        SaleID = Convert.ToInt32(row["SaleID"]),
                                        FullClientName = row["FullClientName"].ToString(),
                                        FullEmployeeName = row["FullEmployeeName"].ToString(),
                                        SaleDate = Convert.ToDateTime(row["SaleDate"]),
                                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                                        Discount = Convert.ToDecimal(row["Discount"]),
                                        SaleStatus = row["SaleStatus"].ToString()
                                    })
                            );

                            SalesDataGrid.ItemsSource = SalesData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о продажах: {ex.Message}"); 
            }
        }

        private async Task LoadSaleStatuses()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT DISTINCT SaleStatus FROM Sale";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            SaleStatuses = new ObservableCollection<string>();
                            while (await reader.ReadAsync())
                            {
                                SaleStatuses.Add(reader["SaleStatus"].ToString());
                            }

                            FilterByStatusComboBox.ItemsSource = SaleStatuses;
                            SaleStatuses.Insert(0, "Все");
                            FilterByStatusComboBox.ItemsSource = SaleStatuses;
                            FilterByStatusComboBox.SelectedIndex = 0;

                            // Заполняем ComboBoxColumn данными о статусах
                            DataGridComboBoxColumn statusColumn = (DataGridComboBoxColumn)SalesDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "Статус");
                            if (statusColumn != null)
                            {
                                statusColumn.ItemsSource = SaleStatuses.Where(s => s != "Все").ToList(); // Исключаем "Все" из списка статусов в DataGrid
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов продаж: {ex.Message}");
            }
        }

        private async void FilterByStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedStatus = FilterByStatusComboBox.SelectedItem?.ToString();
            // If "All" is selected, pass null to LoadSalesData to load all data
            if (selectedStatus == "Все")
            {
                await LoadSalesData(null);
            }
            else
            {
                await LoadSalesData(selectedStatus);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void SalesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale)
            {
                int saleId = selectedSale.SaleID;
                await LoadSaleDetails(saleId);
            }
            else
            {
                SaleDetails = null;
                SaleDetailsDataGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadSaleDetails(int saleId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                SELECT 
                    p.ProductName,
                    sd.Quantity,
                    sd.Price,
                    sd.ProductID
                    FROM SaleDetail sd
                    INNER JOIN Product p ON sd.ProductID = p.ProductID
                    WHERE sd.SaleID = @SaleID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SaleID", saleId);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            SaleDetails = new ObservableCollection<SaleDetailViewModel>(
                                dataTable.AsEnumerable()
                                    .Select(row => new SaleDetailViewModel
                                    {
                                        ProductName = row["ProductName"].ToString(),
                                        Quantity = Convert.ToInt32(row["Quantity"]),
                                        Price = Convert.ToDecimal(row["Price"]),
                                        ProductID = Convert.ToInt32(row["ProductID"])
                                    })
                            );

                            SaleDetailsDataGrid.ItemsSource = SaleDetails;
                            SaleDetailsDataGrid.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей продажи: {ex.Message}");
            }
        }

        private void AddSaleButton_Click(object sender, RoutedEventArgs e)
        {
            int employeeID = ((SellerMainForm)Window.GetWindow(this)).LoggedInEmployeeID;
            SaleEditForm addForm = new SaleEditForm(employeeID);
            Window window = new Window
            {
                Content = addForm,
                Title = "Добавить продажу",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Closed += (s, args) =>
            {
                LoadSalesData();
            };
            window.ShowDialog();
        }

        private async void SalesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.DataContext is SaleViewModel editedSale)
                {
                    string originalStatus = await GetOriginalSaleStatus(editedSale.SaleID);

                    // Проверяем, изменился ли статус
                    if (editedSale.SaleStatus != originalStatus)
                    {
                        // Подтверждаем изменение статуса через MessageBox
                        MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите изменить статус?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Обновляем статус в базе данных
                            if (await UpdateSaleStatus(editedSale))
                            {
                                // Проверяем статус и выполняем соответствующие действия
                                if (editedSale.SaleStatus == "Отменен")
                                {
                                    await ReturnProductsToStock(editedSale.SaleID);
                                }
                                else if (editedSale.SaleStatus == "Завершен" && originalStatus != "Завершен") // Check original status to avoid double subtraction
                                {
                                    await SubtractProductsFromStock(editedSale.SaleID);
                                }

                                await LoadSalesData(); // Refresh data grid
                            }
                        }
                        else
                        {
                            await LoadSalesData();
                        }
                    }
                }
            }
        }

        private async Task<string> GetOriginalSaleStatus(int saleID)
        {
            string originalStatus = null;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT SaleStatus FROM Sale WHERE SaleID = @SaleID";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SaleID", saleID);

                        object result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            originalStatus = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении оригинального статуса продажи: {ex.Message}");
            }
            return originalStatus;
        }

        private async Task<bool> UpdateSaleStatus(SaleViewModel sale)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE Sale SET SaleStatus = @SaleStatus WHERE SaleID = @SaleID";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SaleID", sale.SaleID);
                        command.Parameters.AddWithValue("@SaleStatus", sale.SaleStatus);

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса продажи: {ex.Message}");
                return false;
            }
        }

        private async Task ReturnProductsToStock(int saleID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Получаем детали продажи (SaleDetail)
                    await LoadSaleDetailsAsync(saleID);

                    if (SaleDetails != null && SaleDetails.Any())
                    {
                        // Начинаем транзакцию, чтобы гарантировать целостность данных
                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var detail in SaleDetails)
                                {
                                    // Возвращаем количество продуктов на склад
                                    string updateQuery = "UPDATE Product SET StockQuantity = StockQuantity + @Quantity WHERE ProductID = @ProductID";
                                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                                    {
                                        updateCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                        updateCommand.Parameters.AddWithValue("@ProductID", detail.ProductID);

                                        await updateCommand.ExecuteNonQueryAsync();
                                    }
                                }

                                // Если все прошло успешно, подтверждаем транзакцию
                                await transaction.CommitAsync();
                                MessageBox.Show("Товары возвращены на склад.");
                            }
                            catch (Exception ex)
                            {
                                // Если произошла ошибка, откатываем транзакцию
                                await transaction.RollbackAsync();
                                MessageBox.Show($"Ошибка при возврате товаров на склад: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}");
            }
        }

        private async Task SubtractProductsFromStock(int saleID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Получаем детали продажи (SaleDetail)
                    await LoadSaleDetailsAsync(saleID);

                    if (SaleDetails != null && SaleDetails.Any())
                    {
                        // Начинаем транзакцию, чтобы гарантировать целостность данных
                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var detail in SaleDetails)
                                {
                                    // Проверяем, достаточно ли товара на складе
                                    string checkQuery = "SELECT StockQuantity FROM Product WHERE ProductID = @ProductID";
                                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection, transaction))
                                    {
                                        checkCommand.Parameters.AddWithValue("@ProductID", detail.ProductID);
                                        object result = await checkCommand.ExecuteScalarAsync();

                                        if (result != null && result != DBNull.Value)
                                        {
                                            long stockQuantityLong;

                                            if (result is long)
                                            {
                                                stockQuantityLong = (long)result;
                                            }
                                            else if (result is int)
                                            {
                                                stockQuantityLong = (int)result;
                                            }
                                            else
                                            {
                                                await transaction.RollbackAsync();
                                                MessageBox.Show($"Неожиданный тип данных для StockQuantity: {result.GetType().Name}");
                                                return;
                                            }

                                            if (stockQuantityLong < detail.Quantity)
                                            {
                                                await transaction.RollbackAsync();
                                                MessageBox.Show($"Недостаточно товара '{detail.ProductName}' на складе.  Доступно: {stockQuantityLong},  Требуется: {detail.Quantity}");
                                                return; // Выходим из функции, транзакция отменена
                                            }
                                        }
                                        else
                                        {
                                            await transaction.RollbackAsync();
                                            MessageBox.Show($"Не удалось получить StockQuantity для ProductID: {detail.ProductID}");
                                            return;
                                        }
                                    }

                                    // Вычитаем количество продуктов со склада
                                    string updateQuery = "UPDATE Product SET StockQuantity = StockQuantity - @Quantity WHERE ProductID = @ProductID";
                                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                                    {
                                        updateCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                        updateCommand.Parameters.AddWithValue("@ProductID", detail.ProductID);

                                        await updateCommand.ExecuteNonQueryAsync();
                                    }
                                }

                                // Если все прошло успешно, подтверждаем транзакцию
                                await transaction.CommitAsync();
                                MessageBox.Show("Товары списаны со склада.");
                            }
                            catch (Exception ex)
                            {
                                // Если произошла ошибка, откатываем транзакцию
                                await transaction.RollbackAsync();
                                MessageBox.Show($"Ошибка при списании товаров со склада: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}");
            }
        }

        private async Task LoadSaleDetailsAsync(int saleId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT 
                            p.ProductName,
                            sd.Quantity,
                            sd.Price,
                            sd.ProductID
                        FROM SaleDetail sd
                        INNER JOIN Product p ON sd.ProductID = p.ProductID
                        WHERE sd.SaleID = @SaleID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SaleID", saleId);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            SaleDetails = new ObservableCollection<SaleDetailViewModel>(
                                dataTable.AsEnumerable()
                                    .Select(row => new SaleDetailViewModel
                                    {
                                        ProductName = row["ProductName"].ToString(),
                                        Quantity = Convert.ToInt32(row["Quantity"]),
                                        Price = Convert.ToDecimal(row["Price"]),
                                        ProductID = Convert.ToInt32(row["ProductID"])
                                    })
                            );

                            SaleDetailsDataGrid.ItemsSource = SaleDetails;
                            SaleDetailsDataGrid.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей продажи: {ex.Message}");
            }
        }
    }
}
