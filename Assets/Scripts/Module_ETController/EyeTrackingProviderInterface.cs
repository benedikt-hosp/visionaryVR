using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

public delegate void NewGazeSampleReady(SampleData sd);

public delegate void OnCalibrationStarted();
public delegate void OnCalibrationSucceeded();
public delegate void OnCalibrationFailed();

public delegate void OnAutoIPDCalibrationStarted();
public delegate void OnAutoIPDCalibrationSucceeded();
public delegate void OnAutoIPDCalibrationFailed();

public interface EyeTrackingProviderInterface
{
    event OnCalibrationStarted OnCalibrationStartedEvent;
    event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    event OnCalibrationFailed OnCalibrationFailedEvent;

    event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;

    event NewGazeSampleReady NewGazesampleReady;

    List<SampleData> getCurrentSamples { get; }



    void clearQueue();
    bool isCalibrating { get; set; }
    bool isQueueGazeSignal { get; set; }
    public IEnumerator ProcessEyeDataCoroutine();

    bool initializeDevice();
    void calibrateET();
    void calibratePositionAndIPD();
    void startETThread();
    void stopETThread();
    void getGazeQueue();
    void close();
    bool subscribeToGazeData();
    bool UnsubscribeToGazeData();
}