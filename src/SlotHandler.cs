using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using StratosLogging;
using Satchel;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using System.Collections;
using System.Text.RegularExpressions;

namespace CasinoKnight
{
    public class SlotLever
    {
        public static GameObject LeverPrefab;

        // Preloaded ["Ruins1_23"]["Lift Call Lever"]
        // https://github.com/PrashantMohta/Satchel/blob/master/Custom/CustomArrowPrompt.cs 
        public static void Prepare(GameObject lever)
        {
            LeverPrefab = lever;
        }

        public static GameObject GetNewLever(string name, Action<FsmState> OnLeverHit)
        {
            if (LeverPrefab == null)
            {
                Log.Error("LeverPrefab not set. Set to Town/Mines Lever");
            }

            Log.Info("Creating lever!");
            var go = GameObject.Instantiate(LeverPrefab);
            go.name = name;

            // Interesting FSM editting reference
            // https://github.com/SFGrenade/TestOfTeamwork/blob/f2fb8212fa4cc2a725c29e57ca5e2675383adc82/src/MonoBehaviours/WeaverPrincessBossIntro.cs#L45
            PlayMakerFSM fsm = go.LocateMyFSM("Call Lever");

            var call_state = fsm.GetState("Send Msg");
   
            // Remove elevator call action
            Satchel.FsmUtil.RemoveAction(call_state, 2);
            // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Futils/FsmUtils.cs#L340
            Satchel.FsmUtil.AddCustomAction(call_state, () =>
            {
                Log.Debug(go.name.ToString() + " hit!");
                fsm.FsmVariables.BoolVariables[0] = true;
                OnLeverHit(call_state);
                fsm.FsmVariables.BoolVariables[0] = false;

            }
            );

            return go;
        }
    }

    public class SlotHandler : MonoBehaviour
    {
        
        internal GameObject[] ReelObjs = new GameObject[3];
        internal Dictionary<int, int[]> ReelMaps = new Dictionary<int, int[]>()
        {
            {0, new int[] {0,0,0,1,1,1,2,2,2,3,4}},
            {1, new int[] {0,0,0,1,1,2,2,3,3,4}},
            {2, new int[] {0,0,0,1,1,2,2,3,4}},
        };
        internal List<(Regex, int)> ReelPayout = new List<(Regex, int)>()
        {
            (new Regex(@"4.."), 1),
            (new Regex(@".4."), 1),
            (new Regex(@"..4"), 1),
            (new Regex(@"44."), 2),
            (new Regex(@"4.4"), 2),
            (new Regex(@".44"), 2),
            (new Regex(@"000"), 4),
            (new Regex(@"111"), 8),
            (new Regex(@"222"), 16),
            (new Regex(@"333"), 32),
            (new Regex(@"444"), 100),
        };
        internal System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());

        internal GameObject arrowPrompt;

        private AudioSource win_sfx, lose_sfx;
        private int bet = 10;
        private bool slotRunning = false; // If slot is currently running
        private void Awake()
        {
            Log.Info("Test");
            // Initialize
            ReelObjs[0] = gameObject.transform.Find("machine/reel0").gameObject;
            ReelObjs[1] = gameObject.transform.Find("machine/reel1").gameObject;
            ReelObjs[2] = gameObject.transform.Find("machine/reel2").gameObject;

            Log.Info("Stuff: " + gameObject.transform.Find("machine/win_sfx").gameObject.ToString());

            win_sfx = gameObject.transform.Find("machine/win_sfx").gameObject.GetComponent<AudioSource>();
            lose_sfx = gameObject.transform.Find("machine/lose_sfx").gameObject.GetComponent<AudioSource>();

            // Needs preloads["Ruins1_23"]["Lift Call Lever"]; on mod init
            var lever = SlotLever.GetNewLever("slot_lever", Play);
            lever.SetActive(true);
            lever.SetScale(0.7f, 0.7f);
            lever.transform.parent = gameObject.transform;
            lever.transform.localPosition = new Vector3(0f, -1.25f, 0f);
            Log.Info("Lever pos" + lever.transform.position.ToString() + gameObject.transform.position.ToString());

            // Needs preloads["Cliffs_01"]["Cornifer Card"]; on mod init
            // Assumes CustomArrowPrompt.Prepare(CardPrefab); has been called
            // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Custom/CustomArrowPrompt.cs
            arrowPrompt = gameObject.transform.Find("prompt_marker").gameObject;
            CustomArrowPrompt.GetAddCustomArrowPrompt(arrowPrompt, String.Format("Bet {0}", bet), null);
            arrowPrompt.SetActive(true);
            Log.Info("arrow prompt updated");
        }

        public void Play(FsmState state)
        {
            int numGeo = PlayerData.instance.GetInt("geo");
            if (slotRunning)
            {
                return;
            } else if (numGeo < bet)
            {
                Log.Error("Not enough money!");
                return;
            }

            slotRunning = true;
            int[] values = new int[3];
            for (int i = 0; i <3; i++)
            {
                int reel_index = rand.Next(0, ReelMaps[i].Length); // Index slot on reel
                values[i] = ReelMaps[i][reel_index];

                var animator = ReelObjs[i].GetComponent<Animator>();
                animator.SetInteger("PREV", animator.GetInteger("VALUE"));
                animator.SetInteger("VALUE", values[i]);
                animator.Play("Start");
            }

            int payout = -bet;
            string value_pattern = String.Format("{0}{1}{2}", values[0], values[1], values[2]);
            Log.Info("Slot pattern: " + value_pattern);
            foreach ((Regex, int) item in ReelPayout)
            {
                if (item.Item1.IsMatch(value_pattern))
                {
                    Log.Info("Slot win match found: " + value_pattern + " -> " + item.Item2.ToString());
                    payout = item.Item2 * bet;
                }
            }
            
            // Start payout coroutine
            StartCoroutine(SlotPayoutCoroutine(payout));
        }

        IEnumerator SlotPayoutCoroutine(int payout)
        {
            //yield on a new YieldInstruction that waits for 0.5 for animation to finish.
            yield return new WaitForSeconds(1.25f);

            slotRunning = false;
            // For geo utils see the following reference:
            // https://github.com/dpinela/Transcendence/blob/aa075856eabeffc5ba0c4e3c09aab5309229e905/Transcendence/Charms/MillibellesBlessing.cs#L26
            if (payout > 0)
            {
                Log.Info("Win Payout: " + payout.ToString());
                HeroController.instance.AddGeo(payout);
                if (CasinoKnight.GS.EnableSFX)
                {
                    win_sfx.Play();
                    yield return new WaitForSeconds(win_sfx.clip.length);
                }
            }
            else
            {
                Log.Info("Loss Payout: " + payout.ToString());
                HeroController.instance.TakeGeo(-payout);
                if (CasinoKnight.GS.EnableSFX)
                {
                    lose_sfx.Play();
                    yield return new WaitForSeconds(lose_sfx.clip.length);
                }
            }
            
            yield break;
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.name == "Knight" && Mathf.Abs(other.attachedRigidbody.velocity.x) < 0.1)
            {
                arrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Show();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.name == "Knight")
            {
                arrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Hide();
            }
        }

        public int BetAmount
        {
            get
            {
                return this.bet;
            }
            set
            {
                this.bet = value;
                CustomArrowPromptBehaviour prompt = this.arrowPrompt.GetComponent<CustomArrowPromptBehaviour>();
                prompt.PromptText = String.Format("Bet {0}", value);
            }
        }
    }
}
