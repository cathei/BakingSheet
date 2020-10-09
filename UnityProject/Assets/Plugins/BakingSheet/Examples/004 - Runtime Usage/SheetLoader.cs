using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class SheetLoader : MonoBehaviour
    {
        public SheetContainer Sheet { get; private set; }

        private async void Start()
        {
            Sheet = new SheetContainer(new UnityLogger());
            await Sheet.Load(Path.Combine(Application.streamingAssetsPath, "Excel"));

            Debug.Log(Sheet.Constants.Count);
            Debug.Log(Sheet.Heroes["HERO001"].Count);
            Debug.Log(Sheet.Items["ITEM_POTION001"].Name);
        }
    }
}