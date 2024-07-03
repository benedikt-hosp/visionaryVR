using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using UnityEngine;

public class DepthCalibrationWriter
{

    StreamWriter depthCalibrationWriter;
    public string eventFile;
    public MonoBehaviour _mb;
    bool isWriting = false;



    string header = "System Timestamp\tMessage\tMessage Type\t" +
        "GT depth\t" +
        "World Gaze Point X\t" + "World Gaze Point Y\t" + "World Gaze Point Z\t" +
        "World Gaze Origin R X\t"  + "World Gaze Origin R Z\t" +
        "World Gaze Origin L X\t" + "World Gaze Origin L Z\t" +
        "World Gaze Direction R X\t" + "World Gaze Direction R Y\t" + "World Gaze Direction R Z\t" +
        "World Gaze Direction L X\t" + "World Gaze Direction L Y\t" + "World Gaze Direction L Z\t" + "Estimated Depth";        

    public DepthCalibrationWriter(string userFolder, EyeTrackingProviderInterface eyeTrackerObject, bool isCalibration)
    {
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();

        if(isCalibration)
        { 
            this.eventFile = Path.Combine(userFolder, "depthCalibration.csv");
            Debug.Log("Saving calibration file to " + eventFile);
        }
        else
        {
            this.eventFile = Path.Combine(userFolder, "depthCalibration_eval.csv");
            Debug.Log("Saving evaluation file to " + eventFile);

        }

        this.depthCalibrationWriter = new StreamWriter(eventFile);
        this.depthCalibrationWriter.WriteLine(header);
        this.depthCalibrationWriter.Flush();

    }


    public void startWriting()
    {
        Debug.Log("DEPTH CALIBRATOR: Started writing");
        string msg = (getCurrentSystemTimestamp()).ToString(CultureInfo.InvariantCulture) + "\t" + "Data collection started\t" + "DepthCalibration\t";
        this.isWriting = true;
        this.depthCalibrationWriter.WriteLine(msg);
        this.depthCalibrationWriter.Flush();

    }

    public void stopWriting()
    {
        Debug.Log("DEPTH CALIBRATOR: Stopped writing");

        string msg = (getCurrentSystemTimestamp()).ToString(CultureInfo.InvariantCulture) + "\t" + "Data collection ended\t" + "DepthCalibration\t";
        this.depthCalibrationWriter.WriteLine(msg);
        this.depthCalibrationWriter.Flush();
        this.depthCalibrationWriter.Close();
        this.isWriting = false;

        this.depthCalibrationWriter = null;

    }


    public long getCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }


    internal void writeMsg(double currentDistance, Vector3 currentGazePoint2D, Vector3 currentGazeOrigin_R, Vector3 currentGazeOrigin_L, Vector3 currentGazeDirection_R, Vector3 currentGazeDirection_L, double estimatedDepth)
    {
        if(this.isWriting)
        { 
        string sampleLine = getCurrentSystemTimestamp().ToString(CultureInfo.InvariantCulture) + "\t sample\t" + "DepthCalibration\t" + currentDistance.ToString(CultureInfo.InvariantCulture) +"\t" + 
                currentGazePoint2D.x.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazePoint2D.y.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazePoint2D.z.ToString(CultureInfo.InvariantCulture) + "\t" +
                currentGazeOrigin_R.x.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazeOrigin_R.z.ToString(CultureInfo.InvariantCulture) + "\t" +
                currentGazeOrigin_L.x.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazeOrigin_L.z.ToString(CultureInfo.InvariantCulture) + "\t" +
                currentGazeDirection_R.x.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazeDirection_R.y.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazeDirection_R.z.ToString(CultureInfo.InvariantCulture) + "\t" +
                currentGazeDirection_L.x.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazeDirection_L.y.ToString(CultureInfo.InvariantCulture) + "\t" + currentGazeDirection_L.z.ToString(CultureInfo.InvariantCulture) + "\t" + estimatedDepth.ToString(CultureInfo.InvariantCulture);

        this.depthCalibrationWriter.WriteLine(sampleLine);
        this.depthCalibrationWriter.Flush();
        }
    }

    public void Close()
    {
        this.depthCalibrationWriter.Close();    
    }

}

