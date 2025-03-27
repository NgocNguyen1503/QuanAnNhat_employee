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
    partial class EmployeeTableViewModel : ObservableObject
    {
        public ObservableCollection<Table> Tables { get; set; }

        public EmployeeTableViewModel()
        {
            Tables = new ObservableCollection<Table>();
            GetTables();
        }

        public void GetTables ()
        {
            using (var context = new QuanannhatContext())
            {
                var res = context.Tables.ToArray();
                foreach (Table table in res)
                {
                    Tables.Add(table);
                }
            }
        }
    }
}
