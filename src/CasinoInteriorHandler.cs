using StratosLogging;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using StratosLogging;
using GlobalEnums;
using UnityEngine.SceneManagement;
using Modding;
using UObject = UnityEngine.Object;

namespace HollowKnightTreasureHunt
{
    public class CasinoInteriorHandler : MonoBehaviour
    {

        public static AssetBundle casinoScene;
        private void Awake()
        {
            //casinoScene = LoadAssetBundle();
            //Log.Warning(casinoScene.GetAllScenePaths()[0]);
            //UnityEngine.SceneManagement.SceneManager.LoadScene(casinoScene.GetAllScenePaths()[0]);
            //Log.Info(casinoScene.GetAllScenePaths()[0]);

            // GameObject objab = Instantiate(casinoScene.LoadAsset<GameObject>("Assets/Mod/stratos.prefab"));
            /*HeroController.instance.transform.position = new Vector3(0.257999986f, 0f, 0f);
            string[] assetNames = casinoScene.GetAllAssetNames();
            foreach (string assetName in assetNames)
            {
                Log.Info(">>>" + assetName);
            }*/
        }

        private AssetBundle LoadAssetBundle(string name = "HollowKnightTreasureHunt.Resources.casinoscene")
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream(name))
            {
                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                s.Dispose();
                Log.Info("Loading bundle: " + name);
                return AssetBundle.LoadFromMemory(buffer);
            }
        }


        private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<LoadScene>();
        }

        public void Unload()
        {
            ModHooks.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.NewGameHook -= AddComponent;

            // ReSharper disable once Unity.NoNullPropogation
            var x = GameManager.instance?.gameObject.GetComponent<LoadScene>();
            if (x == null) return;
            UObject.Destroy(x);
        }
    }
}
