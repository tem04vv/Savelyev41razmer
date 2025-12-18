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

namespace Savelyev41razmer
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public static List<Product> CurrentProducts = new List<Product>();
        private User _currentUser;
        private static Dictionary<string, int> _orderItems = new Dictionary<string, int>();

        public static event Action OnCartCleared;

        public static void ClearOrderItems()
        {
            _orderItems.Clear();
            OnCartCleared?.Invoke();
        }
        public ProductPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            OnCartCleared += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    OrderBtn.Visibility = Visibility.Collapsed;
                });
            };
            Loaded += (s, e) =>
            {
                if (_orderItems.Count == 0)
                {
                    OrderBtn.Visibility = Visibility.Collapsed;
                }
            };
            if (user != null)
            {
                FIOTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                switch (user.UserRole)
                {
                    case 1:
                        RoleTB.Text = "Клиент"; break;
                    case 2:
                        RoleTB.Text = "Менеджер"; break;
                    case 3:
                        RoleTB.Text = "Администратор"; break;
                    case 4:
                        RoleTB.Text = "Гость"; break;
                }
            }
            else
            {
                FIOTB.Text = " Вы вошли как гость";
            }
            var currentProduct = Savelyev41Entities1.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProduct;

            ComboType.SelectedIndex = 0;
            UpdateProduct();
            UpdateOrderButtonVisibility();
        }
        public static event EventHandler OrderItemsChanged;

        private static void OnOrderItemsChanged()
        {
            OrderItemsChanged?.Invoke(null, EventArgs.Empty);
        }
        private void AddToOrder(Product product)
        {
            string article = product.ProductArticleNumber;

            if (_orderItems.ContainsKey(article))
            {
                _orderItems[article]++;
            }
            else
            {
                _orderItems[article] = 1;
            }

            UpdateOrderButtonVisibility();
            MessageBox.Show($"Товар \"{product.ProductName}\" добавлен к заказу");
            OnOrderItemsChanged();
            UpdateOrderButtonVisibility();
        }
        private void UpdateOrderButtonVisibility()
        {
            bool hasItems = _orderItems.Count > 0;

            int totalCount = _orderItems.Sum(item => item.Value);
            hasItems = totalCount > 0;

            OrderBtn.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProduct();
        }

        //private void RadioButton_Checked(object sender, RoutedEventArgs e)
        //{

        //}

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        int count;
        int count2;
        private void UpdateProduct()
        {
            var currentProduct = Savelyev41Entities1.GetContext().Product.ToList();
            count = currentProduct.Count;

            if (TBoxSearch.Text.Length > 0)
                currentProduct = currentProduct.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();


            if (ComboType.SelectedIndex == 0)
            {
                currentProduct = currentProduct.Where(p => (p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount <= 100)).ToList();
            }
            if (ComboType.SelectedIndex == 1)
            {
                currentProduct = currentProduct.Where(p => (p.ProductDiscountAmount > 0 && p.ProductDiscountAmount <= 9)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentProduct = currentProduct.Where(p => (p.ProductDiscountAmount > 9 && p.ProductDiscountAmount <= 14)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentProduct = currentProduct.Where(p => (p.ProductDiscountAmount > 14 && p.ProductDiscountAmount <= 100)).ToList();
            }

            currentProduct = currentProduct.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            ProductListView.ItemsSource = currentProduct.ToList();

            if (RButtonDown.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentProduct.OrderByDescending(p => p.ProductCost).ToList();
            }
            if (RButtonUp.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentProduct.OrderBy(p => p.ProductCost).ToList();
            }

            count2 = currentProduct.Count;
            TBAllRecords.Text = "кол-во " + count2.ToString() + " из " + count.ToString();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Product selectedProduct)
            {
                AddToOrder(selectedProduct);
            }
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrderProducts = new List<OrderProduct>();
            var selectedProducts = new List<Product>();

            foreach (var item in _orderItems)
            {
                var product = Savelyev41Entities1.GetContext().Product
                    .FirstOrDefault(p => p.ProductArticleNumber == item.Key);

                if (product != null)
                {
                    selectedProducts.Add(product);
                    var orderProduct = new OrderProduct
                    {
                        ProductArticleNumber = product.ProductArticleNumber,
                        Count = item.Value
                    };
                    selectedOrderProducts.Add(orderProduct);
                }
            }

            var orderWindow = new OrderWindow(selectedOrderProducts, selectedProducts, _currentUser);
            orderWindow.Owner = Application.Current.MainWindow;

            orderWindow.Closed += (s, args) =>
            {
                UpdateOrderButtonVisibility();

                if (_orderItems.Count == 0)
                {
                    OrderBtn.Visibility = Visibility.Collapsed;
                }
            };

            orderWindow.ShowDialog();

            UpdateOrderButtonVisibility();
        }

    }
}
