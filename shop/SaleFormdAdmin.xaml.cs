using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Linq;

namespace shop
{
    public partial class SaleFormdAdmin : UserControl, INotifyPropertyChanged
    {
        private string connectionString;
        private ObservableCollection<SaleViewModel> salesData;
        private ObservableCollection<SaleDetailViewModel> saleDetails;

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

        public SaleFormdAdmin()
        {
            InitializeComponent();
            DataContext = this;

            connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

            LoadSalesData();
            LoadSaleStatuses();
        }

        private void LoadSalesData(string filterStatus = null)
        {
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
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void LoadSaleStatuses()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT SaleStatus FROM Sale";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            ObservableCollection<string> statuses = new ObservableCollection<string>();
                            while (reader.Read())
                            {
                                statuses.Add(reader["SaleStatus"].ToString());
                            }

                            FilterByStatusComboBox.ItemsSource = statuses;
                            statuses.Insert(0, "Все");
                            FilterByStatusComboBox.ItemsSource = statuses;
                            FilterByStatusComboBox.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов продаж: {ex.Message}");
            }
        }
        private void FilterByStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedStatus = FilterByStatusComboBox.SelectedItem?.ToString();
            if (selectedStatus == "Все")
            {
                LoadSalesData(null);
            }
            else
            {
                LoadSalesData(selectedStatus);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SalesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale)
            {
                int saleId = selectedSale.SaleID;
                LoadSaleDetails(saleId);
            }
            else
            {
                SaleDetails = null;
                SaleDetailsDataGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSaleDetails(int saleId)
        {
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
                                        Price = Convert.ToDecimal(row["Price"])
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

    public class SaleViewModel
    {
        public int SaleID { get; set; }
        public string FullClientName { get; set; }
        public string FullEmployeeName { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public string SaleStatus { get; set; }
    }

    public class SaleDetailViewModels
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
