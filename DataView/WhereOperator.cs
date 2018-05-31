using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuncanApps.DataView
{
    public enum WhereOperator
    {
        IsEqualTo,
        IsNotEqualTo,
        IsLessThan,
        IsLessThanOrEqualTo,
        IsGreaterThanOrEqualTo,
        IsGreaterThan,
        StartsWith,
        EndsWith,
        Contains
    }
}
