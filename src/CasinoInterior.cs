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

            // Use Satchel to create a custome scene
            // https://github.com/PrashantMohta/Satchel/blob/master/Core.cs#L177
            casinoInterior = satchelCore.GetCustomScene("CasinoScene", refTileMap, refSceneManager);
            casinoInterior.OnLoaded += SceneOnload;

            // Can edit properties in the settings object, will by default use the value in the reference scene manager
            // which is the mappers store room in this case
            // https://github.com/PrashantMohta/Satchel/blob/master/Utils/SceneUtils.cs#L36
            CustomSceneManagerSettings settings = new SceneUtils.CustomSceneManagerSettings(refSceneManager.GetComponent<SceneManager>());
            settings.saturation = 1.2f;
            settings.heroLightColor = new Color(1.0f, 0.28f, 0.3f, 0.05f);
            casinoInterior.Config(40, 25, settings);
            // Satchel will handle set up of the scene manager, but we need our own call back for modifying objects
            // Add call back to load the scene on change
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

            // Store preloaded fabs
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
                var tp = gate.AddComponent<TransitionPoint>();

                // See Casino Exterior for linked Gate
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

        private void SceneOnload(object sender, SceneLoadedEventArgs e)
        {
            Log.Info("Casino Scene loading complete");
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
