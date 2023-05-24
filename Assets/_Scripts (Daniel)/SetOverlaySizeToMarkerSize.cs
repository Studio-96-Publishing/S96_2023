using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Vuforia;

public class SetOverlaySizeToMarkerSize : MonoBehaviour
{
    //VideoPlayer videoPlayer;
    VideoAspectRatio videoAspectRatio;
    RenderTexture renderTexture;
    Material material;
    ImageTargetBehaviour imageTargetBehaviour;
    public Vector2 imageTargetSize;
    public Vector2 imageScaleModifier;
    public bool cropMatchToHeight = false;

    
    // Start is called before the first frame update
    void Awake()
    {
        //videoPlayer = GetComponent<VideoPlayer>();
        material = GetComponent<MeshRenderer>().material;
    }

    void Start(){
        Crop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetSizeOfImageTarget(){
        imageTargetBehaviour = transform.GetComponentInParent<ImageTargetBehaviour>();
        imageTargetSize = imageTargetBehaviour.GetSize()/10;

        imageTargetSize.x += imageScaleModifier.x;
        imageTargetSize.y += imageScaleModifier.y;
    }

    public void Crop(){
        GetSizeOfImageTarget();

        if(cropMatchToHeight)
            CropMatchHeight();
        else CropMatchWidth();
    }

    [ContextMenu("CropVideo")]
    public void CropMatchHeight()
    {
        //Get scaling coefficient that would match current transform height to image height.
        // scale.z * x = imageTargetSize.y;
        float scaleCoefficientZ = imageTargetSize.y / transform.localScale.z;
        
        //scale video to image height and width.
        transform.localScale = new Vector3(imageTargetSize.x, 0,
        imageTargetSize.y);

        float originalTilingWidth = 1;
        float newTileWidth = 1/ scaleCoefficientZ;
        
        
        //multiply current material tiling by scaling coefficient
        material.mainTextureScale = new Vector2(newTileWidth, 1);

        float widthDifference = originalTilingWidth - newTileWidth;

        // set material maintexturescale offset.x to
        material.mainTextureOffset = new Vector2(widthDifference/2, 0);

        
    }

    public void CropMatchWidth()
    {
        //Get scaling coefficient that would match current transform height to image height.
        // scale.x * x = imageTargetSize.x;
        float scaleCoefficientX = imageTargetSize.x / transform.localScale.x;
        
        //scale object to image height and width.
        transform.localScale = new Vector3(imageTargetSize.x, 0,
        imageTargetSize.y);

        float originalTilingHeight = 1;
        float newTileHeight = 1/ scaleCoefficientX;
        
        
        //multiply current material tiling by scaling coefficient
        material.mainTextureScale = new Vector2(1, newTileHeight);

        float heightDifference = originalTilingHeight - newTileHeight;

        // set material maintexturescale offset.x to
        material.mainTextureOffset = new Vector2(0, heightDifference/2);

        
    }
}
