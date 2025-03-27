using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanAnNhat_admin.DBContext;
using QuanAnNhat_admin.Models;
using QuanAnNhat_admin.Singleton;

namespace QuanAnNhat_admin.ViewModels
{
    partial class EmployeeSidebarViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject? _SelectedViewModel;

        public EmployeeSidebarViewModel()
        {
            SelectedViewModel = new EmployeeOrderViewModel();
        }

        [RelayCommand]
        public void ExcuteUpdateView(object parameter)
        {
            Console.WriteLine(parameter.ToString());
            switch (parameter.ToString())
            {
                case "Orders":
                    SelectedViewModel = new EmployeeOrderViewModel();
                    //Console.WriteLine("Order");
                    break;
                case "Tables":
                    SelectedViewModel = new EmployeeTableViewModel();
                    break;
                case "Menu":
                    SelectedViewModel = new EmployeeMenuViewModel();
                    break;
                //case "Bills":
                //    SelectedViewModel = new EmployeeBillViewModel();
                //    break;
            }
        }
    }
}
