using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Defocus : MonoBehaviour
{
    public Shader defocusShader;
    public bool usePostPass = true;
    public bool showDepth = false;
    public bool showDistance = false;
    [Range(0.0f, 10.0f)]
    public float targetOpticalPower = 0.0f;
    public float opticalPower = 0.0f;
    public float powerChangePerSec = 8.0f;
    public float pupilSize = 5.0f;
    [Range(1f, 30f)]
    public float bokehRadius = 13.0f;
    [Range(1f, 30f)]
    public float cocConstant = 2.0f;
    public int downscaleFactor = 2;
    [NonSerialized] // TODO: what is non serialized
    Material defocusMaterial;
    const int cocPass = 0;
    const int preFilterPass = 1;
    const int blurPass = 2;
    const int postFilterPass = 3;
    const int combinePass = 4;
    const int depthPass = 5;
    const int distancePass = 6;



    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        if (defocusMaterial == null)
        {

            defocusMaterial = new Material(defocusShader);
            defocusMaterial.hideFlags = HideFlags.HideAndDontSave; // TODO: doing what here?
        }

        cocConstant = 0.057f * pupilSize * Mathf.Deg2Rad; // coc diameter in radians
        cocConstant = Mathf.Tan(cocConstant / 2) / Mathf.Tan(GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad / 2) * GetComponent<Camera>().pixelHeight;


        RenderTexture coc = RenderTexture.GetTemporary(
            source.width, source.height, 0,
            RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear
            );
        // create two twmporary (downscaled) textures
        int downscaledWidth = source.width / downscaleFactor;
        int downscaledHeight = source.height / downscaleFactor;
        RenderTexture tmp_tex1 = RenderTexture.GetTemporary(downscaledWidth, downscaledHeight, 0, source.format);
        RenderTexture tmp_tex2 = RenderTexture.GetTemporary(downscaledWidth, downscaledHeight, 0, source.format);

        defocusMaterial.SetTexture("_defocusTexture", coc);
        defocusMaterial.SetTexture("_blurredTex", tmp_tex1);

        defocusMaterial.SetFloat("_OpticalPower", opticalPower);
        defocusMaterial.SetFloat("_CocConstant", cocConstant);
        defocusMaterial.SetFloat("_BokehRadius", bokehRadius);
        defocusMaterial.SetInt("_downscaleFactor", downscaleFactor);
        if (showDepth)
        {
            showDistance = false;
            Graphics.Blit(source, destination, defocusMaterial, depthPass); // write defocus map into coc    
        }
        else if (showDistance)
        {
            showDepth = false;
            Graphics.Blit(source, destination, defocusMaterial, distancePass); // write defocus map into coc    
        }
        else
        {
            //Graphics.Blit(source, tmp_tex1); // create downscaled version of source
            Graphics.Blit(source, coc, defocusMaterial, cocPass); // write defocus map into coc
            Graphics.Blit(source, tmp_tex1, defocusMaterial, preFilterPass); // downsampling of source and coc (coc with custom downsampling)
            Graphics.Blit(tmp_tex1, tmp_tex2, defocusMaterial, blurPass);
            if (usePostPass)
            {
                Graphics.Blit(tmp_tex2, tmp_tex1, defocusMaterial, postFilterPass);

            }
            else
            {
                Graphics.Blit(tmp_tex2, tmp_tex1);

            }
            Graphics.Blit(source, destination, defocusMaterial, combinePass);
            //Graphics.Blit(tmp_tex1, destination);
        }
        RenderTexture.ReleaseTemporary(coc);
        RenderTexture.ReleaseTemporary(tmp_tex1);
        RenderTexture.ReleaseTemporary(tmp_tex2);


        //TODO: we assume that we're rendering in linear HDR space, so configure the project and camera accordingly
        //if (saveTex)
        //{
        //   Debug.Log("Saving texture");
        //   Debug.Log(source.width);
        //   SaveTextureAsPNG(source,"./test.png");
        //   saveTex = false;
        //}
    }

    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        //Debug.Log(GetComponent<Camera>().depthTextureMode);
        opticalPower = targetOpticalPower; // set the lens current power to the target power
    }

    // Update is called once per frame
    void Update()
    {
        // check if current power is different from target power
        if (targetOpticalPower > opticalPower)
        {
            opticalPower += powerChangePerSec * Time.deltaTime;
            if (opticalPower > targetOpticalPower)
            {
                opticalPower = targetOpticalPower;
            }
        }

        if (targetOpticalPower < opticalPower)
        {
            opticalPower -= powerChangePerSec * Time.deltaTime;
            if (opticalPower < targetOpticalPower)
            {
                opticalPower = targetOpticalPower;
            }
        }
    }

    public void SetFocusDistance(float focusDistance)
    {
        targetOpticalPower = 1.0f / focusDistance;
    }
}