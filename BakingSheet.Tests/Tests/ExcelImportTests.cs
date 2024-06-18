// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class ExcelImportTests : IDisposable
    {
        private TestLogger _logger;
        private ExcelTestContainer _container;

        private const string BasePath = "Data/ExcelImportTests";

        public ExcelImportTests()
        {
            _logger = new TestLogger();
            _container = new ExcelTestContainer(_logger);
        }

        public void Dispose()
        {
            CultureInfo.DefaultThreadCurrentCulture = null;
        }

        [Theory]
        [InlineData("ko-KR", "1234.5")]
        [InlineData("fr-FR", "1234,5")]
        public async Task TestImportTypesJson(string cultureName, string expect)
        {
            // Setting underlying culture
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;

            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            Assert.Equal(expect, 1234.5f.ToString());

            var converter = new ExcelSheetConverter(BasePath, formatProvider: CultureInfo.InvariantCulture);
            var result = await _container.Bake(converter);

            _logger.VerifyNoError();

            Assert.True(result);
            Assert.NotNull(_container.Tests);

            Assert.Equal("Hello, World!", _container.Tests["LiteralTest"].Content);
            Assert.Equal("30", _container.Tests["FunctionTest"].Content);

            Assert.Equal(6, _container.Simple.Count);
            Assert.Equal("Health Potion", _container.Simple["Potion001"].Name);
            Assert.Equal("Blood Potion", _container.Simple["Potion003"].Name);

            Assert.Equal(1.4f, _container.Hero["HERO001"].GetLevel(3).StatMultiplier, 4);
            Assert.Equal(1.2f, _container.Hero["HERO002"].GetLevel(2).StatMultiplier, 4);
        }

        private class SimpleSheet : Sheet<SimpleSheet.Row>
        {
            public class Row : SheetRow
            {
                public string Name { get; set; }
                public int Price { get; set; }
            }
        }

        public class HeroSheet : Sheet<HeroSheet.Row>
        {
            public class Row : SheetRowArray<Elem>
            {
                public string Name { get; private set; }

                public int Strength { get; private set; }
                public int Intelligence { get; private set; }
                public int Vitality { get; private set; }

                public Elem GetLevel(int level)
                {
                    return this[level - 1];
                }
            }

            public class Elem : SheetRowElem
            {
                public float StatMultiplier { get; private set; }
                public int RequiredExp { get; private set; }
                public string RequiredItem { get; private set; }
            }
        }

        private class ExcelTestContainer : SheetContainerBase
        {
            public ExcelTestContainer(ILogger logger) : base(logger)
            {
            }

            public TestSheet Tests { get; set; }
            public SimpleSheet Simple { get; set; }
            public HeroSheet Hero { get; set; }
        }
    }
}
