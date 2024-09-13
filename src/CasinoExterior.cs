using GlobalEnums;
using StratosLogging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;
using Satchel;
using Modding;

namespace CasinoKnight
{
    public class CasinoExterior
    {
        private AssetBundle cassinoBundle;
        public CasinoExterior()
        {
            // Hook onto unity scene change
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void LogComponents(GameObject gameObject, string pname = "")
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                StratosLogging.Log.Info(pname+"/"+gameObject.name +" Child: " + gameObject.transform.GetChild(i).name);
                Component[] components = gameObject.GetComponents(typeof(Component));
                foreach (Component component in components)
                {
                    StratosLogging.Log.Info(pname + "/" + gameObject.name + ">" + component.ToString());
                }

                // Go deeper
                LogComponents(gameObject.transform.GetChild(i).gameObject, pname + "/" + gameObject.name);

            }
        }

        public void OnSceneChange(Scene from, Scene to)
        {

            Log.Debug("Scene changed!! " + from.name + " to " + to.name);
            // Dirtmouth scene name
            if (to.name == "Town")
            {
                string bundle_name = "CasinoKnight.Resources.casinoknighttown";

                if (cassinoBundle == null)
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    using (Stream s = asm.GetManifestResourceStream(bundle_name))
                    {
                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();
                        Log.Info("Loading bundle: " + bundle_name);
                        cassinoBundle = AssetBundle.LoadFromMemory(buffer);
                    }
                    // Debugging
                    /*foreach (string name in asm.GetManifestResourceNames())
                    {
                        StratosLogging.Log.Info("Embedded resource: " + name);
                    }

                    foreach (string name in cassinoBundle.GetAllAssetNames())
                    {   
                        StratosLogging.Log.Info("Asset: " + name);
                    }*/
                }

                // Note the path is based on where you saved the prefab
                GameObject objab = GameObject.Instantiate(cassinoBundle.LoadAsset<GameObject>("Assets/Mod/casinoknighttown.prefab"));
                objab.transform.position = UnityEngine.Vector3.zero;
                objab.SetActive(true);
                ResetPrefabMaterials(objab);

                // Add a transition hook door collider
                // Note that doors are a little more tricky than other gates, other gates will run the animation for you but not doors
                // So instead what we will do is listen for the up / enter key press then trigger the transition and animation ourselves
                // See CasinoTownDoorHandler
                GameObject gate = objab.transform.Find("casino/door_casino").gameObject;
                gate.AddComponent<CasinoTownDoorHandler>();
                var tp = gate.AddComponent<TransitionPoint>();
                tp.isADoor = true;
                tp.alwaysEnterLeft = false;
                tp.alwaysEnterRight = false;

                GameObject rm = objab.transform.Find("casino/door_casino/Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.AddComponent<HazardRespawnMarker>();
                tp.respawnMarker.respawnFacingRight = true;
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;


                // Add in lighting objects
                GameObject flysPrefab = GameObject.Find("_Scenery/lamp_flys/flys").gameObject;
                GameObject lightPrefab = GameObject.Find("_Scenery/point_light/HeroLight 3").gameObject;

                var go = GameObject.Instantiate(flysPrefab);
                go.transform.parent = GameObject.Find("casino/lamp_post/light_marker_1").gameObject.transform;
                go.SetActive(true);
                go.transform.localPosition = new UnityEngine.Vector3(0f, 0f, -0.1f);

                go = GameObject.Instantiate(flysPrefab);
                go.transform.parent = GameObject.Find("casino/lamp_post/light_marker_2").gameObject.transform;
                go.SetActive(true);
                go.transform.localPosition = new UnityEngine.Vector3(0f, 0f, -0.1f);

                go = GameObject.Instantiate(lightPrefab);
                go.transform.parent = GameObject.Find("casino/lamp_post/light_marker_1").gameObject.transform;
                go.SetActive(true);
                go.transform.localPosition = new UnityEngine.Vector3(0f, 0f, -1f);

                go = GameObject.Instantiate(lightPrefab);
                go.transform.parent = GameObject.Find("casino/lamp_post/light_marker_2").gameObject.transform;
                go.SetActive(true);
                go.transform.localPosition = new UnityEngine.Vector3(0f, 0f, -1f);

                // Move gate infront of building
                GameObject.Find("_Scenery/town_layered/town_layered_0006_31").gameObject.transform.localPosition = new UnityEngine.Vector3(-16f, 2.5f, -1.25f);

                Log.Warning("Casino town asset set up");
            }
            else if (cassinoBundle != null)
            {
                Log.Info("Unloading asset");
                cassinoBundle.Unload(true);
            }
        }

        // https://radiance.synthagen.net/apidocs/_images/Assets.html?highlight=getexecutingassembly#using-our-loaded-stuff
        private void ResetPrefabMaterials(GameObject obj)
        {
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

        private void PrintAllSceneTransitionPoints()
        {
            TransitionPoint[] transions = UnityEngine.Object.FindObjectsOfType<TransitionPoint>();
            foreach (TransitionPoint tobj in transions)
            {
                Log.Info(tobj.name + " to: " + tobj.targetScene + " _ " + tobj.entryPoint);
            }
        }

    }

    public class CasinoTownDoorHandler : MonoBehaviour
    {
        private GameManager gm;
        private tk2dSpriteAnimator heroAnim;
        private GameObject doorArrowPrompt;

        private bool door_active = false;

        private void Awake()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            gm = GameManager.instance;
            heroAnim = HeroController.instance.GetComponent<tk2dSpriteAnimator>();
            doorArrowPrompt = CreatePromptPrehab();
            doorArrowPrompt.SetActive(true);

        }

        private GameObject CreatePromptPrehab(string text = "SLOTS")
        {
            // Needs preloads["Cliffs_01"]["Cornifer Card"]; on mod init
            // Assumes CustomArrowPrompt.Prepare(CardPrefab); has been called
            // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Custom/CustomArrowPrompt.cs
            GameObject prefab = new GameObject("Door Arrow");
            prefab.transform.position = transform.Find("Prompt Marker").position;
            CustomArrowPrompt.GetAddCustomArrowPrompt(prefab, text, null);
            prefab.SetActive(false);
            DontDestroyOnLoad(prefab);

            return prefab;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.name == "Knight")
            {
                door_active = true;
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.name == "Knight" && Mathf.Abs(other.attachedRigidbody.velocity.x) < 0.1)
            {
                doorArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Show();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.name == "Knight")
            {
                door_active = false;
                doorArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Hide();
            }
        }

        public void OnHeroUpdate()
        {
            // In the modding tutorial you will see the use of Input.GetKeyDown
            // We dont use that here because we want to be agnostic to the key bind for up and also support controllers
            // User the inputHanlder instead
            if (GameManager.instance.inputHandler.inputActions.up.IsPressed && door_active)
            {
                // From moding API
                // Can discover entry gates with
                /*var foundGates = FindObjectsOfType<TransitionPoint>();
                foreach (var obj in foundGates)
                {
                    StratosLogging.Log.Info(obj.transform.parent.name);
                }*/
                door_active = false;
                // Trigger a scene transition
                gm.BeginSceneTransition(new GameManager.SceneLoadInfo
                {
                    SceneName = "CasinoScene",
                    EntryGateName = "left_01",
                    HeroLeaveDirection = GatePosition.bottom,
                    EntryDelay = 0.2f,
                    WaitForSceneTransitionCameraFade = true,
                    PreventCameraFadeOut = false,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = false,
                    forceWaitFetch = false
                });
                // Must be after, the Begin transition will cancel animation if before
                HeroController.instance.StartCoroutine(PlayExitAnimation());
            }
        }

        private IEnumerator PlayExitAnimation()
        {
            HeroController.instance.StopAnimationControl();
            // Used door_bretta FSM in Town scene to figure out the name
            yield return heroAnim.PlayAnimWait("Enter");

            HeroController.instance.StartAnimationControl();

            yield break;
        }
    }

}
