using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Tobii.XR;
using static EyeTrackingProviderInterface;

public class TobiiXRProvider : EyeTrackingProviderInterface
{
    private static TobiiXRProvider instance;

    public event OnCalibrationStarted OnCalibrationStartedEvent;
    public event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    public event OnCalibrationFailed OnCalibrationFailedEvent;

    public event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    public event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    public event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;
    public event NewGazeSampleReady NewGazesampleReady;

    private bool isCalibrationRunning = false;
    public bool isCalibrating { get { return isCalibrationRunning; } set { this.isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }
    public bool isQueueGazeSignal { get; set; }
    private bool isHarvestingGaze = false;

    public List<SampleData> gazeSamplesOfCP;
    private static ConcurrentQueue<SampleData> gazeQueue;
    private SampleData _sampleData;
    private MonoBehaviour _mb;

    // Tobii XR specific
    private bool isTobiiXR;

    public TobiiXRProvider()
    {
        instance = this;
        _mb = UnityEngine.Object.FindFirstObjectByType<MonoBehaviour>();
    }

    public static TobiiXRProvider Instance => instance;

    public bool initializeDevice()
    {
        _sampleData = new SampleData();
        gazeQueue = new ConcurrentQueue<SampleData>();

        TobiiXR_Settings settings = new TobiiXR_Settings();
        isTobiiXR = TobiiXR.Start(settings);

        UnityEngine.Debug.Log("Inside initialize TobiiXR");

        if (isTobiiXR)
        {
            UnityEngine.Debug.Log("Provider TobiiXR found eye-tracker!!!");
            return true;
        }
        else
        {
            UnityEngine.Debug.LogError("Provider TobiiXR could not connect to an eye-tracker.");
            return false;
        }
    }

    public IEnumerator ProcessEyeDataCoroutine()
    {
        UnityEngine.Debug.LogError("Dequeued outside");

        while (true)
        {

            yield return null;
        }
    }

    public void clearQueue()
    {
        gazeQueue.Clear();
    }

    public void getGazeQueue()
    {
        this.gazeSamplesOfCP = gazeQueue.ToList();
        this.clearQueue();
    }

    public void calibrateET()
    {
        UnityEngine.Debug.LogError("TobiiXR Provider started calibration.");
        OnCalibrationStartedEvent?.Invoke();
        isCalibrating = true;

        Process calibrationProcess = new Process();
        calibrationProcess.StartInfo.FileName = @"C:\Program Files (x86)\Tobii\Tobii EyeX Config\Tobii.EyeX.Configuration.exe";
        calibrationProcess.StartInfo.Arguments = "-quick -calibration";
        calibrationProcess.Start();

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

    private IEnumerator getGaze()
    {
        while (isHarvestingGaze)
        {
            /*
            var data_local = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);
            var data_world = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

            this._sampleData = new SampleData();
            if (data_world != default(TobiiXR_EyeTrackingData) && data_world != null)
            {
                this._sampleData.timeStamp = GetCurrentSystemTimestamp();
                this._sampleData.deviceTimestamp = (long)data_world.Timestamp * 1000;

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

            if (isQueueGazeSignal)
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

        var angleBetween = Vector3.Angle(l, r);
        var alpha = Vector3.Angle(xAxis, l);
        var beta = Vector3.Angle(xAxis, r);
        var gamma = Vector3.Angle(l, r);

        if (alpha > 90 || r == Vector3.zero)
        {
            float alpha1 = 180 - alpha;
            var a = Vector3.Magnitude(l);
            vergenceDepth = (float)(Math.Sin(alpha1) / (float)a);
        }
        else if (beta > 90 || l == Vector3.zero)
        {
            float beta1 = 180 - beta;
            var b = Vector3.Magnitude(r);
            vergenceDepth = (float)(Math.Sin(beta1) / (float)b);
        }
        else
        {
            vergenceDepth = (float)((ipd / 2) / (Math.Tan(angleBetween / 2)));
        }

        var deptHScaled = Mathf.Abs(vergenceDepth * 10);
        UnityEngine.Debug.Log("Vergence mode: scaled depth: " + deptHScaled.ToString() + " and vergence depth: " + vergenceDepth.ToString() + " because of EVA: " + angleBetween.ToString() + " degree.");

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
        return; // done automatically by calibration procedure
    }
}
