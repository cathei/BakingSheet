// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public class SheetConvertingContext
    {
        public SheetContainerBase Container { get; }
        public ILogger Logger { get; }

        public SheetConvertingContext(SheetContainerBase container, ILogger logger)
        {
            Container = container;
            Logger = logger;
        }
    }

    public class SheetVerifyingContext : SheetConvertingContext
    {
        public SheetVerifier[] Verifiers { get; }

        public SheetVerifyingContext(SheetContainerBase container, ILogger logger, SheetVerifier[] verifiers)
            : base(container, logger)
        {
            Verifiers = verifiers;
        }
    }
}
