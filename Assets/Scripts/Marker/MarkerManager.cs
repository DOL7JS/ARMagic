using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Net;
using Utility;
using UnityEngine.Networking;
using System.Text;
using System.Linq;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using EasyUI.Toast;
using Siccity.GLTFUtility;

public class MarkerManager : MonoBehaviour
{


    [SerializeField]
    private Transform contentModels;
    [SerializeField]
    private Transform contentConfigurations;
    [SerializeField]
    private GameObject contentItemPrefab;
    [SerializeField]
    private RecognitionMarker recognitionMarker;
    [SerializeField]
    private GameObject toggleGroup;
    [SerializeField]
    private GameObject ChooseImagePanel;
    [SerializeField]
    private InputField newConfigurationInputField;
    [SerializeField]
    private UtilityApp utilityApp;
    [SerializeField]
    private Toggle triAxisToggle;
    [SerializeField]
    private ARTrackedImageManager ARTrackedImageManager;


    private List<string> namesOfModels;
    private List<string> namesOfConfigurations;
    private RuntimeReferenceImageLibrary library;
    private ARTrackedImageManager m_TrackedImageManager;
    private string actualConfigurationName;


    void Awake()
    {
        namesOfModels = new List<string>();
        namesOfConfigurations = new List<string>();
        PlayerPrefs.SetString("NameOfMarkerModel", "");
        actualConfigurationName = "";
        m_TrackedImageManager = ARTrackedImageManager;

        library = m_TrackedImageManager.CreateRuntimeLibrary();
        m_TrackedImageManager.referenceLibrary = library;
        m_TrackedImageManager.requestedMaxNumberOfMovingImages = 1;
        m_TrackedImageManager.enabled = true;
        LoadAllEdisonModels();
        SetExistingImage();
    }
    private void Start()
    {

    }
    private void LoadAllEdisonModels()
    {
        utilityApp.SetItemsForScrollView(EnumFolders.marker_objects.ToString(), contentModels, OnClickButtonToChooseModel, names: namesOfModels);
        utilityApp.SetItemsForScrollView(EnumFolders.marker_objects_configurations.ToString(), contentConfigurations, OnClickButtonToChooseConfiguration, names: namesOfConfigurations);
    }

    void OnClickButtonToChooseModel(string modelName)
    {
        if (PlayerPrefs.GetString("NameOfMarkerModel").Equals(modelName))
        {
            PlayerPrefs.SetString("NameOfMarkerModel", "");
            return;
        }
        utilityApp.ClearButtonsColor(contentConfigurations);


        if (!ModelInstances.models.Exists(item => item.name == modelName))
        {
            GameObject progressCirclePrefab = Resources.Load("ProgressCircleBar") as GameObject;
            GameObject progressCircle = Instantiate(progressCirclePrefab, contentModels.parent.parent.transform);
            progressCircle.GetComponent<ProgressCircleBar>().StartProgressBar();
            utilityApp.GetModel(EnumFolders.marker_objects.ToString(), modelName, go =>
            {
                if (go != null)
                {
                    ModelInstances.AddModel(go, modelName);
                }
                else
                {
                    Debug.Log("Model NOT FOUND");
                }
                progressCircle.GetComponent<ProgressCircleBar>().StopProgressBar();
                Destroy(progressCircle);
            });
        }
        PlayerPrefs.SetString("NameOfMarkerModel", modelName);
    }

    void OnClickButtonToChooseConfiguration(string configurationName)
    {
        PrepareForConfiguration();

        GameObject progressCirclePrefab = Resources.Load("ProgressCircleBar") as GameObject;
        GameObject progressCircle = Instantiate(progressCirclePrefab, contentConfigurations.parent.parent.transform);
        progressCircle.GetComponent<ProgressCircleBar>().StartProgressBar();

        utilityApp.GetConfiguration(configurationName, EnumFolders.marker_objects_configurations.ToString(), mods =>
        {

            bool isActuallyInUse = actualConfigurationName.Equals(configurationName);
            recognitionMarker.ConfigurationsInScene.Clear();

            if (isActuallyInUse)
            {
                progressCircle.GetComponent<ProgressCircleBar>().StopProgressBar();
                Destroy(progressCircle);
                actualConfigurationName = "";
                return;
            }
            actualConfigurationName = configurationName;
            int counter = 0;
            foreach (ModelConfiguration mod in mods)
            {
                recognitionMarker.ConfigurationsInScene.Add(mod);
            }
            List<string> modelsToAdd = ModelsToAdd(mods);
            int countModelsToAdd = modelsToAdd.Count();
            if (countModelsToAdd == 0)
            {
                recognitionMarker.AreNewModels = true;
                progressCircle.GetComponent<ProgressCircleBar>().StopProgressBar();
                Destroy(progressCircle);
            }
            string missingModelsBaseMessage = "Missing models:\n";
            string missingModelsNames = "";
            foreach (string name in modelsToAdd)
            {
                if (!ModelInstances.models.Exists(item => item.name == name))
                {
                    //exists in list
                    utilityApp.GetModel(EnumFolders.marker_objects.ToString(), name, go =>
                     {
                         if (go != null)
                         {
                             ModelInstances.AddModel(go, name);
                         }
                         else
                         {
                             missingModelsNames += name + ", ";
                         }
                         counter++;
                         CheckIfAllModelsLoaded(counter, countModelsToAdd, progressCircle, missingModelsBaseMessage, missingModelsNames);
                     });
                }
            }
        });
        PlayerPrefs.SetString("NameOfMarkerModel", "");
    }

    private void PrepareForConfiguration()
    {
        utilityApp.ClearButtonsColor(contentModels);
        toggleGroup.transform.GetChild(0).gameObject.GetComponent<Toggle>().SetIsOnWithoutNotify(false);//adding Toggle
        toggleGroup.transform.GetChild(1).gameObject.GetComponent<Toggle>().SetIsOnWithoutNotify(false);//deleting Toggle
        SetAvailabilityScrollViewModels(false);
        utilityApp.RemoveModelsByTag(EnumModelType.MarkerObject.ToString());
    }

    private List<string> ModelsToAdd(List<ModelConfiguration> configurations)
    {
        return configurations.Select(item => item.name).Distinct().Where(item => !ModelInstances.models.Any(item2 => item2.name == item)).ToList();
    }


    public void ShowChooseImagePanel()
    {
        ChooseImagePanel.SetActive(true);
    }

    private void SetExistingImage()
    {
        if (PlayerPrefs.GetString("RecognisionImage").Length != 0)
        {
            if (File.Exists(PlayerPrefs.GetString("RecognisionImage")))
            {
                Transform rawImageForRecognision = ChooseImagePanel.transform.Find("ImageForRecognision");
                Transform imageWidth = ChooseImagePanel.transform.Find("InputFieldWidth");
                utilityApp.GetTexture(PlayerPrefs.GetString("RecognisionImage"), image =>
                {
                    image.name = PlayerPrefs.GetString("RecognisionImage");
                    rawImageForRecognision.gameObject.GetComponent<RawImage>().texture = image;
                    imageWidth.gameObject.GetComponent<InputField>().text = PlayerPrefs.GetFloat("RecognisionImageWidth").ToString();
                    AddImageToLibrary(image as Texture2D, PlayerPrefs.GetFloat("RecognisionImageWidth"));
                });
            }
            else
            {
                Debug.Log("RecognisionImage NOT EXISTS: " + PlayerPrefs.GetString("RecognisionImage"));

            }
        }
        else
        {
            Debug.Log("RecognisionImage NOT DOUNF");
        }
    }

    public void SetDeletingModel()
    {
        contentModels.parent.parent.GetComponent<Selectable>().interactable = recognitionMarker.DeletingObjects;
        recognitionMarker.DeletingObjects = !recognitionMarker.DeletingObjects;
        PlayerPrefs.SetString("NameOfMarkerModel", "");

        if (recognitionMarker.DeletingObjects)
        {
            DisableScrollViews(!recognitionMarker.DeletingObjects);
        }
        else
        {
            SetAvailabilityScrollViewConfigurations(true);
        }

    }
    public void SetAddingModel()
    {
        SetAvailabilityScrollViewModels(toggleGroup.transform.GetChild(0).gameObject.GetComponent<Toggle>().isOn);
        utilityApp.ClearButtonsColor(contentModels);

        if (!toggleGroup.transform.GetChild(0).gameObject.GetComponent<Toggle>().isOn)
        {
            PlayerPrefs.SetString("NameOfMarkerModel", "");
        }
    }
    private void CheckIfAllModelsLoaded(int counter, int countModelsToAdd, GameObject progressCircle, string missingModelsBaseMessage, string missingModelsNames)
    {
        if (counter == countModelsToAdd)
        {
            recognitionMarker.AreNewModels = true;
            progressCircle.GetComponent<ProgressCircleBar>().StopProgressBar();
            Destroy(progressCircle);
            if (missingModelsNames.Length != 0)
            {
                missingModelsNames = missingModelsNames[0..^2];
                Toast.Show(missingModelsBaseMessage + missingModelsNames, Color.red);
            }
        }
    }
    private void DisableScrollViews(bool state)
    {
        SetAvailabilityScrollViewModels(state);
        SetAvailabilityScrollViewConfigurations(state);
    }
    private void SetAvailabilityScrollViewModels(bool state)
    {
        contentModels.parent.parent.GetComponent<ScrollRect>().vertical = state;

        foreach (Transform item in contentModels.transform)
        {
            item.GetChild(0).GetComponent<Button>().interactable = state;
        }
    }
    private void SetAvailabilityScrollViewConfigurations(bool state)
    {
        contentConfigurations.parent.parent.GetComponent<ScrollRect>().vertical = state;

        foreach (Transform item in contentConfigurations.transform)
        {
            item.GetChild(0).GetComponent<Button>().interactable = state;
        }
    }
    public void ChooseImageToRecognize()
    {

        utilityApp.ChooseImage(image =>
        {
            Transform rawImageForRecognision = ChooseImagePanel.transform.Find("ImageForRecognision");
            if (rawImageForRecognision != null)
            {
                rawImageForRecognision.gameObject.GetComponent<RawImage>().texture = image;
                PlayerPrefs.SetString("RecognisionImage", image.name);
            }
            else
            {
                Debug.Log("ImageForRecognision not found");
            }
        });

    }
    public void OnTriAxisToggleChange()
    {
        if (recognitionMarker.TriAxisInstantiated != null)
        {
            recognitionMarker.TriAxisInstantiated.SetActive(triAxisToggle.isOn);
        }
    }
    public void ChooseImageConfirm()
    {
        string widthText = ChooseImagePanel.transform.Find("InputFieldWidth").GetComponent<InputField>().text;
        if (widthText.Length == 0)
        {
            Toast.Show("Width can't be empty.", ToastColor.Red);
            return;
        }
        if (!float.TryParse(widthText, out float width))
        {
            Toast.Show("Width have to be number.", ToastColor.Red);
            return;
        }
        if (width <= 0)
        {
            Toast.Show("Width have to be positive.", ToastColor.Red);
            return;
        }
        if (PlayerPrefs.GetString("RecognisionImage").Length == 0)
        {
            Debug.Log("IMAGE NOT EXISTS");
            Toast.Show("Image can't be empty.", ToastColor.Red);
            return;
        }
        Texture2D image = ChooseImagePanel.transform.Find("ImageForRecognision").gameObject.GetComponent<RawImage>().texture as Texture2D;
        recognitionMarker.TriAxisInstantiated = null;
        AddImageToLibrary(image, width);
        ChooseImagePanel.SetActive(false);
        utilityApp.ClearButtonsColor(contentConfigurations);
        utilityApp.ClearButtonsColor(contentModels);
    }

    private void AddImageToLibrary(Texture2D image, float width)
    {
        m_TrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        library = m_TrackedImageManager.CreateRuntimeLibrary();
        m_TrackedImageManager.referenceLibrary = library;
        if (library is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            var jobState = mutableLibrary.ScheduleAddImageWithValidationJob(image, image.name, (width / 100.0f));
            PlayerPrefs.SetString("RecognisionImage", image.name);
            PlayerPrefs.SetFloat("RecognisionImageWidth", width);
        }
    }


    public void UploadActualConfiguration()
    {
        if (newConfigurationInputField.text.Length == 0)
        {
            Toast.Show("New configuration name can't be empty.", ToastColor.Red);
            return;
        }

        if (namesOfConfigurations.Contains(newConfigurationInputField.text + ".json"))
        {
            Toast.Show("New configuration name already exists.", ToastColor.Red);
            return;
        }
        utilityApp.UploadConfiguration(EnumModelType.MarkerObject.ToString(), EnumFolders.marker_objects_configurations.ToString(), newConfigurationInputField.text, result =>
        {
            if (result)
            {
                utilityApp.AddButtonToScrollView(newConfigurationInputField.text + ".json", OnClickButtonToChooseConfiguration, contentConfigurations, false);
                foreach (Transform child in contentConfigurations.transform)
                {
                    if (child.GetComponentInChildren<Text>().text.Equals(newConfigurationInputField.text + ".json"))
                    {
                        child.GetComponentInChildren<Image>().color = new Color32(200, 200, 200, 255);
                    }
                    else
                    {
                        child.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 255);
                    }
                }
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag(EnumModelType.MarkerObject.ToString()))
                {
                    recognitionMarker.ConfigurationsInScene.Add(new ModelConfiguration(obj.name.Replace("(Clone)", ""), obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale));
                }
                utilityApp.SortScrollView(contentConfigurations);
                actualConfigurationName = newConfigurationInputField.text + ".json";
                newConfigurationInputField.text = "";
            }
        });

    }


    public void OnToggleAxisXChange()
    {
        recognitionMarker.RotationOfPrefab = new Vector3(1, 0, 0);
    }
    public void OnToggleAxisYChange()
    {
        recognitionMarker.RotationOfPrefab = new Vector3(0, 1, 0);
    }
    public void OnToggleAxisZChange()
    {
        recognitionMarker.RotationOfPrefab = new Vector3(0, 0, 1);
    }

    public void ResetAllRotationOnModels()
    {
        GameObject[] models = GameObject.FindGameObjectsWithTag(EnumModelType.MarkerObject.ToString());
        Debug.Log("MODELS: " + models.Length);
        foreach (GameObject go in models)
        {
            GameObject gameObject1 = ModelInstances.models.Find(item => item.name == go.name.Replace("(Clone)", ""));
            go.transform.parent.gameObject.transform.localRotation = gameObject1.transform.localRotation;
        }
    }

    public void ResetActualPrefab()
    {
        if (recognitionMarker.ActualPrefab != null)
        {
            GameObject gameObject1 = ModelInstances.models.Find(item => item.name == recognitionMarker.ActualPrefab.transform.GetChild(0).name.Replace("(Clone)", ""));
            recognitionMarker.ActualPrefab.transform.localRotation = gameObject1.transform.localRotation;
            Debug.Log("ROT: " + recognitionMarker.ActualPrefab.transform.rotation.eulerAngles);
        }
    }

}
