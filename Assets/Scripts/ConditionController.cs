using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using WebSocketSharp;
using System.Globalization;
using OpenCVForUnity.CoreModule;
using static UnityEngine.ParticleSystem;
using System.Diagnostics;
using UnityEngine.Device;
using Debug = UnityEngine.Debug;
using System.Collections;
using Source.ExperimentManagement;
using Unity.VisualScripting;
using SharpLearning.Containers.Extensions;
using Random = System.Random;
using UnityEngine.UI;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

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
    //public static int cropWidth = 
    //public static int cropHeight = 100;

    /* Public variables */
    public int nTrials;
    public int trial;
    public float matchRate; // how many of the trials should have a match
    public DateTime stopwatch;
    public float gtDepth;
    public Condition currentCondition;
    public int repetitions = 3;
    private int currentRepetition;
    Random random;
    public int[] trialsGT;
    public string[] trialPosition;

    public int[] permutation1, permutation2;
    public int[] screenPermutation;
    public bool match;
    public int stimulusIndex1, stimulusIndex2;

    // Flags to continuation of the experiment by the user
    bool isETReady;


    // Video Writing Threads
    Thread depthMapRecordingThread;
    Thread rgbMapRecordingThread;
    public SaveThreadObject saveDepthVideo;
    public SaveThreadObject saveRGBVideo;


    // Array / lists / Collections
    public int[,] trialStimulus;
    public int[] trialAnswer;
    public float[] trialTimes;
    List<double[]> depthSamples = new List<double[]>();
    List<double> estimatedDepths;



    // Booleans
    //public bool isDepthCalibration = true;
    public bool isDepthCalibration;
    public bool isDepthCalibrated = false;
    public bool isDepthCalibrationModelTrained;
    public double estimatedDepth;
    public string conditionFolder;
    bool isTaskRunning = false;
    bool isAnswerGiven = false;
    //bool nextTrial = false;



    /* Private Variables */
    private bool isEndOfScene = false;
    private Vector3 currentGazePoint2D;
    private Vector3 currentGazeOrigin_L;
    private Vector3 currentGazeOrigin_R;
    private Vector3 currentGazeDirection_L;
    private Vector3 currentGazeDirection_R;
    private string userFolder;
    private string performanceMeasuresFile;
    private string performanceMeasuresFile2;
    float stim1_posx;
    float stim1_posy;
    float stim2_posx;
    float stim2_posy;


    /* Events */
    public event SaveMSG OnSaveMsgEvent;
    public event OnSaveCamMovement OnSaveCamMovement;
    public event OnSaveAutofocalChange OnSaveAutofocalChange;

    /* Objects */
    ETController etController;
    ExperimentRecorder experimentRecorder;
    GazeTracker gazeTracker;
    DepthCalibrationWriter depthCalibrationWriter;
    DepthCalibrationWriter depthEvaluationWriter;

    XTAL_ControllerInput xtalControllerEventChecker;
    Autofocal_Controller autofocal_Controller;
    StimulusManager screen;
    GazeDepthmapEstimator smartVEstimator;
    DepthModel depthModel;

    /* GameObjects */
    private GameObject interiorScene;
    private GameObject calibrationRoom;
    GameObject camObj;
    GameObject depthCalibrationGO;
    GameObject sun;
    Canvas instructionCanvas;
    Image instructionsImage;
    
    string instructionImagePath = "Images/InstructionsMap_startIPD";
    string instructionImagePath2 = "Images/InstructionsMap_startCalibration";

    string instructionImagePath3 = "Images/InstructionsMap_general";


    private int continueToNextPhase;
    bool isAllowedToContinue;
    private bool isDepthEvaluated;
    private bool isReadyForTask;
    private bool instructionDeactivated;
    private bool isETStarted;
    private bool isIPDReady;

    public override void StartState()
    {
        // Initialize scene managing variables
        isReadyForTask = false;
        isETStarted = false;
    

        instructionDeactivated = false;
        isAllowedToContinue = false;
        isDepthCalibrated = false;
        isETReady = false;
        isIPDReady = false;
        continueToNextPhase = 0;
        this.estimatedDepths = new List<double>();
        currentRepetition = repetitions;
        random = new Random();


        // Find UI components
        instructionCanvas = GameObject.FindObjectOfType<Canvas>();
        instructionsImage = GameObject.Find("InstructionsImage").GetComponent<Image>();
        instructionCanvas.worldCamera = Camera.main;
        instructionCanvas.enabled = true;
        interiorScene = GameObject.Find("Interior");
        calibrationRoom = GameObject.Find("CalibrationRoom");

        sun = GameObject.Find("SUN");


        // Get globale configuration variables
        matchRate = ExperimentController._model.GetMatchRate();
        trial = ExperimentController._model.GetTrial();
        nTrials = ExperimentController._model.GetNrOfTrials();

        // Read in current scene command (which condition)
        var currentCommand = ExperimentController._model.ReturnCommandOfRunningScene().Split((','));
        currentCondition = (Condition)Int16.Parse(currentCommand[0]);
        ExperimentController._model.SetCurrentCondition(currentCondition.ToString());
        Debug.Log("Started condition " + currentCondition.ToString());

        // Read current user folder to save files to
        userFolder = ExperimentController._model.GetUserFolder();


        this.camObj = Camera.main.gameObject;

        // Setup eye tracking object
        this.etController = ExperimentController._model.Zero;
 
        EyeTrackingProviderInterface.OnCalibrationStartedEvent += PauseGame;
        EyeTrackingProviderInterface.OnCalibrationSucceededEvent += ResumeGame;


        // Setup controller objects
        /*this.xtalControllerEventChecker = ExperimentController._model.xtalControllerController;
        this.xtalControllerEventChecker.isCheckingUpdates = true;
        this.xtalControllerEventChecker.isQuestionnaireControl = false;
        this.xtalControllerEventChecker.PressedBEvent += ReceiveUserAnswer;
        this.xtalControllerEventChecker.PressedAEvent += ReceiveUserAnswer;
        this.xtalControllerEventChecker.ProceedButtonPressed += ProceedWithExperiment;*/



        // Condition 1:  Activate manual objects
        if (currentCondition == Condition.Manual)
        {
            instructionImagePath3 = "Images/InstructionsMap_manual2";
            conditionFolder = "/Manual/";
            interiorScene.SetActive(true);
            calibrationRoom.SetActive(false);
            sun.SetActive(true);

            this.xtalControllerEventChecker.ThumbstickPushedForwardEvent += jumpForwardInFocusPlane;
            this.xtalControllerEventChecker.ThumbstickPushedbackwardsEvent += jumpBackInFocusPlane;
        }

        // Condition 2: Activate gaze controllers
        if (currentCondition == Condition.Gaze)
        {
            conditionFolder = "/Gaze/";
            //this.autofocal_Controller = new Autofocal_Controller();
            interiorScene.SetActive(true);
            calibrationRoom.SetActive(false);
            sun.SetActive(true);
            this.etController.getSetEyetrackingProvider.getSetETProvider.NewGazesampleReady += SetSpecificFocusPlaneByGaze;

        }


        // Condition 3: Activate vergence objects
        if (currentCondition == Condition.Vergence)
        {
            conditionFolder = "/Vergence/";
            instructionCanvas.enabled = false;

            interiorScene.SetActive(false);
            calibrationRoom.SetActive(true);
            sun.SetActive(false);

            /* Vergence Depth Calibration Methods */
            DepthCalibration.OnDepthCalibrationStartedEvent += startWriting;
            DepthCalibration.OnDepthCalibrationFinishedEvent += stopWriting;

            this.etController.etpc.eyeTrackingProviderInterface.NewGazesampleReady += onCalibrationSampleReady;

            this.depthModel = new DepthModel(userFolder + conditionFolder);
            this.depthCalibrationGO = new GameObject();
            this.depthCalibrationGO.AddComponent<DepthCalibration>();
            this.depthCalibrationGO.name = "DepthCalibrationSequenceController";


            this.depthCalibrationWriter = new DepthCalibrationWriter(userFolder + conditionFolder, this.etController.etpc.eyeTrackingProviderInterface, true);
            this.depthEvaluationWriter = new DepthCalibrationWriter(userFolder + conditionFolder, this.etController.etpc.eyeTrackingProviderInterface, false);



        }
     
        // show instruction image
        setInstructionImageToPanel(instructionImagePath);





        // Create condition configured objects
        gazeTracker = new GazeTracker(userFolder + conditionFolder, this.etController.etpc.eyeTrackingProviderInterface, this, null);
        experimentRecorder = new ExperimentRecorder(userFolder + conditionFolder, this.etController.etpc.eyeTrackingProviderInterface, this, null);

        // Write performance file with header
        performanceMeasuresFile = userFolder + conditionFolder + "stimuli.csv";
        performanceMeasuresFile2 = userFolder + conditionFolder + "performance.csv";
        using (StreamWriter sw = File.AppendText(performanceMeasuresFile2))
        {
            sw.WriteLine("Screen1\tScreen2\tScreen3\tUserAnswer\tTime\tGT\tPosition");
            sw.Flush();
        }


        // Adding calibration room to scene
        calibrationRoom.transform.parent = Camera.main.transform;
        //Vector3 offset = new Vector3(3, 3, -13);
        Vector3 offset = new Vector3(0, 5, -13);
        calibrationRoom.transform.localPosition = offset;
        calibrationRoom.transform.localRotation = Quaternion.identity;

        // Setting task variables
        screen = GameObject.Find("StimulusManagerGO").GetComponent<StimulusManager>();
        trialAnswer = new int[nTrials];
        trialTimes = new float[nTrials];
        trialsGT = new int[nTrials];
        trialStimulus = new int[nTrials, 3];
        trialPosition = new string[nTrials];

    }

    public override void EndState()
    {
        unregisterAllGazeListeners();

        if(this.camObj.GetComponent<QueueRenderTexture>() != null)
            this.camObj.GetComponent<QueueRenderTexture>().recordingStarted = false;



        OnSaveMsgEvent?.Invoke("Task finished");
        Debug.Log("EndState called!");

        OnSaveMsgEvent?.Invoke("Condition " + currentCondition.ToString() + " recording ended");
        if (gazeTracker != null)
            gazeTracker.stopWriting();
        gazeTracker = null;

        if (saveDepthVideo != null)
        { 
            saveDepthVideo.isRunning = false;
        }

        if (saveRGBVideo != null)
        {
            saveRGBVideo.isRunning = false;
        }



        if (this.depthCalibrationWriter != null)
        {
            this.depthCalibrationWriter.Close();
            this.depthCalibrationWriter = null;
        }

        if (this.depthEvaluationWriter != null)
        {
            this.depthEvaluationWriter.Close();
            this.depthEvaluationWriter = null;
        }

        if (depthCalibrationGO != null)
        {
            Destroy(depthCalibrationGO.GetComponent<DepthCalibration>().target);
            Destroy(depthCalibrationGO);
        }

        calibrationRoom.SetActive(false);
        sun.SetActive(true);


        if (rgbMapRecordingThread != null)
            rgbMapRecordingThread.Join();

        if (depthMapRecordingThread != null)
            depthMapRecordingThread.Join();

        if (this.autofocal_Controller != null)
            this.autofocal_Controller = null;

        if (VrgHmd.mainCameraRig.leftGO.GetComponent<Defocus>() != null)
            Destroy(VrgHmd.mainCameraRig.leftGO.GetComponent<Defocus>());

        if (VrgHmd.mainCameraRig.rightGO.GetComponent<Defocus>() != null)
            Destroy(VrgHmd.mainCameraRig.rightGO.GetComponent<Defocus>());

        if (experimentRecorder != null)
            experimentRecorder.stopWriting();
        experimentRecorder = null;


        if (this.etController != null)
            this.etController = null;
            Debug.Log("Finished EndState");




    }

    void OnActivateET()
    {

        isETStarted = true;
        Debug.Log("OnActivate ET called!");
        this.etController.startET();
        this.autofocal_Controller = new Autofocal_Controller();
        OnSaveMsgEvent?.Invoke("Condition " + currentCondition.ToString() + " started.");
        gazeTracker.startGazeWriting();
        experimentRecorder.start();
    }

    private void unregisterAllGazeListeners()
    {
        // manual
        xtalControllerEventChecker.ThumbstickPushedForwardEvent -= jumpForwardInFocusPlane;
        xtalControllerEventChecker.ThumbstickPushedbackwardsEvent -= jumpBackInFocusPlane;

        // gaze
        this.etController.getSetEyetrackingProvider.getSetETProvider.NewGazesampleReady -= SetSpecificFocusPlaneByGaze;

        // vergence
        this.etController.getSetEyetrackingProvider.getSetETProvider.NewGazesampleReady -= SetSpecificFocusPlaneByVergence;
        this.etController.getSetEyetrackingProvider.getSetETProvider.NewGazesampleReady -= onCalibrationSampleReady;

        DepthCalibration.OnDepthCalibrationStartedEvent -= startWriting;
        DepthCalibration.OnDepthCalibrationFinishedEvent -= stopWriting;

    }

    private void stopWriting()
    {
        Debug.Log("Called stop writing");
        if (isDepthCalibration)
        {
            Debug.Log("Stopped calib writer");
            isDepthCalibration = false;

            if (this.depthCalibrationWriter != null)
            {
                this.depthCalibrationWriter.stopWriting();
                this.depthCalibrationWriter = null;
            }

            if (this.depthModel.trainOnline())
            {
                Debug.Log("Training finished");
                isDepthCalibrationModelTrained = true;
            }
            else
            {
                Debug.Log("Could not train.");
            }
            

        }
        else
        {
            Debug.Log("Stopped eval writer too");
            isDepthCalibrated = true;
            depthCalibrationGO.GetComponent<DepthCalibration>().target.SetActive(false);
            interiorScene.SetActive(true);
            calibrationRoom.SetActive(false);
            sun.SetActive(true);

            this.etController.etpc.eyeTrackingProviderInterface.NewGazesampleReady -= onCalibrationSampleReady;
            this.depthEvaluationWriter.stopWriting();
            this.depthEvaluationWriter = null;

            this.etController.getSetEyetrackingProvider.getSetETProvider.NewGazesampleReady += SetSpecificFocusPlaneByVergence;

            instructionCanvas.enabled = true;

        }
    }

    private void startWriting()
    {
        if (isDepthCalibration)
        {
            this.depthCalibrationWriter.startWriting();
        }
        else
        {
            isDepthCalibration = false;         // seems important
            this.depthEvaluationWriter.startWriting();
        }
    }

    public void onCalibrationSampleReady(SampleData gazeData)
    {
        // Target GT
        if (gazeData != null && gazeData.eye == Eye.Both)
        {
            Ray ray = new Ray(gazeData.worldGazeOrigin, gazeData.worldGazeDirection);
            currentGazePoint2D = ray.GetPoint(gazeData.vergenceDepth);
            currentGazeOrigin_L = gazeData.worldGazeOrigin_L;
            currentGazeOrigin_R = gazeData.worldGazeOrigin_R;
            currentGazeDirection_L = gazeData.worldGazeDirection_L;
            currentGazeDirection_R = gazeData.worldGazeDirection_R;


            gtDepth = depthCalibrationGO.GetComponent<DepthCalibration>().get_target_distance;


            if (isDepthCalibration)
            {
                estimatedDepth = -1;
                this.depthCalibrationWriter.writeMsg(gtDepth, currentGazePoint2D, currentGazeOrigin_R, currentGazeOrigin_L, currentGazeDirection_R, currentGazeDirection_L, estimatedDepth);
            }
            else
            {
                if (isDepthCalibrationModelTrained)
                {
                    //Debug.Log("DIST eval: " + Math.Sqrt(gtDepth));

                    double[] sample = new double[6] { currentGazeDirection_R.x, currentGazeDirection_R.y, currentGazeDirection_R.z, currentGazeDirection_L.x, currentGazeDirection_L.y, currentGazeDirection_L.z };
                    depthSamples.Add(sample);

                    estimatedDepth = this.depthModel.PredictOnline(depthSamples.ToF64Matrix(), gtDepth);
                    depthSamples.Clear();
                    

                    Debug.Log("True depth: " + Math.Sqrt(gtDepth).ToString(CultureInfo.InvariantCulture) + " vs Estimated depth: " + estimatedDepth.ToString(CultureInfo.InvariantCulture));
                    this.depthEvaluationWriter.writeMsg(Math.Sqrt(gtDepth), currentGazePoint2D, currentGazeOrigin_R, currentGazeOrigin_L, currentGazeDirection_R, currentGazeDirection_L, Math.Abs(estimatedDepth));

                }
                else
                    Debug.LogError("XGBoost Model is not trained! OR writing is not enabled.");
            }
        }
    }

    // Condition 1: manual
    public void jumpForwardInFocusPlane()
    {
        Debug.Log("Jumped forward");
        if (this.autofocal_Controller == null)
            this.autofocal_Controller = new Autofocal_Controller();
        //if (this.autofocal_Controller == null)
        //    return;
        switch (currentDepth)
        {
            case FocusPlane.near:
                this.autofocal_Controller.SetFocusDistance(FocusPlane.mid);
                OnSaveAutofocalChange?.Invoke(1.0f, "Controller");
                currentDepth = FocusPlane.mid;
                break;
            case FocusPlane.mid:
                this.autofocal_Controller.SetFocusDistance(FocusPlane.far);
                OnSaveAutofocalChange?.Invoke(6.0f, "Controller");
                currentDepth = FocusPlane.far;
                break;
            case FocusPlane.far:
                this.autofocal_Controller.SetFocusDistance(FocusPlane.far);
                OnSaveAutofocalChange?.Invoke(6.0f, "Controller");
                currentDepth = FocusPlane.far;
                break;
            default:
                break;
        }
    }

    public void jumpBackInFocusPlane()
    {
        Debug.Log("Jumped backwards");
        if (this.autofocal_Controller == null)
            this.autofocal_Controller = new Autofocal_Controller();
        //if (this.autofocal_Controller == null)
        //    return;

        switch (currentDepth)
        {
            case FocusPlane.near:
                this.autofocal_Controller.SetFocusDistance(FocusPlane.near);
                OnSaveAutofocalChange?.Invoke(0.3f, "Controller");
                currentDepth = FocusPlane.near;
                break;
            case FocusPlane.mid:
                this.autofocal_Controller.SetFocusDistance(FocusPlane.near);
                OnSaveAutofocalChange?.Invoke(0.3f, "Controller");
                currentDepth = FocusPlane.near;
                break;
            case FocusPlane.far:
                this.autofocal_Controller.SetFocusDistance(FocusPlane.mid);
                OnSaveAutofocalChange?.Invoke(1.0f, "Controller");
                currentDepth = FocusPlane.mid;
                break;
            default:
                break;
        }
    }

    // Condition 2: gaze
    public void SetSpecificFocusPlaneByGaze(SampleData sd)
    {
        if (this.autofocal_Controller == null)
            this.autofocal_Controller = new Autofocal_Controller();

        Debug.Log("Gaze vector:" + sd.worldGazeDirection.ToString());

        // Debug.DrawRay(sd.worldGazeOrigin, sd.worldGazeDirection, Color.green, 100, false);

        if (sd != null)
        {
            Ray ray = new Ray(sd.worldGazeOrigin, sd.worldGazeDirection);         // ON AIR 
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
    }

    // Condition 3: vergence
    public void SetSpecificFocusPlaneByVergence_pure(SampleData sd)
    {
        Debug.DrawRay(sd.worldGazeOrigin, sd.worldGazeDirection, Color.green, 100, false);

        Debug.Log("Listener called");
        if (this.autofocal_Controller == null)
            this.autofocal_Controller = new Autofocal_Controller();

        if (sd != null && this.autofocal_Controller != null && sd.eye == Eye.Both)
        {
            var vergenceDepth = sd.vergenceDepth;
            Debug.Log("Vergence depth: " + vergenceDepth.ToString());

            OnSaveAutofocalChange?.Invoke((float)vergenceDepth, "Vergence");
            this.autofocal_Controller.SetFocusDistance(Math.Abs(vergenceDepth));

        }
    }

    public void SetSpecificFocusPlaneByVergence(SampleData sd)
    {
        //Debug.DrawRay(sd.worldGazeOrigin, sd.worldGazeDirection, Color.green, 100, false);

        Debug.Log("Listener called");
        if (this.autofocal_Controller == null)
            this.autofocal_Controller = new Autofocal_Controller();

        if (sd != null && this.autofocal_Controller != null && sd.eye == Eye.Both)
        {
            depthSamples.Clear();
            double[] sample = new double[6] { sd.worldGazeDirection_R.x, sd.worldGazeDirection_R.y, sd.worldGazeDirection_R.z,
                sd.worldGazeDirection_L.x, sd.worldGazeDirection_L.y, sd.worldGazeDirection_L.z };
            depthSamples.Add(sample);

            estimatedDepth = this.depthModel.PredictOnline(depthSamples.ToF64Matrix(), -1);

            //estimatedDepth = Math.Abs(Math.Log10((double)estimatedDepth) * 10);

            //estimatedDepth = sd.vergenceDepth;  // ON AIR

            Debug.Log("Vergence depth: " + estimatedDepth.ToString());

            OnSaveAutofocalChange?.Invoke((float)estimatedDepth, "Vergence");
            this.autofocal_Controller.SetFocusDistance(Math.Abs((float)estimatedDepth));

        }
    }
    void Update()
    {

        OnSaveCamMovement?.Invoke(Camera.main);

        if (Input.GetKeyUp(KeyCode.Space))
        {
            ProceedWithExperiment();
        }

        // 1. AutoIPD CALIB: Calibrate AutoIPD
        if (isAllowedToContinue && !isIPDReady && !isETReady)
        {
            Debug.Log("Starte AutoIPD");
            isAllowedToContinue = false;
            OnSaveMsgEvent?.Invoke("AutoIPD  started.");
            this.etController.etpc.CalibratePositionAndIPD();
            isIPDReady = true;
            setInstructionImageToPanel(instructionImagePath2);


        }



        // 2. ET CALIB: Calibrate EYE Tracker anytime
        if (isAllowedToContinue && isIPDReady && !isETReady)
        {
            Debug.Log("Starte Calibration");
            isAllowedToContinue = false;
            OnSaveMsgEvent?.Invoke("Eye tracking calibration started.");
            this.etController.etpc.CalibrateEyeTracker();
            StartCoroutine(SetCalibrationFinished());           // waits end sets ready to true
            //isETReady = true;

        }


        // DEPTH CALIBRATION: VERGENCE 
        if (isETReady && isAllowedToContinue &&  currentCondition == Condition.Vergence && !isDepthCalibrated)
        {
            OnActivateET();
            isAllowedToContinue = false;
            instructionCanvas.enabled = false;
            OnSaveMsgEvent?.Invoke("Vergence depth calibration started.");
            depthCalibrationGO.GetComponent<DepthCalibration>().startAnimation = true;
            isDepthCalibrated = true;
        }


        // DEPTH EVALUATION: VERGENCE 
        if (isETReady && isAllowedToContinue &&  currentCondition == Condition.Vergence && isDepthCalibrated && !isDepthEvaluated)
        {
            isAllowedToContinue = false;
            instructionCanvas.enabled = false;

            /* Start the movement of the stimulus 2nd time => will trigger evaluation. */
            OnSaveMsgEvent?.Invoke("Vergence depth evaluation started.");

            depthCalibrationGO.GetComponent<DepthCalibration>().startAnimation = true;
            Debug.Log("Starting vergence depth evaluation procedure.");
            isDepthEvaluated = true;

            instructionDeactivated = true;
            isReadyForTask = true;

        }

        if ((isETReady && isAllowedToContinue && currentCondition == Condition.Vergence && isDepthEvaluated && isDepthCalibrated)     // in case of vergence we first need to be sure that the model has been trained
         || (isETReady && isAllowedToContinue && currentCondition != Condition.Vergence ))                                             // in case of another condition we can simply start
        {
            //instructionCanvas.enabled = false;
            instructionDeactivated = true;
            isReadyForTask = true;
        }

        //// START Task (3x Repetitions a 2 position combos a 10 Trials
        if (isAllowedToContinue && isETReady && isReadyForTask && instructionDeactivated == true)
        {
            if (!isTaskRunning)
            {
                if(!isETStarted)
                    OnActivateET();

                isTaskRunning = true;
                currentRepetition = repetitions;
                StartCoroutine(StartConditionSequence());
            }
            else
            {
                isReadyForTask = false;
            }

        }


        if (isEndOfScene)
        {
            ExperimentController.ChangeToNextStateOfExperiment();
        }



        /*
         * Keyboard actions for researcher
         */

        // calibrate IPD
        if (Input.GetKeyUp(KeyCode.A))
        {
            OnSaveMsgEvent?.Invoke("IPD calibration started.");
            this.etController.etpc.CalibratePositionAndIPD();
        }

        // Calibrate eye tracker
        if (Input.GetKeyUp(KeyCode.C))
        {
            OnSaveMsgEvent?.Invoke("ET calibration started.");
            this.etController.etpc.CalibrateEyeTracker();
        }


        // LEAVE: If we want to skip the current scene
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            instructionCanvas.enabled = false;
            
            ExperimentController.ChangeToNextStateOfExperiment();
        }
        if(Input.GetKeyUp(KeyCode.B))
        {
            ExperimentController.ReturnToLastStateOfExperiment();
        }

        // Repeat: If we want to repeat the current condition
        if (Input.GetKeyUp(KeyCode.R))
        {
            instructionCanvas.enabled = false;
            ExperimentController.ReloadStateOfExperiment();
        }


    }

    IEnumerator SetCalibrationFinished()
    {
        yield return new WaitForSeconds(9);
        isETReady = true;
        setInstructionImageToPanel(instructionImagePath3);

    }


    private void setInstructionImageToPanel(string path)
    {
        Debug.Log("Path: " + path);
        Sprite test = Resources.Load<Sprite>(path);
        instructionsImage.sprite = test;
        instructionsImage.enabled = true;


    }


    private void ProceedWithExperiment()
    {
        isAllowedToContinue = true;
        continueToNextPhase++;
    }

    IEnumerator StartConditionSequence()
    {

        // Repetitions counter
        while (currentRepetition > 0)
        {
            //isAnswerGiven = false;

            Debug.Log("Current repetition is: " + currentRepetition.ToString());
            OnSaveMsgEvent?.Invoke("Repetition " + currentRepetition.ToString() + " started.");

            // Random decision about which position to start
            Array values = Enum.GetValues(typeof(PositionCombo));
            PositionCombo currentRandomPosition;

            // for each position combo repeat number of trials
            for (int i = 0; i < values.Length; i++)
            {
                currentRandomPosition = (PositionCombo)values.GetValue(i);

                Debug.Log("Current combo is: " + currentRandomPosition.ToString());
                OnSaveMsgEvent?.Invoke("Combo " + currentRandomPosition.ToString() + " started.");

                // repeat trial "nTrials" times
                for (int j = 0; j < nTrials; j++)
                {
                    Debug.Log("Current trial number is: " + j + "/" + nTrials);
                    OnSaveMsgEvent?.Invoke("Task " + i + " started.");

                    SetStimuli(8, currentRandomPosition);
                    StartTimer();
                    // waiting for answer from user to continue
                    while (!isAnswerGiven)
                    {
                        yield return null;
                    }
                    EndTimer();
                    writeCurrentTrialToFile();
                    isAnswerGiven = false;


                }
            }
            currentRepetition--;
        }
        isTaskRunning = false;
        Debug.Log("Reached end of third repetition. Continuing with next condition.");
        isEndOfScene = true;        // IMPORTANT
    }


    public void ReceiveUserAnswer(bool isPositiveAnswer)
    {
        if (isPositiveAnswer)
        {
            Debug.Log("Answer: is match");
            trialAnswer[trial] = 1;
            isAnswerGiven = true;
        }
        else
        {
            Debug.Log("Answer: is no match");
            trialAnswer[trial] = -1;
            isAnswerGiven = true;
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








    // ================================================================== TASK RELEVANT METHODS



    void StartTimer()
    {
        stopwatch = DateTime.Now;
    }

    void EndTimer()
    {
        trialTimes[trial] = (float)(DateTime.Now - stopwatch).TotalSeconds;
    }




    // set the stimulus params for next trial
    void SetStimuli(int stimulusMaxInd, PositionCombo currentCombo)
    {

        // get two random permutations for the "stimulus table" shown on screen 0
        permutation1 = ExperimentPreparation.RandIndexPermutation(stimulusMaxInd, false);
        permutation2 = ExperimentPreparation.RandIndexPermutation(stimulusMaxInd, false);

        // get random order of screens -> randomly change which screen shows which stimulus
        screenPermutation = ExperimentPreparation.RandIndexPermutation(3, false);

        // define stimulus position on screen TODO: depending on easy/hard condition place in center or on of the four corners


        if (currentCombo == PositionCombo.center)
        {
            stim1_posx = 0.5f;
            stim1_posy = 0.5f;
            stim2_posx = 0.5f;
            stim2_posy = 0.5f;
        }
        else if (currentCombo == PositionCombo.corners)
        {
            // 0 = totally random for each corner and x and y
            // 1 = only flip left bottom and right top corner
            // 2 = always same corner
            (stim1_posx, stim1_posy, stim2_posx, stim2_posy) = shuffleScreenPosition(0);
        }



        match = (UnityEngine.Random.Range(0.0f, 1.0f) <= matchRate); // random match vs. non-match, following the defined matchRate

        stimulusIndex1 = UnityEngine.Random.Range(0, stimulusMaxInd); // random index for screen 1


        if (match) // for matching stimuli, choose the two indices to have matching permutations
        {
            stimulusIndex2 = 0; // start at zero and loop through indices until permutation matches
            while (!MatchInPermutation(stimulusIndex1, stimulusIndex2, permutation1, permutation2))
            {
                stimulusIndex2++;
            }
            trialsGT[trial] = 1;
        }
        else // stimuli should not match -> they should have different permutation index
        {
            stimulusIndex2 = UnityEngine.Random.Range(0, stimulusMaxInd); // random index for stimulus on screen 2
            while (MatchInPermutation(stimulusIndex1, stimulusIndex2, permutation1, permutation2)) // in case they match get new index2
            {
                stimulusIndex2 = UnityEngine.Random.Range(0, stimulusMaxInd);
            }
            trialsGT[trial] = -1;

        }

        // get textures and show on screens
        screen.SetStimulusTex(screenPermutation[0], screen.GetStimulusTable(1, 2, permutation1, permutation2));
        screen.SetStimulusTex(screenPermutation[1], screen.GetScreenTex(1, stimulusIndex1, stim1_posx, stim1_posy));
        screen.SetStimulusTex(screenPermutation[2], screen.GetScreenTex(2, stimulusIndex2, stim2_posx, stim2_posy));

        trialStimulus[trial, 0] = permutation1[0];
        trialStimulus[trial, 1] = stimulusIndex1;
        trialStimulus[trial, 2] = stimulusIndex2;
        trialPosition[trial] = currentCombo.ToString();


        //TODO: add stimulus pos and screenPermutation What is the purpose of this function?
        //AddLineIntArray(performanceMeasuresFile, permutation1, permutation2); 

    }

    public void writeCurrentTrialToFile()
    {
        WritePerformanceFileLine(performanceMeasuresFile2, permutation1[0], stimulusIndex1, stimulusIndex2, trialAnswer[trial], trialTimes[trial], trialsGT[trial], trialPosition[trial]);
    }

    void WritePerformanceFileLine(string path, int trialStimulusS1, int trialStimulusS2, int trialStimulusS3, int trialsAnswer, float trialTimes, int trialGT, string trialPosition)
    {
        using (StreamWriter sw = File.AppendText(path))
        {
            sw.WriteLine(trialStimulusS1.ToString() + "\t" +
                        trialStimulusS2.ToString() + "\t" +
                        trialStimulusS3.ToString() + "\t" +
                        trialsAnswer.ToString() + "\t" +
                        trialTimes.ToString() + "\t" +
                        trialGT.ToString() + "\t" +
                        trialPosition);
            sw.Flush();
        }
    }


    public (float, float, float, float) shuffleScreenPosition(int method)
    {

        switch (method)
        {
            case 0: // totally random
                float[] values = new float[2] { 0.1f, 0.9f };
                var randomIndex = random.Next(0, values.Length);
                stim1_posx = values[randomIndex];
                randomIndex = random.Next(0, values.Length);
                stim1_posy = values[randomIndex];
                randomIndex = random.Next(0, values.Length);
                stim2_posx = values[randomIndex];
                randomIndex = random.Next(0, values.Length);
                stim2_posy = values[randomIndex];
                break;
            case 1: // switching between left bottom and top right corner
                stim1_posx = 0.9f;
                stim1_posy = 0.9f;
                stim2_posx = 0.1f;
                stim2_posy = 0.1f;
                break;
            case 2:     // always show it at left bottom corner
                stim1_posx = 0.1f;
                stim1_posy = 0.1f;
                stim2_posx = 0.1f;
                stim2_posy = 0.1f;
                break;
            default:
                values = new float[2] { 0.1f, 0.9f };
                randomIndex = random.Next(0, values.Length);
                stim1_posx = values[randomIndex];
                randomIndex = random.Next(0, values.Length);
                stim1_posy = values[randomIndex];
                randomIndex = random.Next(0, values.Length);
                stim2_posx = values[randomIndex];
                randomIndex = random.Next(0, values.Length);
                stim2_posy = values[randomIndex];
                break;

        }
        return (stim1_posx, stim1_posy, stim2_posx, stim2_posy);
    }


    void SaveData(string path)
    {
        // delete file to overwrite it
        //File.Delete(path);


        // save trial angles and answers
        ExperimentPreparation.SaveArrayToCSV(trialStimulus, path, new List<string> { "screen1", "screen2", "screen3" });
        ExperimentPreparation.SaveArrayToCSV(trialAnswer, path, "answer");
        ExperimentPreparation.SaveArrayToCSV(trialTimes, path, "duration");
        ExperimentPreparation.SaveArrayToCSV(trialsGT, path, "groundtruth");
        ExperimentPreparation.SaveArrayToCSV(trialPosition, path, "position");
        // TODO: We should also add the correct answer to the file. Then we dont need to calculate it offline afterwards.
    }

    // Check if for the given two permutations p1 and p2 there is a x so that p1(x)=stimulusIndex1 AND p2(x)=stimulusIndex2 -> maticng stimuli
    bool MatchInPermutation(int stimulusIndex1, int stimulusIndex2, int[] permutation1, int[] permutation2)
    {
        int x = 0;
        while (permutation1[x] != stimulusIndex1)
        {
            x++;
        }
        // now p1[x] = stimulusIndex1
        return (permutation2[x] == stimulusIndex2);
    }
    void AddLineIntArray(string path, int[] array1, int[] array2)
    {
        using (StreamWriter sw = File.AppendText(path))
        {
            sw.WriteLine(String.Join("\t", new List<int>(array1).ConvertAll(i => i.ToString()).ToArray())
                       + "-" + String.Join("\t", new List<int>(array2).ConvertAll(i => i.ToString()).ToArray()));
        }
    }

}
