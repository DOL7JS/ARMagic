using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using EasyUI.Toast;
using System.IO;
using UnityEngine.Video;
using Utility;

public class ImageRecognitionForVideo : MonoBehaviour
{

    [SerializeField]
    private Material material;
    [SerializeField]
    private Slider sliderScale;
    [SerializeField]
    private GameObject instantiatedObject;
    [SerializeField]
    private UtilityApp utilityApp;

    private List<ImageVideoContainer> videoImageContainers;
    private Dictionary<GameObject, ARTrackedImage> videosInScene;
    private RuntimeReferenceImageLibrary library;
    private ARTrackedImageManager m_TrackedImageManager;
    private VideoPlayer videoPlayerComponent;
    private AudioSource audioSourceComponent;

    private void Awake()
    {
        videosInScene = new Dictionary<GameObject, ARTrackedImage>();
        m_TrackedImageManager = new ARTrackedImageManager();
        m_TrackedImageManager = transform.gameObject.AddComponent<ARTrackedImageManager>();
        library = m_TrackedImageManager.CreateRuntimeLibrary();
        m_TrackedImageManager.referenceLibrary = library;
        m_TrackedImageManager.requestedMaxNumberOfMovingImages = 10;
        m_TrackedImageManager.enabled = true;
        if (File.Exists(Path.Combine(Application.persistentDataPath, EnumFolders.imagesVideosPairs.ToString() + ".json")))
        {
            LoadDictoinaryFromFile();
            AddImagesToLibrary();
        }
    }
    public void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    public void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void LoadDictoinaryFromFile()
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, EnumFolders.imagesVideosPairs.ToString() + ".json")))
        {
            string result = File.ReadAllText(Path.Combine(Application.persistentDataPath, EnumFolders.imagesVideosPairs.ToString() + ".json"));
            videoImageContainers = JsonHelper.FromJson<ImageVideoContainer>(result);
        }
    }

    private void AddImagesToLibrary()
    {
        foreach (var item in videoImageContainers)
        {
            LoadImage(item.ImagePath, item.ImageWidth);
        }
    }


    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            material.mainTexture = trackedImage.referenceImage.texture;
            GameObject image = Instantiate(instantiatedObject, trackedImage.transform.position, Quaternion.identity, trackedImage.transform);
            image.transform.localScale = new Vector3(trackedImage.size.x, trackedImage.size.y, trackedImage.size.x);
            videosInScene.Add(image, trackedImage);
            StartCoroutine(PlayVideo(image, trackedImage));

        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            foreach (KeyValuePair<GameObject, ARTrackedImage> item in videosInScene)
            {
                if (item.Value.trackingState == TrackingState.Tracking)
                {
                    if (item.Key.activeSelf == false)
                    {
                        item.Key.transform.localScale = new Vector3(item.Value.size.x, item.Value.size.y, item.Value.size.x);
                        item.Key.transform.GetChild(0).gameObject.transform.localScale = new Vector3(1, 1, 1);

                        item.Key.GetComponentInChildren<MeshRenderer>().material.mainTexture = item.Value.referenceImage.texture;
                        item.Key.SetActive(true);
                        StartCoroutine(PlayVideo(item.Key, item.Value));
                    }
                }
                else
                {
                    item.Key.SetActive(false);
                }
            }
        }
    }
    IEnumerator PlayVideo(GameObject prefab, ARTrackedImage image)
    {
        PrepareForVideo(prefab);
        if (utilityApp.IsFileCached(EnumFolders.videos.ToString(), videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == image.referenceImage.name).VideoPath))
        {
            videoPlayerComponent.url = Path.Combine(Application.persistentDataPath, EnumFolders.videos.ToString(), Path.GetFileName(videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == image.referenceImage.name).VideoPath));
        }
        else if (utilityApp.IsFileOnServer(EnumFolders.videos.ToString(), videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == image.referenceImage.name).VideoPath))
        {
            videoPlayerComponent.url = Path.Combine(UtilityAddress.GetURLToWebserver(), EnumFolders.videos.ToString(), Path.GetFileName(videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == image.referenceImage.name).VideoPath));
            StartCoroutine(utilityApp.DownloadAndCacheFile(EnumFolders.videos.ToString(), videoImageContainers.Find(item => Path.GetFileName(item.ImagePath) == image.referenceImage.name).VideoPath));
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

        prefab.GetComponentInChildren<MeshRenderer>().material = new Material(material);
        prefab.GetComponentInChildren<MeshRenderer>().material.mainTexture = videoPlayerComponent.texture;

        videoPlayerComponent.Play();
        audioSourceComponent.Play();

        while (videoPlayerComponent.isPlaying)
        {
            yield return null;
        }
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
        sliderScale.SetValueWithoutNotify(0.5f);
        videoPlayerComponent = prefab.AddComponent<VideoPlayer>();
        audioSourceComponent = prefab.AddComponent<AudioSource>();

        videoPlayerComponent.isLooping = FindObjectOfType<Toggle>().isOn;

        videoPlayerComponent.playOnAwake = false;
        audioSourceComponent.playOnAwake = false;

        videoPlayerComponent.source = VideoSource.Url;
        videoPlayerComponent.audioOutputMode = VideoAudioOutputMode.AudioSource;

        videoPlayerComponent.EnableAudioTrack(0, true);
        videoPlayerComponent.SetTargetAudioSource(0, audioSourceComponent);
    }

    private void LoadImage(string path, float width)
    {
        utilityApp.GetTexture(path, texture =>
        {
            if (texture == null)
            {
                Debug.LogError("Failed to load texture url:" + path);
                Toast.Show("Failed to load image path: " + path, ToastColor.Red);
            }
            else
            {
                Texture2D data = texture as Texture2D;
                if (library is MutableRuntimeReferenceImageLibrary mutableLibrary)
                {
                    var jobState = mutableLibrary.ScheduleAddImageWithValidationJob(data, texture.name, width);
                }
            }
        });
    }


}