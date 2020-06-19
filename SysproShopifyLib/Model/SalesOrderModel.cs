using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysproShopifyLib.Model
{
    public class SalesOrderModel
    {
        public string Customer { get; set; }
        public string CustomerPoNumber { get; set; }
        public List<SalesOrderLine> lines { get; set; } 

    }
    public class SalesOrderLine
    {
        public int CustomerPoLine { get; set; }
        public string StockCode { get; set; }

        public decimal OrderQty { get; set; }
       
    }
}
