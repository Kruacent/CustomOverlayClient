using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlay.Inventory
{
    public class InventoryItem
    {
        public ushort Serial {  get; }
        public string ItemType {  get; }
        public bool IsCustom {  get; }
        public string CustomItemName { get; }

        private InventoryItem (ushort serial, string itemType, bool isCustom, string customItemName)
        {
            Serial = serial;
            ItemType = itemType;
            IsCustom = isCustom;
            CustomItemName = customItemName;
        }

        public static bool TryParse(string text, out InventoryItem item)
        {
            item = null;

            string[] parts = text.Split(':');
            if (parts.Length != 4)
            {
                return false;
            }

            // item serial check
            if (!ushort.TryParse(parts[0], out ushort serial) || serial == 0) return false;

            // item type check
            if (string.IsNullOrEmpty(parts[1])) return false;
            string itemType = parts[1];


            // cehck if is custom item
            if (parts[2] != "0" && parts[2] != "1") return false;
            bool isCustom = (parts[2] == "1");

            bool hasCustomName = parts[3].Length > 0;
            if (isCustom != hasCustomName) return false;

            string customItemName = null;
            if (isCustom)
            {
                customItemName = parts[3];
            }

            item = new InventoryItem(serial, itemType, isCustom, customItemName);
            return true;
        }
    }
}
