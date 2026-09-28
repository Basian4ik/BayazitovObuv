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

namespace BayazitovObuv
{
    /// <summary>
    /// Логика взаимодействия для ShoesPage.xaml
    /// </summary>
    public partial class ShoesPage : Page
    {
        public ShoesPage(Users user)
        {
            InitializeComponent();
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

        }

        private void UpdateProductes()
        {
            var currentShoes = Bayazitov_Shoes1Entities.GetContext().Products.ToList();

            if (ComboType.SelectedIndex == 0)
            {
                currentShoes = currentShoes.Where(p => (Convert.ToInt32(p.ID_Category) >= 0 && Convert.ToInt32(p.ID_Category) <= 3)).ToList();
            }

            if (ComboType.SelectedIndex == 1)
            {
                currentShoes = currentShoes.Where(p => (Convert.ToInt32(p.ID_Category) == 1)).ToList();
            }

            if (ComboType.SelectedIndex == 2)
            {
                currentShoes = currentShoes.Where(p => (Convert.ToInt32(p.ID_Category) == 2)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentShoes = currentShoes.Where(p => (Convert.ToInt32(p.ID_Category) == 3)).ToList();
            }

            currentShoes = currentShoes.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            ProductListView.ItemsSource = currentShoes.ToList();

            if (RButtonDown.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentShoes.OrderByDescending(p => p.ProductCost).ToList();
            }

            if (RButtonUp.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentShoes.OrderBy(p => p.ProductCost).ToList();
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

    }
}
