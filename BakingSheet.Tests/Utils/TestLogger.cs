// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

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
                (scopes == null || scopes.SequenceEqual(entry.scopes))
            );
        }

        public void VerifyNoError()
        {
            Assert.DoesNotContain(entries, entry =>
                entry.level >= LogLevel.Error && !entry.message.Contains("Failed to find sheet"));
        }
    }
}
