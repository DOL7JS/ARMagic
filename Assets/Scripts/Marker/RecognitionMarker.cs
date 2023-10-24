using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using Utility;
using UnityEngine.EventSystems;

public class RecognitionMarker : MonoBehaviour
{
    [SerializeField]
    public ARRaycastManager aRRaycastManager;
    [SerializeField]
    private GameObject triAxis;
    [SerializeField]
    private UtilityApp utilityApp;
    [SerializeField]
    private ARTrackedImageManager ARTrackedImageManager;

    public bool AreNewModels { get; set; }
    public List<ModelConfiguration> ConfigurationsInScene { get; set; }
    public bool DeletingObjects { get; set; }
    public Vector3 RotationOfPrefab { get; set; } = new Vector3(1, 0, 0);
    public GameObject ActualPrefab { get; set; }
    public GameObject TriAxisInstantiated { get; set; }


    private ARTrackedImage actualImage;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject prefab;

    private void Start()
    {
        ConfigurationsInScene = new List<ModelConfiguration>();
        AreNewModels = false;
        DeletingObjects = false;
    }
    public void OnEnable()
    {
        ARTrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    public void OnDisable()
    {
        ARTrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void Update()
    {
        if (IsTouchPhaseBegan() && PlayerPrefs.GetString("NameOfMarkerModel").Length != 0)
        {
            if (!IsPointerOverUIObject(Input.GetTouch(0).position))
            {
                if (aRRaycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
                {
                    GameObject prefab = ModelInstances.models.Find(item => item.name == PlayerPrefs.GetString("NameOfMarkerModel"));

                    if (prefab != null)
                    {
                        GameObject go = Instantiate(prefab, actualImage.transform);
                        go.SetActive(true);
                        go.transform.GetChild(0).gameObject.SetActive(true);
                        go.transform.GetChild(0).gameObject.tag = EnumModelType.MarkerObject.ToString();
                        go.transform.position = new Vector3(hits[0].pose.position.x,
                                                            actualImage.transform.position.y,
                                                            hits[0].pose.position.z);

                    }
                    else
                    {
                        Debug.Log("GameObject not found");
                    }
                }
            }
        }

        if (Input.touchCount != 0 && PlayerPrefs.GetString("NameOfMarkerModel").Length == 0)
        {
            if (!IsPointerOverUIObject(Input.GetTouch(0).position))
            {
                Touch touch = Input.touches[0];
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (aRRaycastManager.Raycast(touch.position, hits))
                {
                    if (IsTouchPhaseBegan() && prefab == null)
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit))
                        {
                            if (hit.collider.CompareTag(EnumModelType.MarkerObject.ToString()))
                            {
                                GameObject touchedObject = hit.collider.gameObject.transform.parent.gameObject;
                                if (DeletingObjects)
                                {
                                    Destroy(touchedObject);
                                }
                                else
                                {
                                    if (ActualPrefab != null)
                                    {
                                        utilityApp.SetColorToModel(ActualPrefab, true);
                                    }
                                    utilityApp.SetColorToModel(hit.collider.gameObject, false);
                                    ActualPrefab = touchedObject;
                                    prefab = touchedObject;
                                    prefab.GetComponent<Lean.Touch.LeanTwistRotateAxis>().Axis = RotationOfPrefab;
                                }
                            }
                            else
                            {
                                Debug.Log("Tag not found: " + hit.collider.name);
                            }
                        }
                    }

                    else if (HasTouchPhaseMoved() && prefab != null)
                    {
                        prefab.transform.position = new Vector3(hits[0].pose.position.x,
                                                            actualImage.transform.position.y,
                                                            hits[0].pose.position.z);

                    }
                    else if (HasTouchPhaseEnded())
                    {
                        prefab = null;
                    }
                }
            }
        }
    }


    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            SetModel(trackedImage);
        }
        foreach (var trackedImage in args.updated)
        {
            if (TrackingState.Tracking == trackedImage.trackingState)
            {
                SetModel(trackedImage);
            }
        }
    }

    private void SetModel(ARTrackedImage image)
    {
        actualImage = image;
        if (AreNewModels)
        {
            AreNewModels = false;
            foreach (ModelConfiguration modelConfig in ConfigurationsInScene)
            {
                GameObject model = ModelInstances.models.Find(item => item.name == modelConfig.name);

                if (model != null)
                {
                    GameObject go = Instantiate(model, actualImage.transform);
                    go.transform.GetChild(0).gameObject.tag = EnumModelType.MarkerObject.ToString();
                    go.transform.localPosition = modelConfig.position;
                    go.transform.localRotation = modelConfig.rotation;
                    go.transform.localScale = modelConfig.scale;
                    go.SetActive(true);
                    go.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    Debug.Log("Model for config NOT FOUND");
                }
            }
        }
        if (TriAxisInstantiated == null)
        {
            TriAxisInstantiated = Instantiate(triAxis, image.transform.position, image.transform.rotation, image.transform);
        }
    }

    private bool IsPointerOverUIObject(Vector2 touchPosition)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = touchPosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }
    private bool IsTouchPhaseBegan()
    {
        return Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began;
    }
    private bool HasTouchPhaseMoved()
    {
        return Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved;
    }
    private bool HasTouchPhaseEnded()
    {
        return Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended;
    }

}
