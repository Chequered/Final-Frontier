using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Graphics;

namespace EndlessExpedition
{
    namespace Items
    {
        public class ItemContainer
        {
            private ItemStack[] m_itemStacks;
            private int m_containerSize;

            public delegate void StackUpdateEventHandler(int slotIndex);
            public StackUpdateEventHandler OnStackUpdate;

            public ItemContainer(int containerSize)
            {
                m_itemStacks = new ItemStack[containerSize];
                m_containerSize = containerSize;
            }

            //Take Items
            public ItemStack TakeStackAt(int slotIndex)
            {
                ItemStack result = null;

                if (slotIndex < m_containerSize)
                    if (m_itemStacks[slotIndex] != null)
                    {
                        result = m_itemStacks[slotIndex];
                        m_itemStacks[slotIndex] = null;
                    }

                if (OnStackUpdate != null)
                    OnStackUpdate(slotIndex);

                return result;
            }
            public ItemStack[] TakeItems(Item item, int amount)
            {
                ItemStack[] stacks;
                ItemStack[] stacksToTakeFrom = GetAllStacks(item.identity);
                int itemsLeftToTake = amount;
                MasterItemStack masterStack = new MasterItemStack(item, 0);

                for (int i = 0; i < stacksToTakeFrom.Length; i++)
                {
                    if (itemsLeftToTake <= 0)
                        break;
                    else
                    {
                        if (stacksToTakeFrom[i].amount >= itemsLeftToTake)
                        {
                            masterStack.amount += stacksToTakeFrom[i].TakeAmount(itemsLeftToTake).amount;
                        }
                        else
                        {
                            masterStack.amount += stacksToTakeFrom[i].TakeAmount(stacksToTakeFrom[i].amount).amount;
                        }
                        itemsLeftToTake = amount - masterStack.amount;

                        if (OnStackUpdate != null)
                            OnStackUpdate(i);
                    }
                }

                int stacksNeeded = UnityEngine.Mathf.CeilToInt(masterStack.amount / ItemStack.MAX_STACK_VALUE);
                stacks = new ItemStack[stacksNeeded];

                for (int i = 0; i < stacks.Length; i++)
                {
                    if (masterStack.amount >= ItemStack.MAX_STACK_VALUE)
                    {
                        stacks[i] = new ItemStack(item, ItemStack.MAX_STACK_VALUE);
                    }
                    else
                    {
                        stacks[i] = new ItemStack(item, masterStack.amount);
                    }
                    masterStack.amount -= stacks[i].amount;
                }

                return stacks;
            }
            public ItemStack[] TakeAllStacks(string itemIdentity)
            {
                List<ItemStack> result = new List<ItemStack>();
                List<int> resultIndexes = new List<int>();

                //look through our itemstacks
                for (int i = 0; i < m_itemStacks.Length; i++)
                {
                    if (m_itemStacks[i].item.identity == itemIdentity)
                    {
                        result.Add(m_itemStacks[i]);
                        resultIndexes.Add(i);
                    }
                }

                //remove the items from the container
                for (int i = 0; i < resultIndexes.Count; i++)
                {
                    m_itemStacks[resultIndexes[i]] = null;

                    if (OnStackUpdate != null)
                        OnStackUpdate(i);
                }

                return result.ToArray();
            }

            //Give Items
            public bool AddStackAtEnd(ItemStack itemStack)
            {
                for (int i = 0; i < m_itemStacks.Length; i++)
                {
                    if (m_itemStacks[i] == null)
                    {
                        m_itemStacks[i] = itemStack;
                        
                        if (OnStackUpdate != null)
                            OnStackUpdate(i);

                        return true;
                    }
                }
                return false;
            }
            public bool AddStackAt(ItemStack itemStack, int slotIndex)
            {
                if (m_itemStacks[slotIndex] == null)
                {
                    m_itemStacks[slotIndex] = itemStack;

                    if (OnStackUpdate != null)
                        OnStackUpdate(slotIndex);
                    return true;
                }
                return false;
            }
            public void AddFromProduction(MasterItemStack production)
            {
                List<ItemStack> avaiableStacks = new List<ItemStack>();
                int spaceNeeded = production.amount;
                for (int s = 0; s < m_itemStacks.Length; s++)
                {
                    if (m_itemStacks[s] == null)
                        continue;
                    if (m_itemStacks[s].item == production.item)
                    {
                        spaceNeeded -= m_itemStacks[s].emptySpaceLeft;
                        avaiableStacks.Add(m_itemStacks[s]);
                    }
                    if (spaceNeeded <= 0)
                        break;
                }

                if (spaceNeeded > 0)
                {
                    for (int i = 0; i < m_itemStacks.Length; i++)
                    {
                        if (m_itemStacks[i] == null)
                        {
                            m_itemStacks[i] = new ItemStack(production.item, 0);
                            avaiableStacks.Add(m_itemStacks[i]);

                            spaceNeeded -= ItemStack.MAX_STACK_VALUE;
                        }
                        if (spaceNeeded <= 0)
                            break;
                        else
                        {
                            //TODO: Print no iventory space warning
                        }
                    }
                }

                for (int a = 0; a < avaiableStacks.Count; a++)
                {
                    avaiableStacks[a].AddAmountFromMasterStack(production, production.amount);

                    if (OnStackUpdate != null)
                        OnStackUpdate(a);
                }
            }

            //Check Items
            public bool ContainsItems(Item item, int amount)
            {
                bool result = false;
                int leftToFind = amount;

                for (int i = 0; i < m_itemStacks.Length; i++)
                {
                    if (m_itemStacks[i].item == item)
                        leftToFind -= m_itemStacks[i].amount;
                }

                if (leftToFind <= 0)
                    result = true;

                return result;
            }

            //Get Items
            public ItemStack[] GetAllStacks(string itemIdentity)
            {
                List<ItemStack> result = new List<ItemStack>();

                //look through our itemstacks
                for (int i = 0; i < m_itemStacks.Length; i++)
                {
                    if (m_itemStacks[i].item.identity == itemIdentity)
                        result.Add(m_itemStacks[i]);
                }

                return result.ToArray();
            }
            public ItemStack[] GetAllStacks()
            {
                return m_itemStacks;
            }
            public ItemStack[] GetAllFilledStacks()
            {
                List<ItemStack> result = new List<ItemStack>();
                for (int i = 0; i < m_itemStacks.Length; i++)
                {
                    if (m_itemStacks[i] != null)
                        result.Add(m_itemStacks[i]);
                }
                return result.ToArray();
            }
            public ItemStack GetStackAt(int slotIndex)
            {
                if (slotIndex < m_containerSize)
                {
                    return m_itemStacks[slotIndex];
                }
                return null;
            }

            //Organize Items
            public void MergeAllStacks()
            {
                //gather all out items
                Dictionary<Item, int> itemDic = new Dictionary<Item, int>();
                for (int i = 0; i < m_itemStacks.Length; i++)
                {
                    if (m_itemStacks[i] != null)
                    {
                        if (!itemDic.ContainsKey(m_itemStacks[i].item))
                        {
                            itemDic.Add(m_itemStacks[i].item, m_itemStacks[i].amount);
                        }
                        else
                        {
                            foreach (KeyValuePair<Item, int> stack in itemDic)
                            {
                                if (stack.Key == m_itemStacks[i].item)
                                {
                                    itemDic[m_itemStacks[i].item] += m_itemStacks[i].amount;
                                }
                            }
                        }
                    }
                }

                //create our new stacks
                List<ItemStack> newStacks = new List<ItemStack>();
                foreach (var itemType in itemDic)
                {
                    while (itemType.Value > 0)
                    {
                        ItemStack newStack = new ItemStack(itemType.Key, itemType.Value);
                        itemDic[itemType.Key] -= itemType.Value;

                        newStacks.Add(newStack);
                    }
                }

                //destroy our old stacks
                m_itemStacks = new ItemStack[m_containerSize];

                //assign our new stacks
                for (int i = 0; i < newStacks.Count; i++)
                {
                    m_itemStacks[i] = newStacks[i];
                }
            }

            //Change Capacity
            public void AddSlots(int slotsToAdd)
            {
                m_containerSize += slotsToAdd;
            }
        }
    }
}