using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Office.Interop.Word;
using System.Windows.Forms;

namespace shop
{
    /// <summary>
    /// Логика взаимодействия для CheckForm.xaml
    /// </summary>
    public partial class CheckForm : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private string connectionString;
        private ObservableCollection<SaleViewModel> salesData;
        private ObservableCollection<SaleDetailViewModel> saleDetails;
        private SaleViewModel selectedSale;

        private Microsoft.Office.Interop.Word.Application wordApp = null;
        private Document wordDoc = null;
        private object missing = System.Reflection.Missing.Value;

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

        public SaleViewModel SelectedSale
        {
            get { return selectedSale; }
            set
            {
                selectedSale = value;
                OnPropertyChanged(nameof(SelectedSale));
            }
        }


        public CheckForm()
        {
            InitializeComponent();
            DataContext = this;

            connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

            LoadSalesData();
            LoadSaleStatuses();

            System.Windows.Application.Current.Exit += Application_Exit;
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
                            System.Data.DataTable dataTable = new System.Data.DataTable();
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
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}");
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
                System.Windows.MessageBox.Show($"Ошибка при загрузке статусов продаж: {ex.Message}");
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
                SelectedSale = selectedSale;
                LoadSaleDetails(selectedSale.SaleID);
                SaleDetailsDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                SaleDetailsDataGrid.Visibility = Visibility.Collapsed;
                SaleDetails = null;
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
                            System.Data.DataTable dataTable = new System.Data.DataTable();
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке деталей продажи: {ex.Message}");
            }
        }

        private void GenerateCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSale == null || SaleDetails == null || SaleDetails.Count == 0)
            {
                System.Windows.MessageBox.Show("Пожалуйста, выберите продажу для формирования чека.");
                return;
            }

            GenerateWordDocument(SelectedSale, SaleDetails);
        }

        private void GenerateWordDocument(SaleViewModel sale, ObservableCollection<SaleDetailViewModel> saleDetails)
        {
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
                System.Windows.MessageBox.Show($"Ошибка при формировании чека: {ex.Message}");
            }
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
}
