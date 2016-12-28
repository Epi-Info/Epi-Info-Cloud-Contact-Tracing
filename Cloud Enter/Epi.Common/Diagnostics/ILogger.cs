namespace Epi.Common.Diagnostics
{
    public interface ILogger
    {
        void Information(string message);
        void Information(string fmt, params object[] vars);
        void Information(System.Exception exception, string fmt, params object[] vars);

        void Warning(string message);
        void Warning(string fmt, params object[] vars);
        void Warning(System.Exception exception, string fmt, params object[] vars);

        void Error(string message);
        void Error(string fmt, params object[] vars);
        void Error(System.Exception exception, string fmt, params object[] vars);

        void TraceApi(string componentName, string method, System.TimeSpan timespan);
        void TraceApi(string componentName, string method, System.TimeSpan timespan, string properties);
        void TraceApi(string componentName, string method, System.TimeSpan timespan, string fmt, params object[] vars);
    }
}
