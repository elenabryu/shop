using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;

namespace shop
{
    public partial class ReferenceForm : UserControl
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

        public ReferenceForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadCategories();
            LoadBrands();
            LoadSuppliers();
            LoadRoles();
        }

        private void LoadCategories()
        {
            LoadData("SELECT CategoryID, Name, Description FROM Category", CategoryDataGrid);
        }

        private void LoadBrands()
        {
            LoadData("SELECT BrandID, Name, Description FROM Brand", BrandDataGrid);
        }

        private void LoadSuppliers()
        {
            LoadData("SELECT SupplierID, SupplierName FROM Supplier", SupplierDataGrid);
        }

        private void LoadRoles()
        {
            LoadData("SELECT RoleID, RoleName FROM Role", RoleDataGrid);
        }

        private void LoadData(string query, DataGrid dataGrid)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
                AddCategory addCategoryWindow = new AddCategory();
                addCategoryWindow.Owner = Window.GetWindow(this);
                if (addCategoryWindow.ShowDialog() == true)
                {
                    LoadCategories();
                }
        }

        private void AddBrandButton_Click(object sender, RoutedEventArgs e)
        {
                AddBrand addBrandWindow = new AddBrand();
                addBrandWindow.Owner = Window.GetWindow(this);
            if (addBrandWindow.ShowDialog() == true)
            {
                LoadBrands();
            }
        }

        private void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
                AddSupplier addSupplierWindow = new AddSupplier();
                addSupplierWindow.Owner = Window.GetWindow(this);
                if (addSupplierWindow.ShowDialog() == true)
                {
                    LoadSuppliers();
                }
        }

        private void AddRoleButton_Click(object sender, RoutedEventArgs e)
        {
                AddRole addRoleWindow = new AddRole();
                addRoleWindow.Owner = Window.GetWindow(this);
                if (addRoleWindow.ShowDialog() == true)
                {
                    LoadRoles();

                }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string referenceType = button.Tag.ToString();
            DataRowView selectedItem = (DataRowView)button.CommandParameter;
                switch (referenceType)
                {
                    case "Category":
                        int categoryId = Convert.ToInt32(selectedItem["CategoryID"]);
                        string categoryName = selectedItem["Name"].ToString();
                        string categoryDescription = selectedItem["Description"].ToString();
                        AddCategory editCategoryWindow = new AddCategory(categoryId, categoryName, categoryDescription);
                        editCategoryWindow.Owner = Window.GetWindow(this);
                        if (editCategoryWindow.ShowDialog() == true)
                        {
                            LoadCategories();
                        }
                        break;
                    case "Brand":
                        int brandId = Convert.ToInt32(selectedItem["BrandID"]);
                        string brandName = selectedItem["Name"].ToString();
                        string brandDescription = selectedItem["Description"].ToString();
                        AddBrand editBrandWindow = new AddBrand(brandId, brandName, brandDescription);
                        editBrandWindow.Owner = Window.GetWindow(this);
                        if (editBrandWindow.ShowDialog() == true)
                        {
                            LoadBrands();
                        }
                        break;
                    case "Supplier":
                        int supplierId = Convert.ToInt32(selectedItem["SupplierID"]);
                        string supplierName = selectedItem["SupplierName"].ToString();
                        AddSupplier editSupplierWindow = new AddSupplier(supplierId, supplierName);
                        editSupplierWindow.Owner = Window.GetWindow(this);
                        if (editSupplierWindow.ShowDialog() == true)
                        {
                            LoadSuppliers();
                        }
                        break;
                    case "Role":
                        int roleId = Convert.ToInt32(selectedItem["RoleID"]);
                        string roleName = selectedItem["RoleName"].ToString();
                        AddRole editRoleWindow = new AddRole(roleId, roleName);
                        editRoleWindow.Owner = Window.GetWindow(this);
                        if (editRoleWindow.ShowDialog() == true)
                        {
                            LoadRoles();

                        }
                        break;
                }
        }
    }
}
