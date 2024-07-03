using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public enum FocusPlane
{
    near,
    mid,
    far
}

public class Autofocal_Controller
{
    Defocus focusControllerL;
    Defocus focusControllerR;

    public float[] focusPlaneList = new float[] {0.3f , 1.0f , 6.0f};
    //int focusPlaneInd = 0;
    public static GameObject leftEye;
    public static GameObject rightEye;
    public static GameObject eyeCam;
    public float focusDistance;
    //public MonoBehaviour _mb;

    // attach to main camera
    public Autofocal_Controller()
    {
        //leftEye = GameObject.Find("Left Eye");
        leftEye = VrgHmd.mainCameraRig.leftGO;
        focusControllerL = leftEye.GetComponent<Defocus>();


        if (focusControllerL == null)
        {
            // add Defocus component to camera/child cameras
            focusControllerL = leftEye.AddComponent<Defocus>();
        }

        focusControllerL.defocusShader = Shader.Find("Hidden/Defocus");


        //rightEye = GameObject.Find("Right Eye");
        rightEye = VrgHmd.mainCameraRig.rightGO;
        focusControllerR = rightEye.GetComponent<Defocus>();


        if (focusControllerR == null)
        {
            // add Defocus component to camera/child cameras
            focusControllerR = rightEye.AddComponent<Defocus>();
        }

        focusControllerR.defocusShader = Shader.Find("Hidden/Defocus");
    }

    // manually set the focus distance in meters
    public void SetFocusDistance(float dist)
    {
        focusDistance = dist;
        focusControllerL.SetFocusDistance(dist);
        focusControllerR.SetFocusDistance(dist);
    }

    // set focus distance to one of the defined focus planes (near, mid, far)
    public void SetFocusDistance(FocusPlane fcspln)
    {
        SetFocusDistance(focusPlaneList[(int)fcspln]); // use the enum fcspln as an index of the focus plane distance list
    }

    public void SetFocusWithRay(Ray ray)
    {
        RaycastHit hit; 

        if (Physics.Raycast(ray, out hit, 100))
            {
                //Debug.Log(hit.transform.gameObject.name);
                SetFocusDistance(hit.distance);
                Debug.Log("Setting focus distance to " + hit.distance.ToString());
            }
    }
}