using System;
using UnityEngine;

using Tobii.Research.Unity;
using Tobii.Research;
using System.Collections.Generic;
using System.Linq;

using static EyeTrackingProviderInterface;
using System.Collections;
using ViveSR.anipal.Eye;

public class TobiiProProvider : EyeTrackingProviderInterface
{
    // Eye Tracking Interface
    private bool isCalibrationRunning = false;

    public bool isCalibrating { get { return isCalibrationRunning; } set { this.isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }
    Queue<SampleData> gazeQueue;
    public List<SampleData> gazeSamplesOfCP;
    private MonoBehaviour _mb; // The surrogate MonoBehaviour that we'll use to manage this coroutine.

    public bool isQueueGazeSignal { get { return queueGazeSignal; } set { queueGazeSignal = value; } }
    bool queueGazeSignal = false;
    bool isHarvestingGaze = false;


    // TOBII PRO SPECIFIC
    public VRCalibration calibration = VRCalibration.Instance;
    private SampleData _sampleData;
    public VREyeTracker _eyeTracker;

    public event OnCalibrationStarted OnCalibrationStartedEvent;
    public event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    public event OnCalibrationFailed OnCalibrationFailedEvent;

    public event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    public event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    public event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;

    public event NewGazeSampleReady NewGazesampleReady;

    public bool initializeDevice()
    {
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();
        _sampleData = new SampleData();
        gazeQueue = new Queue<SampleData>();

        this._eyeTracker = VREyeTracker.Instance;


        if (this._eyeTracker == null)
        {
            Debug.LogError("Provider Tobii Pro could not connect to an eye tracker.");

            return false;
        }
        else
        {
            Debug.Log("Provider Tobii Pro found VREyeTracker!!!");

            return true;

        }
       
    }


    public IEnumerator ProcessEyeDataCoroutine()
    {
        Debug.LogError("Dequeued outside");

        while (true)
        {
          
            yield return null;
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

    public SampleData getGazeLive()
    {
        SampleData sampleData = gazeQueue.Dequeue();
        return sampleData;
    }

    public void calibrateET()
    {
        OnCalibrationStartedEvent?.Invoke();
        Debug.Log("TobiiProProvider started calibration.");
        isCalibrating = true;
        var calibrationStartResult = calibration.StartCalibration(resultCallback: (calibrationResultAvailable));

    }


    public void calibrationResultAvailable(bool result)
    {
        Debug.Log("Calibration was " + (result ? "successful" : "unsuccessful"));


        if (result)
            OnCalibrationSucceededEvent?.Invoke();
        else
            OnCalibrationFailedEvent?.Invoke();

        isCalibrating = false;
    }

    public void close()
    {
        isHarvestingGaze = false;
        EyeTrackingOperations.Terminate();
        //this._eyeTracker = null;
        //this.calibration = null;
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
        
            var data = this._eyeTracker.NextData;             // always null
            this._sampleData = new SampleData();
            //var data = this._eyeTracker.LatestGazeData;       // always null
            //var data = this._eyeTracker.LatestProcessedGazeData; // always null

            /*
            if (data != default(IVRGazeData) && data != null)
            {
                this._sampleData.systemTimeStamp = GetCurrentSystemTimestamp();
                this._sampleData.deviceTimestamp = data.OriginalGaze.DeviceTimeStamp;                 // long to float
                this._sampleData.combinedEyeConvergenceDistance = -1.0f;

                // Both
                if (data.Right.GazeRayWorldValid && data.Left.GazeRayWorldValid)
                {
                    this._sampleData.isValid = data.CombinedGazeRayWorldValid;
                    this._sampleData.worldGazeOrigin = data.CombinedGazeRayWorld.origin;
                    this._sampleData.ipd = Vector3.Distance(data.Left.GazeOrigin, data.Right.GazeOrigin);

                    this._sampleData.vergenceDepth = CalculateDistanceFromVectors(data.Left.GazeDirection, data.Right.GazeDirection, this._sampleData.ipd);

                } // Right valid
                else if (data.Right.GazeRayWorldValid && !data.Left.GazeRayWorldValid)
                {
                    this._sampleData.isValid = data.Right.GazeRayWorldValid;
                    this._sampleData.worldGazeOrigin_R = data.Right.GazeRayWorld.origin;
                    this._sampleData.worldGazeDirection_R = data.Right.GazeRayWorld.direction;
                    this._sampleData.vergenceDepth = CalculateDistanceFromVectors(Vector3.zero, data.Right.GazeDirection, this._sampleData.ipd);

                }
                else   // Left valid
                {
                    this._sampleData.isValid = data.Left.GazeRayWorldValid;
                    this._sampleData.worldGazeOrigin_L = data.Left.GazeRayWorld.origin;
                    this._sampleData.worldGazeDirection_L = data.Left.GazeRayWorld.direction;
                    this._sampleData.vergenceDepth = CalculateDistanceFromVectors(data.Left.GazeDirection, Vector3.zero,  this._sampleData.ipd);

                }
            }
            

                NewGazesampleReady?.Invoke(this._sampleData);

                if (queueGazeSignal)
                    gazeQueue.Enqueue(this._sampleData);
            */
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
        var angleBetween = Vector3.Angle(l, r);
        var alpha = Vector3.Angle(xAxis, l);
        var beta = Vector3.Angle(xAxis, r);
        var gamma = Vector3.Angle(l, r);


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
            vergenceDepth = (float)((ipd / 2) / (Math.Tan(angleBetween / 2)));
        }


        var deptHScaled = Mathf.Abs(vergenceDepth * 10);
        Debug.Log("Vergence mode: scaled depth: " + deptHScaled.ToString() + " and vergence depth: " + vergenceDepth.ToString() + " because of EVA: " + angleBetween.ToString() + " degree.");


        return vergenceDepth;

    }

    public bool subscribeToGazeData()
    {
        bool success = false;
        if (this._eyeTracker != null)
        { 
            success = this._eyeTracker.SubscribeToGazeData;
            if (success)
                Debug.Log("Subscription successfull!");
            else
                Debug.Log("Subscription failed!");
        }
        else
        {
            Debug.Log("There is no eye tracker attached to subscribe gaze from.");
        }
        return success;

    }
    public bool UnsubscribeToGazeData()
    {
        bool success = false;
        if (this._eyeTracker != null)
        {
            success = this._eyeTracker.SubscribeToGazeData = false;
            if (success)
                Debug.Log("Subscription successfull!");
            else
                Debug.Log("Subscription failed!");
        }
        else
        {
            Debug.Log("There is no eye tracker attached to subscribe gaze from.");
        }
        return success;
    }


    public long GetCurrentSystemTimestamp()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();

    }


    public void calibratePositionAndIPD()
    {
        // As far as i know this is not possible with this headset.
        return;
    }

    public void CalibrateHMDPosition()
    {
        return;
    }

}


