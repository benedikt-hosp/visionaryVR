using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegressionController : MonoBehaviour
{
    DepthModel depthModel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            depthModel = new DepthModel("depthCalibration.csv");
        }
    }

    private void OnDisable()
    {
        depthModel.Dispose();
    }

}
