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

namespace HollowKnightTreasureHunt
{
    public class CasinoShopHandler : MonoBehaviour
    {
        private GameManager gm;
        private AssetBundle cassinoBundle;
        private tk2dSpriteAnimator _anim;
        private GameObject testprompt;
        private bool casino_enter_gate = false;

        private void Awake()
        {

            gm = GameManager.instance;
            // Adopting from press G to Dab
            // https://github.com/Link459/PressGToDab/blob/master/PressGToDab/Emoter.cs#L13
            // Initialize variables or states before the application starts
            this._anim = HeroController.instance.GetComponent<tk2dSpriteAnimator>();
            StratosLogging.Log.Info("Hello world");

            // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Custom/CustomArrowPrompt.cs
            testprompt = new GameObject("Cool GameObject");
            testprompt.transform.position = new UnityEngine.Vector3(140.029999f, 11.6899996f, 0.01f);
            CustomArrowPrompt.GetAddCustomArrowPrompt(testprompt, "SLOTS", null);
            testprompt.SetActive(true);
            DontDestroyOnLoad(testprompt);


            //GameObject test = GameObject.Find("Arrow Prompt New");
            /*var foundCanvasObjects = FindObjectsOfType<tk2dSpriteAnimator>();
            foreach (var obj in foundCanvasObjects)
            {
                StratosLogging.Log.Info(obj.name);
            }*/

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

        private IEnumerator CycleObjects()
        {

            // Find all assets labelled with 'architecture' :

            
            
            
            



            StratosLogging.Log.Info("=====" + testprompt.ToString());

            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            Log.Warning("" + allObjects.Length);
            foreach (GameObject go in allObjects)
            {
                if (go.name.Contains("Prompt")){
                    StratosLogging.Log.Info(go.name + " :: " + go.layer.ToString());
                    // StratosLogging.Log.Info(go.transform.parent.name);

                    /*GameObject[] test = go.GetComponentsInParent<GameObject>();
                    StratosLogging.Log.Warning(test.Length.ToString());
                    foreach (GameObject goc in test)
                    {
                        StratosLogging.Log.Info(goc.name);
                    }

                    StratosLogging.Log.Warning(go.transform.Find("Labels/Sit").ToString());

                    // GameObject mimic = GameObject.Instantiate();
                    go.transform.Find("Labels/Sit").gameObject.GetComponent<TMPro.TextMeshPro>().SetText("Gamble");
                    go.transform.Find("Labels/Sit").gameObject.GetComponent<TMPro.TextMeshPro>().ForceMeshUpdate();

                    Component[] components = go.transform.Find("Labels/Sit").gameObject.GetComponents(typeof(Component));
                    foreach (Component component in components)
                    {
                        StratosLogging.Log.Info(">" + component.ToString());
                    }

                    new WaitForSeconds(0.1f);
                    go.transform.Find("Labels/Sit").gameObject.SetActive(true);*/


                    // LogComponents(go.transform.gameObject);

                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield break;
        }


         void OnTriggerEnter2D(Collider2D other)
        {
            if (other.name == "door_casino")
            {
                casino_enter_gate = true;
                
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.name == "door_casino")
            {
                casino_enter_gate = false;
            }
        }

        public void OnHeroUpdate()
        {

            if (Input.GetKeyDown(KeyCode.G))
            {
                

                StratosLogging.Log.Info(testprompt.ToString());
                // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Monobehaviour/CustomArrowPromptBehaviour.cs
                testprompt.GetComponent<CustomArrowPromptBehaviour>().Show();
                base.StartCoroutine(CycleObjects());
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) && casino_enter_gate)
            {
                // From moding API
                gm.BeginSceneTransition(new GameManager.SceneLoadInfo
                {
                    SceneName = "Crossroads_01",
                    EntryGateName = "top1",
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
                casino_enter_gate = false;
            }
        }


        public void OnSceneChange(Scene scene_1, Scene scene_2)
        {

            StratosLogging.Log.Debug("Scene changed!! " + scene_1.name + " to " + scene_2.name);
            // Dirtmouth scene name
            if (scene_2.name == "Town")
            {
                string bundle_name = "HollowKnightTreasureHunt.Resources.stratos";

                if (cassinoBundle == null)
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    using (Stream s = asm.GetManifestResourceStream(bundle_name))
                    {
                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();
                        StratosLogging.Log.Info("Loading bundle: " + bundle_name);
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
                GameObject objab = cassinoBundle.LoadAsset<GameObject>("Assets/Images/stratos.prefab");
                objab = GameObject.Instantiate(objab);
                objab.transform.position = UnityEngine.Vector3.zero;
                objab.SetActive(true);
                ResetPrefabMaterials(objab);

                // Add a transition hook door collider
                GameObject gate = objab.transform.Find("Casino").gameObject.transform.Find("door_casino").gameObject;
                var tp = gate.AddComponent<TransitionPoint>();
                tp.isADoor = true;
                tp.targetScene = "Crossroads_01";
                tp.entryPoint = "top1";

                GameObject rm = objab.transform.Find("Casino").gameObject.transform.Find("Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.AddComponent<HazardRespawnMarker>();
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;

                StratosLogging.Log.Warning("Gate Set up");

            }
            else
            {
                StratosLogging.Log.Info("Unloading asset");
                cassinoBundle.Unload(true);
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

        private void PrintAllSceneTransitionPoints()
        {
            TransitionPoint[] transions = UnityEngine.Object.FindObjectsOfType<TransitionPoint>();
            foreach (TransitionPoint tobj in transions)
            {
                StratosLogging.Log.Info(tobj.name + " to: " + tobj.targetScene + " _ " + tobj.entryPoint);
            }
        }


        private IEnumerator PlayExitAnimation()
        {
            //foreach (tk2dSpriteAnimationClip clip in _anim.Library.clips)
            //{
            //    StratosLogging.Log.Warning(clip.name);
                // yield return new WaitForSeconds(0.2f);
            //}

            HeroController.instance.StopAnimationControl();
            // Used door_bretta FSM in Town scene to figure out the name
            yield return _anim.PlayAnimWait("Enter");

            HeroController.instance.StartAnimationControl();


            yield break;
        }
    }
}
