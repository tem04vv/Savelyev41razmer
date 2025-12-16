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
    //public partial class OrderWindow : Window
    //{
    //    List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
    //    List<Product> selectedProducts = new List<Product>();
    //    private Order currentOrder = new Order();
    //    private OrderProduct currentOrderProduct = new OrderProduct();
    //    public OrderWindow()
    //    {
    //        InitializeComponent();
    //    }

    //    public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO)
    //    {
    //        InitializeComponent();
    //        var currentPickups = Savelyev41Entities1.GetContext().PickUpPoint.ToList();
    //        PickupCombo.ItemsSource = currentPickups;

    //        ClientTB.Text = FIO;
    //        TBOrderID.Text = selectedOrderProducts.First().OrderID.ToString();

    //        ShoeListView.ItemsSource = selectedProducts;

    //        foreach (Product p in selectedProducts)
    //        {
    //            p.ProductQuantityInStock = 1;
    //            foreach (OrderProduct q in selectedOrderProducts)
    //            {
    //                if (p.ProductArticleNumber == q.ProductArticleNumber)
    //                {
    //                    p.ProductQuantityInStock = q.ProductQuantityInStock;
    //                }
    //            }

    //        }
    //    }
    //}
}
