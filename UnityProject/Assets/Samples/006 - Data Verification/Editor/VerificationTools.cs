using System;
using System.IO;
using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cathei.BakingSheet.Examples
{
    public static class VerificationTools
    {
        class VerificationSheet : Sheet<VerificationSheet.Row>
        {
            public class Row : SheetRow
            {
                 public ResourcePath ResourcePath { get; set; }
            }
        }

        class SheetContainer : SheetContainerBase
        {
            public SheetContainer(ILogger logger) : base(logger) { }

            public VerificationSheet Verification { get; set; }
        }

        [MenuItem("BakingSheet/Sample/Verify Sheet")]
        public static void VerifySheet()
        {
            var sheetContainer = new SheetContainer(new UnityLogger());

            // you can generate sheet with code
            sheetContainer.Verification = new VerificationSheet
            {
                new VerificationSheet.Row
                {
                    Id = "Row1",
                    ResourcePath = new ResourcePath("SamplePrefab1")
                },
                new VerificationSheet.Row
                {
                    Id = "Row2",
                    ResourcePath = new ResourcePath("SamplePrefab2")
                },
                new VerificationSheet.Row
                {
                    Id = "Row3",
                    ResourcePath = new ResourcePath("SamplePrefab3")
                },
            };

            // SheetContainer.Bake will call PostLoad implicitly
            // However you need to call PostLoad manually if you're generating sheet with code
            sheetContainer.PostLoad();

            // call SheetVerifier instances you want to process
            sheetContainer.Verify(new ResourcePathVerifier());

            Debug.Log("End of sheet verification");
        }
    }
}