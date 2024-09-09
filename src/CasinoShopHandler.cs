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

namespace CasinoKnight
{
    public class CasinoShopHandler : MonoBehaviour
    {

        private GameManager gm;
        private AssetBundle cassinoBundle;
        private GameObject testprompt;
        private bool casino_enter_gate = false;

        public static string previousScene = "null";

        private void Awake()
        {
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

        public void OnSceneChange(Scene from, Scene to)
        {

            Log.Debug("Scene changed!! " + from.name + " to " + to.name);
            previousScene = from.name;
            // Dirtmouth scene name
            if (to.name == "Town")
            {
                string bundle_name = "CasinoKnight.Resources.stratos";

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
                GameObject objab = Instantiate(cassinoBundle.LoadAsset<GameObject>("Assets/Mod/stratos.prefab"));
                objab.transform.position = UnityEngine.Vector3.zero;
                objab.SetActive(true);
                ResetPrefabMaterials(objab);

                // Add a transition hook door collider
                GameObject gate = objab.transform.Find("Casino/door_casino").gameObject;
                gate.AddComponent<CasinoTownDoorHandler>();
                var tp = gate.AddComponent<TransitionPoint>();
                tp.isADoor = true;
                tp.alwaysEnterLeft = false;
                tp.alwaysEnterRight = false;

                GameObject rm = objab.transform.Find("Casino/door_casino/Hazard Respawn Marker").gameObject;
                tp.respawnMarker = rm.AddComponent<HazardRespawnMarker>();
                tp.respawnMarker.respawnFacingRight = true;
                tp.sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;


                // TODO Delete after debugging
                GameObject slotMachine = objab.transform.Find("slot").gameObject;
                slotMachine.AddComponent<SlotHandler>();

                Log.Warning("Gate Set up");
            }
            else
            {
                Log.Info("Unloading asset");
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
                Log.Info(tobj.name + " to: " + tobj.targetScene + " _ " + tobj.entryPoint);
            }
        }

    }
}
