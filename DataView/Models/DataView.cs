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

        public static DataView<T> From(IList<T> items, int skipped = 0, int taken = 0)
        {
            return new DataView<T>
            {
                Items = items,
                Total = items.Count,
                Skipped = skipped,
                Taken = taken
            };
        }
    }
}
