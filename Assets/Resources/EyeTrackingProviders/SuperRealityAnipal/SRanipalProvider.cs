using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


using ViveSR.anipal.Eye;
using System.Collections;

public class SRanipalProvider : EyeTrackingProviderInterface
{
    

    private MonoBehaviour _mb; // The surrogate MonoBehaviour that we'll use to manage this coroutine.
    public event NewGazeSampleReady NewGazesampleReady;
    public event OnCalibrationStarted OnCalibrationStartedEvent;
    public event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    public event OnCalibrationFailed OnCalibrationFailedEvent;

    public event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    public event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    public event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;
    bool queueGazeSignal = false;
    public bool isQueueGazeSignal { get { return queueGazeSignal; } set { queueGazeSignal = value; } }

    // Eye Tracking Interface
    private bool isReady = false;
    private bool isCalibrationRunning = false;
    public bool etIsReady { get { return isReady; } }
    public bool isCalibrating { get { return isCalibrationRunning; } set { this.isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }
    public Queue<SampleData> gazeQueue;


    public List<SampleData> gazeSamplesOfCP;
    public SampleData _sampleData;

    // SRanipal Specific
    private GameObject _sranipalGameObject;
    private SRanipal_Eye_Framework sranipal;
    private EyeData_v2 eyeData = new EyeData_v2();


 

    public bool initializeDevice()
    {
        gazeQueue = new Queue<SampleData>();
        this._sampleData = new SampleData();

        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();
        this._sranipalGameObject = new GameObject("EyeFramework");
        this.sranipal = _sranipalGameObject.AddComponent<SRanipal_Eye_Framework>();

        if (!SRanipal_Eye_API.IsViveProEye()) return false;
        this.sranipal.StartFramework();


        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
            {
            this.sranipal.StartFramework();
            }
            return (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING);                
    }


    public void startETThread()
    {
        this._mb.StartCoroutine(GetGaze());
    }

    IEnumerator GetGaze()
    { 
        while(true)
        {
            this._sampleData = new SampleData();
            bool success = SRanipal_Eye_v2.GetVerboseData(out eyeData.verbose_data);


            if (success)
            {
                if (!this.eyeData.Equals(default(EyeData_v2)))
                {
                    Vector3 origin;
                    Vector3 direction;
                    this._sampleData.timeStamp = getCurrentSystemTimestamp();

                    // Both valid
                    if (!this.eyeData.verbose_data.right.Equals(default(SingleEyeData)) && !this.eyeData.verbose_data.right.Equals(default(SingleEyeData)))
                    {

                        this._sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out origin, out direction);
                        this._sampleData.worldGazeOrigin = origin;
                        this._sampleData.worldGazeDirection = direction;
                        this._sampleData.vergenceDepth = eyeData.verbose_data.combined.convergence_distance_mm / 1000;

                    } // Right valid
                    else if (!this.eyeData.verbose_data.right.Equals(default(SingleEyeData)) && this.eyeData.verbose_data.left.Equals(default(SingleEyeData)))
                    {
                        this._sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out origin, out direction);
                        this._sampleData.worldGazeOrigin = origin;
                        this._sampleData.worldGazeDirection = direction;
                        this._sampleData.ipd = Vector3.Distance(eyeData.verbose_data.left.gaze_origin_mm, eyeData.verbose_data.right.gaze_origin_mm);
                        this._sampleData.vergenceDepth = CalculateDistanceFromVectors(eyeData.verbose_data.right.gaze_direction_normalized, Vector3.zero, this._sampleData.ipd);

                    }
                    else // Left valid
                    {
                        this._sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out origin, out direction);
                        this._sampleData.worldGazeOrigin = origin;
                        this._sampleData.worldGazeDirection = direction;
                        this._sampleData.vergenceDepth = CalculateDistanceFromVectors(Vector3.zero, eyeData.verbose_data.right.gaze_direction_normalized, this._sampleData.ipd);
                    }
                    NewGazesampleReady(this._sampleData);

                    gazeQueue.Enqueue(this._sampleData);

                }        
            }
            yield return null;

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



    public void clearQueue()
    {
            gazeQueue.Clear();
    }

    public void getGazeQueue()
    {
        this.gazeSamplesOfCP = gazeQueue.ToList();
        this.clearQueue();

    }

    public SampleData getGazeLive()
    {
        SampleData sampleData = gazeQueue.Dequeue();
        return sampleData;
    }

    public void Destroy()
    {
        this.sranipal = null;
            
    }

    public void calibrateET()
    {
        SRanipal_Eye_v2.LaunchEyeCalibration();
        
    }

    public void close()
    {
        this.sranipal = null;


    }

    public bool subscribeToGazeData()
    {
         if (!SRanipal_Eye_Framework.Instance.enabled)
         {
            SRanipal_Eye_Framework.Instance.enabled = true;
         }
        return SRanipal_Eye_Framework.Instance.enabled;

    }

    public long getCurrentSystemTimestamp()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();

    }

    public void calibratePositionAndIPD()
    {
        // As far as i know this is not possible with this headset.
    }

    public void stopETThread()
    {
        throw new NotImplementedException();
    }

    public void CalibrateHMDPosition()
    {
        throw new NotImplementedException();
    }

    public bool UnsubscribeToGazeData()
    {
        throw new NotImplementedException();
    }
}

