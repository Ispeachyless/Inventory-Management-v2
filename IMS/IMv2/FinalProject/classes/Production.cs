using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    internal class Production
    {
        private int productionId;
        private int productId;
        private int qty;
        private int qtyUpdate;
        public int ProductionID { get => productionId; set => productionId = value; }
        public int Quantity { get => qty; set => qty = value; }
        public int ProductID { get => productId; set => productId = value; }
        public int QtyUpdate { get => qtyUpdate; set => qtyUpdate = value; }

    }
    internal class TransitOrder
    {
        private int productId;
        private int totalSalesQty;
        private int totalProductionQty;
        public int ProductId { get => productId; set => productId = value; }
        public int TotalSalesQty { get => totalSalesQty; set => totalSalesQty = value; }
        public int TotalProductionQty { get => totalProductionQty; set => totalProductionQty = value; }
    }
    internal class Complete
    {
        private int invoice;
        private int productID;
        private double unitPrice;
        public int Invoice { get => invoice; set => invoice = value; }
        public int ProductID { get => productID; set => productID = value; }
        public double UnitPrice { get => unitPrice; set => unitPrice = value; }


    }
}
