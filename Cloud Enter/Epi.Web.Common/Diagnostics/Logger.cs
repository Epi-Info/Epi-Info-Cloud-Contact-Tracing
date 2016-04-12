using System;
using System.Diagnostics;

namespace Epi.Web.Enter.Common.Diagnostics
{
    public class Logger : ILogger
    {
        public void Information(string message)
        {
            Trace.TraceInformation(message);
        }

        public void Information(string fmt, params object[] vars)
        {
            Trace.TraceInformation(fmt, vars);
        }

        public void Information(System.Exception exception, string fmt, params object[] vars)
        {
            var msg = String.Format(fmt, vars);
            Trace.TraceInformation(string.Format(fmt, vars) + ";Exception Details={0}", exception.ToString());
        }

        public void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        public void Warning(string fmt, params object[] vars)
        {
            Trace.TraceWarning(string.Format(fmt, vars));
        }

        public void Warning(System.Exception exception, string fmt, params object[] vars)
        {
            Trace.TraceWarning(string.Format(fmt, vars) + ";Exception Details={0}", exception.ToString());
        }

        public void Error(string message)
        {
            Trace.TraceError(message);
        }

        public void Error(string fmt, params object[] vars)
        {
            Trace.TraceError(string.Format(fmt, vars));
        }

        public void Error(System.Exception exception, string fmt, params object[] vars)
        {
            Trace.TraceError(string.Format(fmt, vars) + ";Exception Details={0}", exception.ToString());
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan, string properties)
        {
            string message = String.Concat("component:", componentName, ";method:", method, ";timespan:", timespan.ToString(), ";properties:", properties);
            Trace.TraceInformation(message);
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan)
        {
            string message = String.Concat("component:", componentName, ";method:", method, ";timespan:", timespan.ToString());
            Trace.TraceInformation(message);
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan, string fmt, params object[] vars)
        {
            string message = String.Concat("component:", componentName, ";method:", method, ";timespan:", timespan.ToString(), ";info:", string.Format(fmt, vars));
            Trace.TraceInformation(message);
        }
    }
}
