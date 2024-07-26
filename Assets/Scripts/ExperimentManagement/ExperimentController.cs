using System;
using System.Collections;
using Source.DataInformation;
using Unity.VisualScripting;
using UnityEditor;
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

            //[Header("Choose ET-SDK")]     // do not need to set it dynamically
            //public Providers eyeTrackingProvider;
            //public string dataFolder = "D:\\SFB_Subjects\\Userfolder"; // is set by StateMainMenu
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

                // ================== BEST LOCATION TO INITIALIZE ZERO
                _model.Zero = GameObject.Find("ZERO").GetComponent<ETController>();
                _model.Zero.InitController();
                _model.Zero.UpdateUserFolder(_model.GetUserId(), _model.GetUserAge(), _model.GetGender(), _model.GetLatinsquaregroup(), _model.GetETEx(), _model.GetVREX());
                DontDestroyOnLoad(GameObject.Find("ZERO"));

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

               
            }

            private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode arg1)
            {
                
                _runningState = UnityEngine.Object.FindFirstObjectByType<ExperimentState>();

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
                Debug.Log("Loading " + sceneID);
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
                    Debug.LogError("Reached final and will close app");
                    // Stop experiment
                    _model.Zero.close();
                    _model.Zero = null;
#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
                    Destroy(this);

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

        }
    }
}
