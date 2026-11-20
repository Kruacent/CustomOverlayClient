using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlay.Inventory
{
    internal sealed class InventorySlots
    {
        public const int Count = 8;

        private readonly ushort[] _serials = new ushort[Count];
        private readonly Dictionary<ushort, InventoryItem> _items = new Dictionary<ushort, InventoryItem>();

        public InventoryItem ItemAt(int slot)
        {
            if (slot < 0 || slot >= Count) return null;
            ushort slotItemSerial = _serials[slot];


            InventoryItem item = null;
            _items.TryGetValue(_serials[slot], out item);

            return item;
        }

        public void Apply(InventoryMessage message)
        {
            RememberItems(message);
            FreeRemovedSlots();
            PlaceNewItems(message.Items);
        }

        public void Clear()
        {
            _items.Clear();
            FreeRemovedSlots();
        }

        public void Swap(int from, int to)
        {
            if (from < 0 || from >= Count) return;
            if (to < 0 || to >= Count) return;
            if (from == to) return;
            if (_serials[from] == 0) return;

            ushort temp = _serials[from];
            _serials[from] = _serials[to];
            _serials[to] = temp;
        }

        private void RememberItems(InventoryMessage message)
        {
            _items.Clear();
            foreach (InventoryItem item in message.Items)
            {
                _items[item.Serial] = item;
            }
        }

        private void FreeRemovedSlots()
        {
            for (int i = 0; i < _serials.Length; i++)
            {
                ushort slotSerial = _serials[i];
                if (!_items.ContainsKey(slotSerial))
                {
                    _serials[i] = 0;
                }
            }
        }

        private void PlaceNewItems(IReadOnlyList<InventoryItem> items)
        {
            foreach (InventoryItem item in items)
            {
                if (Array.IndexOf(_serials, item.Serial) >= 0) continue;

                int freeSlot = Array.IndexOf(_serials, (ushort)0);
                if ( freeSlot == -1) return;

                _serials[freeSlot] = item.Serial;
            }
        }
    }
}
