using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Microsoft.Win32;
using System.Diagnostics;
using Word = Microsoft.Office.Interop.Word;
using System.Reflection;

namespace shop
{
    public partial class ReportsForm : UserControl
    {
        private string connectionString;

        public ReportsForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            StartDatePicker.DisplayDateEnd = DateTime.Now;
            EndDatePicker.DisplayDateEnd = DateTime.Now;
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            if (!startDate.HasValue || !endDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите начальную и конечную даты.");
                return;
            }

            if (startDate > endDate)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной.");
                return;
            }

            try
            {
                DataTable revenueData = GetRevenueData(startDate.Value, endDate.Value);

                if (revenueData == null || revenueData.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных о выручке за выбранный период.");
                    return;
                }

                GenerateWordReport(revenueData, startDate.Value, endDate.Value);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}");
            }
        }

        private DataTable GetRevenueData(DateTime startDate, DateTime endDate)
        {
            DataTable dataTable = new DataTable();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                    SELECT
                        Sale.SaleID,
                        Sale.SaleDate,
                        Sale.TotalAmount,
                        GROUP_CONCAT(CONCAT(Product.ProductName, ' (', SaleDetail.Quantity, ' x ', SaleDetail.Price, ')') SEPARATOR '; ') AS OrderDetails
                    FROM Sale
                    INNER JOIN SaleDetail ON Sale.SaleID = SaleDetail.SaleID
                    INNER JOIN Product ON SaleDetail.ProductID = Product.ProductID
                    WHERE Sale.SaleDate BETWEEN @StartDate AND @EndDate
                    GROUP BY Sale.SaleID, Sale.SaleDate, Sale.TotalAmount
                    ORDER BY Sale.SaleDate;
                    ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка при подключении к базе данных или выполнении запроса: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                    return null;
                }
            }
            return dataTable;
        }


        private void GenerateWordReport(DataTable dataTable, DateTime startDate, DateTime endDate)
        {
            Word.Application wordApp = new Word.Application();
            Word.Document wordDoc = null;

            try
            {
                object missing = Missing.Value;

                wordDoc = wordApp.Documents.Add(ref missing, ref missing, ref missing, ref missing);

                Word.Paragraph titlePara = wordDoc.Paragraphs.Add(ref missing);
                titlePara.Range.Text = $"Отчет о выручке за период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                titlePara.Range.Font.Bold = 1;
                titlePara.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                titlePara.Range.InsertParagraphAfter();

                Word.Table table = wordDoc.Tables.Add(titlePara.Range, dataTable.Rows.Count + 1, 4, ref missing, ref missing);
                table.Borders.Enable = 1;

                table.Cell(1, 1).Range.Text = "ID продажи";
                table.Cell(1, 2).Range.Text = "Дата продажи";
                table.Cell(1, 3).Range.Text = "Сумма";
                table.Cell(1, 4).Range.Text = "Детали заказа";

                table.Rows[1].Range.Font.Bold = 1;

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = dataTable.Rows[i]["SaleID"].ToString();
                    table.Cell(i + 2, 2).Range.Text = Convert.ToDateTime(dataTable.Rows[i]["SaleDate"]).ToString("dd.MM.yyyy HH:mm");
                    table.Cell(i + 2, 3).Range.Text = Convert.ToDecimal(dataTable.Rows[i]["TotalAmount"]).ToString("F2");
                    string orderDetails = dataTable.Rows[i]["OrderDetails"].ToString();
                    table.Cell(i + 2, 4).Range.Text = string.IsNullOrEmpty(orderDetails) ? "Нет данных" : orderDetails;
                }

                decimal totalRevenue = dataTable.AsEnumerable().Sum(row => row.Field<decimal>("TotalAmount"));

                Word.Paragraph totalRevenuePara = wordDoc.Paragraphs.Add(ref missing);
                totalRevenuePara.Range.Text = $"Общая выручка: {totalRevenue:F2}";
                totalRevenuePara.Range.Font.Bold = 1;
                totalRevenuePara.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

                wordApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании Word документа: {ex.Message}");

                if (wordDoc != null)
                {
                    wordDoc.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
                    wordDoc = null;
                }
                if (wordApp != null)
                {
                    wordApp.Quit(Word.WdSaveOptions.wdDoNotSaveChanges); 
                    wordApp = null;
                }

            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
