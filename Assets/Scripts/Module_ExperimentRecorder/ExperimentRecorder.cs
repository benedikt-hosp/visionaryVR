using System;
using System.Collections;
using System.IO;
using UnityEngine;




public class ExperimentRecorder
{

    private string directoryPath;    
    public bool isWritingMovements = false;

    // Timing features
    private float baseTime;

    // Stream Writers
    public StreamWriter movementWriter;
    public StreamWriter autofocalDistanceWriter;
    private string cameraMovementFile;
    private string autoFocalDistanceFile;
    private string camMovementsHeader = "System Timestamp\tRelative Timestamp\tCameraRotationX\tCameraRotationY\tCameraRotationZ\tCameraRotationW\tCameraPositionX\tCameraPositionY\tCameraPositionZ\tCameraScaleX\tCameraScaleY\tCameraScaleZ";
    private string depthHeader = "System Timestamp\tDepth\tMethod";


    public ExperimentRecorder(string path, EyeTrackingProviderInterface eyeTrackingProviderInterface, ConditionController cCon, BaselineLevelController blCon)
    {
        baseTime = Time.time * 1000;
        this.directoryPath = path;
        this.cameraMovementFile = Path.Combine(this.directoryPath + "cameraMovements.csv");
        this.movementWriter = new StreamWriter(this.cameraMovementFile);

        // write header to file
        this.movementWriter.WriteLine(camMovementsHeader);
        this.movementWriter.Flush();


        if (cCon != null)
        {
            cCon.OnSaveCamMovement += WriteCamSampleToFile;
            cCon.OnSaveAutofocalChange += WriteAutofocalChangeToFile;
        }

        if (blCon != null)
     
            blCon.OnSaveCamMovement += WriteCamSampleToFile;


        this.autoFocalDistanceFile = Path.Combine(this.directoryPath + "autofocalDepths.csv");
        this.autofocalDistanceWriter = new StreamWriter(this.autoFocalDistanceFile);

        // write header to file
        this.autofocalDistanceWriter.WriteLine(depthHeader);
        this.autofocalDistanceWriter.Flush();

    }

    public void WriteAutofocalChangeToFile(float newDepth, string method)
    {
        if(this.isWritingMovements)
        { 
            string msg = (getCurrentSystemTimestamp()).ToString() + "\t" + newDepth.ToString() + "\t" + method;
            this.autofocalDistanceWriter.WriteLine(msg);
            this.autofocalDistanceWriter.Flush();

        }

    }

    public void start()
    {
        this.isWritingMovements = true;
    }

    public void stopWriting()
    {
        this.isWritingMovements = false;
        this.movementWriter.Close();
        this.movementWriter.Dispose();

        this.autofocalDistanceWriter.Close();
        this.autofocalDistanceWriter.Dispose();
    }

    public void WriteCamSampleToFile(Camera cam)
    {
        if (this.isWritingMovements)
        {
            this.WriteCamData(cam);

        }
    }

    private void WriteCamData(Camera cam)
    {
        var relativeTimeStamp = (Time.time * 1000) - baseTime;
        string newLine = (getCurrentSystemTimestamp()).ToString() + "\t" + relativeTimeStamp + "\t" +
            cam.transform.rotation.x + "\t" + cam.transform.rotation.y + "\t" + cam.transform.rotation.z + "\t" + cam.transform.rotation.w + "\t" +
            cam.transform.position.x + "\t" + cam.transform.position.y + "\t" + cam.transform.position.z + "\t" +
            cam.transform.localScale.x + "\t" + cam.transform.localScale.y + "\t" + cam.transform.localScale.z;
        this.movementWriter.WriteLine(newLine);
        this.movementWriter.Flush();
    }
    public long getCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }

}
