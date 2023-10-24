using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using EasyUI.Toast;
using Utility;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using System;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;
using CustomYieldInstructions;
using System.Text.RegularExpressions;

public class OptionsManager : MonoBehaviour
{
    [SerializeField]
    private GameObject IPFolderPanel;
    [SerializeField]
    private UtilityApp utilityApp;
    [SerializeField]
    private UtilityDownloader utilityDownloader;
    [SerializeField]
    private Text URLText;
    [SerializeField]
    private InputField inputIPAdress;
    [SerializeField]
    private InputField inputFolder;

    private string IPAddress;
    private string folder;
    private readonly string IPAddressPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\:(?:[1-9][0-9]{0,4})$";


    public void TestConnection()
    {
        if (UtilityAddress.GetURLToWebserver().Length < 8)
        {
            Toast.Show("IP address is not set.", Color.red);
            return;
        }
        Debug.Log("IP: " + UtilityAddress.GetURLToWebserver());
        StartCoroutine(StartTestConnection());
    }

    IEnumerator StartTestConnection()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(UtilityAddress.GetURLToWebserver()))
        {
            webRequest.timeout = 1;
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Toast.Show("Fail" + webRequest.error, 3f, Color.red);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Toast.Show("HTTP Fail" + webRequest.error, 3f, Color.red);
                    break;
                case UnityWebRequest.Result.Success:
                    Toast.Show("Success", 3f);
                    break;
            }
        }
    }

    public void DownloadAllModelsFromServerToCache()
    {
        utilityDownloader.IsServerAvailable(isAvailable =>
        {
            if (isAvailable)
            {
                StartCoroutine(DownloadAllModelsFromServerToCacheEnumerator());
            }
            else
            {
                Toast.Show("Server not available", Color.red);
            }
        });
    }
    public IEnumerator DownloadAllModelsFromServerToCacheEnumerator()
    {
        GameObject objPrefab = Resources.Load("ProgressCircleBar") as GameObject;
        GameObject canvas = GameObject.Find("Canvas");
        GameObject progressBar = Instantiate(objPrefab, canvas.transform);
        progressBar.GetComponent<ProgressCircleBar>().StartProgressBar();
        yield return utilityApp.CacheAllFilesFromServer();
        progressBar.GetComponent<ProgressCircleBar>().StopProgressBar();
        Destroy(progressBar);
    }
    public void ClearCache()
    {
        foreach (GameObject go in ModelInstances.models)
        {
            Destroy(go);
        }
        ModelInstances.models.Clear();
        bool success = Caching.ClearCache();
        if (Directory.Exists(Application.temporaryCachePath))
        {
            Directory.Delete(Application.temporaryCachePath, true);
        }
        foreach (var item in Enum.GetValues(typeof(EnumFolders)))
        {
            if (Directory.Exists(Path.Combine(Application.persistentDataPath, item.ToString())))
            {
                Directory.Delete(Path.Combine(Application.persistentDataPath, item.ToString()), true);
            }
        }
        if (Directory.Exists(Application.persistentDataPath))
        {
            Directory.Delete(Application.persistentDataPath, true);
        }
        PlayerPrefs.DeleteKey("RecognisionImage");
        PlayerPrefs.DeleteKey("RecognisionImageWidth");
        if (!success)
        {
            Toast.Show("Unable to clear cache", 3f);
        }
        else
        {
            Toast.Show("Cache cleared", 3f);
        }
    }
    public void ShowSetIPAndFolderPanel()
    {
        IPFolderPanel.SetActive(true);
        inputIPAdress.text = UtilityAddress.GetIPAddressOfWebserver();
        inputFolder.text = UtilityAddress.GetNameOfFolderOfWebserver();
    }

    public void SetIPAndFolderPanel()
    {
        if (inputIPAdress.text.Length == 0)
        {
            Toast.Show("IP address can't be empty.", Color.red);
            return;
        }
        if (inputFolder.text.Length == 0)
        {
            Toast.Show("Folder can't be empty.", Color.red);
            return;
        }

        bool isIpAddressWithPort = Regex.IsMatch(inputIPAdress.text, IPAddressPattern);
        if (isIpAddressWithPort)
        {
            int port = int.Parse(inputIPAdress.text.Split(':')[1]);
            bool isMatch = (port >= 1 && port <= 65535);
            if (isMatch)
            {
                UtilityAddress.SetURLToWebserver(inputIPAdress.text, inputFolder.text);
                Debug.Log("SetIPAndFolderPanel: CORRECT");
                Toast.Show("IP address was set.");
                CloseIPAndFolder();
            }
            else
            {
                Toast.Show("IP address is not in correct format.", Color.red);
            }
        }
        else
        {
            Toast.Show("IP address is not in correct format.", Color.red);
        }

    }
    public void OnIPAddressChange(string value)
    {
        IPAddress = value;
        SetURL(IPAddress, folder);
    }
    public void OnFolderChange(string value)
    {
        folder = value;
        SetURL(IPAddress, folder);
    }

    public void CloseIPAndFolder()
    {
        IPFolderPanel.SetActive(false);
    }

    private void SetURL(string IPAddress, string folder)
    {
        URLText.text = "http://" + IPAddress + "/" + folder;
    }
}

