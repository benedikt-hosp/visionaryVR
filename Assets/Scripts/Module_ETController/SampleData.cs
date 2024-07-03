using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
public class SampleData
{
    public float vergenceAngle_R;
    public float vergenceAngle_L;
    internal long deviceTimestamp { get; set; }

    public long timeStamp { get; set; }
    public bool isValid { get; set; }
    public bool exclude { get; set; }
    public float targetId { get; set; }

    public double ipd { get; set; }

    public Eye eye { get; set; }

    public Vector3 cameraPosition { get; set; }
    public Vector3 localGazeOrigin { get; set; }
    public Vector3 localGazeDirection { get; set; }
    public Vector3 worldGazeOrigin { get; set; }
    public Vector3 worldGazeDirection { get; set; }
    public Vector3 worldGazeOrigin_R { get; set; }
    public Vector3 worldGazeOrigin_L { get; set; }
    public Vector3 worldGazeDirection_R { get; set; }
    public Vector3 worldGazeDirection_L { get; set; }
    public float vergenceDepth { get; set; }
    public Vector3 worldGazePoint { get; set; }
    public float OffsetAngle { get; set; }
    public float interSampleAngle { get; set; }

    public bool isBlink { get; set; }

    public override string ToString()
    {
        return this.timeStamp.ToString() + " " + isValid.ToString() + " " + worldGazeOrigin.ToString() + " " + worldGazeDirection.ToString() + " " + worldGazePoint.ToString();
    }

    internal bool checkValidity()
    {
        if (!float.IsNaN(this.timeStamp) && !float.IsNaN(this.worldGazeOrigin.x) && !float.IsNaN(this.worldGazeOrigin.y) && !float.IsNaN(this.worldGazeOrigin.z) && !float.IsNaN(this.worldGazeDirection.x) && !float.IsNaN(this.worldGazeDirection.y) && !float.IsNaN(this.worldGazeDirection.z) && !float.IsNaN(this.vergenceDepth))
            this.isValid = true;
        else
            this.isValid = false;


        return this.isValid;


    }
}