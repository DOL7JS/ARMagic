using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utility;

public class ARAreaScript : MonoBehaviour
{

    [SerializeField]
    private ARRaycastManager aRRaycastManager;
    [SerializeField]
    private UtilityApp utilityApp;
    [SerializeField]
    private ARSession aRSession;
    [SerializeField]
    private GameObject triAxis;
    public GameObject prefab;
    public Camera arCam;
    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public bool TriAxisToggle { get; set; } = false;
    public bool DeletingObjects { get; set; }
    public Vector3 RotationOfPrefab { get; set; } = new Vector3(1, 0, 0); //rotation around x, y or z
    public GameObject ActualPrefab { get; set; }
    public GameObject TriAxisInstantiated { get; set; }


    void Update()
    {


        if (IsTouchPhaseBegan() && IsModelNameSet())
        {
            if (!IsPointerOverUIObject(Input.GetTouch(0).position))
            {
                if (aRRaycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinBounds))
                {
                    if (TriAxisToggle && TriAxisInstantiated == null)
                    {
                        TriAxisInstantiated = Instantiate(triAxis);
                        TriAxisInstantiated.transform.position = hits[0].pose.position;
                        TriAxisInstantiated.transform.localScale = new Vector3(10, 10, 10);
                        TriAxisInstantiated.tag = "TriAxisObject";
                        Lean.Touch.LeanSelectableByFinger leanSelectableByFinger = TriAxisInstantiated.AddComponent<Lean.Touch.LeanSelectableByFinger>();
                        Lean.Touch.LeanPinchScale leanPinchScale = TriAxisInstantiated.AddComponent<Lean.Touch.LeanPinchScale>();
                        leanPinchScale.Use.RequiredSelectable = leanSelectableByFinger;
                        PlayerPrefs.SetString("NameOfAreaModel", "");
                    }
                    else
                    {
                        GameObject prefab = ModelInstances.models.Find(item => item.name == PlayerPrefs.GetString("NameOfAreaModel"));
                        if (prefab != null)
                        {
                            GameObject go = Instantiate(prefab);
                            go.SetActive(true);
                            go.transform.GetChild(0).gameObject.SetActive(true);
                            go.transform.GetChild(0).gameObject.tag = EnumModelType.AreaObject.ToString();
                            go.transform.position = hits[0].pose.position;
                        }
                        else
                        {
                            Debug.Log("GO NOT FOUND !!!");
                        }
                    }

                }
            }
        }

        if (IsScreenTouchedAndModelNameNotSet())
        {
            if (!IsPointerOverUIObject(Input.GetTouch(0).position))
            {
                Touch touch = Input.touches[0];
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (aRRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    if (IsTouchPhaseBegan() && prefab == null)
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit))
                        {
                            if (hit.collider.CompareTag(EnumModelType.AreaObject.ToString()))
                            {
                                if (DeletingObjects)
                                {
                                    Destroy(hit.collider.gameObject);
                                }
                                else
                                {
                                    prefab = hit.collider.gameObject.transform.parent.gameObject;
                                    if (ActualPrefab != null)
                                    {
                                        utilityApp.SetColorToModel(ActualPrefab, true);
                                    }
                                    utilityApp.SetColorToModel(hit.collider.gameObject, false);
                                    ActualPrefab = hit.collider.gameObject.transform.parent.gameObject;
                                    prefab.GetComponent<Lean.Touch.LeanTwistRotateAxis>().Axis = RotationOfPrefab;
                                }
                            }
                            else if (hit.collider.CompareTag("TriAxisObject"))
                            {
                                prefab = hit.collider.gameObject;
                            }
                            else
                            {
                                Debug.Log("TAG NOT FOUND: " + hit.collider.gameObject.name);
                            }
                        }
                    }
                    else if (HasTouchPhaseMoved() && prefab != null)
                    {
                        prefab.transform.position = hits[0].pose.position;
                    }
                    else if (HasTouchPhaseEnded())
                    {
                        prefab = null;
                    }
                }
            }
        }
    }
    private bool IsModelNameSet()
    {
        return PlayerPrefs.GetString("NameOfAreaModel").Length != 0;
    }
    private bool IsScreenTouchedAndModelNameNotSet()
    {
        return Input.touchCount != 0 && !IsModelNameSet();
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

    private bool IsPointerOverUIObject(Vector2 touchPosition)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = touchPosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }

}
