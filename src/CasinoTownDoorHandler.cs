using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;
using StratosLogging;
using GlobalEnums;
using Satchel;
using System.Collections;
using System.Runtime.CompilerServices;

namespace HollowKnightTreasureHunt
{
    public class CasinoTownDoorHanlder : MonoBehaviour
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

            if (Input.GetKeyDown(KeyCode.UpArrow) && door_active)
            {
                // From moding API
                // Can discover entry gates with
                /*var foundGates = FindObjectsOfType<TransitionPoint>();
                foreach (var obj in foundGates)
                {
                    StratosLogging.Log.Info(obj.transform.parent.name);
                }*/
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
                door_active = false;
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