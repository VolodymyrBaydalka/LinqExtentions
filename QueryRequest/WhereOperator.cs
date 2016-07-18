using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZV.LinqExtentions
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
