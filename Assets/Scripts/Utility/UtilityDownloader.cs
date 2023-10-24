using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Net;
using System.Text.RegularExpressions;

namespace Utility
{
    public class UtilityDownloader : MonoBehaviour
    {
        public void IsServerAvailable(Action<bool> callback)
        {
            if (UtilityAddress.GetIPAddressOfWebserver().Length == 0)
            {
                callback(false);
                return;
            }

            StartCoroutine(TestConnection(callback));
        }
        public bool IsFileOnServer(string folder, string file)
        {
            string fileURL = Path.Combine(UtilityAddress.GetURLToWebserver(), folder, Path.GetFileName(file));

            using (UnityWebRequest www = UnityWebRequest.Head(fileURL))
            {
                www.SendWebRequest();

                while (!www.isDone) { }

                return www.result == UnityWebRequest.Result.Success;
            }
        }

        IEnumerator TestConnection(Action<bool> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(UtilityAddress.GetURLToWebserver()))
            {
                webRequest.timeout = 3;
                yield return webRequest.SendWebRequest();
                callback(webRequest.result == UnityWebRequest.Result.Success);
            }
        }
        public IEnumerator DownloadConfiguration(string name, string folder, Action<UnityWebRequest> callback)
        {
            UnityWebRequest www = UnityWebRequest.Get(Path.Combine(UtilityAddress.GetURLToWebserver(), folder, name));
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(null);
            }
            else
            {
                callback(www);
            }
        }
        public IEnumerator DownloadFile(string folder, string name, Action<UnityWebRequest> callback)
        {

            using (UnityWebRequest webRequest = UnityWebRequest.Get(Path.Combine(UtilityAddress.GetURLToWebserver(), folder, Path.GetFileName(name))))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    callback(null);
                }
                else
                {
                    callback(webRequest);
                }
            }
        }
        public List<string> GetItemsInFolderFromServer(string folder)
        {
            List<string> names = new List<string>();
            WebRequest request = WebRequest.Create(Path.Combine(UtilityAddress.GetURLToWebserver(), folder));
            WebResponse response = request.GetResponse();
            Regex regex = new Regex("<a[^>]*href\\s*=\\s*\"(?<name>[^\"]*\\.(?:glb|mp4|mov|mkw|json))\"[^>]*>.*?<\\/a>");
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string result = reader.ReadToEnd();
                MatchCollection matches = regex.Matches(result);
                if (matches.Count == 0)
                {
                    Debug.Log("parse failed.");
                    return names;
                }
                foreach (Match match in matches)
                {
                    if (!match.Success) { continue; }
                    names.Add(match.Groups["name"].Value);
                }
                return names;
            }
        }
    }
}
