using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;
using AOT;
using System.Linq;
using System.Threading;
//using Tobii.Research;

public class SRanipalProvider : MonoBehaviour, EyeTrackingProviderInterface
{
    private static SRanipalProvider instance;

    public event NewGazeSampleReady NewGazesampleReady;
    public event OnCalibrationStarted OnCalibrationStartedEvent;
    public event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    public event OnCalibrationFailed OnCalibrationFailedEvent;

    public event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    public event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    public event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;

    public bool isQueueGazeSignal { get; set; }

    public bool etIsReady { get; private set; }
    public bool isCalibrating { get; set; }
    public List<SampleData> getCurrentSamples => gazeSamplesOfCP;
    public static Queue<SampleData> gazeQueue = new Queue<SampleData>();

    public List<SampleData> gazeSamplesOfCP = new List<SampleData>();
    public static SampleData _sampleData = new SampleData();

    private GameObject _sranipalGameObject;
    private SRanipal_Eye_Framework sranipal;
    private EyeData_v2 eyeData_v2 = new EyeData_v2();
    public static EyeData eyeData = new EyeData();
    private bool eye_callback_registered = false;
    private GameObject experimentController;
    private GameObject camRig;
    private bool running = false;
    private Thread eyeDataThread;


    public static ConcurrentQueue<EyeData_v2> eyeDataQueue = new ConcurrentQueue<EyeData_v2>();

    public static class StaticData
    {
        public static string CalibrationStatus = "Not Started";
        public static int SampleRate = 120;
    }

    public SRanipalProvider()
    {
        instance = this;
    }

    public static SRanipalProvider Instance => instance;


    /*private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }*/



    public bool initializeDevice()
    {
        gazeQueue = new Queue<SampleData>();
        _sampleData = new SampleData();

        this._sranipalGameObject = new GameObject("EyeFramework");
        this.sranipal = _sranipalGameObject.AddComponent<SRanipal_Eye_Framework>();

        Debug.Log($"Calibration Status: {StaticData.CalibrationStatus}");
        Debug.Log($"Sample Rate: {StaticData.SampleRate}");

        if (!SRanipal_Eye_API.IsViveProEye()) return false;
        this.sranipal.StartFramework();

        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            this.sranipal.StartFramework();
        }

        UnityEngine.Object.DontDestroyOnLoad(this._sranipalGameObject);
        UnityEngine.Object.DontDestroyOnLoad(this.sranipal);


        SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
        eye_callback_registered = true;
        running = true;
        


        return (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING);
    }


    /// <summary>
    /// Required class for IL2CPP scripting backend support
    /// </summary>
    internal class MonoPInvokeCallbackAttribute : System.Attribute
    {
        public MonoPInvokeCallbackAttribute() { }
    }

    /// <summary>
    /// Eye tracking data callback thread.
    /// Reports data at ~120hz
    /// MonoPInvokeCallback attribute required for IL2CPP scripting backend
    /// </summary>
    /// <param name="eye_data">Reference to latest eye_data</param>
    [MonoPInvokeCallback]
    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        Debug.LogError("Callback got new samples");
        eyeDataQueue.Enqueue(eye_data);
    }



    public IEnumerator ProcessEyeDataCoroutine()
    {
        while (true)
        {
            while (eyeDataQueue.TryDequeue(out var data))
            {
                ProcessEyeData(data);
                //Debug.LogError("Dequeued samples");
            }
            yield return null;
        }
    }

    private void ProcessEyeData(EyeData_v2 data)
    {
         eyeData_v2 = data;
         _sampleData = new SampleData
         {
             systemTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
             deviceTimestamp = eyeData_v2.timestamp,
             frameSequence = eyeData_v2.frame_sequence,

             // Left Eye Data
             leftEyeValidityVector = GetValidityVector(eyeData_v2.verbose_data.left),
             leftEyeIsValid = IsAllOnes(GetValidityVector(eyeData_v2.verbose_data.left)),
             leftEyeLocalOrigin = new Vector3(-1 * eyeData_v2.verbose_data.left.gaze_origin_mm.x,
                                             eyeData_v2.verbose_data.left.gaze_origin_mm.y,
                                             eyeData_v2.verbose_data.left.gaze_origin_mm.z
                                             ),

             // leftEyeLocalDirection = eyeData_v2.verbose_data.left.gaze_direction_normalized,
             leftEyeLocalDirection = new Vector3(
                                             -1 * eyeData_v2.verbose_data.left.gaze_direction_normalized.x,
                                             eyeData_v2.verbose_data.left.gaze_direction_normalized.y,
                                             eyeData_v2.verbose_data.left.gaze_direction_normalized.z
                                             ),

             leftEyeWorldOrigin = Camera.main.transform.TransformPoint(eyeData_v2.verbose_data.left.gaze_origin_mm),
             leftEyeWorldDirection = Camera.main.transform.TransformDirection(eyeData_v2.verbose_data.left.gaze_direction_normalized),


             leftEyeOpenness = eyeData_v2.verbose_data.left.eye_openness,
             leftEyePupilDiameter = eyeData_v2.verbose_data.left.pupil_diameter_mm,
             leftEyeWide = eyeData_v2.expression_data.left.eye_wide,
             leftEyeSqueeze = eyeData_v2.expression_data.left.eye_squeeze,
             leftEyeFrown = eyeData_v2.expression_data.left.eye_frown,

             // Right Eye Data
             rightEyeValidityVector = GetValidityVector(eyeData_v2.verbose_data.right),
             rightEyeIsValid = IsAllOnes(GetValidityVector(eyeData_v2.verbose_data.right)),

             //rightEyeLocalOrigin = eyeData_v2.verbose_data.right.gaze_origin_mm,
             //rightEyeLocalDirection = eyeData_v2.verbose_data.right.gaze_direction_normalized,

             rightEyeLocalOrigin = new Vector3(-1 * eyeData_v2.verbose_data.right.gaze_origin_mm.x,
                                         eyeData_v2.verbose_data.right.gaze_origin_mm.y,
                                         eyeData_v2.verbose_data.right.gaze_origin_mm.z
                                     ),
             rightEyeLocalDirection = new Vector3(-1 * eyeData_v2.verbose_data.right.gaze_direction_normalized.x,
                                         eyeData_v2.verbose_data.right.gaze_direction_normalized.y,
                                         eyeData_v2.verbose_data.right.gaze_direction_normalized.z
                                     ),


             rightEyeWorldOrigin = Camera.main.transform.TransformPoint(eyeData_v2.verbose_data.right.gaze_origin_mm),
             rightEyeWorldDirection = Camera.main.transform.TransformDirection(eyeData_v2.verbose_data.right.gaze_direction_normalized),

             rightEyeOpenness = eyeData_v2.verbose_data.right.eye_openness,
             rightEyePupilDiameter = eyeData_v2.verbose_data.right.pupil_diameter_mm,
             rightEyeWide = eyeData_v2.expression_data.right.eye_wide,
             rightEyeSqueeze = eyeData_v2.expression_data.right.eye_squeeze,
             rightEyeFrown = eyeData_v2.expression_data.right.eye_frown,

             // Combined Eye Data
             combinedEyeValidityVector = GetValidityVector(eyeData_v2.verbose_data.combined.eye_data),
             combinedEyeIsValid = IsAllOnes(GetValidityVector(eyeData_v2.verbose_data.combined.eye_data)),
             //combinedLocalEyeOrigin = eyeData_v2.verbose_data.combined.eye_data.gaze_origin_mm,
             //combinedLocalEyeDirection = eyeData_v2.verbose_data.combined.eye_data.gaze_direction_normalized,

             combinedEyeLocalOrigin = new Vector3(-1 * eyeData_v2.verbose_data.combined.eye_data.gaze_origin_mm.x,
                                             eyeData_v2.verbose_data.combined.eye_data.gaze_origin_mm.y,
                                             eyeData_v2.verbose_data.combined.eye_data.gaze_origin_mm.z
                                         ),
             combinedEyeLocalDirection = new Vector3(-1 * eyeData_v2.verbose_data.combined.eye_data.gaze_direction_normalized.x,
                                             eyeData_v2.verbose_data.combined.eye_data.gaze_direction_normalized.y,
                                             eyeData_v2.verbose_data.combined.eye_data.gaze_direction_normalized.z
                                         ),

             combinedEyeWorldOrigin = Camera.main.transform.TransformPoint(eyeData_v2.verbose_data.combined.eye_data.gaze_origin_mm),
             combinedEyeWorldDirection = Camera.main.transform.TransformDirection(eyeData_v2.verbose_data.combined.eye_data.gaze_direction_normalized),

             combinedEyeOpenness = eyeData_v2.verbose_data.combined.eye_data.eye_openness,
             combinedEyePupilDiameter = eyeData_v2.verbose_data.combined.eye_data.pupil_diameter_mm,
             combinedEyeConvergenceValidity = eyeData_v2.verbose_data.combined.convergence_distance_validity,
             combinedEyeConvergenceDistance = eyeData_v2.verbose_data.combined.convergence_distance_mm / 1000.0f,

             // Tracking Improvements
             trackingImprovements = eyeData_v2.verbose_data.tracking_improvements
         };

         //Vector3 origin;
         //Vector3 direction;

         // Combined Eye Data
         //_sampleData.combinedEyeIsValid = SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out origin, out direction);
         //_sampleData.combinedWorldEyeOrigin = origin;
         //_sampleData.combinedWorldEyeDirection = direction;

         Debug.Log($"Sample Rate in GetGaze: {StaticData.SampleRate}");
         NewGazesampleReady?.Invoke(_sampleData);

         gazeQueue.Enqueue(_sampleData);
    }

    private bool IsAllOnes(Vector3 vector)
    {
        return vector.x == 1 && vector.y == 1 && vector.z == 1;
    }

    private Vector3 GetValidityVector(SingleEyeData eyeData)
    {
        return new Vector3(
            eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) ? 1 : 0,
            eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) ? 1 : 0,
            eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) ? 1 : 0
        );
    }

    private Vector3 GetValidityVector(CombinedEyeData combinedEyeData)
    {
        return new Vector3(
            combinedEyeData.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) ? 1 : 0,
            combinedEyeData.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) ? 1 : 0,
            combinedEyeData.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) ? 1 : 0
        );
    }


    private float CalculateDistanceFromVectors(Vector3 l, Vector3 r, double ipd)
    {
        float vergenceDepth;
        Vector3 xAxis = Vector3.right;

        var vergenceAngle = Vector3.Angle(l, r);
        var alpha = Vector3.Angle(xAxis, l);
        var beta = Vector3.Angle(xAxis, r);

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
            vergenceDepth = (float)((ipd / 2) / (Math.Tan(vergenceAngle / 2)));
        }

        var deptHScaled = Mathf.Abs(vergenceDepth * 10);
        return deptHScaled;
    }

    public void clearQueue()
    {
        gazeQueue.Clear();
    }

    public void getGazeQueue()
    {
        gazeSamplesOfCP = gazeQueue.ToList();
        clearQueue();
    }

    public SampleData getGazeLive()
    {
        return gazeQueue.Dequeue();
    }

    public void Destroy()
    {
        sranipal = null;
    }

    public void calibrateET()
    {
        SRanipal_Eye_v2.LaunchEyeCalibration();
    }

    public void close()
    {
        if (eye_callback_registered)
        {
            Debug.LogWarning("Unregistered Eye Tracker Callback");
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        Debug.LogWarning("Stopping Eye Tracker");
        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            if (this.sranipal != null)
                this.sranipal.StopFramework();
        }

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
        return; // not implemented in SRanipal
    }

    public void stopETThread()
    {
        Debug.LogError("Stopped thread");
  
        if (eye_callback_registered)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        this.sranipal = null;

    }

    public void startETThread()
    {

        

        if (!eye_callback_registered)
        {
            Debug.LogError("Started thread");
            this._sranipalGameObject = new GameObject("EyeFramework");
            this.sranipal = _sranipalGameObject.AddComponent<SRanipal_Eye_Framework>();

            Debug.Log($"Calibration Status: {StaticData.CalibrationStatus}");
            Debug.Log($"Sample Rate: {StaticData.SampleRate}");

            this.sranipal.StartFramework();
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        
        

    }


    public void CalibrateHMDPosition()
    {
        Debug.Log("CalibrateHMDPosition not implemented.");
        //throw new NotImplementedException();
    }

    public bool UnsubscribeToGazeData()
    {
        //Debug.Log("Unsubscribe TO Gaze data not implemented.");
        return true;
        //throw new NotImplementedException();
    }


}