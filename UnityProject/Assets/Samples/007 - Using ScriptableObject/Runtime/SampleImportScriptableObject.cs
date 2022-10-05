using Cathei.BakingSheet.Unity;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class SampleImportScriptableObject : MonoBehaviour
    {
        private SheetContainer container;
        public SheetContainerScriptableObject containerSO;

        public async void Start()
        {
            var importer = new ScriptableObjectSheetImporter(containerSO);

            container = new SheetContainer();
            await container.Bake(importer);

            foreach (var dungeon in container.Dungeons)
            {
                Debug.Log(dungeon.Name);
                Debug.Log(dungeon.Monsters[0].Ref.Name);
                Debug.Log(dungeon.Items[0].Ref.Name);
            }

            foreach (var asset in container.Assets)
            {
                Debug.Log(asset.Direct.Get<Object>());
                Debug.Log(asset.Resource.Load<Object>());
            }
        }
    }
}
