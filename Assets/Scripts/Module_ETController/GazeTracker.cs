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
            "Camera Position X\t" + "Camera Position y\t" + "Camera position z\t" +
           "World Gaze Origin X\t" + "World Gaze Origin Y\t" + "World Gaze Origin Z\t" +
           "World Gaze Direction X\t" + "World Gaze Direction Y\t" + "World Gaze Direction Z\t" +
           "World Gaze Distance\t" +
           "World Gaze Point X\t" + "World Gaze Point Y\t" + "World Gaze Point Z";

    public GazeTracker(string userFolder, EyeTrackingProviderInterface eyeTrackerObject, ConditionController cController, BaselineLevelController baselineLevelController)
    {
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();
        //this.metricsCalculator = new MetricsCalculator(userFolder);

        this.m_EyeTrackingProvider = eyeTrackerObject;
        this.gazeFile = Path.Combine(userFolder, "gaze.csv");
        
        //Debug.Log("Saving event file to " + gazeFile);
        this.eventWriter = new StreamWriter(gazeFile);
        this.eventWriter.WriteLine(header);
        this.eventWriter.Flush();

        if( cController != null)
            cController.OnSaveMsgEvent += WriteMsg;

        if (baselineLevelController != null)
            baselineLevelController.OnSaveMsgToFileEvent += WriteMsg;

        eyeTrackerObject.NewGazesampleReady += WriteGazeSampleToFile;

        EyeTrackingProviderInterface.OnCalibrationStartedEvent += OnCalibrationStarted;
        EyeTrackingProviderInterface.OnCalibrationFailedEvent += OnCalibrationFailed;
        EyeTrackingProviderInterface.OnCalibrationSucceededEvent += OnCalibrationSucceded;


    }


    public void startGazeWriting()
    {
        this.isWriting = true;
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();
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
        //Debug.Log("Writing gaze " + gazeData.worldGazePoint.x.ToString() + " " +gazeData.worldGazePoint.y.ToString());
        string sampleLine = (getCurrentSystemTimestamp()).ToString() + "\t" + gazeData.timeStamp.ToString() + "\tgaze sample\tgs\t-1\t" + 
            gazeData.isValid.ToString() + "\t" + 
            gazeData.cameraPosition.x + "\t" + gazeData.cameraPosition.y + "\t" + gazeData.cameraPosition.z + "\t" +
            gazeData.worldGazeOrigin.x.ToString() + "\t" + gazeData.worldGazeOrigin.y + "\t" + gazeData.worldGazeOrigin.y + "\t" +
            gazeData.worldGazeDirection.x.ToString() + "\t" + gazeData.worldGazeDirection.y + "\t" + gazeData.worldGazeDirection.z + "\t" +
            gazeData.vergenceDepth.ToString() + "\t" + 
            gazeData.worldGazePoint.x.ToString() + "\t" + gazeData.worldGazePoint.y.ToString() + "\t" + gazeData.worldGazePoint.z.ToString();

            this.eventWriter.WriteLine(sampleLine);
            this.eventWriter.Flush();
            //Debug.Log("Wrote all samples of current queue to file");
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

