using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using System.Linq;
using EasyUI.Toast;
using UnityEngine.XR.ARFoundation;

public class FaceManager : MonoBehaviour
{
    [SerializeField]
    private Transform contentFaces;
    [SerializeField]
    private Transform contentFacesConfigurations;
    [SerializeField]
    private ARFaceRegocnition arFaceRegocnition;
    [SerializeField]
    private InputField newConfigurationInputField;
    [SerializeField]
    private UtilityApp utilityApp;

    [SerializeField]
    private ARSession arSession;

    private List<string> namesOfConfigurations;
    private Dictionary<string, bool> configurationActive;

    void Start()
    {
        namesOfConfigurations = new List<string>();
        configurationActive = new Dictionary<string, bool>();
        LoadAllFaces();
    }

    private void LoadAllFaces()
    {
        utilityApp.SetItemsForScrollView(EnumFolders.face_objects.ToString(), contentFaces, OnClickButtonToChooseFace, stayClicked: true);
        utilityApp.SetItemsForScrollView(EnumFolders.face_configurations.ToString(), contentFacesConfigurations, OnClickButtonToChooseFaceConfiguration, names: namesOfConfigurations);

        foreach (string name in namesOfConfigurations)
        {
            configurationActive.Add(name, false);
        }
    }

    void OnClickButtonToChooseFace(string faceName)
    {
        PrepareOnClickFace();

        if (!ModelInstances.models.Exists(item => item.name == faceName))
        {
            GameObject progressCirclePrefab = Resources.Load("ProgressCircleBar") as GameObject;
            GameObject progressCircle = Instantiate(progressCirclePrefab, contentFaces.parent.parent.transform);
            progressCircle.GetComponent<ProgressCircleBar>().StartProgressBar();
            utilityApp.GetModel(EnumFolders.face_objects.ToString(), faceName, go =>
            {
                if (go != null)
                {
                    ModelInstances.AddFaceModel(go, faceName);
                    arFaceRegocnition.ModelsOnFace.Add(faceName);
                }
                else
                {
                    Debug.Log("Model not found");
                }
                progressCircle.GetComponent<ProgressCircleBar>().StopProgressBar();
                Destroy(progressCircle);
            });
        }
        else
        {
            if (!arFaceRegocnition.ModelsOnFace.Exists(item => item == faceName))
            {
                arFaceRegocnition.ModelsOnFace.Add(faceName);
            }
            else
            {
                arFaceRegocnition.ModelsOnFace.RemoveAll(item => item == faceName);
            }
        }
    }

    void OnClickButtonToChooseFaceConfiguration(string configurationName)
    {

        if (!PrepareOnClickFaceConfiguration(configurationName))
        {
            return;
        }
        string missingModelsBaseMessage = "Missing models:\n";
        string missingModelsNames = "";

        utilityApp.GetConfiguration(configurationName, EnumFolders.face_configurations.ToString(), mods =>
        {
            GameObject progressCirclePrefab = Resources.Load("ProgressCircleBar") as GameObject;
            GameObject progressCircle = Instantiate(progressCirclePrefab, contentFacesConfigurations.parent.parent.transform);
            progressCircle.GetComponent<ProgressCircleBar>().StartProgressBar();

            int facesCount = mods.Count;
            SetChosenButtons(mods);

            foreach (ModelConfiguration modelProperty in mods)
            {
                if (ModelInstances.models.Exists(item => item.name == modelProperty.name))
                {
                    arFaceRegocnition.ModelsOnFace.Add(modelProperty.name);
                    CheckIfAllModelsLoaded(arFaceRegocnition.ModelsOnFace.Count, facesCount, progressCircle, missingModelsBaseMessage, missingModelsNames);
                }
                else
                {
                    utilityApp.GetModel(EnumFolders.face_objects.ToString(), modelProperty.name, go =>
                    {
                        if (go != null)
                        {
                            ModelInstances.AddFaceModel(go, modelProperty.name);
                            arFaceRegocnition.ModelsOnFace.Add(modelProperty.name);
                            CheckIfAllModelsLoaded(arFaceRegocnition.ModelsOnFace.Count, facesCount, progressCircle, missingModelsBaseMessage, missingModelsNames);
                        }
                        else
                        {
                            facesCount--;
                            missingModelsNames += modelProperty.name + ", ";
                            CheckIfAllModelsLoaded(arFaceRegocnition.ModelsOnFace.Count, facesCount, progressCircle, missingModelsBaseMessage, missingModelsNames);
                        }

                    });
                }
            }
        });

    }



    private void CheckIfAllModelsLoaded(int counter, int countModelsToAdd, GameObject progressCircle, string missingModelsBaseMessage, string missingModelsNames)
    {
        if (counter == countModelsToAdd)
        {
            progressCircle.GetComponent<ProgressCircleBar>().StopProgressBar();
            Destroy(progressCircle);
            if (missingModelsNames.Length != 0)
            {
                missingModelsNames = missingModelsNames[0..^2];
                Toast.Show(missingModelsBaseMessage + missingModelsNames, Color.red);
            }
        }
    }
    private void SetChosenButtons(List<ModelConfiguration> mods)
    {
        utilityApp.ClearButtonsColor(contentFaces);
        Text[] texts = contentFaces.GetComponentsInChildren<Text>();
        List<Text> texts1 = texts.Where(text => mods.Any(mod => mod.name == text.text)).ToList();

        foreach (Text text in texts1)
        {
            text.transform.parent.GetComponent<Image>().color = new Color32(200, 200, 200, 255);
        }
    }
    private bool PrepareOnClickFaceConfiguration(string configurationName)
    {
        if (!configurationActive.ContainsKey(configurationName))
        {
            configurationActive.Add(configurationName, false);
        }
        if (configurationActive.TryGetValue(configurationName, out bool isActive))
        {
            utilityApp.RemoveModelsByTag(EnumModelType.FaceObject.ToString());
            arFaceRegocnition.ModelsOnFace.Clear();
            utilityApp.ClearButtonsColor(contentFaces);
            configurationActive[configurationName] = !isActive;
            if (isActive)
            {
                return false;
            }
        }
        foreach (var key in configurationActive.Keys.ToList())
        {
            if (key != configurationName)
            {
                configurationActive[key] = false;
            }
        }
        arFaceRegocnition.ModelsOnFace.Clear();
        return true;
    }
    private void PrepareOnClickFace()
    {
        utilityApp.ClearButtonsColor(contentFacesConfigurations);
        foreach (var key in configurationActive.Keys.ToList())
        {
            configurationActive[key] = false;
        }
    }
    public void UploadFaceConfiguration()
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

        utilityApp.UploadConfiguration(EnumModelType.FaceObject.ToString(), EnumFolders.face_configurations.ToString(), newConfigurationInputField.text, result =>
        {
            if (result)
            {
                utilityApp.AddButtonToScrollView(newConfigurationInputField.text + ".json", OnClickButtonToChooseFaceConfiguration, contentFacesConfigurations, false);

                foreach (Transform child in contentFacesConfigurations.transform)
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
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag(EnumModelType.FaceObject.ToString()))
                {
                    arFaceRegocnition.ModelsOnFace.Add(obj.name.Replace("(Clone)", ""));
                }
                foreach (var key in configurationActive.Keys.ToList())
                {
                    configurationActive[key] = false;
                }
                configurationActive[newConfigurationInputField.text + ".json"] = true;
                utilityApp.SortScrollView(contentFacesConfigurations);
                newConfigurationInputField.text = "";
            }

        });

    }
    public void ClearAllModels()
    {
        utilityApp.RemoveModelsByTag(EnumModelType.FaceObject.ToString());
        utilityApp.ClearButtonsColor(contentFaces);
        utilityApp.ClearButtonsColor(contentFacesConfigurations);
        arFaceRegocnition.ModelsOnFace.Clear();

    }

}
