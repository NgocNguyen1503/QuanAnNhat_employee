using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanAnNhat_admin.DBContext;
using QuanAnNhat_admin.Models;
using QuanAnNhat_admin.Singleton;
using QuanAnNhat_admin.Views;
using Windows.Media.Streaming.Adaptive;

namespace QuanAnNhat_admin.ViewModels
{
    partial class EmployeeMenuViewModel : ObservableObject
    {
        public ObservableCollection<Dishlist> Dishlist { get; set; }
        public ObservableCollection<Dish> Dishes { get; set; }
        public ObservableCollection<Table> Tables { get; set; }
        public ObservableCollection<Dish> Cart { get; set; }

        public Dish _selectedDish;
        public Dish? selectedDish
        {
            get => _selectedDish;
            set
            {
                SetProperty(ref _selectedDish, value);
            }
        }
        EditDishQuantity editDishQuantityView;
        private Table? _SelectedTable;

        private string? _Note;
        private int? _TotalPrice = 0;
        private string? _SearchText;
        public string? SearchText
        {
            get => _SearchText;
            set
            {
                SetProperty(ref _SearchText, value);
                SearchDishes(SearchText);
            }
        }
        public Table? SelectedTable
        {
            get => _SelectedTable;
            set
            {
                SetProperty(ref _SelectedTable, value);
            }
        }

        public string? Notes
        {
            get => _Note;
            set
            {
                SetProperty(ref _Note, value);
            }
        }
        public int? totalPrice
        {
            get => _TotalPrice;
            set
            {
                SetProperty(ref _TotalPrice, value);
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private ObservableCollection<Table> _Tables;
        //public ObservableCollection<Table> Tables
        //{
        //    get => _Tables;
        //    set => SetProperty(ref _Tables, value);
        //}

        public EmployeeMenuViewModel ()
        {
            Dishlist = new ObservableCollection<Dishlist>();
            Dishes = new ObservableCollection<Dish>();
            Tables = new ObservableCollection<Table>();
            Cart = new ObservableCollection<Dish>();

            GetDishlist();
            GetDishes();
            GetListTable();
            _ = LiveDataQuantity();
        }

        public void GetDishlist ()
        {
            using (var context = new QuanannhatContext())
            {
                var res = context.Dishlists.ToArray();
                foreach (Dishlist dishlist in res)
                {
                    Dishlist.Add(dishlist);
                }
            }
        }

        public void GetDishes ()
        {
            using (var context = new QuanannhatContext())
            {
                var res = context.Dishes.ToArray();
                foreach (Dish dish in res)
                {
                    //Console.WriteLine($"DishId: {dish.Id}");
                    Dishes.Add(dish);
                }
            }
        }

        public void SearchDishes(string? searchText)
        {
            Dishes.Clear();
            using (var context = new QuanannhatContext())
            {
                var _dishes = context.Dishes.Where(dish => dish.Name.Contains(searchText)).ToList();
                foreach (var item in _dishes)
                {
                    Dishes.Add(item);
                }
            }
        }

        [RelayCommand]
        public void GetDishesByMenu(string? dishlistName)
        {
            Dishes.Clear();
            using (var context = new QuanannhatContext())
            {
                var dishlist = context.Dishlists.Where(dishlist => dishlist.Name == dishlistName).First();
                var _dishes = context.Dishes.Where(dish => dish.DishlistId == dishlist.Id).ToList();
                foreach (Dish dish in _dishes)
                {
                    Dishes.Add(dish);
                }
            }
        }

        public void GetListTable ()
        {
            using (var context = new QuanannhatContext())
            {
                var res = context.Tables.ToArray();
                foreach (Table table in res)
                {
                    //Console.WriteLine($"TableId: {table.Id}");
                    Tables.Add(table);
                }
            }
        }

        public void AddToCart(int dishId)
        {
            var dish = DataProvider.Ins.Context.Dishes.FirstOrDefault(x => x.Id == dishId);
            using (var context = new QuanannhatContext())
            {
                var availableDish = context.Dishes.Find(dishId);
                if (availableDish != null && availableDish.Quantity > 0)
                {
                    var cartItem = Cart.FirstOrDefault(x => x.Id == dishId);
                    if (cartItem == null)
                    {
                        dish.Quantity = 1;
                        Cart.Add(dish);
                        totalPrice += dish.Price;
                        //Console.WriteLine(totalPrice);
                    }
                    else if (availableDish.Quantity > cartItem.Quantity)
                    {
                        cartItem.Quantity++;
                        totalPrice += dish.Price;
                        //Console.WriteLine(totalPrice);
                    }
                }
            }
        }

        [RelayCommand]
        public void Plus(object parameter)
        {
            using (var context = new QuanannhatContext())
            {
                foreach (var dish in Cart.ToList())
                {
                    if (dish.Name.Equals(parameter))
                    {
                        var availableDish = context.Dishes.FirstOrDefault(x => x.Id == dish.Id);
                        if (availableDish != null && availableDish.Quantity > dish.Quantity)
                        {
                            dish.Quantity++;
                            totalPrice += dish.Price;
                            //Console.WriteLine(totalPrice);
                        }
                    }
                }
            }
        }

        public async Task LiveDataQuantity()
        {
            while (true)
            {
                await Task.Delay(1000);

                using (var context = new QuanannhatContext())
                {
                    foreach (var dish in Dishes)
                    {
                        var dbDish = context.Dishes.FirstOrDefault(d => d.Id == dish.Id);
                        if (dbDish != null && dbDish.Quantity != dish.Quantity)
                        {
                            dish.Quantity = dbDish.Quantity;
                        }
                    }
                }
            }
        }

        [RelayCommand]
        public void Minus(object parameter)
        {
            foreach (var dish in Cart.ToList())
            {
                if (dish.Name.Equals(parameter))
                {
                    dish.Quantity -= 1;
                    totalPrice = totalPrice - dish.Price;
                    if (dish.Quantity < 1)
                    {
                        Cart.Remove(dish);
                    }
                }
            }
        }

        public bool ValidateOrder()
        {
            if (SelectedTable == null)
            {
                MessageBox.Show("Vui lòng chọn bàn");
                return false;
            }
            if (SelectedTable.Status == 1)
            {
                MessageBox.Show("Bàn đã được đặt, vui lòng chọn bàn khác");
                return false;
            }
            if (Cart.Count < 1)
            {
                MessageBox.Show("Giỏ hàng trống, vui lòng chọn món");
                return false;
            }

            using (var context = new QuanannhatContext())
            {
                foreach (var dish in Cart)
                {
                    var dbDish = context.Dishes.FirstOrDefault(d => d.Id == dish.Id);
                    if (dbDish == null) continue;

                    if (dbDish.Quantity == 0)
                    {
                        MessageBox.Show($"Món {dish.Name} đã hết, vui lòng chọn món khác.");
                        return false;
                    }

                    if (dbDish.Quantity < dish.Quantity)
                    {
                        MessageBox.Show($"Món {dish.Name} chỉ còn {dbDish.Quantity} phần, vui lòng giảm số lượng.");
                        return false;
                    }
                }
            }

            return true;
        }

        public void createBill ()
        {
            DateTime dateTime = DateTime.Now;
            using (var context = new QuanannhatContext())
            {
                int _orderId = context.OrderBills.Count() + 1;
                OrderBill orderBill = new OrderBill()
                {
                    Id = _orderId,
                    UserId = 10,
                    TotalPrice = (int)totalPrice,
                    OrderStatus = 2,
                    BillStatus = 3,
                    Note = Notes,
                    TableId = SelectedTable.Id
                };
                context.OrderBills.Add(orderBill);
                context.SaveChanges();
            }

            using (var context = new QuanannhatContext())
            {
                context.Tables.Where(table => table.Id == SelectedTable.Id)
                    .ExecuteUpdate(tbl => tbl.SetProperty(tbl => tbl.Status, 1));
                SelectedTable = null;
            }
        }

        public void CreateOrders()
        {
            using (var context = new QuanannhatContext())
            {
                int _orderId = context.Orders.Count();
                var res = context.Dishes.ToList();

                foreach (var dish in Cart)
                {
                    var dbDish = res.FirstOrDefault(d => d.Id == dish.Id);
                    if (dbDish != null)
                    {
                        _orderId += 1;
                        Order order = new Order()
                        {
                            Id = _orderId,
                            DishId = dish.Id,
                            Quantity = dish.Quantity,
                            TotalPrice = (int)(dish.Price * dish.Quantity),
                            OrderbillId = context.OrderBills.Count()
                        };
                        context.Orders.Add(order);

                        dbDish.Quantity -= dish.Quantity;
                    }
                }
                context.SaveChanges();
            }

            MessageBox.Show("Đặt món thành công!");
            Cart.Clear();
            totalPrice = 0;
        }

        [RelayCommand]
        public void ShowEditQuantityView(int dishId)
        {
            editDishQuantityView = new();
            editDishQuantityView.DataContext = this;
            editDishQuantityView.Show();
            using (var context = new QuanannhatContext())
            {
                selectedDish = context.Dishes.Find(dishId);
            }
        }

        [RelayCommand]
        public void EditQuantity (string _quantity)
        {
            int quantity = Convert.ToInt32(_quantity);
            Console.WriteLine(quantity);
            using (var context = new QuanannhatContext())
            {
                context.Dishes.Where(dish => dish.Id == selectedDish.Id)
                .ExecuteUpdate(update => update.SetProperty(dish => dish.Quantity, quantity));
            }
            editDishQuantityView.Close();
        }

        [RelayCommand]
        public void ExcuteAddToCart(int dishId)
        {
            Console.WriteLine($"DishId: {dishId}");
            AddToCart(dishId);
        }

        [RelayCommand]
        public void ExcuteOrder ()
        {
            if (ValidateOrder())
            {
                createBill();
                CreateOrders();
            }
        }
    }
}
