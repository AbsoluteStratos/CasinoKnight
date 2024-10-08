﻿using GlobalEnums;
using Modding;
using Satchel.BetterMenus;
using System.Collections.Generic;
using UnityEngine;

namespace CasinoKnight
{
    public class CasinoKnight : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod, ITogglableMod
    {

        new public string GetName() => "Casino Knight";
        public override string GetVersion() => "0.1.1";

        internal static GlobalSettings GS = new GlobalSettings();
        internal static CasinoKnight Instance;

        public Dictionary<string, Dictionary<string, GameObject>> preloads;

        private CasinoInterior casinoInterior;
        private CasinoExterior casinoExterior;
        private Menu MenuRef;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            StratosLogging.Log.Info("Casino Knight initializing");

            Instance = this;

            ModHooks.HeroUpdateHook += OnHeroUpdate;

            // Prepare classes from preloaded objects
            // https://github.com/PrashantMohta/Smolknight/blob/6a6253ca3ea6549cc17bff47c33ade2ac28054e7/Smolknight.cs#L134
            // Arrow prompt
            Satchel.CustomArrowPrompt.Prepare(preloadedObjects["Cliffs_01"]["Cornifer Card"]);
            // Slot machine lever
            SlotLever.Prepare(preloadedObjects["Ruins1_23"]["Lift Call Lever"]);
            // Create casino interior scene object
            casinoInterior = new CasinoInterior(
                preloadedObjects["Room_mapper"]["TileMap"],
                preloadedObjects["Room_mapper"]["_SceneManager"],
                preloadedObjects["Town"]["_Scenery/point_light/HeroLight 3"],
                preloadedObjects["Town"]["_Scenery/lamp_flys/flys"]
           );
            // Create casino interior scene object
            casinoExterior = new CasinoExterior();

            StratosLogging.Log.Info("Casino Knight loaded!");
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
                ("Town","_Scenery/lamp_flys/flys"),
                ("Town", "_Scenery/point_light/HeroLight 3"),
                ("Ruins1_23", "Lift Call Lever")
            };
        }

        public void OnHeroUpdate()
        {
            /*if (Input.GetKeyDown(KeyCode.J))
            {
                HeroController.instance.AddGeo(100);
            }*/
            // Debugging / Dev util for jumping to new scene
            /*if (Input.GetKeyDown(KeyCode.O))
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
            }*/
        }

        void IGlobalSettings<GlobalSettings>.OnLoadGlobal(GlobalSettings s)
        {
            GS = s;
        }

        GlobalSettings IGlobalSettings<GlobalSettings>.OnSaveGlobal()
        {
            return GS;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates)
        {
            if (MenuRef == null)
            {
                MenuRef = ModMenu.PrepareMenu((ModToggleDelegates)modtoggledelegates);
            }
            return MenuRef.GetMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu => true;

        public void Unload()
        {
            Log("Unloaded");
            casinoInterior = null;
            casinoExterior = null;
            
        }
    }
}