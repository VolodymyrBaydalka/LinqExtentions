using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuncanApps.DataView
{
    public class OrderClause
    {
        public string Field { get; set; }
        public ListSortDirection Direction { get; set; }

        public override string ToString()
        {
            return $"{Field} {Direction}";
        }

        public override bool Equals(object obj)
        {
            return obj is OrderClause clause && Field == clause.Field && Direction == clause.Direction;
        }

        public override int GetHashCode()
        {
            var hashCode = -652798679;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Field);
            hashCode = hashCode * -1521134295 + Direction.GetHashCode();
            return hashCode;
        }
    }
}
