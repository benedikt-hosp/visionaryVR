using System;
using System.Collections;
using Source.DataInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Source.ExperimentManagement
{
    // Author: Benedikt Hosp & Martin Dechant
    namespace Management
    {
        /// <summary>
        /// The "Main Controller" for the overall experiment. Use this if you need to implement global Information
        /// 
        /// </summary>
        public class ExperimentController : MonoBehaviour
        {
            public ProtocolInformation[] protocols;
            private ProtocolInformation _protocol;
            private int defaultLoadingInCaseOfError = 0;
            private string serverUrl;
            private ExperimentState _runningState;
            public ExperimentStateModel _model;
            private static ExperimentController _instController;
            public static int scalefactorRGB = 1;
            public static int scalefactorDEPTH = 4;        // bei 1 sind die ergebnisse besser (kann aber auch an schlechten et daten liegen)

            public static int resolutionX = 3840;
            public static int resolutionY = 2160;


            public IDataWriting WritingInterface { get; private set; }

            [Header("Choose ET-SDK")]     // do not need to set it dynamically
            public Providers eyeTrackingProvider;
            public string dataFolder = "D:\\SFB_Subjects\\Userfolder"; // is set by StateMainMenu
            string lastState;

            public static ExperimentController Instance()
            {
                return _instController;
            }


            private void Awake()
            {
                if (_instController == null)
                {
                    _instController = this;
                }
                else
                {
                    Destroy(this.gameObject);
                    return;
                }

                // Subscribe to the Scene manager to notify other scenes when the loading was done.
                SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
                DontDestroyOnLoad(this.gameObject);

                _model = new ExperimentStateModel();

                // Create Writing Interface os ehte IDataWriting Interface to create custom writer
                WritingInterface = new CSVWriter();


                // SFB-RV: if we want to sepcify it specifically
                //vrProvider = VRProdivers.XTAL;
                //eyeTrackingProvider = Providers.XTAL;


                _model.Zero = new ETController(eyeTrackingProvider);

                // Only for XTAL
                //_model.xtalControllerController = new XTAL_ControllerInput();


                // Position the camera prefab
                //GameObject cameraObject = GameObject.Find("CameraOrigin");          // XTAL
                // UnityEngine.Object.DontDestroyOnLoad(cameraObject);



                //cameraObject.transform.Rotate(new Vector3(0, 0, 0));
                //cameraObject.transform.Translate(new Vector3(0f, 0f, 0f));

                //var cams = cameraObject.GetComponentsInChildren<Camera>();
                //for (int i = 0; i < cams.Length; i++)
                //{
                    //Debug.Log(cams[i].name);
                 //   if (i == 0)
                  //  {
                  //      cams[i].tag = "MainCamera";
                    //    cams[i].enabled = true;
                   // }
                //}

            }

            public GameObject loadGameobject(string path, string name)
            {
                GameObject instance = null;
                if (!GameObject.Find(name))
                {
                    instance = GameObject.Instantiate(Resources.Load(path + name, typeof(GameObject))) as GameObject;
                    instance.name = name;
                    UnityEngine.Object.DontDestroyOnLoad(instance);
                }




                return instance;
            }

            private void Update()
            {

                // WORKAROUND BECAUSE WE NEED STEAM VR AND HTC VIVE. Unity is taking
                //rvCameratransform.localRotation = new Quaternion((rvCameratransform.rotation.x + 90), rvCameratransform.rotation.y, rvCameratransform.rotation.z, rvCameratransform.rotation.w);

            }

            private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode arg1)
            {
                if (_instController != null)
                {
                    //GameObject[] temp = GameObject.FindGameObjectsWithTag("GameController");

                    //foreach (GameObject obj in temp)
                    //{
                    //    if (obj.GetComponent<ExperimentController>().Equals(this) == false)
                    //    {
                    //        Destroy(obj);
                    //    }
                    //}
                }

                _runningState = FindObjectOfType<ExperimentState>();

                // We dont' have a model yet; Wait for the last call of the Loading function
                if (_model != null)
                {
                    _runningState.SetupState(_model, this);
                    _runningState?.StartState();
                }

            }

            /// <summary>
            /// use this call to continue the experiment following the instructions of the experiment parameters object
            /// </summary>
            public void ChangeToNextStateOfExperiment()
            {
                Debug.Log("Started next scene");
                lastState = _model.GetCurrentSceneNameToLoad(); 
                string sceneID = _model.GetNextSceneNameToLoad();
                if (sceneID != "END")
                {
                    _runningState?.EndState();
                    _runningState = null;
                    if (sceneID != "END")
                    {
                        SceneManager.LoadScene(sceneID);
                    }

                }
                else
                {
                    //StopExperimentOnWebsite();
                    _model.Zero.close();
                    _model.Zero = null;
                    _model.xtalControllerController = null;
                    Destroy(this);

                    Application.Quit();
                }

            }

            public void ReloadStateOfExperiment()
            {
                var sceneID = _model.GetCurrentSceneNameToLoad();

                _runningState?.EndState();
                _runningState = null;
                if (sceneID != "END")
                {
                    SceneManager.LoadScene(sceneID);
                }

            }

            public void ReturnToLastStateOfExperiment()
            {
                string sceneID = _model.GetLastSceneNameToLoad();
                _runningState?.EndState();
                _runningState = null;

                    SceneManager.LoadScene(sceneID);


            }

            public void StopExperimentOnWebsite()
            {
                Debug.Log("End App");
                //Application.ExternalCall("EndGame");
            }


        }
    }
}
