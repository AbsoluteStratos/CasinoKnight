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
using HutongGames.PlayMaker.Actions;

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

        private Tk2dPlayAnimation exitDoor;

        private CasinoShopHandler casino;

        public GameObject CardPrefab;
        public Dictionary<string, Dictionary<string, GameObject>> preloads;
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

            // UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

            StratosLogging.Log.Info("Initialized");

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            On.HeroController.Awake += new On.HeroController.hook_Awake(this.OnHeroControllerAwake);

            // https://github.com/PrashantMohta/Smolknight/blob/6a6253ca3ea6549cc17bff47c33ade2ac28054e7/Smolknight.cs#L134
            preloads = preloadedObjects;
            StratosLogging.Log.Info(preloads.Keys.ToString());
            CardPrefab = preloads["Cliffs_01"]["Cornifer Card"];
            CustomArrowPrompt.Prepare(CardPrefab);

            StratosLogging.Log.Warning(CustomArrowPrompt.ArrowPromptPrefab.name);

            StratosLogging.Log.Info("Loaded!!!");
        }

        // https://prashantmohta.github.io/ModdingDocs/preloads.html#how-to-preload-an-object
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Cliffs_01","Cornifer Card"),
            };
        }


        private void OnHeroControllerAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig.Invoke(self);
            casino = self.gameObject.GetAddComponent<CasinoShopHandler>();
        }

        public void OnHeroUpdate()
        {

            casino.OnHeroUpdate();
            // Here we use the Player Action to detect the input
            // This WasPressed is defined in the subclass `OneAxisInputControl`
            if (Input.GetKeyDown(KeyCode.O))
            {


                Log("Key Pressed");

                //HeroController.instance.StartCoroutine(PlayExitAnimation());

            }


        }

        public void OnSceneChange(Scene scene_1, Scene scene_2)
        {

            TransitionPoint[] transions = UnityEngine.Object.FindObjectsOfType<TransitionPoint>();
            foreach (TransitionPoint tobj in transions)
            {
                StratosLogging.Log.Info(tobj.name + " to: " + tobj.targetScene + " _ " + tobj.entryPoint);
            }

            


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

                for (int i = 0; i < objab.transform.childCount; i++)
                {
                    GameObject child = objab.transform.GetChild(i).gameObject;
                    child.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                    for (int j = 0; j < child.transform.childCount; j++)
                    {
                        GameObject child2 = child.transform.GetChild(i).gameObject;
                        child2.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                    }
                }

                // Add a transition hook door collider
                GameObject gate = objab.transform.Find("Casino").gameObject.transform.Find("door_casino").gameObject;
                StratosLogging.Log.Warning(gate.name);

                var tp = gate.AddComponent<TransitionPoint>();
           
                /*var bc = gate.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(1f, 4f);
                bc.isTrigger = true;*/
                tp.isADoor = true;
                tp.targetScene = "Crossroads_01";
                tp.entryPoint = "top1";

                PlayMakerFSM fsm = gate.AddComponent<PlayMakerFSM>();
                



                //tp.alwaysEnterLeft = true;
                //tp.alwaysEnterRight = false;

                GameObject rm = objab.transform.Find("Casino").gameObject.transform.Find("Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.AddComponent<HazardRespawnMarker>();
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Dream;

                StratosLogging.Log.Warning("Gate Set up");




            } else {
                StratosLogging.Log.Info("Unloading asset");
                ab.Unload(true);
            }
        }

        private static void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate,
                                  bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
        {
            GameObject gate = new GameObject(gateName);
            gate.transform.SetPosition2D(pos);
            var tp = gate.AddComponent<TransitionPoint>();
            if (!onlyOut)
            {
                var bc = gate.AddComponent<BoxCollider2D>();
                bc.size = size;
                bc.isTrigger = true;
                tp.targetScene = toScene;
                tp.entryPoint = entryGate;
            }
            tp.alwaysEnterLeft = left;
            tp.alwaysEnterRight = right;
            GameObject rm = new GameObject("Hazard Respawn Marker");
            rm.transform.parent = gate.transform;
            rm.tag = "RespawnPoint";
            rm.transform.SetPosition2D(pos);
            tp.respawnMarker = rm.AddComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = vis;
        }


        

        IEnumerator ExampleCoroutine()
        {
            //Print the time of when the function is first called.
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(5);

            //After we have waited 5 seconds print the time again.
            Debug.Log("Finished Coroutine at timestamp : " + Time.time);
        }

        private IEnumerator PlayExitAnimation()
        {
            tk2dSpriteAnimator _anim = HeroController.instance.GetComponent<tk2dSpriteAnimator>();

            foreach (tk2dSpriteAnimationClip clip in _anim.Library.clips)
            {
                StratosLogging.Log.Warning(clip.name);
                // yield return new WaitForSeconds(0.2f);
            }

            HeroController.instance.StopAnimationControl();

            yield return _anim.PlayAnimWait("Enter");

            HeroController.instance.StartAnimationControl();

            yield break;
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