using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vrg;
using static vrg.Controllers;


public delegate void PressedHairTrigger();
public delegate void PressedAEvent(bool isPositiveAnswer);
public delegate void PressedBEvent(bool isPositiveAnswer);
public delegate void ThumbstickPushedForwardEvent();
public delegate void ThumbstickPushedbackwardsEvent();

public delegate void PressedButtonToSubmitQuestionnaireEvent();
public delegate void ThumbstickPushedRightEvent();
public delegate void ThumbstickPushedLeftEvent();
public delegate void MoveToPreviousSliderEvent();
public delegate void MoveToNextSliderEvent();

public delegate void ProceedButtonPressed();

    /* id 0 = right controller
     * id 1 = left controller
     * 
     * */

public class XTAL_ControllerInput
{
    public event PressedHairTrigger PressedHairTrigger;
    public event PressedAEvent PressedAEvent;
    public event PressedAEvent PressedBEvent;
    public event ThumbstickPushedForwardEvent ThumbstickPushedForwardEvent;
    public event ThumbstickPushedbackwardsEvent ThumbstickPushedbackwardsEvent;

    public event ThumbstickPushedLeftEvent ThumbstickPushedLeftEvent;
    public event ThumbstickPushedRightEvent ThumbstickPushedRightEvent;
    public event MoveToPreviousSliderEvent MoveToPreviousSliderEvent;
    public event MoveToNextSliderEvent MoveToNextSliderEvent;
    public event PressedButtonToSubmitQuestionnaireEvent PressedButtonToSubmitQuestionnaireEvent;

    public event ProceedButtonPressed ProceedButtonPressed;

    /* Additional events
     * public static event pressedSystemEvent pressedSystemEvent;
    public static event pressedGripEvent pressedGripEvent;
    public static event pressedDPadLeftEvent pressedDPadLeftEvent;
    public static event pressedDPadUpEvent pressedDPadUpEvent;
    public static event pressedDPadRightEvent pressedDPadRightEvent;
    public static event pressedDPadDownEvent pressedDPadDownEvent;
    public static event pressedProximitySensorEvent pressedProximitySensorEvent;
    public static event pressedTouchPadEvent pressedTouchPadEvent;
    public static event pressedSteamTriggerEvent pressedSteamTriggerEvent;
    public static event pressedDashboardBackEvent pressedDashboardBackEvent;
    */

    public VRg_SteamVR_Controller controller_leftObj;
    public VRg_SteamVR_Controller controller_rightObj;
    private ConditionController control;
    private MonoBehaviour _mb; // The surrogate MonoBehaviour that we'll use to manage this coroutine.

    private TrackedDevicePose2_t pose;
    private Controllers.VRControllerState_t state;
    private TrackedDevicePose2_t[] poses;
    private Controllers.VRControllerState_t[] states;
    List<uint> indicesOfDevices;

    int startIDLeft;
    int startIDRight;
    private Camera[] cams;
    public bool isCheckingUpdates = true;

    public GameObject rightGO;
    public GameObject leftGO;
    public bool isQuestionnaireControl = false;




    // Bit shift the index of the layer (8) to get a bit mask
    int layerMask = 1 << 5;
    RaycastHit hit;
    Vector3 currentPose;
    private float previousXValue = 0.5f;
    private int tap;
    private bool readyForDoubleTap;
    private float interval = 100f;

    public XTAL_ControllerInput()
    {

//        this.control = control;

        _mb = GameObject.FindObjectOfType<MonoBehaviour>();
        indicesOfDevices = new List<uint>();

        GameObject rightGO = GameObject.Find("Right");
        controller_rightObj = rightGO.GetComponent<VRg_SteamVR_Controller>();
        startIDRight = controller_rightObj.MyID;

        pose = new TrackedDevicePose2_t();
        state = new Controllers.VRControllerState_t();


        // Variables to save current state and pose of controllers
        poses = new TrackedDevicePose2_t[Controllers.k_unMaxTrackedDeviceCount];
        states = new Controllers.VRControllerState_t[Controllers.k_unMaxTrackedDeviceCount];
        this._mb.StartCoroutine(GetUpdates());


    }


    // Update is called once per frame
    //IEnumerator CheckUpdatesOfController()
    IEnumerator GetUpdates()
    {
        while(isCheckingUpdates)
        { 
         

            if (controller_rightObj != null && controller_rightObj.MyID != startIDRight && controller_rightObj.MyID != -1)
            {
                checkRightControllerInputs();
                controller_rightObj.Update();
            }

            yield return null;
        }
    }

    public void shootLaserfromController()
    {

        //// Does the ray intersect any objects excluding the player layer
        //if (Physics.Raycast(currentPose, controller_rightObj.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, Physics.AllLayers))
        //{

        //    //ShootLaserFromTargetPosition(pose.DevicePosition, controller_rightObj.transform.forward, hit.distance, Color.yellow);
        //    Ray ray = new Ray(currentPose, controller_rightObj.transform.forward);
        //    RaycastHit raycastHit;
        //    Vector3 endPosition = currentPose + (hit.distance * controller_rightObj.transform.forward);

        //    if (Physics.Raycast(ray, out raycastHit, hit.distance))
        //    {
        //        endPosition = raycastHit.point;
        //        if (hit.transform.gameObject.GetComponent<Button>() != null)
        //            Debug.Log("Hit a button");

        //    }


        //    lineRenderer.material.color = Color.yellow;
        //    lineRenderer.SetPosition(0, currentPose);
        //    lineRenderer.SetPosition(1, endPosition);
        //    Debug.Log("Did Hit");
        //}
        //else
        //{
        //    Ray ray = new Ray(currentPose, controller_rightObj.transform.forward);
        //    RaycastHit raycastHit;
        //    Vector3 endPosition = currentPose + (laserMaxLength * controller_rightObj.transform.forward);

        //    if (Physics.Raycast(ray, out raycastHit, laserMaxLength))
        //    {
        //        endPosition = raycastHit.point;
        //    }


        //    lineRenderer.material.color = Color.white;
        //    lineRenderer.SetPosition(0, currentPose);
        //    lineRenderer.SetPosition(1, endPosition);
        //    Debug.Log("Did not Hit");
        //}
    }


    IEnumerator DoubleTapInterval()
    {
        yield return new WaitForSeconds(interval);
        readyForDoubleTap = true;
    }

    public void checkRightControllerInputs()
    {

        if(isQuestionnaireControl)
        {
            // control for questionnaire

            //// hair trigger on the back
            if (controller_rightObj.Input().GetPressDown(VRg_SteamVR_Controller.ButtonMask.Trigger))
            {

                tap++;

                if (tap == 1)
                {
                    //do stuff
                    //_mb.StartCoroutine(DoubleTapInterval());
                }

                else if (tap > 1)
                {
                    Debug.Log("pressed trigger 2x on right hand");
                    PressedButtonToSubmitQuestionnaireEvent?.Invoke();

                    tap = 0;
                    
                }
            }

          
       
            // BUTTON A -  move to previous slider
            if (controller_rightObj.Input().GetPressUp(VRg_SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                Debug.Log("Button A -> Previous slider");
                MoveToPreviousSliderEvent?.Invoke();
            }

            // BUTTON B - move to next slider
            if (controller_rightObj.Input().GetPressUp(VRg_SteamVR_Controller.ButtonMask.Grip))
            {
                Debug.Log("Button B -> Next slider");
                MoveToNextSliderEvent?.Invoke();
            }


            // Joystick - positive x axis - move current slider to the right
            //if (controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).x > 0.5 )

            if (controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).x > 0.6 && controller_rightObj.Input().GetPrevState().rAxis0.x <= 0.5)
            {

                previousXValue = controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).x;
                Debug.Log("Axis 0 is pushed right");
                ThumbstickPushedRightEvent?.Invoke();
            }



            // Joystick - negative x axis - move current slider to the left
            //if (controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).x < -0.01 )//&& controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).x > 0.0)//&& controller_rightObj.Input().GetPrevState().rAxis0.x >= -0.5)
            if (controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).x < -0.6 && controller_rightObj.Input().GetPrevState().rAxis0.x >= -0.5)
            {
                previousXValue = controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).x;
                Debug.Log("Axis 0 is pushed left");
                ThumbstickPushedLeftEvent?.Invoke();
            }

        }
        else
        {
            // Proceed with experiment
            // 1. closes the instruction image
            // 2. starts the task (1. of 30)
            // 3. Starts Calibration for Vergence
            // 4. Starts Evaluation for Vergence
            // 
            if (controller_rightObj.Input().GetPressDown(VRg_SteamVR_Controller.ButtonMask.Trigger))
            {
                ProceedButtonPressed?.Invoke();
            }


            //Button A - MATCH
            if (controller_rightObj.Input().GetPressDown(VRg_SteamVR_Controller.ButtonMask.Grip))
            {
                //Debug.Log("pressed Button A on right hand");
                PressedAEvent?.Invoke(true);
            }

            // Button B - NO MATCH
            if (controller_rightObj.Input().GetPressDown(VRg_SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                //Debug.Log("pressed Button B on right hand");
                PressedBEvent?.Invoke(false);
            }

            // Joystick - positive y axis
            if (controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).y > 0.5 && controller_rightObj.Input().GetPrevState().rAxis0.y <= 0.5)
            {
                //Debug.Log("Axis 0 is pushed up");
                ThumbstickPushedForwardEvent?.Invoke();
            }

            // Joystick - negative y axis
            if (controller_rightObj.Input().GetAxis(vrg.Controllers.EVRButtonId.k_EButton_Axis0).y < -0.5 && controller_rightObj.Input().GetPrevState().rAxis0.y >= -0.5)
            {
                //Debug.Log("Axis 0 is pushed down");
                ThumbstickPushedbackwardsEvent?.Invoke();
            }


        }

    }

    public void checkLeftControllerInputs()
    {

        if (controller_leftObj.Input().GetPress(VRg_SteamVR_Controller.ButtonMask.Trigger))
        {
            Debug.Log("Pressed trigger on left hand");

        }


        // Map all Buttons to boolean to check
        // BHO: Button A
        if (controller_leftObj.Input().GetPress(VRg_SteamVR_Controller.ButtonMask.Grip))
        {
            //pressedHairTrigger(1);
            Debug.Log("pressed Button A on left hand ");
        }

        if (controller_leftObj.Input().GetPress(VRg_SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            Debug.Log("pressed Button B on left hand");
        }




       
    }

}
