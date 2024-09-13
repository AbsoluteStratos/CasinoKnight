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
using System.Collections;
using Satchel;
using static Satchel.SceneUtils;
using HutongGames.PlayMaker.Actions;

namespace CasinoKnight
{
    public class CasinoInterior
    {
        public static string sceneName = "CasinoScene";
        public static string sceneNamePath;
        public static AssetBundle casinoBundle;
        private CustomScene casinoInterior;
        private GameObject heroLightPrefab, flysPrefab;

        private static Satchel.Core satchelCore = new Satchel.Core();
        private static string sceneResourceName = "CasinoKnight.Resources.casinoscene";


        public CasinoInterior(GameObject refTileMap, GameObject refSceneManager, GameObject heroLight, GameObject flys)
        {
            // Load scene bundle
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream(sceneResourceName))
            {
                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                s.Dispose();
                Log.Debug("Loading bundle: " + sceneResourceName);
                casinoBundle = AssetBundle.LoadFromMemory(buffer);
            }
            sceneNamePath = casinoBundle.GetAllScenePaths()[0];

            casinoInterior = satchelCore.GetCustomScene("CasinoScene", refTileMap, refSceneManager);
            casinoInterior.OnLoaded += TestOnload;
            
            CustomSceneManagerSettings settings = new SceneUtils.CustomSceneManagerSettings(refSceneManager.GetComponent<SceneManager>());
            casinoInterior.Config(40, 25, settings);
            // Add call back to load the scene on change
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

            heroLightPrefab = heroLight;
            flysPrefab = flys;
        }

        private void OnSceneChange(Scene from, Scene to)
        {
            var currentScene = to.name;
            if (currentScene == sceneName)
            {
                ResetPrefabMaterials(GameObject.Find("root"));
                Log.Info("Casino interior scene change");
                // Manually replicating because I already have game objects in the scene
                // https://github.com/PrashantMohta/Satchel/blob/master/Utils/SceneUtils.cs#L144
                GameObject gate = GameObject.Find("left_01").gameObject;
                Log.Info(gate.transform.position.ToString());
                var tp = gate.AddComponent<TransitionPoint>();

                tp.isADoor = false;
                tp.SetTargetScene("Town");
                tp.entryPoint = "door_casino";
                tp.alwaysEnterLeft = true;
                tp.alwaysEnterRight = false;

                GameObject rm = gate.transform.Find("Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.GetAddComponent<HazardRespawnMarker>();
                tp.respawnMarker.respawnFacingRight = true;
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;

                Log.Warning("Gate Set up");

                GameObject speaker = GameObject.Find("EnterSpeaker").gameObject;
                speaker.AddComponent<AudioBehavior>();
                Log.Info("Speaker Set Up");

                // Initialize the slot machines
                GameObject slotMachine = GameObject.Find("root/slot_10").gameObject;
                slotMachine.AddComponent<SlotHandler>();
                slotMachine.GetComponent<SlotHandler>().BetAmount = 10;

                slotMachine = GameObject.Find("root/slot_100").gameObject;
                slotMachine.AddComponent<SlotHandler>();
                slotMachine.GetComponent<SlotHandler>().BetAmount = 100;

                // Set Up light
                var go = GameObject.Instantiate(flysPrefab);
                go.transform.parent = GameObject.Find("root/lights/flys_marker").gameObject.transform;
                go.SetActive(true);
                go.transform.localPosition = new Vector3(0f, 0f, 0f);

                go = GameObject.Instantiate(heroLightPrefab);
                go.transform.parent = GameObject.Find("root/lights/herolight_marker").gameObject.transform;
                go.SetActive(true);
                go.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        }


        private void TestOnload(object sender, SceneLoadedEventArgs e)
        {
            StratosLogging.Log.Warning("Loading complete");
        }

        private AssetBundle LoadAssetBundle(string name = "CasinoKnight.Resources.casinoscene")
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

        // https://radiance.synthagen.net/apidocs/_images/Assets.html?highlight=getexecutingassembly#using-our-loaded-stuff
        private void ResetPrefabMaterials(GameObject obj)
        {
            //StratosLogging.Log.Info("Setting material of " + obj.name);
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if (child.GetComponent<SpriteRenderer>() != null)
                {
                    child.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }
                ResetPrefabMaterials(child);
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

    public class AudioBehavior : MonoBehaviour
    {
        private void Start()
        {
            if (CasinoKnight.GS.EnableSFX)
            {
                StartCoroutine(PlayIntroAudio(GameObject.Find("EnterSpeaker").GetComponent<AudioSource>()));
            }
        }
        IEnumerator PlayIntroAudio(AudioSource sfx)
        {
            sfx.Play();
            yield return new WaitForSeconds(sfx.clip.length);
            yield break;
        }
    }
}
