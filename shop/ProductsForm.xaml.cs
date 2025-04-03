using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace shop
{
    public partial class ProductsForm : UserControl, INotifyPropertyChanged
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private ObservableCollection<Product> products; 
        private ObservableCollection<Product> _filteredAndSortedProducts;
        private ObservableCollection<Category> categories;
        private string currentSearchText = "";
        private Category currentCategoryFilter = null;
        private string currentSortBy = null;
        private string currentSortOrder = null;

        private string imageFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        private BitmapImage _defaultImage;

        private int _pageSize = 20;
        private int _currentPage = 1;
        private int _totalProductsCount = 0; 
        private ObservableCollection<Product> _currentProductsPageView;
        public ObservableCollection<Product> CurrentProductsPageView
        {
            get { return _currentProductsPageView; }
            set
            {
                _currentProductsPageView = value;
                OnPropertyChanged(nameof(CurrentProductsPageView));
            }
        }

        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage != value && value > 0 && value <= TotalPages)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                    UpdateCurrentPageView();
                }
            }
        }

        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)_totalProductsCount / _pageSize);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Product> Products
        {
            get { return products; }
            set
            {
                products = value;
                OnPropertyChanged(nameof(Products));
                ApplyFiltersAndSorting(); 
            }
        }


        public ObservableCollection<Category> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProductsForm()
        {
            InitializeComponent();
            ProductsDataGrid.AutoGenerateColumns = false;
            DataContext = this;

            _defaultImage = new BitmapImage(new Uri("pack://application:,,,/Resources/default.png"));
            LoadProducts();
            LoadCategories();
        }

        private async void LoadProducts()
        {
            Products = GetProducts(); 
        }

        private ObservableCollection<Product> GetProducts()
        {
            ObservableCollection<Product> productList = new ObservableCollection<Product>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            p.ProductID,
                            p.ProductName,
                            p.ProductDescription,
                            p.Price,
                            p.StockQuantity,
                            c.Name AS CategoryName,
                            b.Name AS BrandName,
                            p.ProductPhoto,
                            s.SupplierName,
                            p.ProductDescription,
                            c.CategoryID
                        FROM Product p
                        LEFT JOIN Category c ON p.CategoryID = c.CategoryID
                        LEFT JOIN Brand b ON p.BrandID = b.BrandID
                        LEFT JOIN Supplier s ON p.SupplierID = s.SupplierID;
                    ";


                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string imagePathFromDb = reader["ProductPhoto"]?.ToString();
                                string fullImagePath = string.IsNullOrEmpty(imagePathFromDb) ? null : Path.Combine(imageFolderPath, imagePathFromDb);

                                Product product = new Product
                                {
                                    ID = Convert.ToInt32(reader["ProductID"]),
                                    Name = reader["ProductName"].ToString(),
                                    Description = reader["ProductDescription"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    Quantity = Convert.ToInt32(reader["StockQuantity"]),
                                    Category = reader["CategoryName"]?.ToString(),
                                    CategoryID = reader["CategoryID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CategoryID"]),
                                    Brand = reader["BrandName"]?.ToString(),
                                    ImageSource = LoadImage(fullImagePath),
                                    Supplier = reader["SupplierName"]?.ToString(),
                                    ProductPhoto = reader["ProductPhoto"]?.ToString()
                                };
                                productList.Add(product);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }

            if (productList.Count < 50)
            {
                for (int i = productList.Count; i < 50; i++)
                {
                    Product product = new Product
                    {
                        ID = i,
                        Name = $"Продукт {i}",
                        Description = $"Описание продукта {i}",
                        Price = 10.0m * i,
                        Quantity = i,
                        Category = "Категория",
                        CategoryID = 1,
                        Brand = "Бренд",
                        ImageSource = _defaultImage,
                        Supplier = "Поставщик",
                        ProductPhoto = ""
                    };
                    productList.Add(product);
                }

            }
            return productList;
        }

        private BitmapImage LoadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                return _defaultImage;
            }

            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(imagePath);
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                return _defaultImage;
            }
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
                            Categories.Add(new Category { CategoryID = 0, Name = "Все категории" });
                            while (reader.Read())
                            {
                                Categories.Add(new Category
                                {
                                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                    Name = reader["Name"].ToString()
                                });
                            }
                            CategoryFilterComboBox.ItemsSource = Categories;
                            CategoryFilterComboBox.DisplayMemberPath = "Name";
                            CategoryFilterComboBox.SelectedValuePath = "CategoryID";
                            CategoryFilterComboBox.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}");
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSearchText = SearchTextBox.Text.ToLower();
            ApplyFiltersAndSorting();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryFilterComboBox.SelectedItem is Category selectedCategory)
            {
                currentCategoryFilter = selectedCategory;
            }
            else
            {
                currentCategoryFilter = null;
            }

            ApplyFiltersAndSorting();
        }

        private void ApplyFiltersAndSorting()
        {
            IEnumerable<Product> filteredProducts = ApplyFilters();

            var sortedProducts = ApplySorting(filteredProducts);

            _filteredAndSortedProducts = new ObservableCollection<Product>(sortedProducts);

            _totalProductsCount = _filteredAndSortedProducts.Count;

            CurrentPage = 1;

            UpdateCurrentPageView();
            UpdatePaginationButtons();

        }

        private IEnumerable<Product> ApplyFilters()
        {
            if (Products == null) return new List<Product>();

            IEnumerable<Product> filteredProducts = Products.AsEnumerable();

            if (currentCategoryFilter != null && currentCategoryFilter.CategoryID != 0)
            {
                filteredProducts = filteredProducts.Where(p => p.CategoryID == currentCategoryFilter.CategoryID);
            }
            string searchTextWithoutSpaces = currentSearchText.Replace(" ", "");

            if (!string.IsNullOrEmpty(currentSearchText) && searchTextWithoutSpaces.Length >= 3)
            {
                filteredProducts = filteredProducts.Where(p =>
                    RemoveWhitespace(p.Name.ToLower()).Contains(searchTextWithoutSpaces) ||
                    RemoveWhitespace(p.Description.ToLower()).Contains(searchTextWithoutSpaces));
            }
            return filteredProducts;
        }

        private string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSortBy = (SortByComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            ApplyFiltersAndSorting();
        }

        private void SortOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSortOrder = (SortOrderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            ApplyFiltersAndSorting();
        }

        private IEnumerable<Product> ApplySorting(IEnumerable<Product> productsToSort)
        {
            if (string.IsNullOrEmpty(currentSortBy) || string.IsNullOrEmpty(currentSortOrder))
            {
                return productsToSort;
            }

            ListSortDirection direction = currentSortOrder == "По возрастанию" ? ListSortDirection.Ascending : ListSortDirection.Descending;

            if (currentSortBy == "Наименование")
            {
                return direction == ListSortDirection.Ascending ? productsToSort.OrderBy(p => p.Name) : productsToSort.OrderByDescending(p => p.Name);
            }
            else if (currentSortBy == "Цена")
            {
                return direction == ListSortDirection.Ascending ? productsToSort.OrderBy(p => p.Price) : productsToSort.OrderByDescending(p => p.Price);
            }
            else
            {
                return productsToSort;
            }
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            currentSearchText = string.Empty;

            CategoryFilterComboBox.SelectedIndex = 0;
            currentCategoryFilter = Categories.FirstOrDefault();

            SortByComboBox.SelectedIndex = -1;
            SortOrderComboBox.SelectedIndex = -1;
            currentSortBy = null;
            currentSortOrder = null;

            ApplyFiltersAndSorting();
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            ProductEditForm editForm = new ProductEditForm();
            if (editForm.ShowDialog() == true)
            {
                if (editForm.Product != null)
                {
                    Products.Add(editForm.Product);
                    LoadProducts();

                    MessageBox.Show($"Продукт '{editForm.Product.Name}' успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            Product selectedProduct = ProductsDataGrid.SelectedItem as Product;
            if (selectedProduct != null)
            {
                ProductEditForm editForm = new ProductEditForm(selectedProduct);
                if (editForm.ShowDialog() == true)
                {
                    int index = Products.IndexOf(selectedProduct);
                    if (index != -1)
                    {
                        Products[index] = editForm.Product;
                        LoadProducts();
                        MessageBox.Show($"Продукт '{editForm.Product.Name}' успешно отредактирован.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите товар для редактирования.");
            }
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            Product selectedProduct = ProductsDataGrid.SelectedItem as Product;
            if (selectedProduct != null)
            {
                if (IsProductUsedInSales(selectedProduct.ID))
                {
                    MessageBox.Show($"Невозможно удалить товар '{selectedProduct.Name}', так как он связан с продажами.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить товар '{selectedProduct.Name}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "DELETE FROM Product WHERE ProductID = @ProductID";
                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@ProductID", selectedProduct.ID);
                                command.ExecuteNonQuery();
                            }
                        }
                        Products.Remove(selectedProduct);
                        LoadProducts();
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show($"Ошибка при удалении товара: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления.");
            }
        }

        private bool IsProductUsedInSales(int productId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM SaleDetail WHERE ProductID = @ProductID";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", productId);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка при проверке связей с продажами: {ex.Message}");
                return true;
            }
        }

        private void UpdateCurrentPageView()
        {
            if (_filteredAndSortedProducts == null || _filteredAndSortedProducts.Count == 0)
            {
                CurrentProductsPageView = new ObservableCollection<Product>();
                return;
            }

            int startIndex = (_currentPage - 1) * _pageSize;
            int endIndex = Math.Min(startIndex + _pageSize, _filteredAndSortedProducts.Count);

            List<Product> pageProducts = _filteredAndSortedProducts.Skip(startIndex).Take(endIndex - startIndex).ToList();
            CurrentProductsPageView = new ObservableCollection<Product>(pageProducts);
            ProductsDataGrid.ItemsSource = CurrentProductsPageView;
            UpdatePaginationButtons();

        }
        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdateCurrentPageView();
            }
        }
        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdateCurrentPageView();
            }
        }
        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Content.ToString(), out int pageNumber))
            {
                CurrentPage = pageNumber;
                UpdateCurrentPageView();
            }
        }

        private void UpdatePaginationButtons()
        {
            PaginationPanel.Children.Clear();

            Button previousButton = new Button { Content = "←", Width = 30 };
            previousButton.Click += PreviousPageButton_Click;
            previousButton.IsEnabled = CurrentPage > 1;
            PaginationPanel.Children.Add(previousButton);

            for (int i = 1; i <= TotalPages; i++)
            {
                Button pageButton = new Button { Content = i.ToString(), Width = 30 };
                pageButton.Click += PageButton_Click;
                if (i == CurrentPage)
                {
                    pageButton.IsEnabled = false;
                }
                PaginationPanel.Children.Add(pageButton);
            }

            Button nextButton = new Button { Content = "→", Width = 30 };
            nextButton.Click += NextPageButton_Click;
            nextButton.IsEnabled = CurrentPage < TotalPages;
            PaginationPanel.Children.Add(nextButton);

        }
    }
}

    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        public int? CategoryID { get; set; }
        public string Brand { get; set; }
        public BitmapImage ImageSource { get; set; }
        public string Supplier { get; set; }

        public string ProductPhoto { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
    }
    public class Brand
    {
        public int BrandID { get; set; }
        public string Name { get; set; }
    }

    public class Supplier
    {
        public int SupplierID { get; set; }
        public string SupplierName
        {
            get; set;
        }
    }