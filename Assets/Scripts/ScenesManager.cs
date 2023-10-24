using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ScenesManager : MonoBehaviour
{
    private static List<string> previousScenes = new List<string>();


    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LoadPreviousScene();
                return;
            }
        }

    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("MenuScene") ||
            SceneManager.GetActiveScene().name.Equals("ModelsShowRoomScene") ||
            SceneManager.GetActiveScene().name.Equals("SetImageAndVideoScene"))
        {
            Screen.sleepTimeout = 120;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
    void Awake()
    {
        if (previousScenes.Count == 0 || SceneManager.GetActiveScene().name != previousScenes[previousScenes.Count - 1])
        {
            if (SceneManager.GetActiveScene().name.Equals("ArVideoScene") && previousScenes[previousScenes.Count - 1].Equals("SetImageAndVideoScene"))
            {
                previousScenes.RemoveAt(previousScenes.Count - 1);
            }
            else
            {
                previousScenes.Add(SceneManager.GetActiveScene().name);
            }

        }
    }

    public void AddCurrentSceneToLoadedScenes()
    {
        if (SceneManager.GetActiveScene().name.Equals("SetImageAndVideoScene"))
        {
            previousScenes.RemoveAt(previousScenes.Count - 1);
        }
        else
        {
            previousScenes.Add(SceneManager.GetActiveScene().name);
        }

    }

    public void LoadPreviousScene()
    {
        string previousScene = string.Empty;

        if (previousScenes.Count > 1)
        {
            Debug.Log("INPUT>1");
            previousScenes.RemoveAt(previousScenes.Count - 1);
            previousScene = previousScenes[previousScenes.Count - 1];
            SceneManager.LoadScene(previousScene);
        }
        else
        {

            Debug.Log("INPUT<=1");
            previousScenes.RemoveAt(previousScenes.Count - 1);
            Application.Quit();
        }
    }

    // Start is called before the first frame update
    public void OpenEdisonScene()
    {
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        xrManagerSettings.DeinitializeLoader();
        xrManagerSettings.InitializeLoaderSync();
        SceneManager.LoadSceneAsync("ARMarkerScene", LoadSceneMode.Single);
    }

    public void OpenArFaceScene()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        xrManagerSettings.DeinitializeLoader();
        xrManagerSettings.InitializeLoaderSync();
        SceneManager.LoadSceneAsync("ArFaceScene", LoadSceneMode.Single);
    }

    public void OpenVideoScene()
    {
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        xrManagerSettings.DeinitializeLoader();
        xrManagerSettings.InitializeLoaderSync();
        SceneManager.LoadScene("ArVideoScene", LoadSceneMode.Single);
    }
    public void OpenARAreaScene()
    {
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        xrManagerSettings.DeinitializeLoader();
        xrManagerSettings.InitializeLoaderSync();
        SceneManager.LoadScene("ARAreaScene", LoadSceneMode.Single);
    }
    public void OpenOptionsScene()
    {
        SceneManager.LoadScene("OptionsScene");
    }
    public void OpenSetImageAndVideoScene()
    {
        SceneManager.LoadScene("SetImageAndVideoScene");
    }
    public void OpenModelsShowRoomScene()
    {
        SceneManager.LoadScene("ModelsShowRoomScene", LoadSceneMode.Single);
    }

}
