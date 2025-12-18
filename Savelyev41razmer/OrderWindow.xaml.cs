using Savelyev41razmer;
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

namespace Savelyev41razmer
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        private List<Product> selectedProducts = new List<Product>();
        private Order currentOrder = new Order();
        private User _currentUser;
        private Product product;

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, User user)
        {
            InitializeComponent();
            var currentPickups = Savelyev41Entities1.GetContext().PickUpPoint.ToList();
            PickUpPointCombo.ItemsSource = currentPickups;
            if (user != null)
            {
                ClientTB.Text = $"{user.UserSurname} {user.UserName} {user.UserPatronymic}";
            }
            else
            {
                ClientTB.Text = "Гость";
            }

            _currentUser = user;


            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;

            foreach (Product p in selectedProducts)
            {
                p.Quantity = 1;
                foreach (OrderProduct q in selectedOrderProducts)
                {
                    if (p.ProductArticleNumber == q.ProductArticleNumber)
                        p.Quantity = q.Count;
                }
            }

            ShoeListView.ItemsSource = selectedProducts;
            DateTime orderDate = DateTime.Now;

            var lastOrder = Savelyev41Entities1.GetContext().Order.OrderByDescending(o => o.OrderID).FirstOrDefault();
            int newOrderNumber = lastOrder != null ? lastOrder.OrderID + 1 : 1;
            OrderID.Text = newOrderNumber.ToString();
            SetDeliveryDate();
            UpdateTotals();
        }

        private void SetDeliveryDate()
        {
            bool fastDeliveryPossible = true;

            foreach (Product product in selectedProducts)
            {
                var orderProduct = selectedOrderProducts
                    .FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

                if (orderProduct != null)
                {
                    if (orderProduct.Count > product.ProductQuantityInStock || product.ProductQuantityInStock < 3)
                    {
                        fastDeliveryPossible = false;
                        break;
                    }
                }
                else
                {
                    fastDeliveryPossible = false;
                    break;
                }
            }
            int deliveryDays = fastDeliveryPossible ? 3 : 6;

            DateTime orderDate = DateTime.Now;
            OrderDP.Text = DateTime.Now.ToString("dd.MM.yyyy");

            var deliveryDate = orderDate.AddDays(deliveryDays);


            if (OrderDeliveryDate != null)
            {
                OrderDeliveryDate.Text = deliveryDate.ToString("dd.MM.yyyy");
            }
            else
            {
                MessageBox.Show($"Дата доставки: {deliveryDate.ToString("dd.MM.yyyy")}");
            }
        }

        private void UpdateTotals()
        {
            decimal totalAmount = 0;
            decimal totalDiscount = 0;

            foreach (Product product in selectedProducts)
            {
                foreach (OrderProduct op in selectedOrderProducts)
                {
                    if (product.ProductArticleNumber == op.ProductArticleNumber)
                    {
                        int quantity = op.Count;
                        totalAmount += product.ProductCost * quantity;
                        totalDiscount += product.ProductCost * quantity * (product.ProductDiscountAmount / 100m);
                    }
                }
            }

            decimal finalAmount = totalAmount - totalDiscount;

        }

        private void OrderDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDeliveryDate();
        }

        private void minusButton_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;
            if (prod == null) return;

            if (prod.Quantity > 1)
            {
                prod.Quantity--;
                var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
                if (selectedOP != null)
                {
                    int index = selectedOrderProducts.IndexOf(selectedOP);
                    selectedOrderProducts[index].Count--;
                }
                SetDeliveryDate();
                UpdateTotals();
                ShoeListView.Items.Refresh();
            }
        }

        private void plusButton_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;
            if (prod == null) return;

            prod.Quantity++;

            var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
            if (selectedOP != null)
            {
                int index = selectedOrderProducts.IndexOf(selectedOP);
                selectedOrderProducts[index].Count++;
            }

            SetDeliveryDate();
            UpdateTotals();
            ShoeListView.Items.Refresh();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Product product)
            {
                var result = MessageBox.Show("Удалить товар из заказа?",
                    "Подтверждение", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    selectedProducts.Remove(product);
                    var orderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);
                    if (orderProduct != null)
                    {
                        selectedOrderProducts.Remove(orderProduct);
                    }

                    ShoeListView.ItemsSource = null;
                    ShoeListView.ItemsSource = selectedProducts;
                    SetDeliveryDate();
                    UpdateTotals();

                    if (selectedProducts.Count == 0)
                    {
                        MessageBox.Show("Заказ пуст");
                        ProductPage.ClearOrderItems();
                        this.DialogResult = false;
                        this.Close();
                    }
                }
            }
        }

        private void SaveOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedProducts.Count == 0)
                {
                    MessageBox.Show("Добавьте товары в заказ!");
                    return;
                }

                if (PickUpPointCombo.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пункт выдачи!");
                    return;
                }

                var selectedPickupPoint = PickUpPointCombo.SelectedItem as PickUpPoint;
                var context = Savelyev41Entities1.GetContext();

                DateTime orderDate = DateTime.Now;
                DateTime deliveryDate;
                if (!DateTime.TryParse(OrderDP.Text, out deliveryDate))
                {
                    deliveryDate = orderDate.AddDays(3);
                }

                int? clientId = null;
                if (_currentUser != null)
                {
                    var existingUser = context.User.FirstOrDefault(u => u.UserID == _currentUser.UserID);
                    if (existingUser != null)
                    {
                        clientId = _currentUser.UserID;
                    }
                    else
                    {
                        clientId = null;
                    }
                }

                currentOrder = new Order
                {
                    OrderDate = orderDate,
                    OrderDeliveryDate = deliveryDate,
                    OrderPickupPoint = selectedPickupPoint.PickUpPointID,
                    OrderClientID = clientId,
                    OrderCode = new Random().Next(100000, 999999),
                    OrderStatus = "Новый"
                };

                context.Order.Add(currentOrder);
                context.SaveChanges();

                foreach (var op in selectedOrderProducts)
                {
                    op.OrderID = currentOrder.OrderID;
                    context.OrderProduct.Add(op);

                    var product = context.Product.FirstOrDefault(p => p.ProductArticleNumber == op.ProductArticleNumber);
                    if (product != null)
                    {
                        product.ProductQuantityInStock -= op.Count;
                    }
                }

                context.SaveChanges();

                MessageBox.Show($"Заказ №{currentOrder.OrderID} оформлен!\nКод: {currentOrder.OrderCode}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
                ProductPage.ClearOrderItems();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
            {
                if (dbEx.InnerException?.InnerException is System.Data.SqlClient.SqlException sqlEx)
                {
                    if (sqlEx.Number == 547)
                    {
                        MessageBox.Show($"Ошибка: Невозможно сохранить заказ. " +
                            $"Проверьте корректность данных пользователя (UserID: {_currentUser?.UserID}).",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                        SaveOrderAsGuest();
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка при сохранении заказа: {dbEx.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveOrderAsGuest()
        {
            try
            {
                var selectedPickupPoint = PickUpPointCombo.SelectedItem as PickUpPoint;
                var context = Savelyev41Entities1.GetContext();

                DateTime deliveryDate;
                if (!DateTime.TryParse(OrderDP.Text, out deliveryDate))
                {
                    deliveryDate = DateTime.Now.AddDays(3);
                }

                currentOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    OrderDeliveryDate = deliveryDate,
                    OrderPickupPoint = selectedPickupPoint.PickUpPointID,
                    OrderClientID = null,
                    OrderCode = new Random().Next(100000, 999999),
                    OrderStatus = "Новый"
                };

                context.Order.Add(currentOrder);
                context.SaveChanges();

                foreach (var op in selectedOrderProducts)
                {
                    op.OrderID = currentOrder.OrderID;
                    context.OrderProduct.Add(op);

                    var product = context.Product.FirstOrDefault(p => p.ProductArticleNumber == op.ProductArticleNumber);
                    if (product != null)
                    {
                        product.ProductQuantityInStock -= op.Count;
                    }
                }

                context.SaveChanges();

                MessageBox.Show($"Гостевой заказ №{currentOrder.OrderID} оформлен!\nКод: {currentOrder.OrderCode}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
                ProductPage.ClearOrderItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении гостевого заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
    public partial class Product
    {
        public int Quantity { get; set; }
        public decimal TotalPrice => ProductCost * Quantity;
    }

    public partial class PickUpPoint
    {
        public string FullAddress => $"{PickUpPointIndex} г. {PickUpCity}, ул. {PickUpStreet}, д. {PickUpHouse}";
    }
}