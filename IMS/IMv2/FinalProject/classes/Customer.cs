using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    internal class Customer
    {
        private int customerId;
        private string customerName;
        public int cId { get => customerId; set => customerId = value; }
        public string fullName { get => customerName; set => customerName = value; }
        
    }
}
