using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private bool changingPhone;

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
                SummaryGrid.ItemsSource = database.GetProductSummary().DefaultView;

                DataTable statistics = database.GetStatistics();
                if (statistics.Rows.Count > 0)
                {
                    DataRow row = statistics.Rows[0];
                    ProductsCountText.Text = row["НаименованийТоваров"].ToString();
                    StockCountText.Text = row["ЕдиницНаСкладе"].ToString();
                    SalesCountText.Text = row["ПроданоЕдиниц"].ToString();
                    RevenueText.Text = Convert.ToDecimal(row["Выручка"]).ToString("N2") + " руб.";
                }

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

            int category = GetId(ProductCategoryBox);

            if (ProductNameBox.Text.Trim() == "" || !TryReadPrice(out price) ||
                !int.TryParse(ProductCountBox.Text, out count) || price < 0 || count < 0 ||
                category == 0)
            {
                ShowError("Заполните данные товара правильно.");
                return;
            }

            try
            {
                database.AddProduct(ProductNameBox.Text.Trim(), price, count, category);
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

            int category = GetId(ProductCategoryBox);

            if (productId == 0 || ProductNameBox.Text.Trim() == "" ||
                !TryReadPrice(out price) || !int.TryParse(ProductCountBox.Text, out count) ||
                price < 0 || count < 0 || category == 0)
            {
                ShowError("Выберите товар и проверьте поля.");
                return;
            }

            try
            {
                database.EditProduct(productId, ProductNameBox.Text.Trim(), price, count, category);
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
            if (CategoryNameBox.Text.Trim() == "")
            {
                ShowError("Введите название категории.");
                return;
            }

            try
            {
                database.AddCategory(CategoryNameBox.Text.Trim());
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
            if (categoryId == 0 || CategoryNameBox.Text.Trim() == "")
            {
                ShowError("Выберите категорию.");
                return;
            }

            try
            {
                database.EditCategory(categoryId, CategoryNameBox.Text.Trim());
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
            if (SupplierNameBox.Text.Trim() == "" || !IsPhoneValid() || !IsEmailValid())
            {
                ShowError("Проверьте название, телефон и Email поставщика.");
                return;
            }

            try
            {
                database.AddSupplier(SupplierNameBox.Text.Trim(),
                    SupplierPhoneBox.Text.Trim(), SupplierEmailBox.Text.Trim());
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
            if (supplierId == 0 || SupplierNameBox.Text.Trim() == "" ||
                !IsPhoneValid() || !IsEmailValid())
            {
                ShowError("Выберите поставщика.");
                return;
            }

            try
            {
                database.EditSupplier(supplierId, SupplierNameBox.Text.Trim(),
                    SupplierPhoneBox.Text.Trim(), SupplierEmailBox.Text.Trim());
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
                !int.TryParse(SupplyCountBox.Text, out count) || count <= 0 ||
                !SupplyDateBox.SelectedDate.HasValue)
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
                !int.TryParse(SupplyCountBox.Text, out count) || count <= 0 ||
                !SupplyDateBox.SelectedDate.HasValue)
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
            SupplySupplierBox.SelectedValue = row["ПоставщикId"];
            SupplyCountBox.Text = row["Количество"].ToString();
            SupplyDateBox.SelectedDate = Convert.ToDateTime(row["ДатаПоставки"]);
        }

        private void AddSale_Click(object sender, RoutedEventArgs e)
        {
            int count;
            int product = GetId(SaleProductBox);

            if (product == 0 || !int.TryParse(SaleCountBox.Text, out count) || count <= 0 ||
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
                count <= 0 ||
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

        private void Integer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(symbol => !char.IsDigit(symbol));
        }

        private void Price_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null) return;

            string text = box.Text.Remove(box.SelectionStart, box.SelectionLength)
                .Insert(box.SelectionStart, e.Text);

            int separators = text.Count(symbol => symbol == ',' || symbol == '.');
            e.Handled = text.Any(symbol => !char.IsDigit(symbol) && symbol != ',' && symbol != '.') ||
                        separators > 1;
        }

        private void Price_LostFocus(object sender, RoutedEventArgs e)
        {
            decimal price;
            if (TryReadPrice(out price))
                ProductPriceBox.Text = price.ToString("0.00");
        }

        private bool TryReadPrice(out decimal price)
        {
            string text = ProductPriceBox.Text.Trim().Replace(',', '.');
            return decimal.TryParse(text, NumberStyles.Number,
                CultureInfo.InvariantCulture, out price);
        }

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(symbol => !char.IsDigit(symbol));
        }

        private void Phone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (changingPhone) return;

            string digits = new string(SupplierPhoneBox.Text.Where(char.IsDigit).ToArray());
            if (digits == "") return;

            if (digits[0] == '8')
                digits = "7" + digits.Substring(1);
            else if (digits[0] != '7')
                digits = "7" + digits;

            if (digits.Length > 11)
                digits = digits.Substring(0, 11);

            string number = digits.Substring(1);
            string formatted = "+7";

            if (number.Length > 0)
                formatted += " (" + number.Substring(0, Math.Min(3, number.Length));
            if (number.Length >= 3)
                formatted += ")";
            if (number.Length > 3)
                formatted += " " + number.Substring(3, Math.Min(3, number.Length - 3));
            if (number.Length > 6)
                formatted += "-" + number.Substring(6, Math.Min(2, number.Length - 6));
            if (number.Length > 8)
                formatted += "-" + number.Substring(8, Math.Min(2, number.Length - 8));

            changingPhone = true;
            SupplierPhoneBox.Text = formatted;
            SupplierPhoneBox.CaretIndex = formatted.Length;
            changingPhone = false;
        }

        private bool IsPhoneValid()
        {
            if (SupplierPhoneBox.Text.Trim() == "") return true;

            string digits = new string(SupplierPhoneBox.Text.Where(char.IsDigit).ToArray());
            return digits.Length == 11;
        }

        private bool IsEmailValid()
        {
            string email = SupplierEmailBox.Text.Trim();
            if (email == "") return true;

            int at = email.IndexOf('@');
            int dot = email.LastIndexOf('.');
            return at > 0 && dot > at + 1 && dot < email.Length - 1;
        }

        private void ShowError(string text)
        {
            MessageBox.Show(text, "Ошибка");
        }
    }
}
