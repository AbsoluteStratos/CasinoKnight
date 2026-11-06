using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HutongGames.PlayMaker;
using Satchel;
using StratosLogging;
using UnityEngine;

namespace CasinoKnight
{
    public class SlotLever
    {
        public static GameObject LeverPrefab;

        public static void Prepare(GameObject lever)
        {
            LeverPrefab = lever;
        }

        public static GameObject GetNewLever(string name, Action<FsmState> OnLeverHit)
        {
            if (LeverPrefab == null)
            {
                Log.Error("LeverPrefab not set.");
            }

            Log.Info("Creating lever!");
            var go = GameObject.Instantiate(LeverPrefab);
            go.name = name;

            PlayMakerFSM fsm = go.LocateMyFSM("Call Lever");

            Satchel.FsmUtil.ChangeTransition(fsm, "Left", "FINISHED", "Send Msg");
            Satchel.FsmUtil.ChangeTransition(fsm, "Right", "FINISHED", "Send Msg");
			
			foreach (var s in fsm.FsmStates)
			{
				foreach (var t in s.Transitions)
				{
					if (t.ToState == "Check Already Called")
					{
						t.ToState = "Send Msg";
					}
				}
			}

            //var call_state = fsm.GetState("Send Msg");
			var call_state = Satchel.FsmUtil.GetValidState(fsm, "Send Msg");
			
            foreach (var state in fsm.FsmStates)
            {
                var cleanActions = new List<HutongGames.PlayMaker.FsmStateAction>();
                foreach (var action in state.Actions)
                {
                    if (!(action is HutongGames.PlayMaker.Actions.SetFsmInt))
                    {
                        cleanActions.Add(action);
                    }
                }
                state.Actions = cleanActions.ToArray();
            }
   
            if (call_state.Actions.Length > 2)
			{
				Satchel.FsmUtil.RemoveAction(call_state, 2);
			}

            Satchel.FsmUtil.AddCustomAction(call_state, () =>
            {
                Log.Debug(go.name.ToString() + " hit!");
                fsm.FsmVariables.BoolVariables[0] = true;
                OnLeverHit(call_state);
                fsm.FsmVariables.BoolVariables[0] = false;
            });

            return go;
        }
    }

    public class SlotHandler : MonoBehaviour
    {
        internal GameObject[] ReelObjs = new GameObject[3];
        internal Dictionary<int, int[]> ReelMaps = new Dictionary<int, int[]>()
        {
            {0, new int[] {0,0,0,0,1,1,1,2,2,3,3,4}},
            {1, new int[] {0,0,0,1,1,2,2,2,2,3,3,3,4}},
            {2, new int[] {0,0,0,1,1,2,2,3,3,3,4}},
        };
        internal List<(Regex, int)> ReelPayout = new List<(Regex, int)>()
        {
            (new Regex(@"4.."), 1),
            (new Regex(@".4."), 1),
            (new Regex(@"..4"), 1),
            (new Regex(@"44."), 2),
            (new Regex(@"4.4"), 2),
            (new Regex(@".44"), 2),
            (new Regex(@"000"), 2),
            (new Regex(@"111"), 4),
            (new Regex(@"222"), 8),
            (new Regex(@"333"), 16),
            (new Regex(@"444"), 100),
        };
        internal System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());

        internal GameObject arrowPrompt;

        private AudioSource win_sfx, lose_sfx, spin_sfx;
        private int bet = 10;
        private bool slotRunning = false;

		private void Awake()
		{
			ReelObjs[0] = gameObject.transform.Find("machine/reel0").gameObject;
			ReelObjs[1] = gameObject.transform.Find("machine/reel1").gameObject;
			ReelObjs[2] = gameObject.transform.Find("machine/reel2").gameObject;

			win_sfx = gameObject.transform.Find("machine/win_sfx").gameObject.GetComponent<AudioSource>();
			lose_sfx = gameObject.transform.Find("machine/lose_sfx").gameObject.GetComponent<AudioSource>();
            
            // AudioSource para el sonido del giro de la máquina
            spin_sfx = gameObject.AddComponent<AudioSource>();

			// Cargar audios .ogg externos
			StartCoroutine(AudioLoader.LoadOgg("spin.ogg", clip => { if (spin_sfx != null) spin_sfx.clip = clip; }));
			StartCoroutine(AudioLoader.LoadOgg("win.ogg", clip => { if (win_sfx != null) win_sfx.clip = clip; }));
			StartCoroutine(AudioLoader.LoadOgg("lose.ogg", clip => { if (lose_sfx != null) lose_sfx.clip = clip; }));

			var lever = SlotLever.GetNewLever("slot_lever", Play);
			lever.SetActive(true);
			lever.SetScale(0.7f, 0.7f);
			lever.transform.parent = gameObject.transform;
			lever.transform.localPosition = new Vector3(0f, -1.25f, 0f);

			arrowPrompt = gameObject.transform.Find("prompt_marker").gameObject;
			CustomArrowPrompt.GetAddCustomArrowPrompt(arrowPrompt, String.Format("Bet {0}", bet), null);
			arrowPrompt.SetActive(true);
		}

        public void Play(FsmState state)
        {
            int numGeo = PlayerData.instance.GetInt("geo");
            if (slotRunning)
            {
                return;
            } 
            else if (numGeo < bet)
            {
                Log.Error("Not enough money!");
                return;
            }

            slotRunning = true;

            // Reproduce el sonido de la animación de girar
            if (CasinoKnight.GS.EnableSFX && spin_sfx != null && spin_sfx.clip != null)
            {
                spin_sfx.Play();
            }

            int[] values = new int[3];
            for (int i = 0; i < 3; i++)
            {
                int reel_index = rand.Next(0, ReelMaps[i].Length);
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
            
            StartCoroutine(SlotPayoutCoroutine(payout));
        }

		IEnumerator SlotPayoutCoroutine(int payout)
		{
			yield return new WaitForSeconds(1.25f);

			slotRunning = false;
			if (payout > 0)
			{
				Log.Info("Win Payout: " + payout.ToString());
				HeroController.instance.AddGeo(payout);
				
				if (CasinoKnight.GS.EnableSFX && win_sfx != null && win_sfx.clip != null)
				{
					win_sfx.Play();
					yield return new WaitForSeconds(win_sfx.clip.length);
				}
			}
			else
			{
				Log.Info("Loss Payout: " + payout.ToString());
				HeroController.instance.TakeGeo(-payout);
				
				if (CasinoKnight.GS.EnableSFX && lose_sfx != null && lose_sfx.clip != null)
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
            get => this.bet;
            set
            {
                this.bet = value;
                CustomArrowPromptBehaviour prompt = this.arrowPrompt.GetComponent<CustomArrowPromptBehaviour>();
                prompt.PromptText = String.Format("Bet {0}", value);
            }
        }
    }
}