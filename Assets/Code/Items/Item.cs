using UnityEngine;

using FinalFrontier.Serialization;
using FinalFrontier.Graphics;

namespace FinalFrontier
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

        public class ItemStack
        {
            public const int MAX_STACK_VALUE = 640;

            private int m_amount;
            private Item m_itemRef;

            public delegate void ItemAmountChangeEventHandler(int newAmount);
            public delegate void DepleteStackEventHandler();
            public delegate void NotEnoughRoomEventHandler(int abundance);

            public ItemAmountChangeEventHandler OnTakeItems;
            public ItemAmountChangeEventHandler OnAddItems;
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
                        OnStackDepletion();
                    else
                        OnTakeItems(m_amount);
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
                    OnAddItems(m_amount);
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
                    OnAddItems(m_amount);
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