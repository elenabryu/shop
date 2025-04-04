﻿using System;
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
    /// <summary>
    /// Логика взаимодействия для ProductsFormSeller.xaml
    /// </summary>
    public partial class ProductsFormSeller : UserControl
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        private ObservableCollection<Product> products;
        private ObservableCollection<Category> categories;
        private string currentSearchText = "";
        private Category currentCategoryFilter = null;
        private string currentSortBy = null;
        private string currentSortOrder = null;

        private string imageFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        private BitmapImage _defaultImage;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ProductsFormSeller()
        {
            InitializeComponent();
            ProductsDataGrid.AutoGenerateColumns = false;
            DataContext = this;

            _defaultImage = new BitmapImage(new Uri("pack://application:,,,/Resources/default.png"));
            LoadProducts();
            LoadCategories();

            ProductsDataGrid.ItemsSource = Products;
        }
        private void LoadProducts()
        {
            Products = GetProducts();
            ProductsDataGrid.ItemsSource = Products;
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
                                    CategoryID = reader["CategoryID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CategoryID"]), // Handle possible null CategoryID
                                    Brand = reader["BrandName"]?.ToString(),
                                    ImageSource = LoadImage(fullImagePath), // Load the image
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
            var filteredProducts = ApplyFilters();

            var sortedProducts = ApplySorting(filteredProducts);

            ProductsDataGrid.ItemsSource = sortedProducts.ToList();
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
    }
}
