using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;
using System.Collections;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.CoreModule;
using System.Collections.Concurrent;
using Source.ExperimentManagement.Management;

public class QueueRenderTexture : MonoBehaviour
{

    public ConcurrentQueue<Mat> _queueDepth;
    public ConcurrentQueue<Mat> _queueRGB;
    private Material depthMaterial;


    public float nearClip = 0.01f;
    public float farClip = 20f;
    private int shaderPass = 2; // which pass of the depth shader should be used
    public bool recordingStarted = false;
    private float depthLevel = 2.0f;
    private Shader _shader;
    private int saveFrequency = 3;              // frequency in frames describing how many frames we need to skip to save without lag
    private int frameCounter = 1;


    int texWidth = 0;
    int texHeight = 0;


    int texWidth2 = 0;
    int texHeight2 = 0;

    private Material _material;

    public void Awake()
    {

        texWidth = ExperimentController.resolutionX / ExperimentController.scalefactorDEPTH;
        texHeight = ExperimentController.resolutionY / ExperimentController.scalefactorDEPTH;


        texWidth2 = ExperimentController.resolutionX / ExperimentController.scalefactorRGB;
        texHeight2 = ExperimentController.resolutionY / ExperimentController.scalefactorRGB;


        _shader = Shader.Find("Custom/RenderDepth");
        _material = new Material(_shader);
        _material.hideFlags = HideFlags.HideAndDontSave;

        Camera cam = GetComponent<Camera>();
        cam.nearClipPlane = nearClip;

        cam.depthTextureMode = DepthTextureMode.Depth;
        _queueRGB = new ConcurrentQueue<Mat>();
        _queueDepth = new ConcurrentQueue<Mat>();
        depthMaterial = new Material(Shader.Find("Hidden/DepthRecording"));
        depthMaterial.SetFloat("_minDist", nearClip);
        depthMaterial.SetFloat("_farDist", farClip);

        Debug.Log("Initialized QRT");
        _material.SetFloat("_DepthLevel", depthLevel);
        _material.SetFloat("_minDist", nearClip);
        _material.SetFloat("_farDist", farClip);


    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        if (recordingStarted)
        {
            //Debug.Log("On render image called");
            if (saveFrequency % frameCounter == 1)                  // Only save every 3rd frame into queue
            {
                StartCoroutine(copyDepthTexture(source));
                StartCoroutine(copyRGBTexture(source));
                frameCounter = 1;
            }
            else
                frameCounter++;

        }
        //// Blit main Texture back
        Graphics.Blit(source, destination);

    }

    IEnumerator copyRGBTexture(RenderTexture source)
    {



        // 2

        ////////// SAVE RGB IMAGES
        RenderTexture tmp2 = RenderTexture.GetTemporary(texWidth2, texHeight2, 32, RenderTextureFormat.ARGBFloat); // GraphicsFormat.RGBA_ASTC10X10_SRGB);  //  ARGBFloat black but correct
        //RenderTexture tmp2 = RenderTexture.GetTemporary(texWidth2, texHeight2, 32, RenderTextureFormat.RGB111110Float); // GraphicsFormat.RGBA_ASTC10X10_SRGB);  //  ARGBFloat black but correct


        Graphics.Blit(source, tmp2, depthMaterial, 0); // blit into tmp
        //Wait another frame
        yield return null;


        RenderTexture lastActive = RenderTexture.active;
        RenderTexture.active = tmp2;

        //Copy the active render texture into a normal Texture2
        //Texture2D tex2 = new Texture2D(texWidth2, texHeight2, TextureFormat.RGB565, false);
        Texture2D tex2 = new Texture2D(texWidth2, texHeight2, TextureFormat.RGBAFloat, false);

        tex2.ReadPixels(new UnityEngine.Rect(0, 0, texWidth2, texHeight2), 0, 0);
        tex2.Apply();

        ////Restore the active render texture and release our temporary tex
        RenderTexture.active = lastActive;
        RenderTexture.ReleaseTemporary(tmp2);
        //Wait another frame
        yield return null;

        Mat frame2 = new Mat(tex2.height, tex2.width, CvType.CV_8UC4);
        Utils.texture2DToMat(tex2, frame2);
        _queueRGB.Enqueue(frame2);

        yield return null;
    }

    IEnumerator copyDepthTexture(RenderTexture source)
    {
        //int texWidth = Screen.width / scalefactorDEPTH;
        //int texHeight = Screen.height / scalefactorDEPTH;

        // Combinations
        // with _material
        // gelb = RenderTexture tmp = RenderTexture.GetTemporary(texWidth, texHeight, 32, RenderTextureFormat.RFloat); 
        // rot  = RenderTexture tmp = RenderTexture.GetTemporary(texWidth, texHeight, 32, RenderTextureFormat.RG32 );
        // geht nicht = RenderTexture tmp = RenderTexture.GetTemporary(texWidth, texHeight, 32, RenderTextureFormat.RGBAUShort);

        // _material
        // grau ! = RenderTexture tmp = RenderTexture.GetTemporary(texWidth, texHeight, 32, RenderTextureFormat.RGB111110Float);
        //          Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false);

        RenderTexture tmp = RenderTexture.GetTemporary(texWidth, texHeight, 64, RenderTextureFormat.RGB111110Float);
        //RenderTexture tmp = RenderTexture.GetTemporary(texWidth, texHeight, 32, RenderTextureFormat.ARGB32);

        Graphics.Blit(source, tmp, depthMaterial, 2); // blit into tmp
        //Graphics.Blit(source, tmp, _material); // blit into tmp

        //Wait until the next frame
        yield return null;

        RenderTexture lastActive = RenderTexture.active;
        RenderTexture.active = tmp;


        //Copy the active render texture into a normal Texture2
        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false); 
        //Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);

        //Debug.Log("Tex width: " + tex.width.ToString() + " tex height: " + tex.height.ToString());

        tex.ReadPixels(new UnityEngine.Rect(0, 0, texWidth, texHeight), 0, 0);
        tex.Apply();

        ////Restore the active render texture and release our temporary tex
        RenderTexture.active = lastActive;
        RenderTexture.ReleaseTemporary(tmp);
        //Wait another frame
        yield return null;

        //this._queue.Enqueue(tex);
        Mat frame = new Mat(tex.height, tex.width, CvType.CV_8UC1);
        Utils.texture2DToMat(tex, frame);
        //Debug.Log("writing frame to queue");

        _queueDepth.Enqueue(frame);


    }

}

