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
using GlobalEnums;
using static Satchel.SceneUtils;
using static Mono.Security.X509.X520;

namespace HollowKnightTreasureHunt
{
    public class HollowKnightTreasureHunt : Mod, ICustomMenuMod
    {
        private Menu MenuRef;

        internal static HollowKnightTreasureHunt Instance;

        public GameObject CardPrefab;
        public Dictionary<string, Dictionary<string, GameObject>> preloads;
        public static Satchel.Core SatchelCore = new Satchel.Core();

        public Satchel.CustomScene casinoInterior;
        public static AssetBundle casinoScene;
        public string sceneName;

        public CasinoInterior Cassino;
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

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            On.HeroController.Awake += new On.HeroController.hook_Awake(this.OnHeroControllerAwake);

            // https://github.com/PrashantMohta/Smolknight/blob/6a6253ca3ea6549cc17bff47c33ade2ac28054e7/Smolknight.cs#L134
            preloads = preloadedObjects;
            StratosLogging.Log.Info(preloads.Keys.ToString());
            CardPrefab = preloads["Cliffs_01"]["Cornifer Card"];
            CustomArrowPrompt.Prepare(CardPrefab);

            // Create casino interio scene object
            Cassino = new CasinoInterior(preloads["Room_mapper"]["TileMap"], preloads["Room_mapper"]["_SceneManager"]);

            StratosLogging.Log.Info("Loaded!!!");
        }

        private IEnumerator PlayExitAnimation()
        {
            yield break;
        }

        // https://prashantmohta.github.io/ModdingDocs/preloads.html#how-to-preload-an-object
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Cliffs_01","Cornifer Card"),
                ("Room_mapper","TileMap"),
                ("Room_mapper","TileMap Render Data"),
                ("Room_mapper","_SceneManager"),
            };
        }


        private void OnHeroControllerAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig.Invoke(self);
            // Attach the casino manager to the GameManager
            var shop = GameManager.instance.gameObject.GetAddComponent<CasinoShopHandler>();
        }

        public void OnHeroUpdate()
        {
            // casino.OnHeroUpdate();
            // Here we use the Player Action to detect the input
            // This WasPressed is defined in the subclass `OneAxisInputControl`
            if (Input.GetKeyDown(KeyCode.O))
            {
                // Quick jump to casino for testing, remove
                GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
                {
                    SceneName = "CasinoScene",
                    EntryGateName = "left_01",
                    HeroLeaveDirection = GatePosition.right,
                    EntryDelay = 0.2f,
                    WaitForSceneTransitionCameraFade = true,
                    PreventCameraFadeOut = false,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = false,
                    forceWaitFetch = false
                });
            }
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