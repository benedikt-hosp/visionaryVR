using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetControl : MonoBehaviour
{
    public float size = 0.1f;
    public float default_distance = 0.25f;
    
    void Start()
    {
        SetPosition(0, 0, default_distance);

    }


    public void SetPosition(float ecc, float meridian, float distance)
    // Set position of target to eccentricity 'ecc' in degree, meridian in degree and distance in m
    {
        float theta = Mathf.Deg2Rad * ecc;
        float phi = Mathf.Deg2Rad * meridian;
        transform.localPosition = distance * new Vector3( Mathf.Cos(phi) * Mathf.Sin(theta), // x-component
                                                          Mathf.Sin(phi) * Mathf.Sin(theta), // y-component
                                                          Mathf.Cos(theta)); // z-component
        transform.localScale = size * distance * new Vector3(1.0f,1.0f,1.0f);
    }
}
