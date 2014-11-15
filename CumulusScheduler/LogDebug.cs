
using System;
namespace CumulusScheduler
{
    public class Debug : LogBase
    {
        public static void WriteLine(string message, params object[] args)
        {
            if (log.IsDebugEnabled) log.DebugFormat(message, args);
        }
        public static void WriteLine(string message, string category)
        {
            if (log.IsDebugEnabled) log.DebugFormat("{0}|{1}", category, message);
        }
        public static void WriteLine(string message)
        {
            if (log.IsDebugEnabled) log.Debug(message);
        }
        public static void WriteLine(object value, string category)
        {
            if (log.IsDebugEnabled) log.DebugFormat("{0}|{1}", category, value.ToString());
        }
        public static void WriteLine(object value)
        {
            if (log.IsDebugEnabled) log.Debug(value.ToString());
        }
        public static void WriteLine(Exception ex)
        {
            if (log.IsDebugEnabled) WriteException(ex);
        }
        public static void Write(string message, string category)
        {
            if (log.IsDebugEnabled) log.DebugFormat("{0}|{1}", category, message);
        }
        public static void Write(string message)
        {
            if (log.IsDebugEnabled) log.Debug(message);
        }
        public static void Write(object value, string category)
        {
            if (log.IsDebugEnabled) log.DebugFormat("{0}|{1}", category, value.ToString());
        }
        public static void Write(object value)
        {
            if (log.IsDebugEnabled) log.Debug(value.ToString());
        }
        public static void Assert(bool condition, string message, string detailMessage)
        {
            System.Diagnostics.Debug.Assert(condition, message, detailMessage);
        }
        public static void Assert(bool condition, string message)
        {
            System.Diagnostics.Debug.Assert(condition, message);
        }
        public static void Assert(bool condition)
        {
            System.Diagnostics.Debug.Assert(condition);
        }
    }
}
