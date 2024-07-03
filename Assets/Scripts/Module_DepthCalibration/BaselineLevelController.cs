/*
 * 
 * depthfile evaluatino writer has no access! 
 * add a second depthcalöibratino writer to it
 */


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

    
    string instructionImagePath = "Images/InstructionsMap_startIPD";
    string instructionImagePath2 = "Images/InstructionsMap_startCalibration";
    string instructionImagePath3 = "Images/InstructionsMap_testRun";

    GameObject room;
    private GameObject interiorScene;
    private XTAL_ControllerInput xtalControllerEventChecker;
    private bool isAllowedToContinue;
    private int continueToNextPhase;
    private bool isIPDReady;
    private bool isETReady;
    Canvas instructionCanvas;
    Image instructionsImage;
    private bool readyForTask;
    private bool isHMDPositionReady;
    private bool isTaskRunning;
    private StimulusManager screen;
    private bool isAnswerGiven;
    public int stimulusIndex1, stimulusIndex2;

    private float stim1_posx = 0.5f;
    private float stim1_posy = 0.5f;
    private float stim2_posx = 0.5f;
    private float stim2_posy = 0.5f;
    public int[] permutation1, permutation2;
    public int[] screenPermutation;
    public bool match;

    public event OnSaveMsgToFile OnSaveMsgToFileEvent;
    public event OnSaveCamMovement OnSaveCamMovement;


    // Awake/Enable/Start
    public override void StartState()
    {

        instructionCanvas = GameObject.FindObjectOfType<Canvas>();
        instructionsImage = GameObject.Find("InstructionsImage").GetComponent<Image>();
        screen = GameObject.Find("StimulusManagerGO").GetComponent<StimulusManager>();

        isAllowedToContinue = false;
        isAnswerGiven = false;

        continueToNextPhase = 0;
        readyForTask = false;
        isIPDReady = false;
        isETReady = false;

        userFolder = ExperimentController._model.GetUserFolder();

        Debug.Log("userfolder is " +userFolder);

        /* Eye Tracking Controller Methods */
        this.etController = ExperimentController._model.Zero;
        //this.etController = new ETController(Providers.XTAL);
        this.gazeTracker = new GazeTracker(userFolder + "/Baseline/", this.etController.etpc.eyeTrackingProviderInterface, null, this);
        this.etController.etpc.eyeTrackingProviderInterface.isQueueGazeSignal = true;

        //this.xtalControllerEventChecker = ExperimentController._model.xtalControllerController;
        //this.xtalControllerEventChecker.isCheckingUpdates = true;
        //this.xtalControllerEventChecker.isQuestionnaireControl = false;
        //this.xtalControllerEventChecker.PressedBEvent += ReceiveUserAnswer;
        //this.xtalControllerEventChecker.PressedAEvent += ReceiveUserAnswer;
        //this.xtalControllerEventChecker.ProceedButtonPressed += ProceedWithExperiment;

        ExperimentController._model.isHardwarCalibrated = false;


        this.gazeTracker.startGazeWriting();

        this.interiorScene = GameObject.Find("Interior");

        setInstructionImageToPanel(instructionImagePath);

        instructionCanvas.worldCamera = Camera.main;
        instructionCanvas.enabled = true;

        //OnActivateET();

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



    // Finalize after finished scene
    public override void EndState()
    {
        OnSaveMsgToFileEvent?.Invoke("Baseline recording ended");
        this.gazeTracker.stopWriting();

        this.gazeTracker = null;
        this.etController.stop();
        //this.etController = null;

    }



    void OnActivateET()
    {
            Debug.Log("OnActivate ET called in Baseline!");
            this.etController.startET();            



    }

    void Update()
    {
        OnSaveCamMovement?.Invoke(Camera.main);


        if (Input.GetKeyUp(KeyCode.Space))
        {
            ProceedWithExperiment();
        }

        // ET CALIB: Calibrate EYE Tracker anytime
        if (isAllowedToContinue && !isIPDReady && !isETReady)
        {
            isAllowedToContinue = false;
            OnSaveMsgToFileEvent?.Invoke("Eye tracking calibration started.");
            Debug.Log("Started calibration");
            this.etController.etpc.CalibratePositionAndIPD();
            isIPDReady = true;
            setInstructionImageToPanel(instructionImagePath2);


        }


        // ET CALIB: Calibrate EYE Tracker anytime
        if (isAllowedToContinue && isIPDReady && !isETReady)
        {
            isAllowedToContinue = false;
            OnSaveMsgToFileEvent?.Invoke("Eye tracking calibration started.");
            Debug.Log("Started calibration");
            this.etController.etpc.CalibrateEyeTracker();
            StartCoroutine(SetCalibrationFinished());           // waits end sets ready to true
            isETReady = true;
            readyForTask = true;

        }


        //// START Task (3x Repetitions a 2 position combos a 10 Trials
        if (isAllowedToContinue && isETReady && readyForTask)
        {

            if (!isTaskRunning)
            {
                OnActivateET();
                isTaskRunning = true;
                StartCoroutine(ShowStimuliStatic());       
            }
        }



        /*
        * Keyboard actions for researcher
        */

        // calibrate IPD
        if (Input.GetKeyUp(KeyCode.A))
        {
            OnSaveMsgToFileEvent?.Invoke("IPD calibration started.");
            this.etController.etpc.CalibratePositionAndIPD();
        }

        // Calibrate eye tracker
        if (Input.GetKeyUp(KeyCode.C))
        {
            OnSaveMsgToFileEvent?.Invoke("ET calibration started.");
            this.etController.etpc.CalibrateEyeTracker();
        }


        // LEAVE: If we want to skip the current scene
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            //instructionCanvas.enabled = false;

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


    // ================================================= Training Stimulus display and procedure control

    IEnumerator ShowStimuliStatic()
    {
        OnSaveMsgToFileEvent?.Invoke("Baseline recording started");
        this.gazeTracker.startGazeWriting();

        isTaskRunning = true;
        // Random decision about which position to start
        Array values = Enum.GetValues(typeof(PositionCombo));
        PositionCombo currentRandomPosition;

        // for each position combo repeat number of trials
        for (int i = 0; i < 1; i++)
        {
            currentRandomPosition = (PositionCombo)values.GetValue(i);

            // show 4 trials
            for (int j = 0; j < 4; j++)
            {

                SetStimuli(8, currentRandomPosition);

                // waiting for answer from user to continue
                while (!isAnswerGiven)
                {
                    yield return null;
                }
                isAnswerGiven = false;

            }
        }
        // Finished the scene => continue to next state
        ExperimentController.ChangeToNextStateOfExperiment();
    }

    IEnumerator SetCalibrationFinished()
    {
        yield return new WaitForSeconds(8);
        isETReady = true;
        Debug.Log("Waited 8 sec for calibration to finish");
        setInstructionImageToPanel(instructionImagePath3);

    }

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



        match = (UnityEngine.Random.Range(0.0f, 1.0f) <= 0.7f); // random match vs. non-match, following the defined matchRate

        stimulusIndex1 = UnityEngine.Random.Range(0, stimulusMaxInd); // random index for screen 1


        if (match) // for matching stimuli, choose the two indices to have matching permutations
        {
            stimulusIndex2 = 0; // start at zero and loop through indices until permutation matches
            while (!MatchInPermutation(stimulusIndex1, stimulusIndex2, permutation1, permutation2))
            {
                stimulusIndex2++;
            }
        }
        else // stimuli should not match -> they should have different permutation index
        {
            stimulusIndex2 = UnityEngine.Random.Range(0, stimulusMaxInd); // random index for stimulus on screen 2
            while (MatchInPermutation(stimulusIndex1, stimulusIndex2, permutation1, permutation2)) // in case they match get new index2
            {
                stimulusIndex2 = UnityEngine.Random.Range(0, stimulusMaxInd);
            }

        }

        // get textures and show on screens
        screen.SetStimulusTex(screenPermutation[0], screen.GetStimulusTable(1, 2, permutation1, permutation2));
        screen.SetStimulusTex(screenPermutation[1], screen.GetScreenTex(1, stimulusIndex1, stim1_posx, stim1_posy));
        screen.SetStimulusTex(screenPermutation[2], screen.GetScreenTex(2, stimulusIndex2, stim2_posx, stim2_posy));


    }

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

    public void ReceiveUserAnswer(bool isPositiveAnswer)
    {
         isAnswerGiven = true;
    }

}
