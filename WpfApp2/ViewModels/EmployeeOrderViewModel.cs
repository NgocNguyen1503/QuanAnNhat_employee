using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using QuanAnNhat_admin.DBContext;
using QuanAnNhat_admin.Models;
using QuanAnNhat_admin.Singleton;

namespace QuanAnNhat_admin.ViewModels
{
    partial class EmployeeOrderViewModel : ObservableObject
    {
        public ObservableCollection<OrderBill> orderBills { get; set; }
        public ObservableCollection<Order> orders { get; set; }

        private int _discount;
        private int _finalPrice;
        public int SelectedStatus;
        private int tmpCount;

        private OrderBill _orderBill;
        private Order _order;
        public OrderBill orderBill
        {
            get => _orderBill;
            set => SetProperty(ref _orderBill, value);
        }
        public Order order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }
        public int discount
        {
            get => _discount;
            set => SetProperty(ref _discount, value);
        }
        public int finalPrice
        {
            get => _finalPrice;
            set => SetProperty(ref _finalPrice, value);
        }

        public EmployeeOrderViewModel()
        {
            orderBills = new ObservableCollection<OrderBill>();
            orders = new ObservableCollection<Order>();
            tmpCount = 0;

            //GetOrders(2);
            GetOrderBill(2);

            NotificationDelay();

            SelectedStatus = 2;
            LiveData();
        }

        public async Task NotificationDelay()
        {
            await Task.Delay(5000);
            orderBills.CollectionChanged += change;
        }

        public async void GetOrders(int status)
        {
            orderBills.Clear();
            using (var context = new QuanannhatContext())
            {
                var res = await context.OrderBills.Where(orderBills => orderBills.OrderStatus == status)
                    .Include(orderBills => orderBills.User)
                    .ThenInclude(user => user.Information)
                    .ToListAsync();
                foreach (OrderBill order in res)
                {
                    orderBills.Add(order);
                }
            }
            if (status == 2)
            {
                tmpCount = orderBills.Count;
            }
        }

        public async void GetOrderBill(int status)
        {
            using (var context = new QuanannhatContext())
            {
                var res = await context.OrderBills.Where(orderBills => orderBills.OrderStatus == status 
                && orderBills.BillStatus == 3)
                    .Include(orderBills => orderBills.User)
                    .ThenInclude(user => user.Information)
                    .ToListAsync();
                foreach (OrderBill orderBill in res)
                {
                    if (orderBill.OrderStatus == 2 && !orderBills.Any(bill => bill.Id == orderBill.Id))
                    {
                        orderBills.Add(orderBill);
                        //Console.WriteLine("get order");
                    }
                }

            }
        }

        [RelayCommand]
        public void GetSelectedStatus(string _status)
        {
            //Console.WriteLine("status");
            int status = Convert.ToInt32(_status);
            Console.WriteLine(status);
            GetOrders(status);
            SelectedStatus = status;
        }

        [RelayCommand]
        public void UpdateStatus(int orderId)
        {
            foreach (OrderBill order in orderBills.ToList())
            {
                if (order.Id == orderId && order.OrderStatus != 4)
                {
                    orderBills.Remove(order);

                    if (order.OrderStatus == 2)
                    {
                        using (var context = new QuanannhatContext())
                        {
                            context.OrderBills.Where(orderBills => orderBills.Id == orderId)
                                .ExecuteUpdate(update => update.SetProperty(orderBills => orderBills.OrderStatus, 3));
                        }
                    }
                    else if (order.OrderStatus == 3)
                    {
                        using (var context = new QuanannhatContext())
                        {
                            context.OrderBills.Where(orderBills => orderBills.Id == orderId)
                                .ExecuteUpdate(update => update.SetProperty(orderBills => orderBills.OrderStatus, 4));
                            context.Tables.Where(table => table.Id == order.TableId)
                                .ExecuteUpdate(update => update.SetProperty(table => table.Status, 2));
                        }
                    }

                }
            }
        }

        [RelayCommand]
        public void GetBillDetail(int billId)
        {
            orders.Clear();
            using (var context = new QuanannhatContext())
            {
                orderBill = context.OrderBills.Where(orderBills => orderBills.Id == billId)
                    .Include(orderBills => orderBills.User)
                    .ThenInclude(user => user.Information)
                    .First();
                var res = context.Orders.Where(order => order.OrderbillId == billId)
                    .Include(order => order.Dish)
                    .ToList();
                foreach (Order order in res)
                {
                    orders.Add(order);
                    Console.WriteLine(order.Dish.Name);
                }
                if (orderBill.DiscountId == null)
                {
                    finalPrice = orderBill.TotalPrice;
                }
                else
                {
                    discount = orderBill.Discount.DiscountPrice;
                    finalPrice = orderBill.TotalPrice - orderBill.Discount.DiscountPrice;
                }
            }
        }

        [RelayCommand]
        public void ExportBillToPDF(int billId)
        {
            try
            {
                PdfDocument billPdf = new PdfDocument();
                PdfPage page = billPdf.AddPage();
                XGraphics graphics = XGraphics.FromPdfPage(page);

                XFont titleFont = new XFont("Arial", 24);
                XFont headerFont = new XFont("Arial", 18);
                XFont itemFont = new XFont("Arial", 16);
                XFont smallFont = new XFont("Arial", 14);
                XFont totalFont = new XFont("Arial", 20);

                int pageWidth = (int)page.Width;
                int startX = 100, startY = 50, lineSpacing = 30;
                int itemStartY = startY + 80;

                // Căn giữa tiêu đề
                string title = $"Saku Ramen Bill {billId}";
                double titleWidth = graphics.MeasureString(title, titleFont).Width;
                graphics.DrawString(title, titleFont, XBrushes.Brown, new XPoint((pageWidth - titleWidth) / 2, startY));

                // Tên khách hàng & thời gian
                string customerInfo = $"Customer name: {orderBill.User.Information.Name}";

                double customerWidth = graphics.MeasureString(customerInfo, headerFont).Width;
                double billTimeWidth = graphics.MeasureString(orderBill.Time.ToString(), smallFont).Width;

                int customerX = startX;
                int billTimeX = pageWidth - startX - (int)billTimeWidth; // Căn phải

                graphics.DrawString(customerInfo, headerFont, XBrushes.Black, new XPoint(customerX, startY + lineSpacing));
                graphics.DrawString(orderBill.Time.ToString(), smallFont, XBrushes.Black, new XPoint(billTimeX, startY + lineSpacing));

                // Danh sách món ăn
                int yOffset = itemStartY;
                decimal subtotal = 0;

                foreach (Order order in orders)
                {
                    decimal totalItemPrice = order.TotalPrice;
                    subtotal += totalItemPrice;

                    // Tên món ăn
                    graphics.DrawString(order.Dish.Name, itemFont, XBrushes.Brown, new XPoint(startX, yOffset));

                    // Giá món ăn
                    graphics.DrawString(order.Dish.Price.ToString(), smallFont, XBrushes.Gray, new XPoint(startX, yOffset + lineSpacing));

                    // Số lượng
                    graphics.DrawString($"x {order.Quantity}", smallFont, XBrushes.Gray, new XPoint(startX + 100, yOffset + lineSpacing));

                    // Tổng tiền món ăn
                    graphics.DrawString($"$ {totalItemPrice}", itemFont, XBrushes.Black, new XPoint(startX + 300, yOffset));

                    yOffset += lineSpacing * 2;
                }

                // Kẻ đường ngang phân cách
                graphics.DrawLine(XPens.Black, startX, yOffset, startX + 350, yOffset);
                yOffset += lineSpacing;

                // Tổng tiền trước giảm giá
                graphics.DrawString("Subtotal", headerFont, XBrushes.Brown, new XPoint(startX, yOffset));
                graphics.DrawString(subtotal.ToString(), headerFont, XBrushes.Black, new XPoint(startX + 300, yOffset));

                yOffset += lineSpacing;

                // Giảm giá
                graphics.DrawString("Discount", smallFont, XBrushes.Gray, new XPoint(startX, yOffset));
                graphics.DrawString("0", smallFont, XBrushes.Gray, new XPoint(startX + 300, yOffset));

                yOffset += lineSpacing;

                // Tổng tiền cuối cùng
                graphics.DrawString("Total", totalFont, XBrushes.Brown, new XPoint(startX, yOffset));
                graphics.DrawString(subtotal.ToString(), totalFont, XBrushes.Black, new XPoint(startX + 300, yOffset));

                // Lưu file
                string fileName = $@"C:\Users\tomng\Downloads\SakuRamenBill_{billId}.pdf";
                billPdf.Save(fileName);

                MessageBox.Show("Xuất PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất PDF: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void LiveData()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (SelectedStatus == 2)
                {
                    GetOrderBill(2);
                }
                if (SelectedStatus == 3)
                {
                    GetOrderBill(3);
                }
            }
        }

        private void change(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Console.WriteLine($"Added!");
                    //if (SelectedStatus == 2 && orderBills.Count != tmpCount)
                    //{
                    //    SendToastNotification();
                    //}
                    break;
            }
        }

        public static void SendToastNotification()
        {
            new ToastContentBuilder().AddText("Bạn có đơn hàng mới!").Show();
        }
    }
}
