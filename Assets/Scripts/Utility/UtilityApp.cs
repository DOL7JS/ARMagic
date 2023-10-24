using EasyUI.Toast;
using Siccity.GLTFUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Utility;
namespace Utility
{
    public class UtilityApp : MonoBehaviour
    {
        [SerializeField]
        private UtilityCache utilityCache;
        [SerializeField]
        private UtilityDownloader utilityDownloader;
        [SerializeField]
        private UtilityUploader utilityUploader;
        [SerializeField]
        private GameObject contentItemPrefab;

        public bool IsFileCached(string folder, string name)
        {
            return utilityCache.IsFileCached(folder, name);
        }
        public bool IsFileOnServer(string folder, string name)
        {
            return utilityDownloader.IsFileOnServer(folder, name);
        }
        public void GetTexture(string path, Action<Texture> callback)
        {
            StartCoroutine(utilityCache.LoadImage(path, callback));
        }

        public IEnumerator CacheAllFilesFromServer()
        {
            //models
            yield return DownloadAndCacheModelsFromServer(EnumFolders.area_objects.ToString());
            yield return DownloadAndCacheModelsFromServer(EnumFolders.marker_objects.ToString());
            yield return DownloadAndCacheModelsFromServer(EnumFolders.face_objects.ToString());

            //configurations
            yield return DownloadAndCacheFilesFromServer(EnumFolders.marker_objects_configurations.ToString());
            yield return DownloadAndCacheFilesFromServer(EnumFolders.face_configurations.ToString());

            //videos
            yield return DownloadAndCacheFilesFromServer(EnumFolders.videos.ToString());
        }

        private IEnumerator DownloadAndCacheFilesFromServer(string folder)
        {
            utilityCache.SetCache(folder);
            List<string> modelNames = utilityDownloader.GetItemsInFolderFromServer(folder);
            foreach (string name in modelNames)
            {
                yield return DownloadAndCacheFile(folder, name);
            }
        }

        private IEnumerator DownloadAndCacheModelsFromServer(string folder)
        {
            utilityCache.SetCache(folder);
            List<string> modelNames = utilityDownloader.GetItemsInFolderFromServer(folder);
            foreach (string name in modelNames)
            {
                yield return DownloadAndCacheModel(folder, name);
            }
        }
        public void RemoveModelsByTag(string tag)
        {

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject item in gameObjects)
            {
                Destroy(item);
            }
        }
        public IEnumerator DownloadAndCacheFile(string folder, string name)
        {
            utilityCache.SetCache(folder);
            yield return StartCoroutine(utilityDownloader.DownloadFile(folder, name, downCallback =>
            {
                utilityCache.CacheFile(folder, name, downCallback);
            }));
        }
        public IEnumerator DownloadAndCacheModel(string folder, string name, Action<GameObject> callback = null)
        {
            yield return StartCoroutine(utilityDownloader.DownloadFile(folder, name, downCallback =>
            {
                if (downCallback != null)
                {
                    utilityCache.CacheFile(folder, name, downCallback);
                    if (callback != null)
                    {

                        utilityCache.GetModelFromCache(folder, name, go =>
                        {
                            callback(go);
                        });
                    }
                }
                else
                {
                    callback?.Invoke(null);
                }

            }));
        }
        public void GetModel(string folder, string name, Action<GameObject> callback)
        {
            utilityCache.SetCache(folder);
            if (utilityCache.IsFileCached(folder, name))
            {
                //cache
                utilityCache.GetModelFromCache(folder, name, go =>
                {
                    callback(go);
                });
            }
            else
            {
                //download
                utilityDownloader.IsServerAvailable(isAvailable =>
                {
                    if (isAvailable)
                    {
                        StartCoroutine(DownloadAndCacheModel(folder, name, go =>
                        {
                            callback(go);
                        }));
                    }
                    else
                    {
                        callback(null);
                    }
                });

            }
        }
        public void UploadConfiguration(string tag, string folder, string nameOfConfiguration, Action<bool> callback)
        {
            GameObject[] models = GameObject.FindGameObjectsWithTag(tag);
            if (models.Length == 0)
            {
                Toast.Show("No models in scene.", ToastColor.Red);
                callback(false);
                return;
            }
            utilityDownloader.IsServerAvailable(isAvailable =>
            {
                if (isAvailable)
                {
                    utilityUploader.UploadConfiguration(models, folder, nameOfConfiguration, tag);
                    callback(true);
                }
                else
                {
                    List<ModelConfiguration> configurations = new List<ModelConfiguration>();
                    if (tag == EnumModelType.MarkerObject.ToString())
                    {
                        Debug.Log("Uploading Markers");
                        foreach (GameObject obj in models)
                        {
                            configurations.Add(new ModelConfiguration(obj.name.Replace("(Clone)", ""), obj.transform.parent.gameObject.transform.localPosition, obj.transform.parent.gameObject.transform.localRotation, obj.transform.parent.gameObject.transform.localScale));
                        }
                    }
                    else
                    {
                        Debug.Log("Uploading faces");
                        foreach (GameObject obj in models)
                        {
                            configurations.Add(new ModelConfiguration(obj.name.Replace("(Clone)", ""), obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale));
                        }
                    }

                    string mods = JsonHelper.ToJson<ModelConfiguration>(configurations);
                    utilityCache.SetCache(folder);
                    File.WriteAllText(Path.Combine(Application.persistentDataPath, folder, nameOfConfiguration + ".json"), mods);
                    Toast.Show("Server is not available. Saved to cache.");
                    callback(true);
                }
            });

        }

        public void ClearButtonsColor(Transform content)
        {
            Color32 basicColor = new Color32(255, 255, 255, 255);
            foreach (Transform child in content.GetComponentInChildren<Transform>())
            {
                child.GetComponentInChildren<Image>().color = basicColor;
            }
        }
        public bool IsColorsEquals(Color32 color1, Color32 color2)
        {
            return color1.r == color2.r && color1.g == color2.g && color1.b == color2.b;
        }

        public void SetItemsForScrollView(string folder, Transform content, Action<string> onClickMethod, List<ImageVideoContainer> imageVideoContainers = null, bool stayClicked = false, List<string> names = null)
        {
            List<string> namesFromCache = utilityCache.GetItemsInFolderFromCache(folder);
            List<string> namesFromServer = new List<string>();

            utilityDownloader.IsServerAvailable(isAvailable =>
            {
                if (isAvailable)
                {
                    namesFromServer = utilityDownloader.GetItemsInFolderFromServer(folder);
                }
                List<string> list = namesFromServer.Union(namesFromCache).ToList();
                list.Sort();
                if (imageVideoContainers != null)
                {
                    List<string> listIntersect = list.Intersect(imageVideoContainers.Select(item => Path.GetFileName(item.VideoPath))).ToList();
                    foreach (string name in listIntersect)
                    {
                        list.Remove(name);
                    }
                }

                foreach (string name in list)
                {
                    names?.Add(name);
                    AddButtonToScrollView(name, onClickMethod, content, stayClicked);
                }
            });

        }

        public void SetAllItemsForScrollView(Transform content, Action<string> onClickMethod)
        {
            List<string> namesFromCache = utilityCache.GetItemsInFolderFromCache(EnumFolders.area_objects.ToString()).Select(item => EnumFolders.area_objects.ToString() + "/" + item).ToList();
            namesFromCache.AddRange(utilityCache.GetItemsInFolderFromCache(EnumFolders.marker_objects.ToString()).Select(item => EnumFolders.marker_objects.ToString() + "/" + item).ToList());
            namesFromCache.AddRange(utilityCache.GetItemsInFolderFromCache(EnumFolders.face_objects.ToString()).Select(item => EnumFolders.face_objects.ToString() + "/" + item).ToList());

            List<string> namesFromServer = new List<string>();

            utilityDownloader.IsServerAvailable(isAvailable =>
            {
                if (isAvailable)
                {
                    namesFromServer.AddRange(utilityDownloader.GetItemsInFolderFromServer(EnumFolders.area_objects.ToString()).Select(item => EnumFolders.area_objects.ToString() + "/" + item).ToList());
                    namesFromServer.AddRange(utilityDownloader.GetItemsInFolderFromServer(EnumFolders.marker_objects.ToString()).Select(item => EnumFolders.marker_objects.ToString() + "/" + item).ToList());
                    namesFromServer.AddRange(utilityDownloader.GetItemsInFolderFromServer(EnumFolders.face_objects.ToString()).Select(item => EnumFolders.face_objects.ToString() + "/" + item).ToList());
                }
                List<string> list = namesFromServer.Union(namesFromCache).ToList();
                list.Sort();

                foreach (string name in list)
                {
                    AddButtonToScrollView(name, onClickMethod, content, false);
                }
            });

        }
        public void AddButtonToScrollView(string name, Action<string> method, Transform content, bool stayClicked)
        {
            var item_go = Instantiate(contentItemPrefab);
            var childButton = item_go.transform.GetChild(0).gameObject;
            childButton.GetComponentInChildren<Text>().text = name;

            childButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Color32 color = childButton.GetComponent<Image>().color;

                if (!stayClicked)
                {
                    ClearButtonsColor(content);
                }
                if (!IsColorsEquals(color, new Color32(200, 200, 200, 255)))
                {
                    childButton.GetComponent<Image>().color = new Color32(200, 200, 200, 255);
                }
                else
                {
                    childButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                }
                method(name);
            });
            item_go.transform.SetParent(content.transform);
            item_go.transform.localPosition = new Vector3(0, 0, 0);
            item_go.transform.localScale = Vector3.one;
        }
        public void SortScrollView(Transform content)
        {
            List<GameObject> rowPrefabs = new List<GameObject>();
            foreach (Transform child in content.transform)
            {
                rowPrefabs.Add(child.gameObject);
            }
            content.DetachChildren();
            rowPrefabs.Sort((b1, b2) => b1.transform.GetComponentInChildren<Text>().text.CompareTo(b2.transform.GetComponentInChildren<Text>().text));
            foreach (GameObject rowPrefab in rowPrefabs)
            {
                rowPrefab.transform.SetParent(content.transform);
            }
        }
        public void GetConfiguration(string name, string folder, Action<List<ModelConfiguration>> callback)
        {
            if (utilityCache.IsFileCached(folder, name))
            {
                string json = Encoding.ASCII.GetString(File.ReadAllBytes(Path.Combine(Application.persistentDataPath, folder, name)));
                List<ModelConfiguration> mods = JsonHelper.FromJson<ModelConfiguration>(json);
                callback(mods);
            }
            else
            {
                StartCoroutine(utilityDownloader.DownloadConfiguration(name, folder, wwwCallback =>
                {
                    if (wwwCallback == null)
                    {
                        callback(null);
                        return;
                    }
                    List<ModelConfiguration> mods = JsonHelper.FromJson<ModelConfiguration>(wwwCallback.downloadHandler.text);
                    utilityCache.SetCache(folder);
                    File.WriteAllBytes(Path.Combine(Application.persistentDataPath, folder, name), wwwCallback.downloadHandler.data);
                    callback(mods);
                }));
            }
        }

        public void ChooseImage(Action<Texture2D> callback)
        {
            string[] fileTypes = new string[] { "image/*" };
            NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
            {
                if (path == null)
                {
                    Debug.Log("Operation cancelled");
                    callback(null);
                }
                else
                {
                    Debug.Log("Picked file: " + path);
                    GetTexture(path, texture =>
                    {
                        if (texture == null)
                        {
                            Debug.LogError("Failed to load texture url:" + path);
                            callback(null);
                        }
                        else
                        {
                            Texture2D data = texture as Texture2D;
                            data.name = path;
                            callback(data);
                        }
                    });
                }
            }, fileTypes);

        }

        public void SetColorToModel(GameObject go, bool brighter)
        {
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (brighter)
                {
                    meshRenderer.material.color = meshRenderer.material.color * 5f;
                }
                else
                {
                    meshRenderer.material.color = meshRenderer.material.color * 0.2f;
                }
            }
        }

    }
}