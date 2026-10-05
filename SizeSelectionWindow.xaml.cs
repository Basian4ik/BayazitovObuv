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
    /// Логика взаимодействия для SizeSelectionWindow.xaml
    /// </summary>
    public partial class SizeSelectionWindow : Window
    {
        public StockItems SelectedStock { get; private set; }

        public SizeSelectionWindow(Products product, List<StockItems> availableStocks)
        {
            InitializeComponent();
            ProductNameTB.Text = product.ProductName;

            // обёртка для красивого отображения
            var items = availableStocks.Select(s => new
            {
                Stock = s,
                DisplayText = $"Размер {s.Sizes.Size} — остаток {s.ItemsQuantity} шт."
            }).ToList();

            SizesList.ItemsSource = items;
            SizesList.DisplayMemberPath = "DisplayText";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (SizesList.SelectedItem == null)
            {
                MessageBox.Show("Выберите размер.");
                return;
            }

            dynamic selected = SizesList.SelectedItem;
            SelectedStock = selected.Stock;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
