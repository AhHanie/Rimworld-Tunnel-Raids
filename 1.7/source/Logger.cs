using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Tunnel_Raids
{
    public static class Logger
    {
        private const string Prefix = "[Tunnel Raids] ";

        [Conditional("DEBUG")]
        public static void Message(string message)
        {
            Log.Message(Prefix + message);
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            Log.Warning(Prefix + message);
        }

        private static readonly HashSet<string> loggedOnce = new HashSet<string>();

        public static void WarningOnce(string key, string message)
        {
            if (loggedOnce.Add(key))
            {
                Log.Warning(Prefix + message);
            }
        }

        public static void Error(string message)
        {
            Log.Error(Prefix + message);
        }

        public static void Exception(Exception exception, string context = null)
        {
            if (exception == null)
            {
                return;
            }

            string prefix = string.IsNullOrWhiteSpace(context) ? Prefix : Prefix + context + ": ";
            Log.Error(prefix + exception);
        }
    }
}
