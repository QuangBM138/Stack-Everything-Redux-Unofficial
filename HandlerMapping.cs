//HandlerMapping.cs
using StackEverythingRedux.MenuHandlers;

namespace StackEverythingRedux
{
    internal static class HandlerMapping
    {
        private static readonly Dictionary<Type, IMenuHandler> HandlerByType = new Dictionary<Type, IMenuHandler>();
        private static readonly Dictionary<string, IMenuHandler> HandlerByName = new Dictionary<string, IMenuHandler>();
        private static readonly Dictionary<Type, IMenuHandler> HandlerSingletons = new Dictionary<Type, IMenuHandler>();


        private static IMenuHandler GetSingleton(Type handlerType)
        {
            if (!typeof(IMenuHandler).IsAssignableFrom(handlerType))
            {
                throw new Exception($"{handlerType} does not implement IMenuHandler!");
            }

            if (!HandlerSingletons.TryGetValue(handlerType, out IMenuHandler handler))
            {
                handler = (IMenuHandler)Activator.CreateInstance(handlerType);
                HandlerSingletons[handlerType] = handler;
            }
            return handler;
        }

        internal static void Add(Type menuType, Type handlerType)
        {
            if (HandlerByType.ContainsKey(menuType))
            {
                Log.Warn($"Redefining handler for {menuType}");
            }

            Add(menuType, GetSingleton(handlerType));
        }

        internal static void Add(string menuClass, Type handlerType)
        {
            if (HandlerByName.ContainsKey(menuClass))
            {
                Log.Warn($"Redefining handler for {menuClass}");
            }

            HandlerByName[menuClass] = GetSingleton(handlerType);
        }

        internal static void Add(Type menuType, IMenuHandler handler)
        {
            if (HandlerByType.ContainsKey(menuType))
            {
                Log.Warn($"Redefining handler for {menuType}");
            }

            HandlerByType[menuType] = handler;
        }

        internal static bool TryGetHandler(Type menuType, out IMenuHandler handler)
        {
            if (HandlerByType.TryGetValue(menuType, out handler))
            {
                return true;
            }

            foreach (KeyValuePair<Type, IMenuHandler> kvp in HandlerByType)
            {
                if (menuType.IsSubclassOf(kvp.Key))
                {
                    handler = kvp.Value;
                    return true;
                }
            }
            return false;
        }

        internal static bool TryGetHandler(string menuClass, out IMenuHandler handler)
        {
            return HandlerByName.TryGetValue(menuClass, out handler);
        }
    }

}
