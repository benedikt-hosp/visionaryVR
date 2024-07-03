using System;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using static EyeTrackingProviderInterface;
using System.Collections;
using System.Collections.Concurrent;

public class XTALProvider : EyeTrackingProviderInterface
{


    public static event OnCalibrationStarted OnCalibrationStartedEvent;
    public static event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    public static event OnCalibrationFailed OnCalibrationFailedEvent;

    public event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    public event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    public event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;

    public event NewGazeSampleReady NewGazesampleReady;



    // Eye Tracking Interface
    private bool isCalibrationRunning = false;
    //public bool etIsReady { get { return isReady; } }
    public bool isCalibrating { get { return isCalibrationRunning; } set { this.isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }
    ConcurrentQueue<SampleData> gazeQueue;
    public List<SampleData> gazeSamplesOfCP;
    public MonoBehaviour _mb = GameObject.FindObjectOfType<MonoBehaviour>();
    // The surrogate MonoBehaviour that we'll use to manage this coroutine.
    SampleData _sampleData;
    public bool isQueueGazeSignal { get { return queueGazeSignal; } set { queueGazeSignal = value; } }
    bool queueGazeSignal = false;
    bool isHarvestingGaze = false;


    // XTAL SPECIFIC
    VrgHmd _eyeTracker;
    VRgEyeTrackingResult l;
    VRgEyeTrackingResult r;


    public bool initializeDevice()
    {
        _sampleData = new SampleData();
        gazeQueue = new ConcurrentQueue<SampleData>();
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();

        this._eyeTracker = GameObject.Find("CameraOrigin").GetComponentInChildren<VrgHmd>();

        if (this._eyeTracker == null)
        {
            Debug.LogError("Provider XTAL could not connect to an eye tracker.");

            return false;
        }
        else
        {
            Debug.Log("Provider XTAL found EyeTracker!!!");

            return true;

        }
       
    }
    public void clearQueue()
    {
        // clear queue
        gazeQueue.Clear();
    }

    public void getGazeQueue()
    {
        this.gazeSamplesOfCP = this.gazeQueue.ToList();
        this.clearQueue();

    }

    public void calibrateET()
    {
        Debug.Log("XTAL Provider started calibration.");
        OnCalibrationStartedEvent?.Invoke();
        isCalibrating = true;
        this._eyeTracker.EyeTrackingCalibrate();       
        isCalibrating = false;
        OnCalibrationSucceededEvent?.Invoke();
    }


    public void close()
    {
        isHarvestingGaze = false;
        this._eyeTracker = null;


    }

    public void stopETThread()
    {
        isHarvestingGaze = false;
    }

    public void startETThread()
    {
        isHarvestingGaze = true;
        this._mb.StartCoroutine(getGaze());
    }

    IEnumerator getGaze()
    {
       

        while (isHarvestingGaze)
        {
            VRgEyeTrackingResult l;
            VRgEyeTrackingResult r;

            this._sampleData = new SampleData();

            this._eyeTracker.GetEyeTrackingRays(out l, out r);
            this._sampleData.timeStamp = (long)((l.TimeS + r.TimeS )*1000 / 2);
            this._sampleData.vergenceDepth = 1.0f;
            
            // data of both eyes availabe
            if (r.EyeRay != null && l.EyeRay != null) 
            {
                this._sampleData.isValid = true;
                this._sampleData.eye = Eye.Both;
                this._sampleData.worldGazeOrigin = (l.EyePosition + r.EyePosition)/2;
                this._sampleData.worldGazeOrigin_R = r.EyePosition;
                this._sampleData.worldGazeOrigin_L = l.EyePosition;

                this._sampleData.worldGazeDirection = (l.EyeRay + r.EyeRay)/2;
                this._sampleData.worldGazeDirection_L = l.EyeRay;
                this._sampleData.worldGazeDirection_R = r.EyeRay;

                this._sampleData.ipd = Vector3.Distance(r.EyePosition, l.EyePosition);
                this._sampleData.vergenceDepth = CalculateDistanceFromVectors(l.EyeRay, r.EyeRay, this._sampleData.ipd);

                this._sampleData.vergenceAngle_R = Vector3.Angle(Vector3.right, r.EyeRay);
                this._sampleData.vergenceAngle_L = Vector3.Angle(Vector3.right, l.EyeRay);



            }       // only right eye data available
            else if (r.EyeRay != null && l.EyeRay == null)
            {
                this._sampleData.isValid = true;
                this._sampleData.eye = Eye.Right;
                this._sampleData.vergenceAngle_R = Vector3.Angle(Vector3.right, r.EyeRay);
                this._sampleData.worldGazeDirection = r.EyeRay;
                this._sampleData.worldGazeDirection_R = r.EyeRay;
                this._sampleData.worldGazeOrigin_R = r.EyePosition;
                this._sampleData.worldGazeOrigin = r.EyePosition;
                this._sampleData.vergenceDepth = CalculateDistanceFromVectors(Vector3.zero, r.EyeRay, this._sampleData.ipd);
            }
            else   // only left eye data available
            {
                this._sampleData.isValid = true;
                this._sampleData.eye = Eye.Left;
                this._sampleData.vergenceAngle_L = Vector3.Angle(Vector3.right, l.EyeRay); ;
                this._sampleData.worldGazeOrigin = l.EyePosition;
                this._sampleData.worldGazeOrigin_L = l.EyePosition; this._sampleData.worldGazeOrigin_R = r.EyePosition;
                this._sampleData.worldGazeDirection_L = l.EyeRay;
                this._sampleData.worldGazeDirection = l.EyeRay;
                this._sampleData.vergenceDepth = CalculateDistanceFromVectors(l.EyeRay, Vector3.zero, this._sampleData.ipd);

            }
            
            NewGazesampleReady?.Invoke(this._sampleData);

            if(queueGazeSignal)
                gazeQueue.Enqueue(this._sampleData);
            yield return null;

            if (!isHarvestingGaze)
                break;
        }
    }

    private float CalculateDistanceFromVectors(Vector3 l, Vector3 r, double ipd)
    {

        float vergenceDepth;
        Vector3 xAxis = Vector3.right;

        // Angle
        var vergenceAngle = Vector3.Angle(l, r);
        var alpha = Vector3.Angle(xAxis, l);
        var beta = Vector3.Angle(xAxis, r);


        // Both eyes detected
        if (alpha > 90 || r == Vector3.zero) // spitzer Winkel nach links
        {
            float alpha1 = 180 - alpha;
            var a = Vector3.Magnitude(l);
            vergenceDepth = (float)(Math.Sin(alpha1) / (float)a);
        }
        else if (beta > 90 || l == Vector3.zero)  // spitzer Winkel nach rechts
        {
            float beta1 = 180 - beta;
            var b = Vector3.Magnitude(r);
            vergenceDepth = (float)(Math.Sin(beta1) / (float)b);
        }
        else   // Winkel mittig
        {
            // equation: distance = (length_IPD/ 2) / tanh(angle/2);
            vergenceDepth = (float)((ipd / 2) / (Math.Tan(vergenceAngle / 2)));
        }
   

        var deptHScaled = Mathf.Abs(vergenceDepth * 10);
        //Debug.Log("Vergence mode: scaled depth: " + deptHScaled.ToString() + " and vergence depth: " + vergenceDepth.ToString() + " because of EVA: " + vergenceAngle.ToString() + " degree.");


        return deptHScaled;

    }


    public bool subscribeToGazeData()
    {
        bool success = true;
        if(this._eyeTracker != null)
            this._eyeTracker.EnableEyeTracking(true);
        else
            success = false;

        return success;

    }

    public bool UnsubscribeToGazeData()
    {
        bool success = true;
        if (this._eyeTracker != null)
            this._eyeTracker.EnableEyeTracking(false);
        else
            success = false;

        return success;

    }

    public long getCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }

    public void calibratePositionAndIPD()
    {
        OnAutoIPDCalibrationStartedEvent?.Invoke();
        this._eyeTracker.RunAutoInterpupillaryDistance();

        //if(true)    // so far we dont get any feedback. we have to wait until vrgineers fix that
            OnAutoIPDCalibrationSucceededEvent?.Invoke();
        //else
        //    OnAutoIPDCalibrationFailedEvent?.Invoke();


    }

}


