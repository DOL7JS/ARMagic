using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Video;
using UnityEngine.UI;

namespace Kakera
{
    public class PickerController : MonoBehaviour
    {
        [SerializeField]
        private Unimgpicker imagePicker;

        [SerializeField]
        private MeshRenderer imageRenderer;

        [SerializeField]
        private VideoPlayer myVideoPlayer;

        [SerializeField]
        private ARTrackedImageManager m_TrackedImageManager;
        private VideoPlayer newVideoPlayer;
        private int[] sizes = { 1024, 256, 16 };

        void Awake()
        {

            //imagePicker.Completed += (string path) =>
            //{ StartCoroutine(LoadImage(path/*, imageRenderer*/)); };
            

            Debug.Log("PickerController - awake");
        }
        public void OnPressPlayVideo1()
        {
            if (myVideoPlayer.isPlaying) {
                myVideoPlayer.Stop();
                Debug.Log("Stopped2");

            }
            //newVideoPlayer = GetComponent<myVideoPlayer>();
            myVideoPlayer.source = VideoSource.Url;
            myVideoPlayer.url = "http://192.168.107.145:8000/unity/video_1.mp4";
            myVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            

            myVideoPlayer.controlledAudioTrackCount = 1;
            myVideoPlayer.EnableAudioTrack(0, false);
            Debug.Log("1");
            //myVideoPlayer.Prepare();
            myVideoPlayer.Play();
        }

        public void OnPressPlayVideo2()
        {
            if (myVideoPlayer.isPlaying)
            {
                myVideoPlayer.Stop();
                Debug.Log("Stopped2");
            }
            myVideoPlayer.source = VideoSource.Url;
            myVideoPlayer.url = "http://192.168.107.145:8000/unity/video_2.mp4";
            myVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            

            myVideoPlayer.controlledAudioTrackCount = 1;
            myVideoPlayer.EnableAudioTrack(0, true);
            Debug.Log("1");
            myVideoPlayer.Play();

        }
       
        public void OnPressShowImagePicker()
        {
            //imagePicker.Show("Select Image", "unimgpicker");

            // Use MIMEs on Android
            string[] fileTypes = new string[] { "image/*" };

            // Pick image(s) and/or video(s)
            NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
            {
                if (path == null)
                    Debug.Log("Operation cancelled");
                else
                {
                    Debug.Log("Picked file: " + path);
                    PlayerPrefs.SetString("PathToImage", path);
                    //StartCoroutine(LoadImage(path/*, imageRenderer*/));
                }
            }, fileTypes);

            Debug.Log("Permission result: " + permission);
        }
        public void OnPressShowVideoPicker()
        {
            string[] fileTypes = new string[] { "image/*" };

            // Pick image(s) and/or video(s)
            NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
            {
                if (path == null)
                    Debug.Log("Operation cancelled");
                else
                    Debug.Log("Picked file: " + path);
                //path = "file://" + path;
                Debug.Log("PATH: " + path);
                //StartCoroutine(this.playVideoInThisURL(path));
                StartCoroutine(UploadFileData(path));
            }, fileTypes);
           

            /*// Use MIMEs on Android
          

            Debug.Log("Permission result: " + permission);
        
            myVideoPlayer.url = "http://192.168.107.145:8000/unity/video_1.mp4";
            myVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            myVideoPlayer.EnableAudioTrack(0, true);
            myVideoPlayer.Prepare();*/
        }
        IEnumerator UploadFileData(string path)
        {
            Uri siteUri = new Uri("http://192.168.107.145:8000/unity/");
            using (var uwr = new UnityWebRequest(siteUri, UnityWebRequest.kHttpVerbPOST))
            {
                Debug.Log("1");
                uwr.uploadHandler = new UploadHandlerFile(path);
                Debug.Log("2");
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success) { 
                    Debug.Log("ERROR");
                    Debug.LogError(uwr.error);
                }   
                else
                {
                    // file data successfully sent
                    Debug.Log("SENT");
                }
            }
        }
        private IEnumerator playVideoInThisURL(string _url)
        {
            string s = "file://" + _url;
            Debug.Log("URL: " + s);
            Debug.Log("Application.streamingAssetsPath: " + Application.streamingAssetsPath); 
            myVideoPlayer.source = VideoSource.Url;
            myVideoPlayer.url = s;


            yield return null;

            /*myVideoPlayer.Prepare();

            while (myVideoPlayer.isPrepared == false)
            {
                yield return null;
            }
            myVideoPlayer.Play();
        */
        }

        private IEnumerator LoadImage(string path/*, MeshRenderer output*/)
        {
            Debug.Log("LoadImage");
            var url = "file:///" + path;
            var unityWebRequestTexture = UnityWebRequestTexture.GetTexture(url);
            yield return unityWebRequestTexture.SendWebRequest();

            var texture = ((DownloadHandlerTexture)unityWebRequestTexture.downloadHandler).texture;
            if (texture == null)
            {
                Debug.LogError("Failed to load texture url:" + url);
            }
            Debug.Log("ADD");
            if (m_TrackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
            {
                Debug.Log("ReferenceImage Added.");
                var jobState = mutableLibrary.ScheduleAddImageWithValidationJob(texture, "keypad", 0.07f);
                Debug.Log("Image Library Count : " + mutableLibrary.count + jobState.status + jobState.ToString());
            }
            Debug.Log("ADDED");
            //output.material.mainTexture = texture;
        }
    }
}