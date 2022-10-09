// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public class SheetConvertingContext
    {
        public SheetContainerBase Container { get; set; }
        public ILogger Logger { get; set; }
        public SheetVerifier[] Verifiers { get; set; }
    }
}
