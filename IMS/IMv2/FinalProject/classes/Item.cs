using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    internal class Item
    {
        private int itemId;
        private string itemName;
        private string item;
        private int quantity;
        public int iId { get => itemId; set => itemId = value; }
        public string iName { get => itemName; set => itemName = value; }
        public string itemOnly { get => item; set => item = value; }
        public int qty { get => quantity; set => quantity = value; }
    }
}
