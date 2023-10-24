using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MenuScriptVideos : MonoBehaviour
{
    public GameObject menuPanel;
    [SerializeField]
    private Slider sliderScale;
    [SerializeField]
    private Toggle toggleLooping;
    [SerializeField]
    private Text textTime;
    private readonly float rewindSeconds = 5.0f;
    private VideoPlayer videoPlayer;
    private void Update()
    {
        if (videoPlayer != null && videoPlayer.isActiveAndEnabled)
        {
            textTime.text = videoPlayer.time.ToString("#.##") + " / " + videoPlayer.length.ToString("#.##");
        }
        else
        {
            textTime.text = "0.00 / 0.00";
            GetVideoPlayer();
        }
    }

    public void ShowHideMenu()
    {
        if (menuPanel != null)
        {
            Animator animator = menuPanel.GetComponent<Animator>();
            if (animator != null)
            {
                bool isOpen = animator.GetBool("showMenu");
                animator.SetBool("showMenu", !isOpen);
            }
        }
    }

    public void PausePlay()
    {
        VideoPlayer videoPlayer = GetVideoPlayer();
        if (videoPlayer != null)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Play();
            }
        }
    }
    public void MinusTimeOnVideo()
    {
        VideoPlayer videoPlayer = GetVideoPlayer();
        if (videoPlayer != null)
        {
            videoPlayer.time -= rewindSeconds;
        }

    }

    public void PlusTimeOnVideo()
    {
        VideoPlayer videoPlayer = GetVideoPlayer();
        if (videoPlayer != null)
        {
            videoPlayer.time += rewindSeconds;
        }
    }

    public void SetLooping()
    {
        VideoPlayer videoPlayer = GetVideoPlayer();
        if (videoPlayer != null)
        {
            videoPlayer.isLooping = toggleLooping.isOn;
            Debug.Log("LOOPING: " + toggleLooping.isOn);
        }
    }

    public void SetScaleOfVideoPlayer()
    {
        GameObject videoObjectTransform = FindObjectOfType<VideoPlayer>()?.transform.GetChild(0).gameObject;
        if (videoObjectTransform != null)
        {
            videoObjectTransform.transform.localScale = new Vector3(sliderScale.value + 0.5f, sliderScale.value + 0.5f, sliderScale.value + 0.5f);
        }
        else
        {
            Debug.Log("NOT FOUND");
        }
    }

    private VideoPlayer GetVideoPlayer()
    {
        videoPlayer = FindObjectOfType<VideoPlayer>();
        return videoPlayer;
    }




}

