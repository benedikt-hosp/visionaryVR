using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void OnDepthCalibrationStarted();
public delegate void OnDepthCalibrationFinished();


public class DepthCalibration: MonoBehaviour
{
    Material blackSphereMaterial;


    // Max values
    float MAX_TARGET_DISTANCE = 9.0f; //9.0f;
    float MAX_TARGET_ECC = 70.0f;
    float MAX_TARGET_ME = 180.0f;

    // Start & min values
    public float MIN_TARGET_DISTANCE = 0.1f;
    public float MIN_TARGET_ECC = 0.1f;
    public float MIN_TARGET_ME = 0.1f;


    // Running values
    public float target_distance;
    public float target_me;
    public float target_ecc;

    // Change rates
    float rate_distance = 0.7f;//0.7f;
    float rate_ecc = 0.5f;//0.5f;
    float rate_me = 60.0f;




    bool procedureStarted = false;


    public float get_target_distance { get { return target_distance; } }
    public float get_target_me { get { return target_me; } }
    public float get_target_ecc { get { return target_ecc; } }
    public bool isProcedureStarted { get { return procedureStarted; } set { procedureStarted = true; } }




    public GameObject target;

    int runs = 0;
    public bool startAnimation = false;
    private bool wasRunning = false;
    public bool firstRun = true;
    /* Events
 * */
    public static event OnDepthCalibrationStarted OnDepthCalibrationStartedEvent;
    public static event OnDepthCalibrationFinished OnDepthCalibrationFinishedEvent;

    // Start is called before the first frame update
    void Start()
    {

        blackSphereMaterial = Resources.Load("Materials/DARK_CROSS_Material", typeof(Material)) as Material;

        target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        target.GetComponent<Renderer>().material = blackSphereMaterial;
        target.AddComponent<TargetControl>();
        target.transform.parent = Camera.main.transform;
        target.GetComponent<TargetControl>().SetPosition(MIN_TARGET_ECC, MIN_TARGET_ME, MIN_TARGET_DISTANCE);


        target.transform.localRotation = Quaternion.identity;

        target.AddComponent<BoxCollider>();

    }


    

    void Update()
    {

        if(startAnimation)
        {
            if(firstRun)
            {
                Debug.Log("Running the first time");
                OnDepthCalibrationStartedEvent?.Invoke();
                firstRun = false;
                wasRunning = true;

            }
            StartCoroutine(ContinousChangeRoutine());
            
        }
        else
        {
            if(wasRunning)
            {
                Debug.Log("After last run");
                OnDepthCalibrationFinishedEvent!.Invoke();
                wasRunning = false;
                firstRun = true;
                runs = 0;
                target_distance = MIN_TARGET_DISTANCE;
                target_me = MIN_TARGET_ME;
                target_ecc = MIN_TARGET_ECC;
                target.GetComponent<TargetControl>().SetPosition(MIN_TARGET_ECC, MIN_TARGET_ME, MIN_TARGET_DISTANCE);
                startAnimation = false;
                StopCoroutine(ContinousChangeRoutine());
            }
        }

    }

    IEnumerator ContinousChangeRoutine()
    {
        // distance
        target_distance += rate_distance * Time.deltaTime;
        if (target_distance >= MAX_TARGET_DISTANCE)
        {
            rate_distance = -1.0f * Mathf.Abs(rate_distance);
            runs++;
        }

        if (target_distance <= MIN_TARGET_DISTANCE)
        {
            rate_distance = Mathf.Abs(rate_distance);
        }


        // eccentricity center to the right
        target_ecc += rate_ecc * Time.deltaTime;
        if (target_ecc >= MAX_TARGET_ECC)
        {
            rate_ecc = -1.0f * Mathf.Abs(rate_ecc);
           // runs++;

        }
        if (target_ecc <= MIN_TARGET_ECC)
        {
            rate_ecc = Mathf.Abs(rate_ecc);
        }

        // Meridian
        target_me += (rate_me * Time.deltaTime);
        if (target_me >= MAX_TARGET_ME)
        {
            
            rate_me = Mathf.Abs(rate_me);

            // runs++;

        }

        if (target_ecc <= MIN_TARGET_ECC)
        {
            rate_me = -1.0f * Mathf.Abs(rate_me);
        }


        target.GetComponent<TargetControl>().SetPosition(target_ecc, target_me, target_distance);
        if (runs > 1)
            startAnimation = false;

        yield return null;
        //}

        // ================ continuous distance
        //target_distance += (rate * run);



        //// ================ continuous ecc
        //target_ecc += (ecc_rate * Time.deltaTime);



        //if (target_ecc >= max_ecc)
        //{
        //    ecc_rate = -1.0f * Mathf.Abs(ecc_rate);
        //    target_ecc = 1.0f;
        //}
        //if (target_ecc <= min_ecc)
        //{
        //    ecc_rate = Mathf.Abs(ecc_rate);
        //    started++;
        //}



        //// ================ continuous meridian
        //target_me += (me_rate * Time.deltaTime);


        //if (target_me >= max_me)
        //{
        //    me_rate = -1.0f * Mathf.Abs(me_rate);
        //    target_me = 0.25f;

        //}
        //if (target_me <= min_me)
        //{
        //    me_rate = Mathf.Abs(me_rate);
        //}


        // set changed values
        
        //OnDepthCalibrationFinishedEvent?.Invoke();

    }

  
}






/*
Material blackSphereMaterial;

public float start_target_distance = 0.25f;
private float start_target_me = 0.25f;
private float start_target_ecc = 0.25f;



public float target_distance = 0.25f;
private float target_me = 0.25f;
private float target_ecc = 0.25f;
bool procedureStarted = false;


public float get_target_distance { get { return target_distance; } }
public float get_target_me { get { return target_me; } }
public float get_target_ecc { get { return target_ecc; } }

public bool isProcedureStarted { get { return procedureStarted; } set { procedureStarted = true; } }

float start_ecc_rate = 0.5f;
float start_me_rate = 60.0f;
float start_dist_rate = 0.5f;


float ecc_rate = 0.8f;  // orig: 0.5
float me_rate = 60.0f;  // orig: 60.0f
float dist_rate = 0.5f; // orig: 0.5

GameObject target;
public int started = 0;
int runTime = 1;
public bool isRunning = false;
public bool isGazed = false;
public float rotSpeed = 400.0f;

// Events
public static event OnDepthCalibrationStarted OnDepthCalibrationStartedEvent;
    public static event OnDepthCalibrationFinished OnDepthCalibrationFinishedEvent;

    // Start is called before the first frame update
    void Start()
    {

        blackSphereMaterial = Resources.Load("Materials/DARK_CROSS_Material", typeof(Material)) as Material;

        target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        target.GetComponent<Renderer>().material = blackSphereMaterial;
        target.AddComponent<TargetControl>();
        target.transform.parent = Camera.main.transform;
        target.AddComponent<BoxCollider>();
        //target.tag = "depthCalibrationStimulusSphere";


        target.transform.parent = Camera.main.transform;
        target.GetComponent<TargetControl>().SetPosition(start_target_ecc, start_target_me, start_target_distance);

    }

    void Update()
    {
        if (procedureStarted)
        {
            if (started == 0 & !isRunning)
            {
                OnDepthCalibrationStartedEvent?.Invoke();
                Debug.Log("Start Recording");
                started = 1;
                isRunning = true;
            }


            if (started <= runTime && isRunning)
            {
                ContinuousChange(0.01f, 9.0f, 0.01f, 20.0f, 0.01f, 360);
                // ContinuousChange(0.25f, 1.0f, 0.25f, 1.0f, 0.0f, 30.0f);
            }
            else
            {
                procedureStarted = false;
                OnDepthCalibrationFinishedEvent?.Invoke();
                started = 0;
                target.GetComponent<TargetControl>().SetPosition(start_target_ecc, start_target_me, start_target_distance);

                ecc_rate = start_ecc_rate;
                me_rate = start_me_rate;
                dist_rate = start_dist_rate;

                target_distance = start_target_distance;
                target_ecc = start_target_ecc;
                target_me = start_target_me;
            }

        }
    }
    void ContinuousChange(float min_dist, float max_dist, float  min_ecc, float max_ecc, float min_me, float max_me)
    {
          // ================ continuous distance
        target_distance += (dist_rate * Time.deltaTime);


        if (target_distance >= max_dist)
        {
            dist_rate = -1.0f * Mathf.Abs(dist_rate);
        }

        if (target_distance <= min_dist)
        {
            dist_rate = Mathf.Abs(dist_rate);
        }


        //// ================ continuous ecc
        target_ecc += (ecc_rate * Time.deltaTime);

        

        if (target_ecc >= max_ecc)
        {
            ecc_rate = -1.0f * Mathf.Abs(ecc_rate);
            //target_ecc = 1.0f;
        }
        if (target_ecc <= min_ecc)
        {
            ecc_rate = Mathf.Abs(ecc_rate);
            started++;
        }



        //// ================ continuous meridian
        target_me += (me_rate * Time.deltaTime);


        if (target_me >= max_me)
        {
            //me_rate = -1.0f * Mathf.Abs(me_rate);
            target_me = 0.25f;
            
        }
        if (target_me <= min_me)
        {
            me_rate = Mathf.Abs(me_rate);
        }

        target.GetComponent<TargetControl>().SetPosition(target_ecc, target_me, target_distance);
    }
}

*/