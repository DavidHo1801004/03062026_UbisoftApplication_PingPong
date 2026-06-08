using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Developer.GameplaySystems.Equipment
{
    /// <summary>
    /// Handling item registration and selection.
    /// </summary>
    public class InventoryHandler
    {
        /// <summary>
        /// Contains data on item update when adding, removing, or updating internal data of <see cref="ItemHandler"/>.
        /// </summary>
        public struct UpdateContext
        {
            public ItemBase item;

            public int slot;
        }

        /// <summary>
        /// Invoked on item added to a specified slot.
        /// </summary>
        /// <remarks>
        /// <b>NOTE:</b> This would be invoked during swapping of abilities if enabled.
        /// </remarks>
        public event Action<UpdateContext> OnItemAdded;

        /// <summary>
        /// Invoked on item removed from a specified slot.
        /// </summary>
        public event Action<UpdateContext> OnItemRemoved;

        /// <summary>
        /// Invoked on any changes to item slot capacity of this handler.
        /// </summary>
        public event Action<int> OnSlotCapacityUpdated;

        /// <summary>
        /// Invoked on selecting a new item.
        /// </summary>
        /// <remarks>
        /// <b>NOTE:</b> If there are no currently selected item, value will be -1.
        /// </remarks>
        public event Action<int> OnNewItemSelected;



        private readonly SortedSet<int> availableSlots = new();

        private readonly Dictionary<int, ItemBase> slotMap = new();

        /// <summary>
        /// Collection of abilities assigned to this handler.
        /// </summary>
        public IReadOnlyList<ItemBase> Abilities => slotMap.Values.ToList();

        /// <summary>
        /// Collection of item slots and their corresponding item if has.
        /// </summary>
        public IReadOnlyDictionary<int, ItemBase> ItemSlots => slotMap;

        private int slotCapacity;

        /// <summary>
        /// Total slot capacity of this handler. <br/>
        /// New abilities can not be added if capacity is full.
        /// </summary>
        public int SlotCapacity
        {
            get => slotCapacity;
            set
            {
                if (slotCapacity == value) return;

                if (slotCapacity < value)
                    for (int i = slotCapacity; i < value; i++)
                    {
                        availableSlots.Add(i);
                    }
                else
                    for (int i = value; i < slotCapacity; i++)
                    {
                        availableSlots.Remove(i);
                        slotMap.Remove(i);
                    }

                slotCapacity = value;
                OnSlotCapacityUpdated?.Invoke(slotCapacity);
            }
        }

        private int selectedItemSlot = -1;

        /// <summary>
        /// The currently selected ability slot.
        /// </summary>
        public int SelectedItemSlot
            => selectedItemSlot;

        /// <summary>
        /// The currently selected ability.
        /// </summary>
        public ItemBase SelectedItem
            => selectedItemSlot == -1 ? null : slotMap[selectedItemSlot];

        public GameObject Owner { get; private set; }



        public InventoryHandler(GameObject _Owner, int _InitCapacity)
        {
            SlotCapacity = _InitCapacity;
            Owner = _Owner;
        }

        public InventoryHandler(GameObject _Owner) : this(_Owner, 0) { }



        #region Collection Controls
        /// <summary>
        /// Adds an item to a specified slot if the slot is available.
        /// </summary>
        /// <param name="_Item"> The <see cref="ItemBase"/> instance to assign to the slot. </param>
        /// <param name="_Slot"> The index of the slot to assign the item to. </param>
        /// <returns>
        /// <see langword="true"/> if the item was successfully added; otherwise, <see langword="false"/>. 
        /// </returns>
        /// <remarks>
        /// Invokes <see cref="OnItemAdded"/> on successful addition.
        /// </remarks>
        public bool AddItem(ItemBase _Item, int _Slot)
        {
            if (_Item == null || !availableSlots.Contains(_Slot)) return false;

            slotMap.Add(_Slot, _Item);
            availableSlots.Remove(_Slot);

            OnItemAdded?.Invoke(new UpdateContext()
            {
                item = _Item,
                slot = _Slot,
            });

            return true;
        }

        /// <summary>
        /// Adds an item to the first available slot if possible.
        /// </summary>
        /// <inheritdoc cref="AddItem(ItemBase, int)"/>
        public bool AddItem(ItemBase _ItemData)
        {
            if (availableSlots.Count == 0) return false;

            return AddItem(_ItemData, availableSlots.First());
        }

        /// <summary>
        /// Removes an item from the specified slot and updates the available slots list.
        /// </summary>
        /// <param name="_Slot"> The index of the item slot to remove. </param>
        /// <returns>            
        /// <see langword="true"/> if the item was successfully removed; otherwise, <see langword="false"/>. 
        /// </returns>
        /// <remarks>
        /// Invokes <see cref="OnItemRemoved"/> on successful removal.
        /// </remarks>
        public bool RemoveItem(int _Slot)
        {
            if (slotMap.ContainsKey(_Slot)) return false;

            var item = slotMap[_Slot];
            slotMap.Remove(_Slot);
            availableSlots.Add(_Slot);

            OnItemRemoved?.Invoke(new UpdateContext()
            {
                item = item,
                slot = _Slot,
            });

            return true;
        }

        /// <inheritdoc cref="RemoveItem(int)"/>
        /// <param name="_Item"> The item to remove. </param>
        public bool RemoveItem(ItemBase _Item)
        {
            return RemoveItem(IndexOf(_Item));
        }

        /// <summary>
        /// Swaps two abilities between specified slots in the item map.
        /// </summary>
        /// <param name="_From">        The index of the item slot to swap from. </param>
        /// <param name="_To">          The index of the item slot to swap to. </param>
        /// <param name="_InvokeEvents">If <see langword="true"/>, invokes <see cref="OnItemAdded"/> on both affected slots. </param>
        /// <returns>                   
        /// <see langword="true"/> if the swap was successful; otherwise, <see langword="false"/>. 
        /// </returns>
        /// <remarks>
        /// The swap will fail if either slot index is invalid or if both indices are the same.
        /// </remarks>
        public bool SwapItem(int _From, int _To, bool _InvokeEvents = false)
        {
            if (!slotMap.ContainsKey(_From) || _To < 0 || _To >= slotCapacity || _From == _To) return false;

            var originalItem = slotMap[_From];
            slotMap[_From] = slotMap[_To];
            slotMap[_To] = originalItem;

            if (_InvokeEvents)
            {
                OnItemAdded?.Invoke(new UpdateContext()
                {
                    item = slotMap[_From],
                    slot = _From
                });
                OnItemAdded?.Invoke(new UpdateContext()
                {
                    item = slotMap[_To],
                    slot = _To
                });
            }

            return true;
        }
        #endregion

        #region Item Controls
        /// <summary>
        /// Selects a new item as the currently active item.
        /// </summary>
        /// <param name="_Item"> The <see cref="ItemBase"/> instance to select. </param>
        /// <returns>               
        /// <see langword="true"/> if the item is registered; 
        /// otherwise, <see langword="false"/>. 
        /// </returns>
        public bool SelectItem(ItemBase _Item)
        {
            if (!Abilities.Contains(_Item) || SelectedItem == _Item) return false;

            return SelectItem(IndexOf(_Item)); ;
        }

        /// <inheritdoc cref="SelectItem(ItemBase)"/>
        /// <param name="_Slot"> The item slot index to select. </param>
        public bool SelectItem(int _Slot)
        {
            if (!slotMap.ContainsKey(_Slot)
                || selectedItemSlot == _Slot) return false;

            SelectedItem?.OnDeselect(Owner);

            selectedItemSlot = _Slot;
            OnNewItemSelected?.Invoke(_Slot);

            SelectedItem?.OnSelect(Owner);

            return true;
        }
        #endregion

        #region Access
        /// <summary>
        /// Retrieves the index of the specified <see cref="ItemBase"/> within the item slot mapping.
        /// </summary>
        /// <param name="_Item"> The <see cref="ItemBase"/> instance to search for. </param>
        public int IndexOf(ItemBase _Item)
        {
            foreach (var item in slotMap)
                if (item.Value == _Item)
                    return item.Key;

            return -1;
        }

        /// <summary>
        /// Get the item at the specified slot if valid.
        /// </summary>
        public ItemBase GetItemAt(int _Slot)
        {
            if (slotMap.TryGetValue(_Slot, out var item)) return null;
            return item;
        }
        #endregion
    }
}
