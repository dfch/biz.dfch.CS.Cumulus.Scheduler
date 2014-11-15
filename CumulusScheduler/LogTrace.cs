
using System;
namespace CumulusScheduler
{
    public class Trace : LogBase
    {
        public static void WriteLine(string message, params object[] args)
        {
            if (log.IsInfoEnabled) log.InfoFormat(message, args);
        }
        public static void WriteLine(string message, string category)
        {
            if (log.IsInfoEnabled) log.InfoFormat("{0}|{1}", category, message);
        }
        public static void WriteLine(string message)
        {
            if (log.IsInfoEnabled) log.Info(message);
        }
        public static void WriteLine(object value, string category)
        {
            if (log.IsInfoEnabled) log.InfoFormat("{0}|{1}", category, value.ToString());
        }
        public static void WriteLine(object value)
        {
            if (log.IsInfoEnabled) log.Info(value.ToString());
        }
        public static void WriteLine(Exception ex)
        {
            if (log.IsInfoEnabled) WriteException(ex);
        }
        public static void Write(string message, string category)
        {
            if (log.IsInfoEnabled) log.InfoFormat("{0}|{1}", category, message);
        }
        public static void Write(string message)
        {
            if (log.IsInfoEnabled) log.Info(message);
        }
        public static void Write(object value, string category)
        {
            if (log.IsInfoEnabled) log.InfoFormat("{0}|{1}", category, value.ToString());
        }
        public static void Write(object value)
        {
            if (log.IsInfoEnabled) log.Info(value.ToString());
        }
        public static void Assert(bool condition, string message, string detailMessage)
        {
            System.Diagnostics.Trace.Assert(condition, message, detailMessage);
        }
        public static void Assert(bool condition, string message)
        {
            System.Diagnostics.Trace.Assert(condition, message);
        }
        public static void Assert(bool condition)
        {
            System.Diagnostics.Trace.Assert(condition);
        }
    }
}
