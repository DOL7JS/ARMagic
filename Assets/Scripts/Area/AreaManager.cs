using EasyUI.Toast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class AreaManager : MonoBehaviour
{
    [SerializeField]
    private Transform contentModels;
    [SerializeField]
    private ARAreaScript aRAreaScript;
    [SerializeField]
    private Toggle toggleAddingObject;
    [SerializeField]
    private Toggle toggleDeletingObject;
    [SerializeField]
    private Toggle toggleTriAxis;
    [SerializeField]
    private UtilityApp utilityApp;

    void Awake()
    {
        PlayerPrefs.SetString("NameOfAreaModel", "");
        LoadAllAreaModels();
    }
    private void LoadAllAreaModels()
    {
        utilityApp.SetItemsForScrollView(EnumFolders.area_objects.ToString(), contentModels, OnClickButtonToChooseModel);
    }

    void OnClickButtonToChooseModel(string modelName)
    {
        if (PlayerPrefs.GetString("NameOfAreaModel").Equals(modelName))
        {
            PlayerPrefs.SetString("NameOfAreaModel", "");
            return;
        }
        if (!ModelInstances.models.Exists(item => item.name == modelName))
        {
            GameObject progressCirclePrefab = Resources.Load("ProgressCircleBar") as GameObject;
            GameObject progressCircle = Instantiate(progressCirclePrefab, contentModels.parent.parent.transform);
            progressCircle.GetComponent<ProgressCircleBar>().StartProgressBar();
            utilityApp.GetModel(EnumFolders.area_objects.ToString(), modelName, go =>
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
        PlayerPrefs.SetString("NameOfAreaModel", modelName);
    }

    public void ClearTriAxis()
    {
        if (aRAreaScript.TriAxisInstantiated != null)
        {
            Destroy(aRAreaScript.TriAxisInstantiated);
        }
        toggleTriAxis.isOn = false;
        toggleAddingObject.isOn = !toggleTriAxis.isOn;
        toggleAddingObject.interactable = !toggleTriAxis.isOn;
        toggleDeletingObject.isOn = false;
        toggleDeletingObject.interactable = !toggleTriAxis.isOn;
        contentModels.parent.parent.GetComponent<Selectable>().interactable = !toggleTriAxis.isOn;
        SetAvailabilityScrollViewModels(!toggleTriAxis.isOn);
    }
    public void SetDeletingModel()
    {
        contentModels.parent.parent.GetComponent<Selectable>().interactable = false;
        aRAreaScript.DeletingObjects = !aRAreaScript.DeletingObjects;
        PlayerPrefs.SetString("NameOfAreaModel", "");
        Debug.Log("SetDeletingModel: " + aRAreaScript.DeletingObjects);
        SetAvailabilityScrollViewModels(false);
    }
    public void SetAddingModel()
    {
        SetAvailabilityScrollViewModels(toggleAddingObject.isOn);
        utilityApp.ClearButtonsColor(contentModels);
        contentModels.parent.parent.GetComponent<Selectable>().interactable = toggleAddingObject.isOn;

        if (!toggleAddingObject.isOn)
        {
            PlayerPrefs.SetString("NameOfAreaModel", "");
        }
    }

    private void SetAvailabilityScrollViewModels(bool state)
    {
        contentModels.parent.parent.GetComponent<ScrollRect>().vertical = state;

        foreach (Transform item in contentModels.transform)
        {
            item.GetChild(0).GetComponent<Button>().interactable = state;
        }
    }

    public void OnToggleTriAxisChange()
    {
        toggleAddingObject.isOn = false;
        toggleDeletingObject.isOn = false;

        aRAreaScript.TriAxisToggle = toggleTriAxis.isOn;
        if (aRAreaScript.TriAxisInstantiated != null)
        {
            aRAreaScript.TriAxisInstantiated.SetActive(toggleTriAxis.isOn);
        }
        if (toggleTriAxis.isOn && aRAreaScript.TriAxisInstantiated == null)
        {
            PlayerPrefs.SetString("NameOfAreaModel", toggleTriAxis.isOn ? "TriAxis" : "");
            Toast.Show("Click to instantiate Tri Axis");
        }
    }
    public void OnToggleAxisXChange()
    {
        aRAreaScript.RotationOfPrefab = new Vector3(1, 0, 0);
    }
    public void OnToggleAxisYChange()
    {
        aRAreaScript.RotationOfPrefab = new Vector3(0, 1, 0);
    }
    public void OnToggleAxisZChange()
    {
        aRAreaScript.RotationOfPrefab = new Vector3(0, 0, 1);
    }

    public void ResetAllRotationOnModels()
    {
        GameObject[] models = GameObject.FindGameObjectsWithTag(EnumModelType.AreaObject.ToString());
        foreach (GameObject go in models)
        {
            GameObject gameObject1 = ModelInstances.models.Find(item => item.name == go.name.Replace("(Clone)", ""));
            go.transform.parent.gameObject.transform.localRotation = gameObject1.transform.localRotation;
        }
    }

    public void ResetActualPrefab()
    {
        if (aRAreaScript.ActualPrefab != null)
        {
            GameObject gameObject1 = ModelInstances.models.Find(item => item.name == aRAreaScript.ActualPrefab.name.Replace("(Clone)", ""));
            aRAreaScript.ActualPrefab.transform.localRotation = gameObject1.transform.localRotation;
        }
    }

}
