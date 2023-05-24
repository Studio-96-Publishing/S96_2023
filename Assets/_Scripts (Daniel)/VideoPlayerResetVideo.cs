using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerResetVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;
    public Material material;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        material = GetComponent<MeshRenderer>().material;
        
    }

    public void ClearRenderTexture()
    {
        RenderTexture.active = videoPlayer.targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
    }

    public void StartVideoWhenPrepared()
    {
        StartCoroutine(WaitForFirstVideoFrame());
    }

    IEnumerator WaitForFirstVideoFrame()
    {
        while (!videoPlayer.isPrepared)
            yield return new WaitForEndOfFrame();
        videoPlayer.frame = 0; //just incase it's not at the first frame
        videoPlayer.Play();
    }

    public void ReleaseTargetTexture(){
        videoPlayer.targetTexture.Release();
    }

    public void PlayVideo(){
        GL.Clear(true, true, Color.black);
        renderTexture = new RenderTexture((int)videoPlayer.clip.width, (int)videoPlayer.clip.height, 0);
        videoPlayer.targetTexture = renderTexture;
        material.mainTexture = renderTexture;
        videoPlayer.Play();
    }

    public void StopVideo(){
        Destroy(renderTexture);
        GL.Clear(true, true, Color.black);
        videoPlayer.targetTexture = null;
        videoPlayer.Stop();
    }
}
