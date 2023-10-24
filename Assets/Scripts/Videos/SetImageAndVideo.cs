using EasyUI.Toast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Utility;

public class SetImageAndVideo : MonoBehaviour
{
    [SerializeField]
    private Transform content;
    [SerializeField]
    private Transform contentPairs;
    [SerializeField]
    private GameObject contentItemPrefab;
    [SerializeField]
    private GameObject contentDeleteItemPrefab;
    [SerializeField]
    private RawImage image;
    [SerializeField]
    private InputField inputFieldWidth;
    [SerializeField]
    private GameObject panelWithDetails;
    [SerializeField]
    private UtilityApp utilityApp;

    private string pathToImage = "";
    private string pathToVideo = "";
    private List<ImageVideoContainer> videoImageContainers;
    private ImageVideoContainer selectedImageVideoContainer;
    private VideoPlayer videoPlayerComponent;
    private AudioSource audioSourceComponent;

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        videoImageContainers = new List<ImageVideoContainer>();
        LoadPairsFromFile();
        LoadPairsVideoImageToScrollView();
        LoadVideosNamesToScrollView();
    }

    public void AddPair()
    {

        if (pathToVideo.Length == 0)
        {
            Toast.Show("Video can't be empty.", ToastColor.Red);
            return;
        }
        if (pathToImage.Length == 0)
        {
            Toast.Show("Image can't be empty.", ToastColor.Red);
            return;
        }
        if (inputFieldWidth.text.Length == 0)
        {
            Toast.Show("Width can't be empty.", ToastColor.Red);
            return;
        }
        if (!float.TryParse(inputFieldWidth.text, out float width))
        {
            Toast.Show("Width have to be number.", ToastColor.Red);
            return;
        }
        if (width <= 0)
        {
            Toast.Show("Width have to be positive.", ToastColor.Red);
            return;
        }
        if (videoImageContainers.Exists(item => item.ImagePath == pathToImage))
        {
            Toast.Show("Image with same path already assign.", ToastColor.Red);
            return;
        }
        videoImageContainers.Add(new ImageVideoContainer(pathToVideo, pathToImage, width / 100.0f));
        LoadPairsVideoImageToScrollView();
        RemoveClickedButtonVideo(Path.GetFileName(pathToVideo));
        inputFieldWidth.text = "";
        pathToImage = "";
        pathToVideo = "";
        image.texture = null;
    }

    private void RemoveClickedButtonVideo(string name)
    {
        foreach (Transform child in content.transform)
        {
            if (child.GetComponentInChildren<Text>().text.Equals(name))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void LoadVideosNamesToScrollView()
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        utilityApp.SetItemsForScrollView(EnumFolders.videos.ToString(), content, OnClickButtonToChooseVideo, videoImageContainers);
    }

    private void LoadPairsVideoImageToScrollView()
    {
        foreach (Transform child in contentPairs.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (ImageVideoContainer item in videoImageContainers)
        {
            var item_go = Instantiate(contentDeleteItemPrefab);
            var childButton = item_go.transform.Find("ButtonDelete").gameObject;
            childButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                videoImageContainers.Remove(item);
                Destroy(childButton.transform.parent.gameObject);
                utilityApp.AddButtonToScrollView(Path.GetFileName(item.VideoPath), OnClickButtonToChooseVideo, content, false);
                utilityApp.SortScrollView(content);
            });
            childButton = item_go.transform.Find("Button").gameObject;
            childButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedImageVideoContainer = item;
                ShowPanelWithDetails();
            });
            Text[] texts = childButton.transform.GetComponentsInChildren<Text>();
            texts[0].text = Path.GetFileName(item.ImagePath);
            texts[1].text = Path.GetFileName(item.VideoPath);
            texts[2].text = (item.ImageWidth * 100).ToString("##.##") + " cm";
            item_go.transform.SetParent(contentPairs.transform);
            item_go.transform.localPosition = new Vector3(0, 0, 0);
            item_go.transform.localScale = Vector3.one;
        }
    }
    void OnClickButtonToChooseVideo(string nameOfVideo)
    {
        pathToVideo = Path.Combine(UtilityAddress.GetURLToWebserver(), EnumFolders.videos.ToString(), nameOfVideo);
        StartCoroutine(utilityApp.DownloadAndCacheFile(EnumFolders.videos.ToString(), nameOfVideo));
    }

    public void OnPressShowImagePicker()
    {
        utilityApp.ChooseImage(imageCallback =>
        {
            image.texture = imageCallback;
            pathToImage = imageCallback.name;
        });
    }
    private void LoadPairsFromFile()
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, EnumFolders.imagesVideosPairs.ToString() + ".json")))
        {
            string result = File.ReadAllText(Path.Combine(Application.persistentDataPath, EnumFolders.imagesVideosPairs.ToString() + ".json"));
            videoImageContainers = JsonHelper.FromJson<ImageVideoContainer>(result);
        }

    }
    public void Confirm()
    {
        string result = JsonHelper.ToJson<ImageVideoContainer>(videoImageContainers);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, EnumFolders.imagesVideosPairs.ToString() + ".json"), result);
    }

    public void ShowPanelWithDetails()
    {
        panelWithDetails.transform.Find("TextImageName").GetComponent<Text>().text = Path.GetFileName(selectedImageVideoContainer.ImagePath);
        panelWithDetails.transform.Find("TextVideoName").GetComponent<Text>().text = Path.GetFileName(selectedImageVideoContainer.VideoPath);
        panelWithDetails.transform.Find("TextWidth").GetComponent<Text>().text = "Width: " + (selectedImageVideoContainer.ImageWidth * 100).ToString() + " cm";

        utilityApp.GetTexture(selectedImageVideoContainer.ImagePath, texture =>
        {
            panelWithDetails.transform.Find("RawImage").GetComponent<RawImage>().texture = texture;
        });
        panelWithDetails.SetActive(true);

        StartCoroutine(PlayVideo(panelWithDetails.transform.Find("RawImageForVideo").GetComponent<RawImage>().gameObject));
    }

    IEnumerator PlayVideo(GameObject prefab)
    {
        PrepareForVideo(prefab);

        if (utilityApp.IsFileCached(EnumFolders.videos.ToString(), videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == Path.GetFileName(selectedImageVideoContainer.ImagePath)).VideoPath))
        {
            videoPlayerComponent.url = Path.Combine(Application.persistentDataPath, EnumFolders.videos.ToString(), Path.GetFileName(selectedImageVideoContainer.VideoPath));
        }
        else if (utilityApp.IsFileOnServer(EnumFolders.videos.ToString(), videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == Path.GetFileName(selectedImageVideoContainer.ImagePath)).VideoPath))
        {
            videoPlayerComponent.url = Path.Combine(UtilityAddress.GetURLToWebserver(), EnumFolders.videos.ToString(), Path.GetFileName(selectedImageVideoContainer.VideoPath));
            StartCoroutine(utilityApp.DownloadAndCacheFile(EnumFolders.videos.ToString(), videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == Path.GetFileName(selectedImageVideoContainer.VideoPath)).VideoPath));
        }
        else
        {
            Toast.Show("Video doesn't exists", Color.red);
            yield break;
        }

        videoPlayerComponent.Prepare();
        while (!videoPlayerComponent.isPrepared)
        {
            yield return null;
        }

        prefab.GetComponent<RawImage>().texture = videoPlayerComponent.texture;
        videoPlayerComponent.Play();
        audioSourceComponent.Play();

        while (videoPlayerComponent.isPlaying)
        {
            yield return null;
        }
    }

    public void HidePanelWithDetails()
    {
        panelWithDetails.SetActive(false);
        panelWithDetails.transform.Find("RawImage").GetComponent<RawImage>().texture = null;
        panelWithDetails.transform.Find("RawImageForVideo").GetComponent<RawImage>().texture = null;
    }
    private void PrepareForVideo(GameObject prefab)
    {
        if (prefab.GetComponent<VideoPlayer>() != null)
        {
            Destroy(prefab.GetComponent<VideoPlayer>());
        }
        if (prefab.GetComponent<AudioSource>() != null)
        {
            Destroy(prefab.GetComponent<AudioSource>());
        }
        videoPlayerComponent = prefab.AddComponent<VideoPlayer>();
        audioSourceComponent = prefab.AddComponent<AudioSource>();

        videoPlayerComponent.playOnAwake = false;
        audioSourceComponent.playOnAwake = false;

        videoPlayerComponent.source = VideoSource.Url;
        videoPlayerComponent.audioOutputMode = VideoAudioOutputMode.AudioSource;

        videoPlayerComponent.EnableAudioTrack(0, true);
        videoPlayerComponent.SetTargetAudioSource(0, audioSourceComponent);
    }
}
