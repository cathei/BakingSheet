using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class SheetLoader : MonoBehaviour
    {
        public SheetContainer Sheet { get; private set; }

        private void Start()
        {
            Sheet = new SheetContainer(new UnityLogger());
            Sheet.Load(Path.Combine(Application.streamingAssetsPath, "Excel"));
        }

    }

}