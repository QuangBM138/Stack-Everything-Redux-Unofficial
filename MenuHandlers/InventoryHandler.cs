using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Diagnostics;

namespace StackEverythingRedux.MenuHandlers
{
    public class InventoryHandler
    {
        /// <summary>If the handler has been initialized.</summary>
        public bool Initialized => NativeInventoryMenu != null;

        /// <summary>Native inventory menu.</summary>
        private InventoryMenu NativeInventoryMenu;

        /// <summary>Inventory interface bounds.</summary>
        private Rectangle Bounds;

        /// <summary>Mouse position where the user clicked to select an item.</summary>
        private int SelectedItemPosition_X;
        private int SelectedItemPosition_Y;

        /// <summary>Reflected field for the hovered item owned by the parent menu.</summary>
        private IReflectedField<Item> HoveredItemField;

        /// <summary>Currently hovered item in the inventory.</summary>
        private Item HoveredItem;

        /// <summary>Cached inventory items to avoid frequent allocations.</summary>
        private readonly List<Item> CachedInventoryItems = new List<Item>();

        /// <summary>Tracks the last known inventory state to detect changes.</summary>
        private int LastInventoryHash;

        /// <summary>Cached count of non-null items in the inventory.</summary>
        private int NonNullItemCount;

        public InventoryHandler()
        {
        }

        /// <summary>Initializes the handler. Must be called when the inventory is opened or resized.</summary>
        /// <param name="inventoryMenu">Native inventory menu.</param>
        /// <param name="hoveredItemField">Reflected field for the hovered item.</param>
        public void Init(InventoryMenu inventoryMenu, IReflectedField<Item> hoveredItemField)
        {
            Debug.Assert(inventoryMenu != null);
            NativeInventoryMenu = inventoryMenu;
            HoveredItemField = hoveredItemField;

            // Update cache and bounds
            UpdateInventoryCache();
            UpdateBounds();

            if (StackEverythingRedux.Config.DebuggingMode)
            {
                Log.Trace($"[DEBUG] InventoryHandler.Init: Inventory size = {NonNullItemCount}");
            }
        }

        /// <summary>Updates the inventory cache only if the inventory has changed.</summary>
        public void UpdateInventoryCache()
        {
            IList<Item> actualInventory = NativeInventoryMenu.actualInventory;
            int currentHash = ComputeInventoryHash(actualInventory);

            if (currentHash != LastInventoryHash || CachedInventoryItems.Count != actualInventory.Count)
            {
                CachedInventoryItems.Clear();
                CachedInventoryItems.AddRange(actualInventory);
                LastInventoryHash = currentHash;

                // Update non-null item count without LINQ
                NonNullItemCount = 0;
                for (int i = 0; i < CachedInventoryItems.Count; i++)
                {
                    if (CachedInventoryItems[i] != null)
                    {
                        NonNullItemCount++;
                    }
                }

                if (StackEverythingRedux.Config.DebuggingMode)
                {
                    Log.Trace($"[DEBUG] InventoryHandler.UpdateInventoryCache: Inventory size = {NonNullItemCount}");
                }
            }
        }

        /// <summary>Computes a simple hash of the inventory to detect changes.</summary>
        private int ComputeInventoryHash(IList<Item> inventory)
        {
            int hash = 17;
            for (int i = 0; i < inventory.Count; i++)
            {
                Item item = inventory[i];
                unchecked
                {
                    hash = hash * 23 + (item?.GetHashCode() ?? 0);
                    if (item != null)
                    {
                        hash = hash * 23 + item.Stack;
                    }
                }
            }
            return hash;
        }

        /// <summary>Updates the bounds of the inventory interface.</summary>
        private void UpdateBounds()
        {
            List<ClickableComponent> inventory = NativeInventoryMenu.inventory;
            if (inventory.Count == 0)
            {
                Bounds = Rectangle.Empty;
                return;
            }

            Rectangle first = inventory[0].bounds;
            Rectangle last = inventory[inventory.Count - 1].bounds;
            Bounds = new Rectangle(
                first.X,
                first.Y,
                last.X + last.Width - first.X,
                last.Y + last.Height - first.Y);
        }

        /// <summary>Checks if the inventory interface was clicked.</summary>
        public bool WasClicked(Point mousePos)
        {
            Debug.Assert(Initialized);
            return Bounds.Contains(mousePos);
        }

        /// <summary>Checks if the inventory interface was clicked.</summary>
        public bool WasClicked(int mouseX, int mouseY)
        {
            Debug.Assert(Initialized);
            return Bounds.Contains(mouseX, mouseY);
        }

        /// <summary>Stores data needed to split an item stack.</summary>
        public void SelectItem(int mouseX, int mouseY)
        {
            Debug.Assert(Initialized);
            SelectedItemPosition_X = mouseX;
            SelectedItemPosition_Y = mouseY;
            HoveredItem = HoveredItemField.GetValue();
        }

        /// <summary>Checks if the selected item can be split.</summary>
        public bool CanSplitSelectedItem()
        {
            Debug.Assert(Initialized);
            Item hoveredItem = HoveredItem;
            Item heldItem = Game1.player.CursorSlotItem;

            return hoveredItem != null
                && hoveredItem.Stack > 1
                && (heldItem == null || (hoveredItem.canStackWith(heldItem) && heldItem.Stack < heldItem.maximumStackSize()));
        }

        /// <summary>Splits the selected item stack.</summary>
        public void SplitSelectedItem(int stackAmount)
        {
            Debug.Assert(Initialized && HoveredItemField != null);

            Stopwatch sw = null;
            int beforeCount = NonNullItemCount;

            if (StackEverythingRedux.Config.DebuggingMode)
            {
                sw = Stopwatch.StartNew();
            }

            Item hoveredItem = HoveredItem;
            int hoveredItemCount = hoveredItem.Stack;
            int maxStack = hoveredItem.maximumStackSize();

            Item heldItem = Game1.player.CursorSlotItem;
            int heldItemCount = heldItem?.Stack ?? 0;

            // Run native right-click to pick up the item
            heldItem = NativeInventoryMenu.rightClick(SelectedItemPosition_X, SelectedItemPosition_Y, heldItem);
            Debug.Assert(heldItem != null);

            // Clamp stack amount
            stackAmount = Math.Min(Math.Max(0, stackAmount), hoveredItemCount);
            if (heldItemCount + stackAmount > maxStack)
            {
                stackAmount = maxStack - heldItemCount;
            }

            heldItemCount += stackAmount;
            hoveredItemCount -= stackAmount;

            if (hoveredItemCount <= 0)
            {
                RemoveItemFromInventory(hoveredItem);
            }
            else
            {
                hoveredItem.Stack = hoveredItemCount;
            }

            heldItem.Stack = heldItemCount;
            Game1.player.CursorSlotItem = heldItem;
            HoveredItem = null;

            // Update cache after modification
            UpdateInventoryCache();

            if (StackEverythingRedux.Config.DebuggingMode)
            {
                sw.Stop();
                Log.Trace($"[DEBUG] SplitSelectedItem: StackAmount={stackAmount}, Inventory before={beforeCount}, after={NonNullItemCount}, took={sw.ElapsedMilliseconds}ms");
            }
        }

        /// <summary>Runs the default shift+right-click behavior.</summary>
        public void CancelSplit()
        {
            if (Initialized && HoveredItem != null)
            {
                SplitSelectedItem(GetDefaultSplitStackAmount());
            }
        }

        /// <summary>Gets the default split stack amount for shift+right-click.</summary>
        public int GetDefaultSplitStackAmount()
        {
            return (HoveredItem.Stack + 1) / 2;
        }

        /// <summary>Removes an item from the inventory.</summary>
        private void RemoveItemFromInventory(Item item)
        {
            IList<Item> inventoryItems = NativeInventoryMenu.actualInventory;
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i] == item)
                {
                    inventoryItems[i] = null;
                    NonNullItemCount--;
                    if (StackEverythingRedux.Config.DebuggingMode)
                    {
                        Log.Trace($"[DEBUG] RemoveItemFromInventory: Removed item at index {i}, Inventory size now = {NonNullItemCount}");
                    }
                    break;
                }
            }
        }
    }
}