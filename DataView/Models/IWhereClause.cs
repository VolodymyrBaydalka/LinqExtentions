using DuncanApps.DataView.Converters;
using System.ComponentModel;

namespace DuncanApps.DataView
{
    [TypeConverter(typeof(WhereClauseConverter))]
    public interface IWhereClause
    {
    }
}
