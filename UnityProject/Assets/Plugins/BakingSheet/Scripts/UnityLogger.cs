using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cathei.BakingSheet
{
    public class UnityLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    Debug.Log(formatter(state, exception));
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning(formatter(state, exception));
                    break;

                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogWarning(formatter(state, exception));
                    break;
            }
        } 
    }
}
