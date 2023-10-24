using EasyUI.Toast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class ModelsShowManager : MonoBehaviour
{

    [SerializeField]
    private Transform contentModels;
    [SerializeField]
    private UtilityApp utilityApp;
    [SerializeField]
    private Slider sliderLeftRight;
    [SerializeField]
    private Slider sliderUpDown;
    [SerializeField]
    private Slider sliderScale;
    [SerializeField]
    private Camera modelCamera;
    private GameObject actualModel;
    private float modelRotationAxisZ;

    void Awake()
    {
        LoadAllModels();
        SetAvailabilityToSliders(false);
    }


    private void LoadAllModels()
    {
        utilityApp.SetAllItemsForScrollView(contentModels, OnClickButtonToChooseModel);
    }

    private void OnClickButtonToChooseModel(string rowName)
    {
        string[] names = rowName.Split("/");
        string folder = names[0];
        string modelName = names[1];

        if (!ModelInstances.models.Exists(item => item.name == modelName))
        {
            GameObject progressCirclePrefab = Resources.Load("ProgressCircleBar") as GameObject;
            GameObject progressCircle = Instantiate(progressCirclePrefab, contentModels.parent.parent.transform);
            progressCircle.GetComponent<ProgressCircleBar>().StartProgressBar();
            utilityApp.GetModel(folder, modelName, go =>
            {

                if (go != null)
                {
                    InstantiateModel(ModelInstances.AddModel(go, modelName).transform.GetChild(0).gameObject);
                    Debug.Log("Model FOUND import");
                }
                else
                {
                    Debug.Log("Model NOT FOUND");
                    Toast.Show("Can't get model. Poor connection.", Color.red);
                }
                progressCircle.GetComponent<ProgressCircleBar>().StopProgressBar();
                Destroy(progressCircle);
            });
        }
        else
        {
            GameObject go = ModelInstances.models.Find(item => item.name == modelName);
            InstantiateModel(go.transform.GetChild(0).gameObject);
        }
    }

    private void SetAvailabilityToSliders(bool value)
    {
        sliderScale.interactable = value;
        sliderLeftRight.interactable = value;
        sliderUpDown.interactable = value;
    }
    private void InstantiateModel(GameObject go)
    {
        go.SetActive(true);
        Destroy(actualModel);
        if (go != null)
        {
            actualModel = Instantiate(go, new Vector3(-500, 0, 0), go.transform.rotation);
            actualModel.transform.localScale = new Vector3(1, 1, 1);
            actualModel.SetActive(true);
            BoxCollider boxCollider = actualModel.GetComponent<BoxCollider>();
            Vector3 worldCenter = actualModel.transform.TransformPoint(boxCollider.center);
            float dist = Vector3.Distance(worldCenter, new Vector3(worldCenter.x, 0, worldCenter.z));
            actualModel.transform.position = new Vector3(0, -dist, Mathf.Max(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) * 1.5f);

            sliderLeftRight.SetValueWithoutNotify(actualModel.transform.eulerAngles.y);
            sliderUpDown.SetValueWithoutNotify(actualModel.transform.eulerAngles.x);
            sliderScale.SetValueWithoutNotify(actualModel.transform.localScale.x);
            modelRotationAxisZ = actualModel.transform.eulerAngles.z;
            SetAvailabilityToSliders(true);
            Toast.Show("Model placed");
        }
    }

    public void OnChangeRotationModel()
    {
        actualModel.transform.eulerAngles = new Vector3(sliderUpDown.value, sliderLeftRight.value, modelRotationAxisZ);
    }
    public void OnChangeScaleSlider()
    {
        actualModel.transform.localScale = new Vector3(sliderScale.value, sliderScale.value, sliderScale.value);
    }
}
