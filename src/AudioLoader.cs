using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CasinoKnight
{
    public static class AudioLoader
    {
        // Obtiene la ruta de la carpeta donde está instalada la DLL del mod
        public static string ModFolder => Path.GetDirectoryName(typeof(CasinoKnight).Assembly.Location);

        public static IEnumerator LoadOgg(string fileName, Action<AudioClip> callback)
        {
            string filePath = Path.Combine(ModFolder, fileName);

            // Si el archivo .ogg no existe en la carpeta, devuelve null
            if (!File.Exists(filePath))
            {
                StratosLogging.Log.Info($"Audio personalizado no encontrado ({fileName}), se omitirá.");
                callback?.Invoke(null);
                yield break;
            }

            string fileUri = "file://" + filePath;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    StratosLogging.Log.Error($"Error al cargar el audio {fileName}: {www.error}");
                    callback?.Invoke(null);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    callback?.Invoke(clip);
                }
            }
        }
    }
}