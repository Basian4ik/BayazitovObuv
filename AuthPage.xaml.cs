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
using System.Windows.Threading;

namespace BayazitovObuv
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {

        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Users guestUser = new Users
            {
                UserLogin = "Гость",
                UserName = "Гость",
                ID_Role = 4
            };

            NavigationService.Navigate(new ShoesPage(guestUser));
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text;

            if (login == "")
            {
                MessageBox.Show("Заполните поле");
                return;
            }


            Users user = Bayazitov_Shoes1Entities.GetContext().Users.ToList()
                .Find(p => p.UserLogin == login);

            if (user != null)
            {
                // Сброс счетчика неудачных попыток
                failedAttempts = 0;

                NavigationService.Navigate(new ShoesPage(user));
                LoginTB.Text = "";
            }
            else
            {
                failedAttempts++;

                if (failedAttempts == 1)
                {
                    MessageBox.Show("Неверный логин. Повторите попытку.");
                    
                }
            }
        }
    }
}
