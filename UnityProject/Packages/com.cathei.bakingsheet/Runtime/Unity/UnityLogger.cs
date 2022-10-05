// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cathei.BakingSheet.Unity
{
    public class UnityLogger : ILogger
    {
        public static readonly UnityLogger Default = new UnityLogger();

        private IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

        private IList<object> scopes = new List<object>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return scopeProvider.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            scopes.Clear();
            scopeProvider.ForEachScope((x, scopes) => scopes.Add(x), scopes);

            var message = formatter(state, exception);
            if (scopes.Count > 0)
                message = $"[{string.Join(">", scopes)}] {message}";

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    Debug.Log(message);
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;

                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError(message);
                    break;
            }
        }
    }
}
