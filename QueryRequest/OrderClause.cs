﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZV.LinqExtentions
{
    public class OrderClause
    {
        public string Field { get; set; }
        public ListSortDirection Direction { get; set; }
    }
}