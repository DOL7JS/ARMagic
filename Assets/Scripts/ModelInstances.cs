using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelInstances : MonoBehaviour
{
    public static List<GameObject> models = new List<GameObject>();

    public static GameObject AddModel(GameObject go, string name)
    {
        go.tag = "Untagged";
        go.name = name;
        go.SetActive(false);

        GameObject parent = new GameObject();
        parent.name = name;
        parent.tag = "Untagged";
        parent.SetActive(false);
        go.transform.SetParent(parent.transform);
        BuildBoxCollider(go);
        DontDestroyOnLoad(parent);
        
        Lean.Touch.LeanPinchScale leanPinchScale = parent.AddComponent<Lean.Touch.LeanPinchScale>();
        leanPinchScale.Use.UpdateRequiredSelectable(parent);
        Lean.Touch.LeanTwistRotateAxis leanTwistRotateAxis = parent.AddComponent<Lean.Touch.LeanTwistRotateAxis>();
        leanTwistRotateAxis.Use.UpdateRequiredSelectable(parent);
        leanTwistRotateAxis.Axis = new Vector3(0, 1, 0);
        Lean.Touch.LeanSelectableByFinger leanSelectableByFinger = parent.AddComponent<Lean.Touch.LeanSelectableByFinger>();
        models.Add(parent);
        return parent;
    }
    public static GameObject AddFaceModel(GameObject go, string name)
    {
        go.tag = "Untagged";
        go.name = name;
        go.SetActive(false);
        DontDestroyOnLoad(go);
        models.Add(go);
        return go;
    }
    private static void BuildBoxCollider(GameObject go)
    {
        Quaternion goRotation = go.transform.rotation;
        Vector3 goPostition = go.transform.position;
        Vector3 goScale = go.transform.localScale;

        go.transform.rotation = Quaternion.Euler(Vector3.zero);
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one;
        BoxCollider boxCollider = go.AddComponent<BoxCollider>();

        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderers.Length > 0)
        {
            CombineInstance[] combine = new CombineInstance[skinnedMeshRenderers.Length];
            int countVertices = 0;
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                combine[i].mesh = skinnedMeshRenderers[i].sharedMesh;
                combine[i].transform = skinnedMeshRenderers[i].transform.localToWorldMatrix;
                countVertices += skinnedMeshRenderers[i].sharedMesh.vertices.Length;
            }

            Mesh mesh = new Mesh();
            if (countVertices > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.CombineMeshes(combine);
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (go.GetComponent<MeshFilter>() == null)
            {
                meshFilter = go.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = skinnedMeshRenderers[0].sharedMaterials;

            boxCollider.center = mesh.bounds.center;
            boxCollider.size = mesh.bounds.size;

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                DestroyImmediate(skinnedMeshRenderers[i]);
            }
        }
        else
        {
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int countVertices = 0;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                countVertices += meshFilters[i].mesh.vertexCount;
            }

            Mesh mesh = new Mesh();
            if (countVertices > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.CombineMeshes(combine);
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (go.GetComponent<MeshFilter>() == null)
            {
                meshFilter = go.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = mesh;

            boxCollider.center = go.transform.InverseTransformPoint(mesh.bounds.center);
            boxCollider.size = new Vector3(mesh.bounds.size.x / go.transform.localScale.x,
                mesh.bounds.size.y / go.transform.localScale.y,
                mesh.bounds.size.z / go.transform.localScale.z);

        }
        go.transform.position = goPostition;
        go.transform.rotation = goRotation;
        go.transform.localScale = goScale;
    }
}
