using System;

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using System.Globalization;
using Debug = UnityEngine.Debug;
using System.Collections;
using Source.ExperimentManagement;
using Unity.VisualScripting;
using SharpLearning.Containers.Extensions;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.InputSystem;

public enum Condition
{
    Baseline,       // 0
    Manual,         // 1
    Gaze,           // 2
    Vergence       // 3
}


public enum PositionCombo
{
    center,
    corners
}

public enum Eye
{
    Both,
    Right,
    Left
}


public delegate void OnConditionStarted(string condition);
public delegate void OnConditionEnded(string condition);
public delegate void SaveMSG(string msg);

public delegate void OnSaveCamMovement(Camera cam);
public delegate void OnSaveAutofocalChange(float newDepth, string method);

public class ConditionController : ExperimentState
{
    /* Static Variables */
    public static FocusPlane currentDepth = FocusPlane.near;


    /* Public variables */
    public DateTime stopwatch;
    public Condition currentCondition;

    Random random;

    // Flags to continuation of the experiment by the user
    bool isETReady;
    bool step;

    /* Private Variables */
    private bool isEndOfScene = false;
    private Vector3 currentGazePoint2D;
    private Vector3 currentGazeOrigin_L;
    private Vector3 currentGazeOrigin_R;
    private Vector3 currentGazeDirection_L;
    private Vector3 currentGazeDirection_R;
    private string userFolder;



    /* Events */
    public event SaveMSG OnSaveMsgEvent;
    public event OnSaveCamMovement OnSaveCamMovement;
    public event OnSaveAutofocalChange OnSaveAutofocalChange;

    /* Objects */
    ETController etController;
    GazeTracker gazeTracker;
    Autofocal_Controller autofocal_Controller;
 
    /* GameObjects */
    GameObject camObj;

    private bool isETStarted;

    public override void StartState()
    {
       
        isETReady = false;
        step = false;
        this.camObj = Camera.main.gameObject;


        /* Read in current scene command to parameterize the scene
         * 
         * 
         */
        var currentCommand = ExperimentController._model.ReturnCommandOfRunningScene().Split((','));
        currentCondition = (Condition)Int16.Parse(currentCommand[0]);
        ExperimentController._model.SetCurrentCondition(currentCondition.ToString());
        Debug.Log("Started condition " + currentCondition.ToString());

        

        // Setup eye tracking object
        this.etController = ExperimentController._model.Zero;
        userFolder = this.etController.GetUserFolder();
        
        // Setup gaze tracker object
        gazeTracker = new GazeTracker(userFolder, this.etController.etpc.eyeTrackingProviderInterface, this, null);
        this.gazeTracker.startGazeWriting();
        OnActivateET();
        isETReady = true;
        
        Debug.LogWarning("Start state of condition done.");

    }

    public override void EndState()
    {
        unregisterAllGazeListeners();
             
        OnSaveMsgEvent?.Invoke("Task finished");
        Debug.Log("EndState called!");

        OnSaveMsgEvent?.Invoke("Condition " + currentCondition.ToString() + " recording ended");
        if (gazeTracker != null)
            gazeTracker.stopWriting();
        gazeTracker = null;
  
        if (this.etController != null)
            this.etController = null;
            Debug.Log("Finished EndState");
    }

    void OnActivateET()
    {
        Debug.Log("OnActivate ET called!");
        this.etController.startET();
        //this.autofocal_Controller = new Autofocal_Controller();
        OnSaveMsgEvent?.Invoke("Condition " + currentCondition.ToString() + " started.");
        
    }

    private void unregisterAllGazeListeners()
    {
       
        // gaze
        this.etController.getSetEyetrackingProvider.getSetETProvider.NewGazesampleReady -= SetSpecificFocusPlaneByGaze;

    }


    
    // Set specific focus plane of autofocal controller by gaze
    public void SetSpecificFocusPlaneByGaze(SampleData sd)
    {

    /*        
        if (this.autofocal_Controller == null)
            this.autofocal_Controller = new Autofocal_Controller();


        if (sd != null)
        {
            Ray ray = new Ray(sd.combinedEyeWorldOrigin, sd.combinedEyeWorldDirection);         // ON AIR 
            RaycastHit hit;

            // 1000000
            if (Physics.Raycast(ray, out hit, 200))
            {
                Debug.Log(hit.transform.gameObject.name);
                hit.distance *= 100000;
                if (hit.distance < 30)
                { 
                    this.autofocal_Controller.SetFocusDistance(hit.distance);
                    OnSaveAutofocalChange?.Invoke(hit.distance, "Gaze");
                    Debug.Log("Gaze mode: Setting focus distance to " + hit.distance.ToString());

                    Debug.Log("Gaze hit: " + hit.transform.position.ToString());
                }
            }
        }
        */
    }

    void Update()
    {

        OnSaveCamMovement?.Invoke(Camera.main);

        // Set isEndofScene when you want to proceed to the next scene
        if (isEndOfScene)
        {
            ExperimentController.ChangeToNextStateOfExperiment();
        }

        /*
         * Keyboard actions for researcher
         */

        // calibrate IPD if Device supports it
        if (Input.GetKeyUp(KeyCode.A))
        {
            OnSaveMsgEvent?.Invoke("IPD calibration started.");
            this.etController.etpc.CalibratePositionAndIPD();
        }

        // Calibrate eye tracker
        if (Input.GetKeyUp(KeyCode.C))
        {
            OnSaveMsgEvent?.Invoke("ET calibration started.");
            Debug.Log("Started calibration");
            this.etController.etpc.CalibrateEyeTracker();

            step = true;
        }


        // LEAVE: If we want to skip the current scene
        if (Input.GetKeyUp(KeyCode.Escape))
        {            
            ExperimentController.ChangeToNextStateOfExperiment();
        }

        //Jump back to previous scene
        if(Input.GetKeyUp(KeyCode.B))
        {
            ExperimentController.ReturnToLastStateOfExperiment();
        }

        // Repeat: If we want to repeat the current condition
        if (Input.GetKeyUp(KeyCode.R))
        {
            ExperimentController.ReloadStateOfExperiment();
        }

    }



    void PauseGame()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }
    void ResumeGame()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

}
