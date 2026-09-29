using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BayazitovObuv
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<OrderItem> _selectedOrderItems = new List<OrderItem>();
        private List<Products> _selectedProducts = new List<Products>();
        private Users _currentUser;

        public OrderWindow(List<OrderItem> selectedOrderItems, List<Products> selectedProducts, Users user)
        {
            InitializeComponent();

            _currentUser = user;
            _selectedOrderItems = selectedOrderItems;
            _selectedProducts = selectedProducts;

            // ФИО клиента
            if (user != null)
                ClientNameText.Text = $"{user.UserSurname} {user.UserName} {user.UserPatronymic}";

            // Синхронизируем Quantity у Products с OrderItem.Count
            foreach (var p in _selectedProducts)
            {
                var op = _selectedOrderItems.FirstOrDefault(o => o.ID_Product == p.ID_Product);
                p.Quantity = op != null ? op.Count : 1;
            }

            OrderItemsListView.ItemsSource = _selectedProducts;

            // Номер заказа — просто порядковый (в БД ничего не пишем)
            OrderNumberText.Text = _selectedProducts.Count.ToString();

            SetDeliveryDate();
        }

        private void SetDeliveryDate()
        {
            // Если каждого товара <= 3 шт. — 3 дня, иначе 6 дней
            bool fastDeliveryPossible = true;
            foreach (var op in _selectedOrderItems)
            {
                if (op.Count > 3)
                {
                    fastDeliveryPossible = false;
                    break;
                }
            }

            int deliveryDays = fastDeliveryPossible ? 3 : 6;

            DateTime orderDate = DateTime.Now;
            OrderDateText.Text = orderDate.ToString("dd.MM.yyyy");
            DeliveryDateText.Text = orderDate.AddDays(deliveryDays).ToString("dd.MM.yyyy");
        }

        // ============ КНОПКИ + / - ============

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button)?.DataContext as Products;
            if (prod == null) return;

            if (prod.Quantity > 1)
            {
                prod.Quantity--;

                var op = _selectedOrderItems.FirstOrDefault(o => o.ID_Product == prod.ID_Product);
                if (op != null) op.Count = prod.Quantity;

                SetDeliveryDate();
                OrderItemsListView.Items.Refresh();
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button)?.DataContext as Products;
            if (prod == null) return;

            prod.Quantity++;

            var op = _selectedOrderItems.FirstOrDefault(o => o.ID_Product == prod.ID_Product);
            if (op != null) op.Count = prod.Quantity;

            SetDeliveryDate();
            OrderItemsListView.Items.Refresh();
        }

        // ============ УДАЛИТЬ ТОВАР ============

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Products product)
            {
                var result = MessageBox.Show("Удалить товар из заказа?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _selectedProducts.Remove(product);
                    var op = _selectedOrderItems.FirstOrDefault(o => o.ID_Product == product.ID_Product);
                    if (op != null) _selectedOrderItems.Remove(op);

                    if (_selectedProducts.Count == 0)
                    {
                        MessageBox.Show("Заказ пуст");
                        ShoesPage.ClearOrderItems();
                        this.DialogResult = false;
                        this.Close();
                    }
                    else
                    {
                        OrderItemsListView.ItemsSource = null;
                        OrderItemsListView.ItemsSource = _selectedProducts;
                        SetDeliveryDate();
                    }
                }
            }
        }

        // ============ СОХРАНИТЬ (пока без БД) ============

        private void SaveOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProducts.Count == 0)
            {
                MessageBox.Show("Добавьте товары в заказ!");
                return;
            }

            MessageBox.Show($"Заказ оформлен!\nТоваров: {_selectedProducts.Count}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            ShoesPage.ClearOrderItems();

            this.DialogResult = true;
            this.Close();
        }
    }

    // ============ ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ============

    /// <summary>
    /// Элемент заказа: ID товара + количество
    /// </summary>
    public class OrderItem
    {
        public int ID_Product { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Расширение модели Products для хранения количества в UI
    /// </summary>
    public partial class Products
    {
        public int Quantity { get; set; }
        public decimal TotalPrice => ProductCost * Quantity;
    }
}
