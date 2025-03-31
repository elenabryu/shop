using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Globalization;

namespace shop
{
    public partial class ProductEditForm : Window, INotifyPropertyChanged
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private ObservableCollection<Category> categories;
        private ObservableCollection<Brand> brands;
        private ObservableCollection<Supplier> suppliers;
        private Product _product;
        private string _selectedImagePath;

        private string imageFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        private BitmapImage _defaultImage;

        public ObservableCollection<Category> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ObservableCollection<Brand> Brands
        {
            get { return brands; }
            set
            {
                brands = value;
                OnPropertyChanged(nameof(Brands));
            }
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get { return suppliers; }
            set
            {
                suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Product Product
        {
            get { return _product; }
            set { _product = value; }
        }

        public bool IsNewProduct { get; set; } = false;
        public ProductEditForm()
        {
            InitializeComponent();
            DataContext = this;
            _product = new Product();
            LoadLookups();
            _defaultImage = new BitmapImage(new Uri("pack://application:,,,/Resources/default.png"));
        }

        public ProductEditForm(Product product)
        {
            InitializeComponent();
            DataContext = this;
            _product = DeepCopy(product);
            LoadLookups();

            ProductNameTextBox.Text = _product.Name;
            ProductDescriptionTextBox.Text = _product.Description;
            PriceTextBox.Text = _product.Price.ToString();
            StockQuantityTextBox.Text = _product.Quantity.ToString();

            if (_product.CategoryID.HasValue)
            {
                CategoryComboBox.SelectedValue = _product.CategoryID.Value;
            }
            if (_product.Brand != null)
            {
                BrandComboBox.SelectedItem = Brands?.FirstOrDefault(b => b.Name == _product.Brand);
            }
            if (_product.Supplier != null)
            {
                SupplierComboBox.SelectedItem = Suppliers?.FirstOrDefault(s => s.SupplierName == _product.Supplier);
            }


            if (_product.ProductPhoto != null)
            {
                ImagePathTextBox.Source = LoadImage(_product.ProductPhoto);
            }
        }
        private Product DeepCopy(Product original)
        {
            if (original == null)
            {
                return null;
            }

            Product copy = new Product
            {
                ID = original.ID,
                Name = original.Name,
                Description = original.Description,
                Price = original.Price,
                Quantity = original.Quantity,
                Category = original.Category,
                CategoryID = original.CategoryID,
                Brand = original.Brand,
                Supplier = original.Supplier,
                ProductPhoto = original.ProductPhoto,
            };

            return copy;
        }

        private BitmapImage LoadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return _defaultImage;
            }
            string fullImagePath = Path.Combine(imageFolderPath, imagePath);
            if (!File.Exists(fullImagePath))
            {
                return _defaultImage;
            }
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(fullImagePath);
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                return _defaultImage;
            }
        }

        private void LoadLookups()
        {
            LoadCategories();
            LoadBrands();
            LoadSuppliers();
        }

        private void LoadCategories()
        {
            Categories = new ObservableCollection<Category>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT CategoryID, Name FROM Category";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Categories.Add(new Category
                                {
                                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                    Name = reader["Name"].ToString()
                                });
                            }
                            CategoryComboBox.ItemsSource = Categories;
                            CategoryComboBox.DisplayMemberPath = "Name";
                            CategoryComboBox.SelectedValuePath = "CategoryID";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}");
            }
        }

        private void LoadBrands()
        {
            Brands = new ObservableCollection<Brand>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT BrandID, Name FROM Brand";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Brands.Add(new Brand
                                {
                                    BrandID = Convert.ToInt32(reader["BrandID"]),
                                    Name = reader["Name"].ToString()
                                });
                            }
                            BrandComboBox.ItemsSource = Brands;
                            BrandComboBox.DisplayMemberPath = "Name";
                            BrandComboBox.SelectedValuePath = "BrandID";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке брендов: {ex.Message}");
            }
        }

        private void LoadSuppliers()
        {
            Suppliers = new ObservableCollection<Supplier>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT SupplierID, SupplierName FROM Supplier";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Suppliers.Add(new Supplier
                                {
                                    SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                    SupplierName = reader["SupplierName"].ToString()
                                });
                            }
                            SupplierComboBox.ItemsSource = Suppliers;
                            SupplierComboBox.DisplayMemberPath = "SupplierName";
                            SupplierComboBox.SelectedValuePath = "SupplierID";
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке поставщиков: {ex.Message}");
            }
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                ImagePathTextBox.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }
        private void PriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string newText = textBox.Text + e.Text;

            if (!char.IsDigit(e.Text[0]) && (e.Text[0] == '.') && (textBox.Text.Contains('.')))
            {
                e.Handled = true;
            }
            else if (!Regex.IsMatch(e.Text, @"^[0-9.]+$"))
            {
                e.Handled = true;
            }
        }

        private void StockQuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void txtAddress_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[а-яА-ЯёЁ .,?!-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ProductDescriptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                string.IsNullOrWhiteSpace(StockQuantityTextBox.Text) ||
                CategoryComboBox.SelectedItem == null ||
                BrandComboBox.SelectedItem == null ||
                SupplierComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return false;
            }
            return true;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }

            MessageBoxResult confirmationResult = MessageBox.Show("Вы уверены, что хотите " + (_product.ID == 0 ? "добавить" : "изменить") + " этот товар?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmationResult == MessageBoxResult.No)
            {
                return;
            }

            _product.Name = ProductNameTextBox.Text;
            _product.Description = ProductDescriptionTextBox.Text;

            string priceText = PriceTextBox.Text.Replace(",", "."); // Заменяем запятые на точки

            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                _product.Price = price;
            }
            else
            {
                MessageBox.Show("Неверный формат цены.");
                return;
            }

            if (int.TryParse(StockQuantityTextBox.Text, out int quantity))
            {
                _product.Quantity = quantity;
            }
            else
            {
                MessageBox.Show("Неверный формат количества.");
                return;
            }

            if (CategoryComboBox.SelectedItem is Category selectedCategory)
            {
                _product.CategoryID = selectedCategory.CategoryID;
                _product.Category = selectedCategory.Name;
            }
            else
            {
                _product.CategoryID = null;
                _product.Category = null;
            }

            if (BrandComboBox.SelectedItem is Brand selectedBrand)
            {
                _product.Brand = selectedBrand.Name;
            }
            else
            {
                _product.Brand = null;
            }

            if (SupplierComboBox.SelectedItem is Supplier selectedSupplier)
            {
                _product.Supplier = selectedSupplier.SupplierName;
            }
            else
            {
                _product.Supplier = null;
            }

            string filename = null;
            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                filename = Path.GetFileName(_selectedImagePath);
                string newFilename = filename;
                string destinationPath = Path.Combine(imageFolderPath, filename);

                if (File.Exists(destinationPath))
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fileExtension = Path.GetExtension(filename);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                    newFilename = fileNameWithoutExtension + "_" + timestamp + fileExtension;
                    destinationPath = Path.Combine(imageFolderPath, newFilename);
                    filename = newFilename;
                }

                try
                {
                    if (!string.IsNullOrEmpty(_product.ProductPhoto) && _product.ProductPhoto != filename && _product.ProductPhoto != newFilename)
                    {
                        string oldImagePath = Path.Combine(imageFolderPath, _product.ProductPhoto);
                        if (File.Exists(oldImagePath))
                        {
                            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                            string fileExtension = Path.GetExtension(oldImagePath);
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(oldImagePath);
                            string newFileName = fileNameWithoutExtension + "_" + timestamp + fileExtension;
                            string newPath = Path.Combine(imageFolderPath, newFileName);

                            try
                            {
                                File.Move(oldImagePath, newPath);
                                Console.WriteLine($"Old image renamed to: {newPath}");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка переименования старого изображения: {ex.Message}");
                                return;
                            }
                        }
                    }
                    File.Copy(_selectedImagePath, destinationPath, true);
                    Console.WriteLine("Image saved to: " + destinationPath);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения изображения: {ex.Message}");
                    return;
                }
            }
            else
            {
                filename = string.IsNullOrEmpty(_product.ProductPhoto) ? null : _product.ProductPhoto;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "";
                    if (_product.ID == 0)
                    {
                        sql = @"
                INSERT INTO Product (ProductName, ProductDescription, Price, StockQuantity, CategoryID, BrandID, ProductPhoto, SupplierID)
                VALUES (@ProductName, @ProductDescription, @Price, @StockQuantity, @CategoryID, @BrandID, @ProductPhoto, @SupplierID);
                SELECT LAST_INSERT_ID();";
                        IsNewProduct = true;
                    }
                    else
                    {
                        sql = @"
                UPDATE Product 
                SET ProductName = @ProductName, 
                    ProductDescription = @ProductDescription, 
                    Price = @Price, 
                    StockQuantity = @StockQuantity, 
                    CategoryID = @CategoryID, 
                    BrandID = @BrandID, 
                    ProductPhoto = @ProductPhoto,
                    SupplierID = @SupplierID
                WHERE ProductID = @ProductID;";
                        IsNewProduct = false;
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", _product.Name);
                        cmd.Parameters.AddWithValue("@ProductDescription", _product.Description);
                        cmd.Parameters.AddWithValue("@Price", _product.Price.ToString(CultureInfo.InvariantCulture)); 
                        cmd.Parameters.AddWithValue("@StockQuantity", _product.Quantity);

                        if (_product.CategoryID.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@CategoryID", _product.CategoryID.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@CategoryID", DBNull.Value);
                        }

                        if (BrandComboBox.SelectedItem is Brand selectedBrandItem)
                        {
                            cmd.Parameters.AddWithValue("@BrandID", selectedBrandItem.BrandID);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@BrandID", DBNull.Value);
                        }

                        if (SupplierComboBox.SelectedItem is Supplier selectedSupplierItem)
                        {
                            cmd.Parameters.AddWithValue("@SupplierID", selectedSupplierItem.SupplierID);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@SupplierID", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@ProductPhoto", filename ?? (object)DBNull.Value);

                        if (_product.ID != 0)
                        {
                            cmd.Parameters.AddWithValue("@ProductID", _product.ID);
                        }

                        if (_product.ID == 0)
                        {
                            _product.ID = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        else
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
