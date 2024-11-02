using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Log
{
    /// <summary>
    /// Logger Output Class
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Add A Info Data
        /// </summary>
        /// <param name="from">The Package Name</param>
        /// <param name="message">The Message</param>
        public static void Info(string from, string message)
        {
            Record(from, "Info", message);
        }

        /// <summary>
        /// Add A Warn Data
        /// </summary>
        /// <param name="from">The Package Name</param>
        /// <param name="message">The Message</param>
        public static void Warn(string from, string message) 
        {
            Record(from, "Warn", message);
        }

        /// <summary>
        /// Add An Error Message
        /// </summary>
        /// <param name="from">The Package Name</param>
        /// <param name="message">The Message</param>
        public static void Error(string from, string message) 
        {
            Record(from, "Error", message);
        }

        internal static void Info(string message)
        {
            Info("PixanKit.LaunchCore", message);
        }

        internal static void Warn(string message)
        {
            Warn("PixanKit.LaunchCore", message);
        }

        internal static void Error(string message)
        {
            Error("PixanKit.LaunchCore", message);
        }

        private static void Record(string from, string type, string message)
        {
            string log = $"[{DateTime.Now} {from}] [{type}] {message}";
            Console.WriteLine(log);
        }
    }
}
