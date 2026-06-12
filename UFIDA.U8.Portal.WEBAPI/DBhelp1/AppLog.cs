using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.Tracing;
using Newtonsoft.Json;
using NLog;


namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public sealed class AppLog : ITraceWriter
    {
        private static readonly Logger AppLogger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<Dictionary<TraceLevel, Action<string>>> LoggingMap = new Lazy<Dictionary<TraceLevel, Action<string>>>(() => new Dictionary<TraceLevel, Action<string>>
    {
        {
            TraceLevel.Info,
            AppLogger.Info
        },
        {
            TraceLevel.Debug,
            AppLogger.Debug
        },
        {
            TraceLevel.Error,
            AppLogger.Error
        },
        {
            TraceLevel.Fatal,
            AppLogger.Fatal
        },
        {
            TraceLevel.Warn,
            AppLogger.Warn
        }
    });

        private Dictionary<TraceLevel, Action<string>> Logger => LoggingMap.Value;

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (level != TraceLevel.Off)
            {
                if (traceAction != null && traceAction.Target != null)
                {
                    category = category + Environment.NewLine + "Action Parameters : " + JsonConvert.SerializeObject(traceAction.Target);
                }
                TraceRecord traceRecord = new TraceRecord(request, category, level);
                traceAction?.Invoke(traceRecord);
                Log(traceRecord);
            }
        }

        private void Log(TraceRecord record)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(record.Message))
            {
                stringBuilder.Append("").Append(record.Message + Environment.NewLine);
            }
            if (record.Request != null)
            {
                if (record.Request.Method != null)
                {
                    stringBuilder.Append(string.Concat("Method : ", record.Request.Method, Environment.NewLine));
                }
                if (record.Request.RequestUri != null)
                {
                    stringBuilder.Append("").Append(string.Concat("URL : ", record.Request.RequestUri, Environment.NewLine));
                }
                if (record.Request.Headers != null && record.Request.Headers.Contains("Token") && record.Request.Headers.GetValues("Token") != null && record.Request.Headers.GetValues("Token").FirstOrDefault() != null)
                {
                    stringBuilder.Append("").Append("Token : " + record.Request.Headers.GetValues("Token").FirstOrDefault() + Environment.NewLine);
                }
            }
            if (!string.IsNullOrWhiteSpace(record.Category))
            {
                stringBuilder.Append("").Append(record.Category);
            }
            if (record.Exception != null && !string.IsNullOrWhiteSpace(record.Exception.GetBaseException().Message))
            {
                Type type = record.Exception.GetType();
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("").Append("Error : " + record.Exception.GetBaseException().Message + Environment.NewLine);
            }
            Logger[record.Level](Convert.ToString(stringBuilder) + Environment.NewLine);
        }
    }
}
