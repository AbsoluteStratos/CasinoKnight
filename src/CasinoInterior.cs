using StratosLogging;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Satchel;
using static Satchel.SceneUtils;

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
            casinoInterior.OnLoaded += SceneOnload;

            CustomSceneManagerSettings settings = new SceneUtils.CustomSceneManagerSettings(refSceneManager.GetComponent<SceneManager>());
            settings.saturation = 1.2f;
            settings.heroLightColor = new Color(1.0f, 0.28f, 0.3f, 0.05f);
            casinoInterior.Config(40, 25, settings);
            
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

                GameObject gate = GameObject.Find("left_01").gameObject;
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

                // Instanciar controlador de música en bucle para el casino
                GameObject musicObj = new GameObject("CasinoMusicManager");
                musicObj.AddComponent<CasinoMusic>();

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
    }

	public class AudioBehavior : MonoBehaviour
	{
		private void Start()
		{
			if (CasinoKnight.GS.EnableSFX)
			{
				AudioSource sfx = GetComponent<AudioSource>();
				
				StartCoroutine(AudioLoader.LoadOgg("intro.ogg", clip => 
				{
					if (clip != null && sfx != null)
					{
						sfx.clip = clip;
						StartCoroutine(PlayIntroAudio(sfx));
					}
				}));
			}
		}

		IEnumerator PlayIntroAudio(AudioSource sfx)
		{
			if (sfx != null && sfx.clip != null)
			{
				sfx.Play();
				yield return new WaitForSeconds(sfx.clip.length);
			}
			yield break;
		}
	}

	public class CasinoMusic : MonoBehaviour
    {
        private AudioSource bgmSource;
        private AudioSource[] originalMusicSources;
        private float[] originalVolumes;

        private void Start()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;

            // Encontrar y silenciar la música original del juego
            if (GameManager.instance != null && GameManager.instance.AudioManager != null)
            {
                // Buscamos los AudioSources que dependen del AudioManager
                originalMusicSources = GameManager.instance.AudioManager.GetComponentsInChildren<AudioSource>();
                originalVolumes = new float[originalMusicSources.Length];

                for (int i = 0; i < originalMusicSources.Length; i++)
                {
                    originalVolumes[i] = originalMusicSources[i].volume;
                    originalMusicSources[i].volume = 0f; // Silenciamos
                }
            }

            // Cargar y reproducir music.ogg
            StartCoroutine(AudioLoader.LoadOgg("music.ogg", clip =>
            {
                if (clip != null && bgmSource != null)
                {
                    bgmSource.clip = clip;
                    bgmSource.volume = 1f;
                    bgmSource.Play();
                }
            }));
        }

        private void OnDestroy()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }

            // Restaurar la música original al salir
            if (originalMusicSources != null && originalVolumes != null)
            {
                for (int i = 0; i < originalMusicSources.Length; i++)
                {
                    if (originalMusicSources[i] != null)
                    {
                        originalMusicSources[i].volume = originalVolumes[i];
                    }
                }
            }
        }
    }
}