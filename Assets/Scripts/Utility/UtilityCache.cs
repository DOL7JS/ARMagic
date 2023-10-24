using Siccity.GLTFUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Utility
{
    public class UtilityCache : MonoBehaviour
    {

        public void CacheFile(string folder, string name, UnityWebRequest webRequest)
        {
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, folder, Path.GetFileName(name)), webRequest.downloadHandler.data);
        }
        public void GetModelFromCache(string folder, string name, Action<GameObject> callback)
        {
            GameObject model = Importer.LoadFromFile(Path.Combine(Application.persistentDataPath, folder, name));
            callback(model);
        }
        public List<string> GetItemsInFolderFromCache(string folder)
        {
            SetCache(folder);
            List<string> names = new List<string>();
            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Application.persistentDataPath, folder));
            FileSystemInfo[] info = dirInfo.GetFiles();

            foreach (var file in info)
            {
                names.Add(file.Name);
            }
            names.Remove("__info");
            return names;
        }

        public void SetCache(string name)
        {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, name)))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, name));
        }

        public bool IsFileCached(string folder, string path)
        {
            return File.Exists(Path.Combine(Application.persistentDataPath, folder, Path.GetFileName(path)));
        }


        public IEnumerator LoadImage(string path, Action<Texture> callback)
        {
            if (path == "")
            {
                yield break;
            }
            Debug.Log("LoadImage: " + Path.GetFileName(path));
            var url = "file:///" + path;
            var unityWebRequestTexture = UnityWebRequestTexture.GetTexture(url);
            yield return unityWebRequestTexture.SendWebRequest();
            while (!unityWebRequestTexture.isDone)
            {
                yield return null;
            }
            var texture = ((DownloadHandlerTexture)unityWebRequestTexture.downloadHandler).texture;
            if (texture == null)
            {
                Debug.LogError("Failed to load texture url:" + url);
                callback(null);
            }
            else
            {
                texture.name = Path.GetFileName(path);
                callback(texture);
                Debug.Log("callback(texture)");

            }
        }
    }
}

