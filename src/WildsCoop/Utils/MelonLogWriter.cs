using MelonLoader;
using OuterWildsServerLib.Utils.Logger;
using System.Threading;

namespace WildsCoop.Utils
{
    public class MelonLogWriter : ILogWriter
    {
        public void Write(string formattedMessage, LogLevel logLevel)
        {
            MelonLogger.Log($"[SERVER/{Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString()}] {formattedMessage}");
        }
    }
}
