using Overlay.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlay.Inventory
{
    public class InventoryMessage
    {
        public bool IsAlive { get; }
        public ushort HeldItemSerial { get; }
        public IReadOnlyList<InventoryItem> Items { get; }
        private const int MinimumMessageLenght = 3;

        private InventoryMessage(bool alive, ushort heldSerial, IReadOnlyList<InventoryItem> items)
        {
            IsAlive = alive;
            HeldItemSerial = heldSerial;
            Items = items;
        }

        public static bool TryParse(string line, out InventoryMessage message)
        {
            message = null;
            string[] parts = line.Split('|');

            if (parts.Length < MinimumMessageLenght || parts[0] != Feature.Inventory) return false;
            
            // is player alive
            if (parts[1] != "0" && parts[1] != "1") return false;
            bool isAlive = (parts[1] == "1");

            if (!ushort.TryParse(parts[2], out ushort heldItemSerial)) return false;

            List<InventoryItem > items = new List<InventoryItem>();
            for (int i = MinimumMessageLenght; i < parts.Length; i++)
            {
                if (!InventoryItem.TryParse(parts[i], out InventoryItem item)) return false;
                items.Add(item);
            }

            message = new InventoryMessage(isAlive, heldItemSerial, items);
            return true;
        }
    }
}
