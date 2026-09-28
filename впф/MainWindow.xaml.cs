using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ShopSimpleWpf
{
    public partial class MainWindow : Window
    {
        private readonly Database database = new Database();
        private int selectedId;

        public MainWindow(bool isAdmin)
        {
            InitializeComponent();

            RoleTextBlock.Text = isAdmin ? "Администратор" : "Пользователь";
            AdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                ProductsGrid.ItemsSource = database.GetProducts().DefaultView;
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            decimal price;
            int count;

            if (!CheckFields(out price, out count))
                return;

            try
            {
                database.AddProduct(NameTextBox.Text, price, count);
                ClearFields();
                LoadProducts();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId == 0)
            {
                ShowError("Выберите товар в таблице.");
                return;
            }

            decimal price;
            int count;

            if (!CheckFields(out price, out count))
                return;

            try
            {
                database.EditProduct(selectedId, NameTextBox.Text, price, count);
                ClearFields();
                LoadProducts();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId == 0)
            {
                ShowError("Выберите товар в таблице.");
                return;
            }

            if (MessageBox.Show("Удалить товар?", "Удаление",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                database.DeleteProduct(selectedId);
                ClearFields();
                LoadProducts();
            }
            catch (Exception error)
            {
                ShowError(error.Message);
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = ProductsGrid.SelectedItem as DataRowView;

            if (row == null || AdminPanel.Visibility != Visibility.Visible)
                return;

            selectedId = Convert.ToInt32(row["Id"]);
            NameTextBox.Text = row["Название"].ToString();
            PriceTextBox.Text = row["Цена"].ToString();
            CountTextBox.Text = row["Количество"].ToString();
        }

        private bool CheckFields(out decimal price, out int count)
        {
            price = 0;
            count = 0;

            if (NameTextBox.Text == "")
            {
                ShowError("Введите название.");
                return false;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out price) || price < 0)
            {
                ShowError("Введите правильную цену.");
                return false;
            }

            if (!int.TryParse(CountTextBox.Text, out count) || count < 0)
            {
                ShowError("Введите правильное количество.");
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            selectedId = 0;
            ProductsGrid.SelectedItem = null;
            NameTextBox.Clear();
            PriceTextBox.Clear();
            CountTextBox.Clear();
        }

        private void ShowError(string text)
        {
            MessageBox.Show(text, "Ошибка");
        }
    }
}
