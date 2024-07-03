using System;
using System.Collections;
using System.Collections.Generic;
//using Tobii.Research.Unity;
using UnityEngine;

public delegate void NewGazeSampleReady(SampleData sd);


public delegate void OnCalibrationStarted();
public delegate void OnCalibrationSucceeded();
public delegate void OnCalibrationFailed();

public delegate void OnAutoIPDCalibrationStarted();
public delegate void OnAutoIPDCalibrationSucceeded();
public delegate void OnAutoIPDCalibrationFailed();

public interface EyeTrackingProviderInterface
{

    public static event OnCalibrationStarted OnCalibrationStartedEvent;
    public static event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    public static event OnCalibrationFailed OnCalibrationFailedEvent;

    public event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    public event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    public event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;

    public event NewGazeSampleReady NewGazesampleReady;

    public List<SampleData> getCurrentSamples { get; }

    public void clearQueue();

    bool isCalibrating { get; set; }
    //public bool etIsReady { get; }

    bool isQueueGazeSignal { get; set; }

    public bool initializeDevice();
    public void calibrateET();

    public void calibratePositionAndIPD();

    public void startETThread();

    public void stopETThread();

    public void getGazeQueue();

    //public void CalibrateHMDPosition();

    public void close();
    public bool subscribeToGazeData();

    public bool UnsubscribeToGazeData();


}
