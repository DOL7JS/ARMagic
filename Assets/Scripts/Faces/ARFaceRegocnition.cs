using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Utility;

public class ARFaceRegocnition : MonoBehaviour
{


    [SerializeField]
    private UtilityApp utilityApp;
    public List<string> ModelsOnFace { get; set; }
    private ARFaceManager arFaceManager;
    private GameObject facePrefab;

    private void Awake()
    {
        arFaceManager = FindObjectOfType<ARFaceManager>();
        ModelsOnFace = new List<string>();
    }
    public void OnEnable()
    {
        arFaceManager.facesChanged += OnFaceChanged;
    }
    public void OnDisable()
    {
        arFaceManager.facesChanged -= OnFaceChanged;
    }
    public void OnFaceChanged(ARFacesChangedEventArgs args)
    {
        foreach (var trackedFace in args.added)
        {
            SetModel(trackedFace);
        }
        foreach (var trackedFace in args.removed)
        {
            DeleteModels(trackedFace);
        }
    }



    private void SetModel(ARFace face)
    {
        foreach (string name in ModelsOnFace)
        {
            if (ModelInstances.models.Exists(item => item.name == name))
            {
                InstantiateModelOnFace(ModelInstances.models.Find(item => item.name == name), face);
            }
            else
            {
                utilityApp.GetModel(EnumFolders.face_objects.ToString(), name, go =>
                {
                    InstantiateModelOnFace(go, face);
                });
            }
        }
    }
    private void DeleteModels(ARFace trackedFace)
    {
        foreach (Transform child in trackedFace.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void InstantiateModelOnFace(GameObject go, ARFace face)
    {
        facePrefab = Instantiate(go, face.transform);
        facePrefab.name = facePrefab.name.Replace("(Clone)", "");
        facePrefab.SetActive(true);
        facePrefab.tag = EnumModelType.FaceObject.ToString();

        facePrefab.transform.localRotation = go.transform.rotation;

        facePrefab.transform.localPosition = go.transform.position;
        Debug.Log("facePrefab Rot: " + facePrefab.transform.rotation.eulerAngles);
        Debug.Log("Face Rot: " + face.transform.rotation.eulerAngles);
        Debug.Log("go Rot: " + go.transform.rotation.eulerAngles);


    }
}
