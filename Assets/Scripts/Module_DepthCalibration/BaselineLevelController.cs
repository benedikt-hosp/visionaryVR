using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using System.Globalization;
using System.Drawing;
using Source.ExperimentManagement;
using UnityEditor;
using SharpLearning.Containers.Matrices;
using SharpLearning.Containers.Extensions;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Random = System.Random;
using System.Runtime.InteropServices;
using ViveSR.anipal.Eye;



public delegate void OnSaveMsgToFile(string msg);


public class BaselineLevelController : ExperimentState
{
    /*
     * Public variables
     */
    public static FocusPlane currentDepth = FocusPlane.near;

    public bool isETActivated = false;
    ETController etController;
    GazeTracker gazeTracker;
    Random random;

    /*
     * Private variables    
     */

    private string userFolder;
    private bool step;
    private bool isETReady;


    public event OnSaveMsgToFile OnSaveMsgToFileEvent;
    public event OnSaveCamMovement OnSaveCamMovement;


    // Awake/Enable/Start
    public override void StartState()
    {
        step = false;
        isETReady = false;
     
        /* Eye Tracking Controller Methods */
        this.etController = ExperimentController._model.Zero;
        userFolder = this.etController.GetUserFolder();
        Debug.LogError("User Folder inBaseline is " + userFolder);
        if(this.etController.ChoseEyeTrackingProvider != Providers.None)
        {
            this.etController.etpc.eyeTrackingProviderInterface.NewGazesampleReady += ShowLiveGaze;
            this.gazeTracker = new GazeTracker(userFolder, this.etController.etpc.eyeTrackingProviderInterface, null, this);
            this.gazeTracker.startGazeWriting();
            isETReady = true;
            OnActivateET();
        }

    }

    private void ShowLiveGaze(SampleData sd)
    {
        Debug.Log("New sample received in Baseline scene. TO access it add your code to ShowLiveGaze Method");
    }





    // Finalize after finished scene
    public override void EndState()
    {
        OnSaveMsgToFileEvent?.Invoke("Baseline recording ended");
        this.gazeTracker.stopWriting();
        this.gazeTracker = null;
        this.etController.stop();

    }



    void OnActivateET()
    {
            this.etController.startET();            
    }

    void Update()
    {
        OnSaveCamMovement?.Invoke(Camera.main);


        /*
        * Keyboard actions for researcher
        */

        // Calibrate eye tracker
        if (Input.GetKeyUp(KeyCode.C))
        {
            OnSaveMsgToFileEvent?.Invoke("Eye tracking calibration started.");
            Debug.Log("Started calibration");
            this.etController.etpc.CalibrateEyeTracker();
            step = true;
        }


        // LEAVE: If we want to skip the current scene
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            //instructionCanvas.enabled = false;
            this.etController.etpc.eyeTrackingProviderInterface.stopETThread();
            ExperimentController.ChangeToNextStateOfExperiment();
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            ExperimentController.ReturnToLastStateOfExperiment();
        }


        // Repeat: If we want to repeat the current condition
        if (Input.GetKeyUp(KeyCode.R))
        {
            //instructionCanvas.enabled = false;
            ExperimentController.ReloadStateOfExperiment();
        }
    }



    IEnumerator SetCalibrationFinished()
    {
        yield return new WaitForSeconds(8);
        isETReady = true;
        Debug.Log("Waited 8 sec for calibration to finish");

    }



}
