using EasyUI.Toast;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Utility
{
    public class UtilityUploader : MonoBehaviour
    {

        public void UploadConfiguration(GameObject[] models, string folder, string nameOfConfiguration, string tag)
        {
            if (nameOfConfiguration.Length == 0)
            {
                return;
            }
            List<ModelConfiguration> configurations = new List<ModelConfiguration>();
            if (tag == EnumModelType.MarkerObject.ToString())
            {
                foreach (GameObject obj in models)
                {
                    configurations.Add(new ModelConfiguration(obj.name.Replace("(Clone)", ""), obj.transform.parent.gameObject.transform.localPosition, obj.transform.parent.gameObject.transform.localRotation, obj.transform.parent.gameObject.transform.localScale));
                }
            }
            else
            {
                foreach (GameObject obj in models)
                {
                    configurations.Add(new ModelConfiguration(obj.name.Replace("(Clone)", ""), obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale));
                }
            }

            WWWForm wwwform = new WWWForm();
            string jsonString = JsonHelper.ToJson(configurations);
            wwwform.AddField("ConfigurationContent", jsonString);
            wwwform.AddField("ConfigurationName", nameOfConfiguration);
            wwwform.AddField("folder", folder);

            UnityWebRequest www = UnityWebRequest.Post(Path.Combine(UtilityAddress.GetURLToWebserver(), "upload.php"), wwwform);
            StartCoroutine(SendRequest(www));
        }

        IEnumerator SendRequest(UnityWebRequest www)
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to send request: " + www.error);
                Toast.Show("Error: " + www.error, Color.red);
            }
            else
            {
                Debug.Log("Request sent successfully: " + www.downloadHandler.text);
                Toast.Show("Configuration uploaded successfully");
            }
        }
    }
}