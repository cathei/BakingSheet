// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public class SheetConvertingContext
    {
        public SheetContainerBase Container { get; }
        public ILogger Logger { get; }
        public SheetVerifier[]? Verifiers { get; }

        public SheetConvertingContext(SheetContainerBase container, ILogger logger, SheetVerifier[]? verifiers = null)
        {
            Container = container;
            Logger = logger;
            Verifiers = verifiers;
        }
    }
}
