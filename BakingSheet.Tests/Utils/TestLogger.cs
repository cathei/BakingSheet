using System;
using Xunit;
using Cathei.BakingSheet.Internal;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cathei.BakingSheet.Tests
{
    public class TestLogger : ILogger
    {
        private IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

        private struct LogEntry
        {
            public LogLevel level;
            public List<object> scopes;
            public string message;

            public override string ToString()
            {
                return $"[{level}] [{string.Join(">", scopes)}] {message}";
            }
        }

        private List<LogEntry> entries = new List<LogEntry>();

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
            var entry = new LogEntry
            {
                level = logLevel,
                scopes = new List<object>(),
                message = formatter(state, exception)
            };

            scopeProvider.ForEachScope((scope, scopes) => scopes.Add(scope), entry.scopes);

            entries.Add(entry);
        }

        public void VerifyLog(LogLevel logLevel, string message, object[] scopes = null)
        {
            Assert.Contains(entries, entry => 
                entry.level == logLevel && entry.message == message &&
                (scopes == null || Enumerable.SequenceEqual(scopes, entry.scopes))
            );
        }
    }
}
