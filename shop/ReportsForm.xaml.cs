using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using System.Diagnostics;

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
                MemoryStream stream = GenerateWordReport(revenueData, startDate.Value, endDate.Value);

                if (stream != null)
                {
                    OpenWordDocument(stream);
                }
                else
                {
                    MessageBox.Show("Не удалось создать отчет."); 
                }
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

        private MemoryStream GenerateWordReport(DataTable dataTable, DateTime startDate, DateTime endDate)
        {
            MemoryStream stream = new MemoryStream();

            try
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    Paragraph titlePara = body.AppendChild(new Paragraph(new Run(new Text($"Отчет о выручке за период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}"))));
                    titlePara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

                    Table table = new Table();

                    TableProperties tblProp = new TableProperties(
                        new TableBorders(
                            new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                        ),
                        new TableWidth() { Width = "0", Type = TableWidthUnitValues.Auto } 
                    );
                    table.AppendChild(tblProp);

                    List<int> columnWidths = new List<int>() { 1000, 2500, 1500, 5000 };
                    int columnIndex = 0;

                    TableRow headerRow = new TableRow();
                    List<string> columnNames = new List<string>() { "SaleID", "SaleDate", "TotalAmount", "OrderDetails" }; 
                    List<string> columnNamesRu = new List<string>() { "ID продажи", "Дата продажи", "Сумма", "Детали заказа" }; 

                    for (int i = 0; i < columnNamesRu.Count; i++)
                    {
                        Run run = new Run();
                        RunProperties runProperties = new RunProperties();
                        runProperties.Bold = new Bold() { Val = OnOffValue.FromBoolean(true) };

                        Text text = new Text(columnNamesRu[i]);
                        text.Space = SpaceProcessingModeValues.Preserve; 
                        run.Append(runProperties);
                        run.Append(text);

                        TableCell headerCell = new TableCell(new Paragraph(run));
                        SetCellWidth(headerCell, columnWidths[columnIndex++]);
                        headerRow.AppendChild(headerCell);
                    }
                    table.AppendChild(headerRow);

                    columnIndex = 0;

                    foreach (DataRow row in dataTable.Rows)
                    {
                        TableRow dataRow = new TableRow();
                        columnIndex = 0; 

                        string saleIdValue = row["SaleID"].ToString();
                        TableCell saleIdCell = new TableCell(new Paragraph(new Run(new Text(saleIdValue)))); 
                        SetCellWidth(saleIdCell, columnWidths[columnIndex++]);
                        dataRow.AppendChild(saleIdCell);

                        TableCell saleDateCell = new TableCell(new Paragraph(new Run(new Text(Convert.ToDateTime(row["SaleDate"]).ToString("dd.MM.yyyy HH:mm")))));
                        SetCellWidth(saleDateCell, columnWidths[columnIndex++]);
                        dataRow.AppendChild(saleDateCell);

                        TableCell totalAmountCell = new TableCell(new Paragraph(new Run(new Text(Convert.ToDecimal(row["TotalAmount"]).ToString("F2")))));
                        SetCellWidth(totalAmountCell, columnWidths[columnIndex++]);
                        dataRow.AppendChild(totalAmountCell);


                        string orderDetails = row["OrderDetails"].ToString();
                        if (string.IsNullOrEmpty(orderDetails))
                        {
                            orderDetails = "Нет данных"; 
                        }

                        TableCell orderDetailsCell = new TableCell(new Paragraph(new Run(new Text(orderDetails))));
                        SetCellWidth(orderDetailsCell, columnWidths[columnIndex++]);
                        dataRow.AppendChild(orderDetailsCell);

                        table.AppendChild(dataRow);
                    }


                    decimal totalRevenue = dataTable.AsEnumerable().Sum(row => row.Field<decimal>("TotalAmount"));

                    Run totalRevenueRun = new Run();
                    RunProperties totalRevenueRunProperties = new RunProperties();
                    totalRevenueRunProperties.Bold = new Bold() { Val = OnOffValue.FromBoolean(true) };
                    totalRevenueRunProperties.FontSize = new FontSize() { Val = "24" };


                    Text totalRevenueText = new Text($"Общая выручка: {totalRevenue:F2}");
                    totalRevenueText.Space = SpaceProcessingModeValues.Preserve;

                    totalRevenueRun.Append(totalRevenueRunProperties);
                    totalRevenueRun.Append(totalRevenueText);


                    Paragraph totalRevenuePara = body.AppendChild(new Paragraph(totalRevenueRun));
                    totalRevenuePara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Right });

                    body.AppendChild(table); 

                    mainPart.Document.Save();
                }
                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании Word документа: {ex.Message}");
                return null;
            }
        }


        private void OpenWordDocument(MemoryStream stream)
        {
            try
            {
                string tempFile = Path.GetTempFileName() + ".docx";
                using (FileStream file = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(file);
                }

                ProcessStartInfo psi = new ProcessStartInfo(tempFile);
                psi.UseShellExecute = true; 
                Process.Start(psi);

                Process process = Process.Start(psi);
                if (process != null)
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += (sender, e) =>
                    {
                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete temp file: {ex.Message}");
                        }
                    };
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии Word: {ex.Message}");
            }
        }


        private static void SetCellWidth(TableCell tableCell, int width)
        {
            TableCellProperties tcp = new TableCellProperties();
            TableWidth tw = new TableWidth() { Width = width.ToString(), Type = TableWidthUnitValues.Dxa }; 
            tcp.Append(tw);
            tableCell.Append(tcp);
        }
    }
}
