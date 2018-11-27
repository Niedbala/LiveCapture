using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Smo.Common.Enums;

namespace Smo.Common
{
    public interface ILoggingService
    {
        void Log(string message, LogLevel logLevel = LogLevel.Information);
    }

    //TODO use log4net as there is a file locking conflict
    public class LoggingService : ILoggingService
    {
        public LoggingService()
        {
           // InitializeLog();
        }


        public void InitializeLog()
        {

            var logFile = @"D:\logfiles\traceLog.txt";

            var mappedPath = HttpContext.Current?.Server?.MapPath(logFile);

            var logPath = (new FileInfo(logFile)).Directory;

            if (!String.IsNullOrWhiteSpace(mappedPath))
            {
                try
                {
                    logPath.Create();
                    Trace.UseGlobalLock = true;

                    var listenersToRemove = new List<TraceListener>();

                    foreach (var listener in Trace.Listeners)
                    {
                        var l = listener as TextWriterTraceListener;
                        if (l != null)
                        {
                            l.Flush();
                            try
                            {
                                l.Close();
                            }
                            catch
                            {

                            }

                            l.Dispose();

                            listenersToRemove.Add(l);
                        }
                    }

                    listenersToRemove.ForEach(l => Trace.Listeners.Remove(l));

                    Trace.Listeners.Add(new IisTraceListener());

                    FileStream fs = new FileStream(mappedPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                        FileShare.ReadWrite);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    System.Diagnostics.TextWriterTraceListener txtListener = new
                        System.Diagnostics.TextWriterTraceListener(sw, "txt_listener");

                    System.Diagnostics.Trace.Listeners.Add(txtListener);
                    System.Diagnostics.Trace.AutoFlush = true;

                    Trace.TraceInformation(
                        "-----------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Log started at path: " + logPath.FullName);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Error while setting log file at path: " + logPath.FullName + " " + e);
                }
            }
        }

        public void Log(string message, LogLevel logLevel = LogLevel.Information)
        {
            try
            {
                message = DateTime.Now.ToString() + " " + message;

                switch (logLevel)
                {
                    case LogLevel.Information:
                        Trace.TraceInformation(message);
                        break;
                    case LogLevel.Warning:
                        Trace.TraceWarning(message);
                        break;
                    case LogLevel.Error:
                        Trace.TraceError(message);
                        break;

                }
            }
            catch
            {

            }

        }
    }
}
