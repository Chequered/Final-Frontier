using UnityEngine;

using System.Collections.Generic;

using EndlessExpedition.Serialization;
using EndlessExpedition.Graphics;

namespace EndlessExpedition
{
    namespace Items
    {
        public class MasterItemStack
        {
            public MasterItemStack(Item item, int amount)
            {
                this.item = item;
                this.amount = amount;
            }

            public Item item;
            public int amount;
        }

        public class ItemStackTransferInfo
        {
            public ItemStack itemStack;
            public ItemContainer itemContainer;
            public int containerIndex;
            public ItemStackTransferInfo(ItemStack s, ItemContainer ic, int i)
            {
                itemStack = s;
                itemContainer = ic;
                containerIndex = i;
            }
        }

        public class ItemStack
        {
            public const int MAX_STACK_VALUE = 640;

            private int m_amount;
            private Item m_itemRef;
            private ItemContainer m_containerRef;

            public delegate void DepleteStackEventHandler();
            public delegate void NotEnoughRoomEventHandler(int abundance);

            public ItemContainer.StackUpdateEventHandler OnTakeItems;
            public ItemContainer.StackUpdateEventHandler OnAddItems;
            public DepleteStackEventHandler OnStackDepletion;

            public ItemStack(Item item, int amount)
            {
                m_itemRef = item;
                m_amount = amount;
            }

            //Take Items
            public ItemStack TakeAmount(int amount)
            {
                ItemStack takeAway = null;
                if (m_amount >= amount)
                {
                    takeAway = new ItemStack(m_itemRef, amount);
                    m_amount -= amount;
                    if (m_amount <= 0)
                        if (OnStackDepletion != null)
                            OnStackDepletion();
                    else if(OnTakeItems != null && containerIndex != System.Int32.MaxValue)
                        OnTakeItems(containerIndex);
                }
                else
                {
                    Debug.LogError("You're trying to take more items that the stack contains.");
                }
                return takeAway;
            }

            //Give Items
            public ItemStack AddAllFromStack(ItemStack itemStack)
            {
                if (m_amount + itemStack.amount < MAX_STACK_VALUE)
                {
                    m_amount += itemStack.TakeAmount(itemStack.amount).amount;
                    if (OnAddItems != null)
                        OnTakeItems(containerIndex);
                }
                else
                {
                    int leftOverRoom = MAX_STACK_VALUE - m_amount;
                    m_amount += itemStack.TakeAmount(leftOverRoom).amount;
                }
                return itemStack;
            }
            public ItemStack AddAmountFromStack(ItemStack itemStack, int amount)
            {
                if (m_amount + amount < MAX_STACK_VALUE)
                {
                    //fits
                    m_amount += itemStack.TakeAmount(amount).amount;
                    if (OnAddItems != null)
                        OnTakeItems(containerIndex);
                }
                else
                {
                    //does not fit
                    int leftOverRoom = MAX_STACK_VALUE - m_amount;
                    m_amount += itemStack.TakeAmount(leftOverRoom).amount;
                }
                return itemStack;
            }
            public void AddAmountFromMasterStack(MasterItemStack masterStack, int amount)
            {
                if(amount <= emptySpaceLeft)
                {
                    masterStack.amount -= amount;
                    m_amount += amount;
                }
                else
                {
                    masterStack.amount -= emptySpaceLeft;
                    m_amount += emptySpaceLeft;
                }
            }

            //Check Items
            public bool ContainsAmount(int amount)
            {
                if (m_amount >= amount)
                    return true;
                return false;
            }
            public Item item
            {
                get
                {
                    return m_itemRef;
                }
            }
            public int amount
            {
                get
                {
                    return m_amount;
                }
            }
            public int emptySpaceLeft
            {
                get
                {
                    return MAX_STACK_VALUE - amount;
                }
            }
            public bool isFull
            {
                get
                {
                    if (amount >= MAX_STACK_VALUE)
                        return true;
                    return false;
                }
            }
            public ItemContainer container
            {
                get
                {
                    return m_containerRef;
                }
                set
                {
                    m_containerRef = value;
                }
            }
            public int containerIndex
            {
                get
                {
                    KeyValuePair<bool, int> result = container.GetStackInfo(this);
                    if (result.Key)
                        return result.Value;
                    return System.Int32.MaxValue;
                }
            }
        }

        public enum ItemType
        {
            Item,
            Currency,
            Essential
        }

        public class Item
        {
            private string m_identity;
            private string m_displayName;
            private string m_description;
            private ItemType m_itemType;

            private ItemGraphics m_graphics;
            
            public Item(Properties properties)
            {
                if (properties.Has("itemType") && properties.Has("identity") && properties.Has("displayName") && properties.Has("description"))
                {
                    m_identity = properties.Get<string>("identity");
                    m_displayName = properties.Get<string>("displayName");
                    m_description = properties.Get<string>("description");
                    switch (properties.Get<string>("itemType"))
                    {
                        case "item":
                            m_itemType = ItemType.Item;
                            break;
                        case "currency":
                            m_itemType = ItemType.Currency;
                            break;
                        case "essential":
                            m_itemType = ItemType.Essential;
                            break;
                    }
                }
                else
                {
                    Debug.LogError("Broken Item properties!");
                }
            }

            public GraphicsBase GetGraphics()
            {
                return m_graphics;
            }

            public void SetGraphics(ItemGraphics graphics)
            {
                m_graphics = graphics;
            }

            #region Getters
            public string identity
            {
                get
                {
                    return m_identity;
                }
            }

            public string displayName
            {
                get
                {
                    return m_displayName;
                }
            }

            public string description
            {
                get
                {
                    return m_description;
                }
            }

            public ItemType type
            {
                get
                {
                    return m_itemType;
                }
            }
            #endregion
        }
    }
}