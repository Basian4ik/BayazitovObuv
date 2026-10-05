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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;

namespace BayazitovObuv
{
    /// <summary>
    /// Логика взаимодействия для ShoesPage.xaml
    /// </summary>
    public partial class ShoesPage : Page
    {
        private Users _currentUser;
        private int _userRole; // роль текущего пользователя

        // Ключ — ID_Product, значение — количество
        private static List<CartItem> _cart = new List<CartItem>();

        public static event Action OnCartCleared;

        public static void ClearOrderItems()
        {
            _cart.Clear();
            OnCartCleared?.Invoke();
        }

        public ShoesPage(Users user)
        {
            InitializeComponent();

            _currentUser = user;
            _userRole = user.ID_Role;

            // FIOTB - TextBlock для отображения ФИО
            FIOTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;

            // RoleTB - TextBlock для отображения роли
            switch (user.ID_Role)
            {
                case 1:
                    RoleTB.Text = "Администратор";
                    break;
                case 2:
                    RoleTB.Text = "Менеджер";
                    break;
                case 3:
                    RoleTB.Text = "Пользователь";
                    break;
                case 4:
                    RoleTB.Text = "Гость";
                    break;
            }

            var currentProducts = Bayazitov_Shoes1Entities.GetContext().Products.ToList();
            ProductListView.ItemsSource = currentProducts;

            ComboType.SelectedIndex = 0;
            UpdateProductes();

            // Гостю убираем контекстное меню (не сможет "Добавить к заказу")
            if (_userRole == 4)
            {
                ProductListView.ContextMenu = null;
            }

            UpdateOrderButtonVisibility();
        }

        private void UpdateProductes()
        {
            var currentShoes = Bayazitov_Shoes1Entities.GetContext().Products.ToList();

            if (ComboType.SelectedIndex == 0)
            {
                currentShoes = currentShoes.Where(p => p.ID_Category >= 0 && p.ID_Category <= 3).ToList();
            }

            if (ComboType.SelectedIndex == 1)
            {
                currentShoes = currentShoes.Where(p => p.ID_Category == 1).ToList();
            }

            if (ComboType.SelectedIndex == 2)
            {
                currentShoes = currentShoes.Where(p => p.ID_Category == 2).ToList();
            }

            if (ComboType.SelectedIndex == 3)
            {
                currentShoes = currentShoes.Where(p => p.ID_Category == 3).ToList();
            }

            currentShoes = currentShoes
                .Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower()))
                .ToList();

            if (RButtonDown.IsChecked == true)
            {
                currentShoes = currentShoes.OrderByDescending(p => p.ProductCost).ToList();
            }

            if (RButtonUp.IsChecked == true)
            {
                currentShoes = currentShoes.OrderBy(p => p.ProductCost).ToList();
            }

            ProductListView.ItemsSource = currentShoes;

            // Счётчик товаров (если есть TextBlock TBlockCount)
            if (TBlockCount != null)
            {
                TBlockCount.Text = $"кол-во {currentShoes.Count} из {Bayazitov_Shoes1Entities.GetContext().Products.Count()}";
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProductes();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProductes();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProductes();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProductes();
        }

        // ============ ЗАКАЗ ============

        private void AddToOrder(Products product)
        {
            if (_userRole == 4)
            {
                MessageBox.Show("Гости не могут оформлять заказы. Войдите в систему.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = Bayazitov_Shoes1Entities.GetContext();

            var availableStocks = context.StockItems
                .Include(s => s.Sizes)
                .Include(s => s.Products)
                .Where(s => s.ID_Product == product.ID_Product && s.ItemsQuantity > 0)
                .ToList();

            if (availableStocks.Count == 0)
            {
                MessageBox.Show($"Товар \"{product.ProductName}\" отсутствует на складе.");
                return;
            }

            StockItems chosenStock;

            if (availableStocks.Count == 1)
            {
                chosenStock = availableStocks[0]; // один размер — не спрашиваем
            }
            else
            {
                var dlg = new SizeSelectionWindow(product, availableStocks)
                {
                    Owner = Application.Current.MainWindow
                };
                if (dlg.ShowDialog() != true) return;
                chosenStock = dlg.SelectedStock;
            }

            var existing = _cart.FirstOrDefault(c => c.Stock.ID_Item == chosenStock.ID_Item);

            if (existing != null)
            {
                if (existing.Quantity >= chosenStock.ItemsQuantity)
                {
                    MessageBox.Show($"На складе доступно только {chosenStock.ItemsQuantity} шт.");
                    return;
                }
                existing.Quantity++;
            }
            else
            {
                _cart.Add(new CartItem { Stock = chosenStock, Quantity = 1 });
            }

            UpdateOrderButtonVisibility();
        }

        private void AddToOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Products selectedProduct)
                AddToOrder(selectedProduct);
            else
                MessageBox.Show("Выберите товар из списка (кликните по строке).");
        }

        private void UpdateOrderButtonVisibility()
        {
            // Гостю кнопку "Просмотреть заказ" вообще не показываем
            if (_userRole == 4)
            {
                ViewOrderBtn.Visibility = Visibility.Collapsed;
                return;
            }

            int totalCount = _cart.Sum(i => i.Quantity);
            ViewOrderBtn.Visibility = totalCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ViewOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 4)
            {
                MessageBox.Show("Гости не могут просматривать заказ.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_cart.Count == 0) return;

            var orderWindow = new OrderWindow(_cart, _currentUser);
            orderWindow.Owner = Application.Current.MainWindow;
            orderWindow.Closed += (s, args) => UpdateOrderButtonVisibility();
            orderWindow.ShowDialog();

            UpdateOrderButtonVisibility();
        }
    }
}
