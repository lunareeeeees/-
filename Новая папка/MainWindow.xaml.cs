using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ShopSimpleWpf
{
    public partial class MainWindow : Window
    {
        private readonly Database database = new Database();

        private int productId;
        private int categoryId;
        private int supplierId;
        private int supplyId;
        private int saleId;

        public MainWindow(string role)
        {
            InitializeComponent();

            RoleTextBlock.Text = role;
            bool isAdmin = role == "Администратор";

            ProductsAdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            CategoriesAdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            SuppliersAdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            SuppliesAdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            SalesAdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            SupplyDateBox.SelectedDate = DateTime.Today;
            SaleDateBox.SelectedDate = DateTime.Today;
            LoadAll();
        }

        private void LoadAll()
        {
            try
            {
                DataTable products = database.GetProducts();
                DataTable categories = database.GetCategories();
                DataTable suppliers = database.GetSuppliers();

                ProductsGrid.ItemsSource = products.DefaultView;
                CategoriesGrid.ItemsSource = categories.DefaultView;
                SuppliersGrid.ItemsSource = suppliers.DefaultView;
                SuppliesGrid.ItemsSource = database.GetSupplies().DefaultView;
                SalesGrid.ItemsSource = database.GetSales().DefaultView;

                ProductCategoryBox.ItemsSource = categories.Copy().DefaultView;
                SupplyProductBox.ItemsSource = products.Copy().DefaultView;
                SaleProductBox.ItemsSource = products.Copy().DefaultView;
                SupplySupplierBox.ItemsSource = suppliers.Copy().DefaultView;
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            decimal price;
            int count;

            if (ProductNameBox.Text == "" ||
                !decimal.TryParse(ProductPriceBox.Text, out price) ||
                !int.TryParse(ProductCountBox.Text, out count))
            {
                ShowError("Заполните данные товара правильно.");
                return;
            }

            try
            {
                database.AddProduct(ProductNameBox.Text, price, count,
                    GetNullableId(ProductCategoryBox));
                ClearProduct();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            decimal price;
            int count;

            if (productId == 0 || ProductNameBox.Text == "" ||
                !decimal.TryParse(ProductPriceBox.Text, out price) ||
                !int.TryParse(ProductCountBox.Text, out count))
            {
                ShowError("Выберите товар и проверьте поля.");
                return;
            }

            try
            {
                database.EditProduct(productId, ProductNameBox.Text, price, count,
                    GetNullableId(ProductCategoryBox));
                ClearProduct();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelete(productId)) return;

            try
            {
                database.DeleteProduct(productId);
                ClearProduct();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError("Нельзя удалить используемый товар.\n" + error.Message);
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = ProductsGrid.SelectedItem as DataRowView;
            if (row == null || ProductsAdminPanel.Visibility != Visibility.Visible) return;

            productId = Convert.ToInt32(row["Id"]);
            ProductNameBox.Text = row["Название"].ToString();
            ProductPriceBox.Text = row["Цена"].ToString();
            ProductCountBox.Text = row["Количество"].ToString();
            ProductCategoryBox.SelectedValue = row["КатегорияId"] == DBNull.Value
                ? null : row["КатегорияId"];
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryNameBox.Text == "")
            {
                ShowError("Введите название категории.");
                return;
            }

            try
            {
                database.AddCategory(CategoryNameBox.Text);
                ClearCategory();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (categoryId == 0 || CategoryNameBox.Text == "")
            {
                ShowError("Выберите категорию.");
                return;
            }

            try
            {
                database.EditCategory(categoryId, CategoryNameBox.Text);
                ClearCategory();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelete(categoryId)) return;

            try
            {
                database.DeleteCategory(categoryId);
                ClearCategory();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError("Нельзя удалить используемую категорию.\n" + error.Message);
            }
        }

        private void CategoriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = CategoriesGrid.SelectedItem as DataRowView;
            if (row == null || CategoriesAdminPanel.Visibility != Visibility.Visible) return;

            categoryId = Convert.ToInt32(row["Id"]);
            CategoryNameBox.Text = row["Название"].ToString();
        }

        private void AddSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierNameBox.Text == "")
            {
                ShowError("Введите название поставщика.");
                return;
            }

            try
            {
                database.AddSupplier(SupplierNameBox.Text,
                    SupplierPhoneBox.Text, SupplierEmailBox.Text);
                ClearSupplier();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void EditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (supplierId == 0 || SupplierNameBox.Text == "")
            {
                ShowError("Выберите поставщика.");
                return;
            }

            try
            {
                database.EditSupplier(supplierId, SupplierNameBox.Text,
                    SupplierPhoneBox.Text, SupplierEmailBox.Text);
                ClearSupplier();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelete(supplierId)) return;

            try
            {
                database.DeleteSupplier(supplierId);
                ClearSupplier();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError("Нельзя удалить используемого поставщика.\n" + error.Message);
            }
        }

        private void SuppliersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = SuppliersGrid.SelectedItem as DataRowView;
            if (row == null || SuppliersAdminPanel.Visibility != Visibility.Visible) return;

            supplierId = Convert.ToInt32(row["Id"]);
            SupplierNameBox.Text = row["Название"].ToString();
            SupplierPhoneBox.Text = row["Телефон"].ToString();
            SupplierEmailBox.Text = row["Email"].ToString();
        }

        private void AddSupply_Click(object sender, RoutedEventArgs e)
        {
            int count;
            int product = GetId(SupplyProductBox);
            int supplier = GetId(SupplySupplierBox);

            if (product == 0 || supplier == 0 ||
                !int.TryParse(SupplyCountBox.Text, out count) || !SupplyDateBox.SelectedDate.HasValue)
            {
                ShowError("Заполните данные поставки правильно.");
                return;
            }

            try
            {
                database.AddSupply(product, supplier, count, SupplyDateBox.SelectedDate.Value);
                ClearSupply();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void EditSupply_Click(object sender, RoutedEventArgs e)
        {
            int count;
            int product = GetId(SupplyProductBox);
            int supplier = GetId(SupplySupplierBox);

            if (supplyId == 0 || product == 0 || supplier == 0 ||
                !int.TryParse(SupplyCountBox.Text, out count) || !SupplyDateBox.SelectedDate.HasValue)
            {
                ShowError("Выберите поставку и проверьте поля.");
                return;
            }

            try
            {
                database.EditSupply(supplyId, product, supplier, count,
                    SupplyDateBox.SelectedDate.Value);
                ClearSupply();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void DeleteSupply_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelete(supplyId)) return;

            try
            {
                database.DeleteSupply(supplyId);
                ClearSupply();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void SuppliesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = SuppliesGrid.SelectedItem as DataRowView;
            if (row == null || SuppliesAdminPanel.Visibility != Visibility.Visible) return;

            supplyId = Convert.ToInt32(row["Id"]);
            SupplyProductBox.SelectedValue = row["ТоварId"];
            SupplySupplierBox.SelectedValue = row["ПоставщикId"] == DBNull.Value
                ? null : row["ПоставщикId"];
            SupplyCountBox.Text = row["Количество"].ToString();
            SupplyDateBox.SelectedDate = Convert.ToDateTime(row["ДатаПоставки"]);
        }

        private void AddSale_Click(object sender, RoutedEventArgs e)
        {
            int count;
            int product = GetId(SaleProductBox);

            if (product == 0 || !int.TryParse(SaleCountBox.Text, out count) ||
                !SaleDateBox.SelectedDate.HasValue)
            {
                ShowError("Заполните данные продажи правильно.");
                return;
            }

            try
            {
                database.AddSale(product, count, SaleDateBox.SelectedDate.Value);
                ClearSale();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void EditSale_Click(object sender, RoutedEventArgs e)
        {
            int count;
            int product = GetId(SaleProductBox);

            if (saleId == 0 || product == 0 || !int.TryParse(SaleCountBox.Text, out count) ||
                !SaleDateBox.SelectedDate.HasValue)
            {
                ShowError("Выберите продажу и проверьте поля.");
                return;
            }

            try
            {
                database.EditSale(saleId, product, count, SaleDateBox.SelectedDate.Value);
                ClearSale();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void DeleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelete(saleId)) return;

            try
            {
                database.DeleteSale(saleId);
                ClearSale();
                LoadAll();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void SalesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = SalesGrid.SelectedItem as DataRowView;
            if (row == null || SalesAdminPanel.Visibility != Visibility.Visible) return;

            saleId = Convert.ToInt32(row["Id"]);
            SaleProductBox.SelectedValue = row["ТоварId"];
            SaleCountBox.Text = row["Количество"].ToString();
            SaleDateBox.SelectedDate = Convert.ToDateTime(row["ДатаПродажи"]);
        }

        private int GetId(ComboBox box)
        {
            return box.SelectedValue == null ? 0 : Convert.ToInt32(box.SelectedValue);
        }

        private int? GetNullableId(ComboBox box)
        {
            return box.SelectedValue == null ? (int?)null : Convert.ToInt32(box.SelectedValue);
        }

        private bool CanDelete(int id)
        {
            if (id == 0)
            {
                ShowError("Сначала выберите запись в таблице.");
                return false;
            }

            return MessageBox.Show("Удалить выбранную запись?", "Удаление",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private void ClearProduct()
        {
            productId = 0;
            ProductNameBox.Clear();
            ProductPriceBox.Clear();
            ProductCountBox.Clear();
            ProductCategoryBox.SelectedIndex = -1;
        }

        private void ClearCategory()
        {
            categoryId = 0;
            CategoryNameBox.Clear();
        }

        private void ClearSupplier()
        {
            supplierId = 0;
            SupplierNameBox.Clear();
            SupplierPhoneBox.Clear();
            SupplierEmailBox.Clear();
        }

        private void ClearSupply()
        {
            supplyId = 0;
            SupplyProductBox.SelectedIndex = -1;
            SupplySupplierBox.SelectedIndex = -1;
            SupplyCountBox.Clear();
            SupplyDateBox.SelectedDate = DateTime.Today;
        }

        private void ClearSale()
        {
            saleId = 0;
            SaleProductBox.SelectedIndex = -1;
            SaleCountBox.Clear();
            SaleDateBox.SelectedDate = DateTime.Today;
        }

        private void ShowError(string text)
        {
            MessageBox.Show(text, "Ошибка");
        }
    }
}
