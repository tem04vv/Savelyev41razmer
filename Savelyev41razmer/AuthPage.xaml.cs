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

namespace Savelyev41razmer
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private DispatcherTimer simpleTimer;
        private int blockTime = 10;
        public AuthPage()
        {
            InitializeComponent();
            simpleTimer = new DispatcherTimer();
            simpleTimer.Interval = TimeSpan.FromSeconds(1);
            simpleTimer.Tick += SimpleTimer_Tick;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text;
            string password = PassTB.Text;
            if (login == "" || password == "") 
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            User user = Savelyev41Entities1.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);

            string currentCaptcha = captchaOneWord.Text + captchaTwoWord.Text + captchaThreeWord.Text + captchaFourWord.Text;

            if (user != null) 
            {
                if (captchaTextBlock.Visibility == Visibility.Visible && captchaTextBox.Visibility == Visibility.Visible) 
                {
                    if (captchaTextBox.Text == "") 
                    {
                        MessageBox.Show("В Captch'у ничего не введено");
                    }
                    else
                    {
                        if (currentCaptcha != captchaTextBox.Text) // если логин и пароль ВЕРНЫЕ, а КАПЧА - не правильная
                        {
                            MessageBox.Show("Введена неправильная Captcha");
                            GenerateCaptcha();
                            SimpleBlockTimer();
                            captchaTextBox.Text = "";
                            return;
                        }
                        else // если логин и пароль ВЕРНЫЕ и КАПЧА - правильная
                        {
                            MessageBox.Show("Captcha введена верно!");
                            Manager.MainFrame.Navigate(new ProductPage(user));
                            LoginTB.Text = "";
                            PassTB.Text = "";
                            captchaTextBox.Text = "";
                            captchaTextBlock.Visibility = Visibility.Collapsed;
                            captchaTextBox.Visibility = Visibility.Collapsed;
                        }
                    }

                }
                else // если логин и пароль - ВЕРНЫЕ
                {
                    Manager.MainFrame.Navigate(new ProductPage(user));
                    LoginTB.Text = "";
                    PassTB.Text = "";
                }
            }
            else 
            {
                if (captchaTextBlock.Visibility != Visibility.Visible && captchaTextBox.Visibility != Visibility.Visible) // при 1-ом заходе, генерируется КАПЧА 
                {
                    MessageBox.Show("Введены неверные данные");
                    LoginButton.IsEnabled = false;
                    LoginButton.IsEnabled = true;
                    captchaTextBlock.Visibility = Visibility.Visible;
                    captchaTextBox.Visibility = Visibility.Visible;
                    GenerateCaptcha();
                }
                else // если уже пробовали вводить КАПЧУ (она была неверной) с неверным логином и паролем
                {
                    if (captchaTextBox.Text == "")
                    {
                        MessageBox.Show("В Captch'у ничего не введено");
                    }
                    else
                    {
                        if (currentCaptcha == captchaTextBox.Text && user != null)
                        {
                            MessageBox.Show("Captcha введена верно!");
                            Manager.MainFrame.Navigate(new ProductPage(user));
                            LoginTB.Text = "";
                            PassTB.Text = "";
                            captchaTextBox.Text = "";
                            captchaTextBlock.Visibility = Visibility.Collapsed;
                            captchaTextBox.Visibility = Visibility.Collapsed;
                        }
                        else if (currentCaptcha == captchaTextBox.Text && user == null)
                        {
                            MessageBox.Show("Введены неверные данные");
                            LoginButton.IsEnabled = false;
                            LoginButton.IsEnabled = true;
                            captchaTextBlock.Visibility = Visibility.Visible;
                            captchaTextBox.Visibility = Visibility.Visible;
                            GenerateCaptcha();
                        }
                        else
                        {
                            MessageBox.Show("Введена неправильная Captcha");
                            GenerateCaptcha();
                            SimpleBlockTimer();
                            captchaTextBox.Text = "";
                            return;
                        }
                    }
                }
               
            }
        }

        private void GenerateCaptcha()
        {
            string symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            Random random = new Random();

            captchaOneWord.Text = symbols[random.Next(0, symbols.Length)].ToString();
            captchaTwoWord.Text = symbols[random.Next(0, symbols.Length)].ToString();
            captchaThreeWord.Text = symbols[random.Next(0, symbols.Length)].ToString();
            captchaFourWord.Text = symbols[random.Next(0, symbols.Length)].ToString();
        }
        private void SimpleBlockTimer()
        {
            LoginButton.IsEnabled = false;
            LoginButton.Content = $"Подождите {blockTime} сек.";

            simpleTimer.Start();
        }

        private void SimpleTimer_Tick(object sender, EventArgs e)
        {
            blockTime--;

            if (blockTime <= 0)
            {
                simpleTimer.Stop();
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
                blockTime = 10;
            }
            else
            {
                LoginButton.Content = $"Подождите {blockTime} сек.";
            }
        }
        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {

            User guestUser = new User()
            {
                UserID = 0,
                UserSurname = "Гость",
                UserName = "",
                UserPatronymic = "",
                UserLogin = "",
                UserPassword = "",
                UserRole = 4
            };

            Manager.MainFrame.Navigate(new ProductPage(guestUser));
        }
    }
}
