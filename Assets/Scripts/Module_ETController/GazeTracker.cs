using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using Tobii.Research.Unity;
using UnityEngine;

public class GazeTracker    
{

    EyeTrackingProviderInterface m_EyeTrackingProvider;
    StreamWriter eventWriter;
    List<SampleData> currentSamples;
    string gazeFile;
    public MonoBehaviour _mb;
    bool isWriting = false;



    string header = "System Timestamp\tDevice Timestamp\tMessage\tMessage Type\tObject Name\t" +
    "isValid\t" +
    "Camera Position X\tCamera Position Y\tCamera Position Z\t" +
    "Left Eye Validity X\tLeft Eye Validity Y\tLeft Eye Validity Z\t" +
    "Left Eye Local Origin X\tLeft Eye Local Origin Y\tLeft Eye Local Origin Z\t" +
    "Left Eye Local Direction X\tLeft Eye Local Direction Y\tLeft Eye Local Direction Z\t" +
    "Left Eye Openness\tLeft Eye Pupil Diameter\tLeft Eye Wide\tLeft Eye Squeeze\tLeft Eye Frown\t" +
    "Right Eye Validity X\tRight Eye Validity Y\tRight Eye Validity Z\t" +
    "Right Eye Local Origin X\tRight Eye Local Origin Y\tRight Eye Local Origin Z\t" +
    "Right Eye Local Direction X\tRight Eye Local Direction Y\tRight Eye Local Direction Z\t" +
    "Right Eye Openness\tRight Eye Pupil Diameter\tRight Eye Wide\tRight Eye Squeeze\tRight Eye Frown\t" +
    "Combined Eye Validity X\tCombined Eye Validity Y\tCombined Eye Validity Z\t" +
    "Combined Local Eye Origin X\tCombined Local Eye Origin Y\tCombined Local Eye Origin Z\t" +
    "Combined Local Eye Direction X\tCombined Local Eye Direction Y\tCombined Local Eye Direction Z\t" +
    "Combined Eye Openness\tCombined Eye Pupil Diameter\t" +
    "Combined Eye Convergence Validity\tCombined Eye Convergence Distance";

    public GazeTracker(string userFolder, EyeTrackingProviderInterface eyeTrackerObject, ConditionController cController, BaselineLevelController baselineLevelController)
    {
        this._mb = UnityEngine.Object.FindFirstObjectByType<MonoBehaviour>();
        //this.metricsCalculator = new MetricsCalculator(userFolder);

        this.m_EyeTrackingProvider = eyeTrackerObject;

        var fileName = Path.Combine(userFolder, "gaze.csv");

        var oldName = Path.Combine(userFolder, "gaze"); // Remove .csv for appending
        var fileCounter = 1;

        while (File.Exists(fileName))
        {
            fileName = oldName + fileCounter.ToString() + ".csv";
            fileCounter += 1;
        }

        this.gazeFile = fileName;

        Debug.Log("Gaze file will be saved as: " + this.gazeFile);


        //Debug.Log("Saving event file to " + gazeFile);
        this.eventWriter = new StreamWriter(gazeFile);
        this.eventWriter.WriteLine(header);
        this.eventWriter.Flush();

        if( cController != null)
            cController.OnSaveMsgEvent += WriteMsg;

        if (baselineLevelController != null)
            baselineLevelController.OnSaveMsgToFileEvent += WriteMsg;

        eyeTrackerObject.NewGazesampleReady += WriteGazeSampleToFile;

        eyeTrackerObject.OnCalibrationStartedEvent += OnCalibrationStarted;
        eyeTrackerObject.OnCalibrationFailedEvent += OnCalibrationFailed;
        eyeTrackerObject.OnCalibrationSucceededEvent += OnCalibrationSucceded;


    }


    public void startGazeWriting()
    {
        this.isWriting = true;
        this._mb = UnityEngine.Object.FindFirstObjectByType<MonoBehaviour>();
        this.m_EyeTrackingProvider.clearQueue();
    }

    public void stopWriting()
    {
        this.isWriting = false;
        this.eventWriter.Close();
        this.eventWriter.Dispose();
    }

    public void WriteGazeSampleToFile(SampleData sampleData)
    {
        if (this.isWriting)
        {
            this.WriteGazeData(sampleData);

        }
    }

    private void WriteGazeData(SampleData gazeData)
    {
        string sampleLine =
      $"{getCurrentSystemTimestamp()}\t" +
      $"{gazeData.systemTimeStamp}\t" +
      $"gaze sample\tgs\t-1\t" +
      $"{gazeData.combinedEyeIsValid}\t" +
      $"{gazeData.cameraPosition.x}\t" +
      $"{gazeData.cameraPosition.y}\t" +
      $"{gazeData.cameraPosition.z}\t" +
      $"{gazeData.leftEyeIsValid}\t" +
      $"{gazeData.leftEyeLocalOrigin.x}\t" +
      $"{gazeData.leftEyeLocalOrigin.y}\t" +
      $"{gazeData.leftEyeLocalOrigin.z}\t" +
      $"{gazeData.leftEyeLocalDirection.x}\t" +
      $"{gazeData.leftEyeLocalDirection.y}\t" +
      $"{gazeData.leftEyeLocalDirection.z}\t" +
      $"{gazeData.leftEyeOpenness}\t" +
      $"{gazeData.leftEyePupilDiameter}\t" +
      $"{gazeData.leftEyeWide}\t" +
      $"{gazeData.leftEyeSqueeze}\t" +
      $"{gazeData.leftEyeFrown}\t" +
      $"{gazeData.rightEyeIsValid}\t" +
      $"{gazeData.rightEyeLocalOrigin.x}\t" +
      $"{gazeData.rightEyeLocalOrigin.y}\t" +
      $"{gazeData.rightEyeLocalOrigin.z}\t" +
      $"{gazeData.rightEyeLocalDirection.x}\t" +
      $"{gazeData.rightEyeLocalDirection.y}\t" +
      $"{gazeData.rightEyeLocalDirection.z}\t" +
      $"{gazeData.rightEyeOpenness}\t" +
      $"{gazeData.rightEyePupilDiameter}\t" +
      $"{gazeData.rightEyeWide}\t" +
      $"{gazeData.rightEyeSqueeze}\t" +
      $"{gazeData.rightEyeFrown}\t" +
      $"{gazeData.combinedEyeIsValid}\t" +
      $"{gazeData.combinedEyeLocalOrigin.x}\t" +
      $"{gazeData.combinedEyeLocalOrigin.y}\t" +
      $"{gazeData.combinedEyeLocalOrigin.z}\t" +
      $"{gazeData.combinedEyeLocalDirection.x}\t" +
      $"{gazeData.combinedEyeLocalDirection.y}\t" +
      $"{gazeData.combinedEyeLocalDirection.z}\t" +
      $"{gazeData.combinedEyeOpenness}\t" +
      $"{gazeData.combinedEyePupilDiameter}\t" +
      $"{gazeData.combinedEyeConvergenceValidity}\t" +
      $"{gazeData.combinedEyeConvergenceDistance}";

        this.eventWriter.WriteLine(sampleLine);
        this.eventWriter.Flush();
    }



    public void OnCalibrationStarted()
    {
        string msg = (getCurrentSystemTimestamp()).ToString() + "\t-1\t-1\t" +
            "Calibration started" + "\tc0\t -1\t" + "-1\t-1\t-1\t-1\t-1\t-1\t- 1\t - 1\t - 1\t - 1\t - 1\t - 1\t-1\t-1";

        this.eventWriter.WriteLine(msg);
        this.eventWriter.Flush();

    }

    public void OnCalibrationSucceded()
    {
        string msg = (getCurrentSystemTimestamp()).ToString() + "\t-1\t" +
         "Calibration succeded" + "\tc1\t -1\t" + "-1\t-1\t-1\t-1\t-1\t-1\t- 1\t - 1\t - 1\t - 1\t - 1\t - 1\t-1\t-1";

        eventWriter.WriteLine(msg);
        eventWriter.Flush();

    }

    public void OnCalibrationFailed()
    {
        string msg = (getCurrentSystemTimestamp()).ToString() + "\t-1\t" +
        "Calibration failed" + "\tc1\t -1\t" + "-1\t-1\t-1\t-1\t-1\t-1\t- 1\t - 1\t - 1\t - 1\t - 1\t - 1\t-1\t-1";

        this.eventWriter.WriteLine(msg);
        this.eventWriter.Flush();

    }


    public long getCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }

    private void WriteMsg(string msg)
    {
        string _msg = (getCurrentSystemTimestamp()).ToString() + "\t-1\t" +
           msg + "\tc0\t -1\t" + "-1\t-1\t-1\t-1\t-1\t-1\t- 1\t - 1\t - 1\t - 1\t - 1\t - 1\t-1\t-1";

        this.eventWriter.WriteLine(_msg);
        this.eventWriter.Flush();
        

    }

    

}

