using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System.Diagnostics;

namespace StackEverythingRedux.MenuHandlers.ShopMenuHandlers
{
    internal class BuyAction : ShopAction
    {
        private readonly Guid GUID = Guid.NewGuid();

        private bool? _CanPerformAction = null;
        private int? _MaxPurchasable = null;

        /// <summary>Constructs an instance.</summary>
        /// <param name="menu">The native shop menu.</param>
        /// <param name="item">The item to buy.</param>
        public BuyAction(ShopMenu menu, ISalable item)
            : base(menu, item)
        {
            // Default
            if (CanPerformAction())
            {
                Amount = Math.Min(StackEverythingRedux.Config.DefaultShopAmount, GetMaxPurchasable());
            }

            Log.TraceIfD($"[{nameof(BuyAction)}] Instantiated for shop {menu} item {item}, Amount = {Amount}, GUID = {GUID}");
        }

        ~BuyAction()
        {
            Log.TraceIfD($"[{nameof(BuyAction)}] Finalized for GUID = {GUID}");
        }

        /// <summary>Verifies the conditions to perform the action.</summary>
        public override bool CanPerformAction()
        {
            if (_CanPerformAction is null)
            {
                Item held = StackEverythingRedux.Reflection.GetField<Item>(NativeShopMenu, "heldItem").GetValue();

                _CanPerformAction =
                    ClickedItem is Item chosen    // not null
                    && chosen.canStackWith(chosen)     // Item type is stackable
                    && (
                        held == null                   // not holding anything, or...
                        || (chosen.canStackWith(held) && held.Stack < held.maximumStackSize())  // item held can stack with chosen and at max
                        )
                    && GetMaxPurchasable() > 0         // Can afford
                    ;
            }
            return _CanPerformAction.Value;
        }

        /// <summary>Does the action.</summary>
        /// <param name="amount">Number of items.</param>
        /// <param name="clickLocation">Where the player clicked.</param>
        public override void PerformAction(int amount, Point clickLocation)
        {
            string pfx = $"[{nameof(BuyAction)}.{nameof(PerformAction)}]";
            Item chosen = ClickedItem;
            int chosen_max = chosen.maximumStackSize();
            ShopMenu nativeMenu = NativeShopMenu;
            Item heldItem = StackEverythingRedux.Reflection.GetField<Item>(nativeMenu, "heldItem").GetValue();

            Log.Trace(
                $"{pfx} chosen = {chosen}, nativeMenu = {nativeMenu}, ShopCurrencyType = {ShopCurrencyType} ({ShopCurrencyName})"
                );

            // Use requested amount but respect maximum purchasable and stack size limits
            int maxPurchasable = GetMaxPurchasable();
            amount = Math.Min(amount, maxPurchasable);
            
            // Consider current held stack when checking against maximum stack size
            int numHeld = heldItem?.Stack ?? 0;
            int remainingStackSpace = chosen_max - numHeld;
            if (remainingStackSpace > 0)
            {
                amount = Math.Min(amount, remainingStackSpace);
            }

            if (amount <= 0)
            {
                Log.Trace($"{pfx} purchasable amount <= 0, purchase aborted");
                return;
            }

            Log.Trace($"{pfx} Purchasing {amount} of {chosen.Name}");

            // Try to purchase the item - method returns true if it should be removed from the shop since there's no more.
            StardewModdingAPI.IReflectedMethod purchaseMethod = StackEverythingRedux.Reflection.GetMethod(nativeMenu, "tryToPurchaseItem");
            if (purchaseMethod.Invoke<bool>(chosen, heldItem, amount, clickLocation.X, clickLocation.Y))
            {
                Log.TraceIfD($"{pfx} Item is limited, reducing stock");
                // remove the purchased item from the stock etc.
                _ = nativeMenu.itemPriceAndStock.Remove(chosen);
                _ = nativeMenu.forSale.Remove(chosen);
            }
        }

        /// <summary>
        /// Determine how many of an item player can purchase based on player's current monies/inventories and shop's current stock
        /// </summary>
        /// <returns>Maximum amount purchasable, cached</returns>
        public int GetMaxPurchasable()
        {
            if (_MaxPurchasable is null)
            {
                string pfx = $"[{nameof(BuyAction)}.{nameof(GetMaxPurchasable)}]";

                Debug.Assert(ClickedItem is not null);
                Item chosen = ClickedItem;
                var priceAndStockMap = NativeShopMenu.itemPriceAndStock;
                Debug.Assert(priceAndStockMap.ContainsKey(chosen));

                // Get stock information using reflection to avoid type mismatch
                var stockInfo = priceAndStockMap[chosen];
                
                // Try to get stock and price values through different methods
                int numInStock;
                int itemPrice;

                try 
                {
                    // First try using fields for infinite stock items
                    itemPrice = StackEverythingRedux.Reflection.GetField<int>(stockInfo, "price").GetValue();
                    var infinite = StackEverythingRedux.Reflection.GetField<bool>(stockInfo, "infinite").GetValue();
                    numInStock = infinite ? int.MaxValue : StackEverythingRedux.Reflection.GetField<int>(stockInfo, "stock").GetValue();
                }
                catch
                {
                    try
                    {
                        // Fallback to checking if it's a shop with infinite stock
                        numInStock = ShopMenu.infiniteStock == int.MaxValue ? int.MaxValue : chosen.Stack;
                        itemPrice = chosen.salePrice();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{pfx} Failed to get stock/price information: {e}");
                        // Default values as last resort
                        numInStock = 1;
                        itemPrice = chosen.salePrice();
                    }
                }

                Log.Trace($"{pfx} Stock: {numInStock}, Price: {itemPrice}, Item: {chosen.Name}");
                
                int currentMonies;
                if (itemPrice > 0)
                {  // using money
                    currentMonies = ShopMenu.getPlayerCurrencyAmount(Game1.player, ShopCurrencyType);
                    Log.TraceIfD($"{pfx} player has {currentMonies} of currency {ShopCurrencyType} ({ShopCurrencyName})");
                }
                else
                {  // barter system
                    try
                    {
                        int tradeCount = StackEverythingRedux.Reflection.GetField<int>(stockInfo, "tradeItemCount").GetValue();
                        string tradeItem = StackEverythingRedux.Reflection.GetField<string>(stockInfo, "tradeItem").GetValue();
                        itemPrice = tradeCount;
                        currentMonies = Game1.player.Items.CountId(tradeItem);
                        Log.TraceIfD($"{pfx} Barter system: player has {currentMonies} of item {tradeItem}");
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{pfx} Failed to get barter information: {e}");
                        currentMonies = 0;
                        itemPrice = 1;
                    }
                }

                Log.Trace($"{pfx} chosen item price is {itemPrice}");
                if (itemPrice <= 0)
                {
                    itemPrice = 1; // Prevent division by zero
                    Log.Trace($"{pfx} Invalid price detected, defaulting to 1");
                }

                _MaxPurchasable = Math.Min(currentMonies / itemPrice, numInStock);
                Log.Trace($"{pfx} Max purchasable: {_MaxPurchasable.Value}");
            }
            return _MaxPurchasable.Value;
        }

        /// <summary>Helper method getting which item in the shop was clicked.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="p">Mouse location.</param>
        /// <returns>The clicked item or null if none was clicked.</returns>
        public static ISalable GetClickedShopItem(ShopMenu shopMenu, Point p)
        {
            List<ISalable> itemsForSale = shopMenu.forSale;
            int index = GetClickedItemIndex(shopMenu, p);
            Debug.Assert(index < itemsForSale.Count);
            return index >= 0 ? itemsForSale[index] : null;
        }

        /// <summary>Gets the index of the clicked shop item. This index corresponds to the list of buttons and list of items.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="p">Mouse location.</param>
        /// <returns>The clicked item or null if none was clicked.</returns>
        public static int GetClickedItemIndex(ShopMenu shopMenu, Point p)
        {
            int currentItemIndex = shopMenu.currentItemIndex;
            int saleButtonIndex = shopMenu.forSaleButtons.FindIndex(button => button.containsPoint(p.X, p.Y));
            return saleButtonIndex > -1 ? currentItemIndex + saleButtonIndex : -1;
        }

        /// <summary>Creates an instance of the action.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="mouse">Mouse position.</param>
        /// <returns>The instance or null if no valid item was selected.</returns>
        public static ShopAction Create(ShopMenu shopMenu, Point mouse)
        {
            ISalable item = GetClickedShopItem(shopMenu, mouse);
            return item != null ? new BuyAction(shopMenu, item) : null;
        }
    }
}
