using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace StackEverythingRedux.MenuHandlers.ShopMenuHandlers
{
    public class SellAction : ShopAction
    {
        private const int SMALL_TILE = Game1.smallestTileSize;
        private const int HALF_TILE = Game1.tileSize / 2;
        private const int FULL_TILE = Game1.tileSize;

        private readonly Guid GUID = Guid.NewGuid();
        private readonly int ClickedItemIndex; // Cached index of ClickedItem
        private readonly float SellPercentage; // Cached sell percentage
        private readonly List<TemporaryAnimatedSprite> Animations; // Cached animations list

        /// <summary>Constructs an instance.</summary>
        /// <param name="menu">The native shop menu.</param>
        /// <param name="item">The item to sell.</param>
        /// <param name="itemIndex">The index of the item in the inventory.</param>
        public SellAction(ShopMenu menu, Item item, int itemIndex)
            : base(menu, item)
        {
            ClickedItemIndex = itemIndex;
            Amount = (ClickedItem.Stack + 1) / 2; // Default amount, rounded up
            SellPercentage = StackEverythingRedux.Reflection.GetField<float>(menu, "sellPercentage").GetValue();
            Animations = StackEverythingRedux.Reflection.GetField<List<TemporaryAnimatedSprite>>(menu, "animations").GetValue();

            Log.TraceIfD($"[{nameof(SellAction)}] Instantiated for shop {menu} item {item} at index {itemIndex}, GUID = {GUID}");
        }

        ~SellAction()
        {
            Log.TraceIfD($"[{nameof(SellAction)}] Finalized for GUID = {GUID}");
        }

        /// <summary>Verifies the conditions to perform the action.</summary>
        public override bool CanPerformAction()
        {
            if (!StackEverythingRedux.Config.EnableStackSplitInShop)
            {
                return false;
            }

            return NativeShopMenu.highlightItemToSell(ClickedItem) && ClickedItem.Stack > 1;
        }

        /// <summary>Performs the sell action.</summary>
        public override void PerformAction(int amount, Point clickLocation)
        {
            System.Diagnostics.Stopwatch sw = null;
            long inventoryMs = 0;
            long animateMs = 0;

            if (StackEverythingRedux.Config.DebuggingMode)
            {
                sw = System.Diagnostics.Stopwatch.StartNew();
                var swInv = System.Diagnostics.Stopwatch.StartNew();
                PerformActionInternal(amount, clickLocation);
                swInv.Stop();
                inventoryMs = swInv.ElapsedMilliseconds;
                sw.Stop();
                Log.Trace($"[DEBUG] {nameof(PerformAction)}: inventory={inventoryMs}ms, animate={animateMs}ms, total={sw.ElapsedMilliseconds}ms");
            }
            else
            {
                PerformActionInternal(amount, clickLocation);
            }
        }

        private void PerformActionInternal(int amount, Point clickLocation)
        {
            System.Diagnostics.Stopwatch swAnim = null;
            long animateMs = 0;

            amount = Math.Min(amount, ClickedItem.Stack);
            Amount = amount;

            if (amount <= 0)
            {
                return;
            }

            // Update inventory
            var inventory = InvMenu.actualInventory;
            if (ClickedItemIndex >= 0 && ClickedItemIndex < inventory.Count && object.ReferenceEquals(inventory[ClickedItemIndex], ClickedItem))
            {
                var item = inventory[ClickedItemIndex];
                item.Stack -= amount;
                if (item.Stack <= 0)
                {
                    inventory[ClickedItemIndex] = null;
                }
            }

            // Animate
            if (StackEverythingRedux.Config.DebuggingMode)
            {
                swAnim = System.Diagnostics.Stopwatch.StartNew();
                Animate(amount, clickLocation);
                swAnim.Stop();
                animateMs = swAnim.ElapsedMilliseconds;
            }
            else
            {
                Animate(amount, clickLocation);
            }
        }

        /// <summary>Creates animation of coins flying from the inventory slot to the shop moneybox.</summary>
        private void Animate(int amount, Point clickLocation)
        {
            if (amount > 50)
            {
                return; // Skip animation for large amounts
            }

            int coins = Math.Min((amount / 8) + 2, 10); // Cap at 10 coins
            Vector2 snappedPosition = InvMenu.snapToClickableComponent(clickLocation.X, clickLocation.Y);
            Vector2 animPos = snappedPosition + new Vector2(HALF_TILE, HALF_TILE);
            Point startingPoint = new((int)snappedPosition.X + HALF_TILE, (int)snappedPosition.Y + HALF_TILE);

            int posX = NativeShopMenu.xPositionOnScreen;
            int posY = NativeShopMenu.yPositionOnScreen;
            int height = NativeShopMenu.height;
            Vector2 endingPoint = new(posX - 36, posY + height - InvMenu.height - 16);

            Vector2 accel1 = new(0f, 0.5f);
            Vector2 accel2 = Utility.getVelocityTowardPoint(startingPoint, endingPoint, 0.5f);
            Vector2 motion2 = Utility.getVelocityTowardPoint(startingPoint, endingPoint, 8f);

            for (int j = 0; j < coins; j++)
            {
                Animations.Add(new TemporaryAnimatedSprite(
                    textureName: Game1.debrisSpriteSheetName,
                    sourceRect: new Rectangle(Game1.random.Next(2) * SMALL_TILE, FULL_TILE, SMALL_TILE, SMALL_TILE),
                    animationInterval: 9999f,
                    animationLength: 1,
                    numberOfLoops: 999,
                    position: animPos,
                    flicker: false,
                    flipped: false
                )
                {
                    alphaFade = 0.025f,
                    motion = new Vector2(Game1.random.Next(-3, 4), -4f),
                    acceleration = accel1,
                    delayBeforeAnimationStart = j * 25,
                    scale = 2f
                });

                Animations.Add(new TemporaryAnimatedSprite(
                    textureName: Game1.debrisSpriteSheetName,
                    sourceRect: new Rectangle(Game1.random.Next(2) * SMALL_TILE, FULL_TILE, SMALL_TILE, SMALL_TILE),
                    animationInterval: 9999f,
                    animationLength: 1,
                    numberOfLoops: 999,
                    position: animPos,
                    flicker: false,
                    flipped: false
                )
                {
                    alphaFade = 0.025f,
                    motion = motion2,
                    acceleration = accel2,
                    delayBeforeAnimationStart = j * 50,
                    scale = 4f
                });
            }
        }

        /// <summary>Calculates the sale price of an item.</summary>
        private int CalculateSalePrice(Item item, int amount)
        {
            float price = SellPercentage * amount;
            price *= item is SObject sobj ? sobj.sellToStorePrice() : item.salePrice() * 0.5f;
            return -(int)price; // Negative to give money to the player
        }

        /// <summary>Creates an instance of the action.</summary>
        public static ShopAction Create(ShopMenu shopMenu, Point mouse)
        {
            InventoryMenu inventory = shopMenu.inventory;
            for (int i = 0; i < inventory.actualInventory.Count; i++)
            {
                Item item = inventory.actualInventory[i];
                if (item != null && inventory.getItemAt(mouse.X, mouse.Y) == item)
                {
                    return new SellAction(shopMenu, item, i);
                }
            }
            return null;
        }
    }
}