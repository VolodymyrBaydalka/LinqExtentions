using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuncanApps.DataView
{
    public class DataView<T>
    {
        public IList<T> Items { get; set; }
        public int Skipped { get; set; }
        public int Taken { get; set; }
        public int Total { get; set; }
    }
}
