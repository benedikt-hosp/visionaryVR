using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using ViveSR.anipal.Eye;
public class SampleData
{
    
    
    public float vergenceAngle_R;
    public float vergenceAngle_L;

    /*
    internal long deviceTimestamp { get; set; }
    public long timeStamp { get; set; }
    public bool isValid { get; set; }
    public bool exclude { get; set; }
    public float targetId { get; set; }
    public double ipd { get; set; }
    public Eye eye { get; set; }
    
    public Vector3 localGazeOriginCombined { get; set; }
    public Vector3 localGazeDirectionCombined { get; set; }
    public Vector3 worldGazeOrigin { get; set; }
    public Vector3 worldGazeDirection { get; set; }
    public Vector3 worldGazeOrigin_R { get; set; }
    public Vector3 worldGazeOrigin_L { get; set; }
    public Vector3 worldGazeDirection_R { get; set; }
    public Vector3 worldGazeDirection_L { get; set; }
    public Vector3 localGazeOrigin_R { get; set; }
    public Vector3 localGazeOrigin_L { get; set; }
    public Vector3 localGazeDirection_R { get; set; }
    public Vector3 localGazeDirection_L { get; set; }
    public float vergenceDepth { get; set; }
    public Vector3 worldGazePoint { get; set; }
    public float OffsetAngle { get; set; }
    public float interSampleAngle { get; set; }
    public bool isBlink { get; set; }
    */

    public Vector3 cameraPosition { get; set; }

    internal long deviceTimestamp { get; set; }
    public long systemTimeStamp { get; set; }
    public int frameSequence { get; set; }

    // Left Eye Data
    public bool leftEyeIsValid { get; set; }
    public Vector3 leftEyeValidityVector { get; set; }

    public Vector3 leftEyeLocalOrigin { get; set; }
    public Vector3 leftEyeWorldOrigin { get; set; }
    public Vector3 leftEyeLocalDirection { get; set; }
    public Vector3 leftEyeWorldDirection { get; set; }
    public float leftEyeOpenness { get; set; }
    public float leftEyePupilDiameter { get; set; }
    public float leftEyeWide { get; set; }
    public float leftEyeSqueeze { get; set; }
    public float leftEyeFrown { get; set; }

    // Right Eye Data
    public bool rightEyeIsValid { get; set; }
    public Vector3 rightEyeValidityVector { get; set; }

    public Vector3 rightEyeLocalOrigin { get; set; }
    public Vector3 rightEyeLocalDirection { get; set; }
    public Vector3 rightEyeWorldOrigin { get; set; }
    public Vector3 rightEyeWorldDirection { get; set; }
    public float rightEyeOpenness { get; set; }
    public float rightEyePupilDiameter { get; set; }
    public float rightEyeWide { get; set; }
    public float rightEyeSqueeze { get; set; }
    public float rightEyeFrown { get; set; }

    // Combined Eye Data
    public bool combinedEyeIsValid { get; set; }
    public Vector3 combinedEyeValidityVector { get; set; }

    public Vector3 combinedEyeLocalOrigin { get; set; }
    public Vector3 combinedEyeLocalDirection { get; set; }
    public Vector3 combinedEyeWorldOrigin { get; set; }
    public Vector3 combinedEyeWorldDirection { get; set; }
    public float combinedEyeOpenness { get; set; }
    public float combinedEyePupilDiameter { get; set; }
    public bool combinedEyeConvergenceValidity { get; set; }
    public float combinedEyeConvergenceDistance { get; set; }

    // Tracking Improvements
    public TrackingImprovements trackingImprovements { get; set; }


    public override string ToString()
    {
        return $"{systemTimeStamp} {combinedEyeIsValid} {combinedEyeLocalOrigin} {combinedEyeLocalDirection}";
    }

}
