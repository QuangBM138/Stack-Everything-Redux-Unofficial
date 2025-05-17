using StackEverythingRedux.Models;
using StardewModdingAPI;

namespace StackEverythingRedux.Network
{
    internal static class Translations
    {
        private static ITranslationHelper i18n;

        public static void Init(ITranslationHelper translationHelper)
        {
            i18n = translationHelper;
        }

        public static string Config_MaxStackingNumber_Name => i18n.Get("config.maxstackingnumber.name");
        public static string Config_MaxStackingNumber_Tooltip => i18n.Get("config.maxstackingnumber.tooltip");
        public static string Config_EnableStackSplitRedux_Name => i18n.Get("config.enablestacksplitredux.name");
        public static string Config_EnableStackSplitRedux_Tooltip => i18n.Get("config.enablestacksplitredux.tooltip");
        public static string Config_DefaultCraftingAmount_Name => i18n.Get("config.defaultcraftingamount.name");
        public static string Config_DefaultCraftingAmount_Tooltip => i18n.Get("config.defaultcraftingamount.tooltip");
        public static string Config_DefaultShopAmount_Name => i18n.Get("config.defaultshopamount.name");
        public static string Config_DefaultShopAmount_Tooltip => i18n.Get("config.defaultshopamount.tooltip");
    }

    internal class GenericModConfigMenuIntegration
    {
        private static ModConfig Config = StackEverythingRedux.Config;

        public static void AddConfig()
        {
            IGenericModConfigMenuApi genericModConfigApi = StackEverythingRedux.Registry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            IManifest mod = StackEverythingRedux.Manifest;

            if (genericModConfigApi is null)
            {
                Log.Trace("GMCM not available, skipping Mod Config Menu");
                return;
            }

            if (Config is null)
            {
                return;
            }

            Translations.Init(StackEverythingRedux.I18n);

            genericModConfigApi.Register(
                mod,
                reset: () => Config = new ModConfig(),
                save: () => StackEverythingRedux.ModHelper.WriteConfig(Config)
            );

            genericModConfigApi.AddNumberOption(
                mod,
                name: () => Translations.Config_MaxStackingNumber_Name,
                tooltip: () => Translations.Config_MaxStackingNumber_Tooltip,
                getValue: () => Config.MaxStackingNumber,
                setValue: value => Config.MaxStackingNumber = value
            );

            genericModConfigApi.AddSectionTitle(mod, () => "Stack Split Redux");

            genericModConfigApi.AddBoolOption(
                mod,
                name: () => Translations.Config_EnableStackSplitRedux_Name,
                tooltip: () => Translations.Config_EnableStackSplitRedux_Tooltip,
                getValue: () => Config.EnableStackSplitRedux,
                setValue: value => Config.EnableStackSplitRedux = value
            );

            genericModConfigApi.AddNumberOption(
                mod,
                name: () => Translations.Config_DefaultCraftingAmount_Name,
                tooltip: () => Translations.Config_DefaultCraftingAmount_Tooltip,
                getValue: () => Config.DefaultCraftingAmount,
                setValue: value => Config.DefaultCraftingAmount = value
            );

            genericModConfigApi.AddNumberOption(
                mod,
                name: () => Translations.Config_DefaultShopAmount_Name,
                tooltip: () => Translations.Config_DefaultShopAmount_Tooltip,
                getValue: () => Config.DefaultShopAmount,
                setValue: value => Config.DefaultShopAmount = value
            );
        }
    }
}