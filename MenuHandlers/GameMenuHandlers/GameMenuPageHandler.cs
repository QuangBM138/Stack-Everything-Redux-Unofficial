//GameMenuPageHandler.cs
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace StackEverythingRedux.MenuHandlers.GameMenuHandlers
{
    public abstract class GameMenuPageHandler<TPageType> : IGameMenuPageHandler where TPageType : IClickableMenu
    {
        /// <summary>The inventory handler.</summary>
        protected InventoryHandler InventoryHandler = null;

        /// <summary>Does this menu have an inventory section.</summary>
        protected bool HasInventory { get; set; } = true;

        /// <summary>The native menu that owns all the pages.</summary>
        protected IClickableMenu NativeMenu { get; private set; }

        /// <summary>The native page this handler is for.</summary>
        protected TPageType MenuPage { get; private set; }

        // Cache reflected fields
        private static readonly IReflectedField<InventoryMenu> _inventoryField;
        private static readonly IReflectedField<Item> _hoveredItemField;
        
        static GameMenuPageHandler()
        {
            // Initialize reflection cache once
            _inventoryField = StackEverythingRedux.Reflection.GetField<IClickableMenu, InventoryMenu>("inventory");
            _hoveredItemField = StackEverythingRedux.Reflection.GetField<IClickableMenu, Item>("hoveredItem");
        }

        /// <summary>Null constructor that currently only logs instantiation</summary>
        public GameMenuPageHandler()
        {
            Log.TraceIfD($"[{nameof(GameMenuPageHandler<TPageType>)}] Instantiated with TPageType = {typeof(TPageType)}");
        }

        ~GameMenuPageHandler()
        {
            Log.TraceIfD($"[{nameof(GameMenuPageHandler<TPageType>)}] Finalized for TPageType = {typeof(TPageType)}");
        }

        public virtual bool Open(IClickableMenu menu, IClickableMenu page, InventoryHandler inventoryHandler)
        {
            MenuPage = page as TPageType;

            if (MenuPage is null) { return false; }

            NativeMenu = menu;
            InventoryHandler = inventoryHandler;

            if (HasInventory)
            {
                InitInventory();
            }

            return true;
        }

        /// <summary>Tell the handler to close.</summary>
        public virtual void Close()
        {
            NativeMenu = null;
            MenuPage = null;
            InventoryHandler = null;
        }

        /// <summary>Initializes the inventory using the most common variable names.</summary>
        public virtual void InitInventory()
        {
            try
            {
                InventoryMenu inventoryMenu = _inventoryField.GetValue(MenuPage);
                if (inventoryMenu != null)
                {
                    Log.TraceIfD(
                        $"[{nameof(GameMenuPageHandler<TPageType>)}.{nameof(InitInventory)}] Initializing InventoryHandler"
                    );
                    InventoryHandler.Init(inventoryMenu, _hoveredItemField);
                }
            }
            catch (Exception e)
            {
                Log.Error($"[{nameof(GameMenuPageHandler<TPageType>)}.{nameof(InitInventory)}] Failed to initialize the inventory handler:\n{e}");
            }
        }

        /// <summary>Tells the handler that the inventory was shift-clicked.</summary>
        /// <param name="stackAmount">The default stack amount to display in the split menu.</param>
        /// <returns>If the input was handled or consumed. Generally returns not handled if an invalid item was selected.</returns>
        public virtual EInputHandled InventoryClicked(out int stackAmount)
        {
            stackAmount = 0;

            // This logic is the same for all the page handlers so we can do it here.
            InventoryHandler.SelectItem(Game1.getMouseX(true), Game1.getMouseY(true));
            if (InventoryHandler.CanSplitSelectedItem())
            {
                stackAmount = InventoryHandler.GetDefaultSplitStackAmount();

                return EInputHandled.Consumed;
            }
            return EInputHandled.NotHandled;
        }

        /// <summary>Tells the handler that the interface recieved the hotkey input.</summary>
        /// <param name="stackAmount">The default stack amount to display in the split menu.</param>
        /// <returns>If the input was handled or consumed. Generally returns not handled if an invalid item was selected.</returns>
        public virtual EInputHandled OpenSplitMenu(out int stackAmount)
        {
            stackAmount = 0;
            return EInputHandled.NotHandled;
        }

        /// <summary>Tells the handler to cancel the move/run the default behaviour.</summary>
        /// <returns>If the input invoking the cancel should be consumed or not.</returns>
        public virtual EInputHandled CancelMove()
        {
            return EInputHandled.NotHandled;
        }

        /// <summary>Lets the handler run the logic for doing the split after the amount has been input.</summary>
        /// <param name="amount">The stack size the user requested.</param>
        public virtual void OnStackAmountEntered(int amount)
        {
            InventoryHandler.SplitSelectedItem(amount);
        }
    }
}
