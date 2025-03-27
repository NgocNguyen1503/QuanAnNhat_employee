using QuanAnNhat_admin.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuanAnNhat_admin.Singleton
{
    class DataProvider
    {
        private static DataProvider _Ins;
        public static DataProvider Ins
        {
            get
            {
                if (_Ins is null) _Ins = new DataProvider();
                return _Ins;
            }
            set
            {
                _Ins = value;
            }
        }
        public QuanannhatContext Context { get; set; }
        private DataProvider()
        {
            Context = new QuanannhatContext();
        }
    }
}
