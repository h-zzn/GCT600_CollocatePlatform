using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CanvasDisplayManager : MonoBehaviour
{
    [Header("Assets")]
    public Texture2D staticImage;
    public RenderTexture videoRT;
    public VideoPlayer videoPlayer;

    [Header("UI")]
    public RawImage imageRaw;
    public RawImage videoRaw;

    [Header("Options")]
    public bool loopVideo = true;

    private bool isVideo = false;

    private void Awake()
    {
        if (videoPlayer != null && videoRT != null)
            videoPlayer.targetTexture = videoRT;

        if (imageRaw != null && staticImage != null)
            imageRaw.texture = staticImage;
        if (videoRaw != null && videoRT != null)
            videoRaw.texture = videoRT;

        if (videoPlayer != null) videoPlayer.isLooping = loopVideo;

        ToImageMode(resetVideo: true);
    }

    public void Toggle()
    {
        if (isVideo) ToImageMode(resetVideo: true);
        else ToVideoMode();
    }

    private void ToImageMode(bool resetVideo)
    {
        isVideo = false;
        if (imageRaw != null) imageRaw.gameObject.SetActive(true);
        if (videoRaw != null) videoRaw.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            if (resetVideo) videoPlayer.time = 0;
            videoPlayer.Pause();
        }
    }

    private void ToVideoMode()
    {
        isVideo = true;
        if (imageRaw != null) imageRaw.gameObject.SetActive(false);
        if (videoRaw != null) videoRaw.gameObject.SetActive(true);

        if (videoPlayer != null) videoPlayer.Play();
    }
}