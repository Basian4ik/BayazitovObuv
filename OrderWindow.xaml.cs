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
        private List<CartItem> cartItems;
        private Users _currentUser;

        public OrderWindow(List<CartItem> items, Users user)
        {
            InitializeComponent();

            cartItems = items;
            _currentUser = user;

            if (user != null)
                ClientLabel.Text = "Клиент: " + user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;

            OrderNumberLabel.Text = "Номер заказа: " + GetNextOrderNumber();
            OrderDateLabel.Text = "Дата заказа: " + DateTime.Now.ToString("dd.MM.yyyy");
            UpdateDeliveryDate();

            OrderListView.ItemsSource = cartItems;
            UpdateTotal();
        }

        private int GetNextOrderNumber()
        {
            var db = Bayazitov_Shoes1Entities.GetContext();
            return (db.Orders.Any() ? db.Orders.Max(o => o.ID_Order) : 0) + 1;
        }

        private void UpdateDeliveryDate()
        {
            bool fast = cartItems.All(ci => ci.Quantity <= 3);
            int days = fast ? 3 : 6;
            DeliveryDateLabel.Text = "Дата доставки: " + DateTime.Now.AddDays(days).ToString("dd.MM.yyyy");
        }

        private void UpdateTotal()
        {
            decimal total = cartItems.Sum(ci => ci.Total);
            TotalTB.Text = total.ToString("0") + " руб.";
            OrderListView.Items.Refresh();
        }

        private void SaveOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }

            var db = Bayazitov_Shoes1Entities.GetContext();

            // Проверка остатков
            foreach (var item in cartItems)
            {
                var stockInDb = db.StockItems.FirstOrDefault(s => s.ID_Item == item.Stock.ID_Item);
                if (stockInDb == null)
                {
                    MessageBox.Show($"Позиция (ID={item.Stock.ID_Item}) не найдена.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (item.Quantity > stockInDb.ItemsQuantity)
                {
                    MessageBox.Show(
                        $"Недостаточно товара «{item.ProductName}» (размер {item.SizeValue}).\n" +
                        $"Запрошено: {item.Quantity}, доступно: {stockInDb.ItemsQuantity}.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                var order = new Orders
                {
                    ID_User = _currentUser.ID_User,
                    OrderDate = DateTime.Now
                };
                db.Orders.Add(order);
                db.SaveChanges(); // получить ID_Order

                foreach (var item in cartItems)
                {
                    db.OrderProducts.Add(new OrderProducts
                    {
                        ID_Order = order.ID_Order,
                        ID_Item = item.Stock.ID_Item,
                        Quantity = item.Quantity
                    });

                    var stock = db.StockItems.First(s => s.ID_Item == item.Stock.ID_Item);
                    stock.ItemsQuantity -= item.Quantity;
                }

                db.SaveChanges();

                MessageBox.Show("Заказ сохранён!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ShoesPage.ClearOrderItems();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PlusBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CartItem;
            if (item == null) return;

            if (item.Quantity >= item.Stock.ItemsQuantity)
            {
                MessageBox.Show(
                    $"На складе доступно только {item.Stock.ItemsQuantity} шт.",
                    "Ограничение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            item.Quantity++;
            UpdateDeliveryDate();
            UpdateTotal();
        }

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CartItem;
            if (item == null) return;

            if (item.Quantity > 1)
                item.Quantity--;
            else
                cartItems.Remove(item);

            UpdateDeliveryDate();
            UpdateTotal();

            if (cartItems.Count == 0)
            {
                this.DialogResult = false;
                this.Close();
            }
        }
    }
}
