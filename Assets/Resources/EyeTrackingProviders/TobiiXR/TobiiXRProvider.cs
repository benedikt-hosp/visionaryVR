using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tobii.XR;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static EyeTrackingProviderInterface;
using System.Collections.Concurrent;
using vrg;

public class TobiiXRProvider : EyeTrackingProviderInterface
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
    public bool isCalibrating { get { return isCalibrationRunning; } set { this.isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }
    ConcurrentQueue<SampleData> gazeQueue;
    public List<SampleData> gazeSamplesOfCP;
    public MonoBehaviour _mb = GameObject.FindObjectOfType<MonoBehaviour>(); // The surrogate MonoBehaviour that we'll use to manage this coroutine
    SampleData _sampleData;
    public bool isQueueGazeSignal { get { return queueGazeSignal; } set { queueGazeSignal = value; } }
    bool queueGazeSignal = false;
    bool isHarvestingGaze = false;



    // Tobii XR sepcific
    bool isTobiiXR;


    public bool initializeDevice()
    {
        _sampleData = new SampleData();
        gazeQueue = new ConcurrentQueue<SampleData>();
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();
        TobiiXR_Settings settings = new TobiiXR_Settings();
        // settings.AdvancedEnabled = true;
        isTobiiXR = TobiiXR.Start(settings);


        Debug.Log("Inside initialize tobiixr");

        if (isTobiiXR == true)
        {
            Debug.LogError("Provider TobiiXR could not connect to an eye-tracker.");

            return false;
        }
        else
        {
            Debug.Log("Provider TobiiXR found eye-tracker!!!");

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


        Debug.LogError("TobiiXR Provider started calibration.");
        OnCalibrationStartedEvent?.Invoke();
        isCalibrating = true;
        // BHO TODO: find a way to call Calibration here
        String filename = @"C:\Program Files (x86)\Tobii\Tobii EyeX Config\Tobii.EyeX.Configuration.exe";
        Process foo = new Process();
        foo.StartInfo.FileName = filename;
        foo.StartInfo.Arguments = "-quick -calibration";
        foo.Start();

        //c:\Program File(x86)\Tobii\Tobii EyeX Config\Tobii.EyeX.Configuration.exe –quick - calibration

        isCalibrating = false;
        OnCalibrationSucceededEvent?.Invoke();

    }


    public void close()
    {
        isHarvestingGaze = false;
        TobiiXR.Stop();
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

            //var data = TobiiXR.Advanced.LatestData;                                           // ONLY WORKS WITH OCUMEN LICENSE

            var data_local = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);           // The local eye tracking space shares origin with the XR camera.
                                                                                                // Data reported in this space is unaffected by head movements and is well
                                                                                                // suited for use cases where you need eye tracking data relative to the head, like avatar eyes.

            var data_world = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);           // 	World space is the main tracking space used by Unity. Eye tracking data in world space uses
                                                                                                // 	the same tracking space as objects in your scene and is useful when computing what object is being focused on by the user.

            this._sampleData = new SampleData();
            if (data_world != default(TobiiXR_EyeTrackingData) && data_world != null)
            {
                this._sampleData.timeStamp = GetCurrentSystemTimestamp();
                this._sampleData.deviceTimestamp = (long)data_world.Timestamp * 1000;                                               // timestamp since application start. When package was received. Seconds as float. We parse them to milliseconds

                // TobiiXR only allows developers to access more details (e.g. both eyes separately) if you have an OCUMEN license.
                // This implementation only works with the normal CORE API
                if (data_world.GazeRay.IsValid)
                {
                    this._sampleData.isValid = true;
                    this._sampleData.worldGazeOrigin = data_world.GazeRay.IsValid ? data_world.GazeRay.Origin : new Vector3(-1, -1, -1);
                    this._sampleData.worldGazeDirection = data_world.GazeRay.IsValid ? data_world.GazeRay.Direction : new Vector3(-1, -1, -1);
                    this._sampleData.vergenceDepth = data_world.ConvergenceDistanceIsValid ? data_world.ConvergenceDistance : -1;
                    this._sampleData.isBlink = data_world.IsLeftEyeBlinking || data_world.IsRightEyeBlinking;

                    if (data_local.GazeRay.IsValid)
                    {
                        this._sampleData.isValid = true;
                        this._sampleData.localGazeOrigin = data_local.GazeRay.IsValid ? data_local.GazeRay.Origin : new Vector3(-1, -1, -1);
                        this._sampleData.localGazeDirection = data_local.GazeRay.IsValid ? data_local.GazeRay.Direction : new Vector3(-1, -1, -1);
                    }

                }
            }


                NewGazesampleReady?.Invoke(this._sampleData);

            if (queueGazeSignal)
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
        return true;
    }


    public bool UnsubscribeToGazeData()
    {
        return true;
    }

    public long GetCurrentSystemTimestamp()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();

    }

    public void calibratePositionAndIPD()
    {
        return; // not implemented in TobiiXR
    }

    public void CalibrateHMDPosition()
    {
        // done automatically by calibration procedure
        return;

    }
}
