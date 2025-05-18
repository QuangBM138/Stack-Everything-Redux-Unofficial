//Log.cs
using StardewModdingAPI;

namespace StackEverythingRedux
{

    public static class Log
    {
        public static void Alert(string msg)
        {
            StackEverythingRedux.Instance.Monitor.Log(msg, LogLevel.Alert);
        }

        public static void Error(string msg)
        {
            StackEverythingRedux.Instance.Monitor.Log(msg, LogLevel.Error);
        }

        public static void Warn(string msg)
        {
            StackEverythingRedux.Instance.Monitor.Log(msg, LogLevel.Warn);
        }

        public static void Info(string msg)
        {
            StackEverythingRedux.Instance.Monitor.Log(msg, LogLevel.Info);
        }

      public static void Debug(string msg)
        {
            StackEverythingRedux.Instance.Monitor.Log(msg, LogLevel.Debug);
        }

        public static void Trace(string msg)
        {
            StackEverythingRedux.Instance.Monitor.Log(msg, LogLevel.Trace);
        }

        public static void TraceIfD(string msg)
        {
#if DEBUG
            bool debugging = true;
#else
            bool debugging = Mod.Config.DebuggingMode;
#endif
            if (debugging)
            {
                Trace(msg);
            }
        }
    }
}
