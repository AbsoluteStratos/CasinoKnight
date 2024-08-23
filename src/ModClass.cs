using Modding;
using Satchel.BetterMenus;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using StratosLogging;
using System.IO;
using System.Reflection;
using Satchel;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HollowKnightTreasureHunt
{
    public class HollowKnightTreasureHunt : Mod, ICustomMenuMod
    {

        public Dictionary<string, AssetBundle> Bundles { get; } = new();

        private Menu MenuRef;

        internal static HollowKnightTreasureHunt Instance;

        private AssetBundle ab = null; // Global


        private GameObject mySprite;

        private GameObject objab;
        //public override List<ValueTuple<string, string>> GetPreloadNames()
        //{
        //    return new List<ValueTuple<string, string>>
        //    {
        //        new ValueTuple<string, string>("White_Palace_18", "White Palace Fly")
        //    };
        //}

        //public HollowKnightTreasureHunt() : base("HollowKnightTreasureHunt")
        //{
        //    Instance = this;
        //}

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            StratosLogging.Log.Info("Initializing");

            Instance = this;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

            StratosLogging.Log.Info("Initialized");

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            On.HeroController.Awake += new On.HeroController.hook_Awake(this.OnHeroControllerAwake);
        }

        public void OnSceneChange(Scene scene_1, Scene scene_2)
        {
            StratosLogging.Log.Info("Scene changed!! " + scene_1.name + " to " + scene_2.name);
            if (scene_2.name == "Town")
            {
                string bundle_name = "HollowKnightTreasureHunt.Resources.stratos";
                
                if (ab == null)
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    using (Stream s = asm.GetManifestResourceStream(bundle_name))
                    {
                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();
                        Log("Loading bundle: " + bundle_name);
                        ab = AssetBundle.LoadFromMemory(buffer);
                    }
                    // Debugging
                    foreach (string name in asm.GetManifestResourceNames())
                    {
                        StratosLogging.Log.Info("Embedded resource: " + name);
                    }

                    foreach (string name in ab.GetAllAssetNames())
                    {
                        StratosLogging.Log.Info("Asset: " + name);
                    }
                }

                // Note the path is based on where you saved the prefab
                objab = ab.LoadAsset<GameObject>("Assets/Images/stratos.prefab");
                objab = GameObject.Instantiate(objab);
                objab.transform.position = Vector3.zero;
                objab.SetActive(true);

                for (int i = 0; i < objab.transform.childCount; i ++)
                {
                    GameObject child = objab.transform.GetChild(i).gameObject;
                    child.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }
            } else {
                StratosLogging.Log.Info("Unloading asset");
                ab.Unload(true);
            }
        }

        public void OnHeroUpdate()
        {
            // Here we use the Player Action to detect the input
            // This WasPressed is defined in the subclass `OneAxisInputControl`
            if (Input.GetKeyDown(KeyCode.O))
            {
                Log("Key Pressed");
                var assembly = Assembly.GetExecutingAssembly();

                string bundleN = "HollowKnightTreasureHunt.Resources.TestObject.unity3d";
                AssetBundle ab = null; // You probably want this to be defined somewhere more global.
                Assembly asm = Assembly.GetExecutingAssembly();



                StratosLogging.Log.Info("Loading bundle!!!");

                foreach (string res in asm.GetManifestResourceNames())
                {
                    StratosLogging.Log.Info("Embedded asset " + res);
                    Log(bundleN == res);
                }

      
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    Log(resourceName);
                    string bundleName = Path.GetExtension(resourceName).Substring(1);
                    if (bundleName != "testobject") continue;

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {

                        if (stream == null || Bundles.ContainsKey(bundleName)) continue;

                        var bundle = AssetBundle.LoadFromStream(stream);
                        Bundles.Add(bundleName, bundle);

                        stream.Dispose();

                    }

                }

                        /*using (Stream s = assembly.GetManifestResourceStream(bundleN))
                    {
                        string bundleName = Path.GetExtension(bundleN).Substring(1);
                        StratosLogging.Log.Info("Loading bundle " + bundleName);
                        // Allows us to directly load from stream.
                        ab = AssetBundle.LoadFromStream(s); // Store this somewhere you can access again.
                        StratosLogging.Log.Info("Done");
                    }*/
                        /*foreach (string res in asm.GetManifestResourceNames())
                        {
                            StratosLogging.Log.Info("Yooo1");
                            using (Stream s = asm.GetManifestResourceStream(res))
                            {
                                if (s == null) continue;
                                string bundleName = Path.GetExtension(res).Substring(1);
                                if (bundleName != bundleN) continue;
                                StratosLogging.Log.Info("Loading bundle " + bundleName);
                                // Allows us to directly load from stream.
                                ab = AssetBundle.LoadFromStream(s); // Store this somewhere you can access again.
                            }
                        }*/

                /*byte[] buffer = Assembly.GetCallingAssembly().GetBytesFromResources(bundleN);
                Log(buffer.Length);
                if (buffer == null)
                {
                    return;
                }
                ab = AssetBundle.LoadFromMemory(buffer);*/

                StratosLogging.Log.Info("Activating Game Object");
                foreach (string res in Bundles["testobject"].GetAllAssetNames())
                {
                    StratosLogging.Log.Warning(res);
                }

                objab = Bundles["testobject"].LoadAsset<GameObject>("Assets/Images/TestObject.prefab");
                objab.transform.position = Vector3.zero;
                StratosLogging.Log.Warning(objab.name);
                mySprite = GameObject.Instantiate(objab);
                mySprite.SetActive(true);
                mySprite.transform.position = HeroController.instance.transform.position;
                StratosLogging.Log.Warning(Shader.Find("Sprites/Default").ToString());
                mySprite.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                //mySprite.layer = 1;
                StratosLogging.Log.Warning(mySprite.GetComponent<SpriteRenderer>().material.name);
                StratosLogging.Log.Warning(mySprite.transform.position.ToString());
            }
        }

        private void OnHeroControllerAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig.Invoke(self);

            
        }

        /// <summary>
        /// Get mode menu, required for Stachel better menu interface
        /// </summary>
        /// <param name="modListMenu"></param>
        /// <param name="modtoggledelegates"></param>
        /// <returns></returns>
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates)
        {
            return ModMenu.GetMenuScreen(MenuRef, modListMenu, modtoggledelegates);
        }

        //a property requuired by the ICustomMenuMod interface to be implemented
        //since our mod is not IToggleable, we can leave it as is (leave it as null)
        public bool ToggleButtonInsideMenu { get; }
    }
}