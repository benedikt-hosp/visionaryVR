//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Xml;
using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Tobii.Research.Unity {
    public class VRSaveData : MonoBehaviour {
        // for the head free condition, have an object anchored to the camera
        // set active
        //
        /// <summary>
        /// Instance of <see cref="VRSaveData"/> for easy access.
        /// Assigned in Awake() so use earliest in Start().
        /// </summary>
        public static VRSaveData Instance { get; private set; }

        [SerializeField]
        [Tooltip("If true, data is saved.")]
        private bool _saveData;

        [SerializeField]
        [Tooltip("If true, Unity3D-converted data is saved.")]
        private bool _saveUnityData = true;

        [SerializeField]
        [Tooltip("If true, raw gaze data is saved.")]
        private bool _saveRawData = true;

        [SerializeField]
        [Tooltip("If true, save the unity phisical coord of the Gaze.")]
        private bool _savePhysicalGazeData = true;


        [SerializeField]
        [Tooltip("Folder in the application root directory where data is saved.")]
        private string _folder = "Data";

        [SerializeField]
        [Tooltip("This key will start or stop saving data.")] ///it was none before
        private KeyCode _toggleSaveData = KeyCode.None;

        /// <summary>
        /// If true, data is saved.
        /// </summary>
        public bool SaveData {
            get {
                return _saveData;
            }

            set {
                _saveData = value;

                if (!value) {
                    CloseDataFile();
                }
            }
        }

        private VREyeTracker _eyeTracker;
        private XmlWriterSettings _fileSettings;
        private XmlWriter _file;

        private void Awake() {
            Instance = this;

        }

        private void Start() {
            _eyeTracker = VREyeTracker.Instance;

        }

        private void Update() {
            if (Input.GetKeyDown(_toggleSaveData)) {
                SaveData = !SaveData;
            }

            if (!_saveData) {
                if (_file != null) {
                    // Closes _file and sets it to null.
                    CloseDataFile();
                }

                return;
            }

            if (_file == null) {
                // Opens data file. It becomes non-null.
                OpenDataFile();
            }

            if (!_saveUnityData && !_saveRawData) {
                // No one wants to save anyway.
                return;
            }

            var data = _eyeTracker.NextData;
            while (data != default(IVRGazeData)) {
                WriteGazeData(data);
                data = _eyeTracker.NextData;
            }
        }

        private void OnDestroy() {
            CloseDataFile();
        }

        private void OpenDataFile() {
            if (_file != null) {
                Debug.Log("Already saving data.");
                return;
            }
            try {
                _folder = "C:";
                _folder = _folder + "\\_ET_Data\\";
            } catch {
            }
            if (!System.IO.Directory.Exists(_folder)) {
                System.IO.Directory.CreateDirectory(_folder);
            }

            _fileSettings = new XmlWriterSettings();
            _fileSettings.Indent = true;
            var fileName = string.Format("vr_data_{0}.xml", System.DateTime.Now.ToString("yyyyMMddTHHmmss"));
            _file = XmlWriter.Create(System.IO.Path.Combine(_folder, fileName), _fileSettings);
            _file.WriteStartDocument();
            _file.WriteStartElement("Data");
        }

        private void CloseDataFile() {
            if (_file == null) {
                return;
            }

            _file.WriteEndElement();
            _file.WriteEndDocument();
            _file.Flush();
            _file.Close();
            _file = null;
            _fileSettings = null;
        }

        private void WriteGazeData(IVRGazeData gazeData) {
            _file.WriteStartElement("GazeData");

            if (_saveUnityData) {
                _file.WriteAttributeString("TimeStamp", gazeData.TimeStamp.ToString());
                // added to have the current time
                _file.WriteAttributeString("TimeStampUnder", System.DateTime.Now.ToString("yyyyMMddTHHmmss"));

                // Head Free
                //foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                // {
                //if (gameObj.name == "1R1C" || gameObj.name == "1R2C" || gameObj.name == "1R3C" || gameObj.name == "1R4C" || gameObj.name == "1R5C" ||
                //gameObj.name == "2R1C" || gameObj.name == "2R2C" || gameObj.name == "2R3C" || gameObj.name == "2R4C" || gameObj.name == "2R5C" ||
                //gameObj.name == "3R1C" || gameObj.name == "3R2C" || gameObj.name == "3R3C" || gameObj.name == "3R4C" || gameObj.name == "3R5C" ||
                //gameObj.name == "4R1C" || gameObj.name == "4R2C" || gameObj.name == "4R3C" || gameObj.name == "4R4C" || gameObj.name == "4R5C" ||
                //gameObj.name == "5R1C" || gameObj.name == "5R2C" || gameObj.name == "5R3C" || gameObj.name == "5R4C" || gameObj.name == "5R5C")
                //)

                Debug.Log("Eye tracking is recording...");
                //Debug.Log("combined" + gazeData.CombinedGazeRayWorld.direction);
                    if (_savePhysicalGazeData) {
                    //Compute the 3D location of the gaze as ray collide in unity from GazeOrigin following GazeDir until collision
                    // RaycastHit hitLeft;
                    // if (Physics.Raycast(gazeData.Left.GazeOrigin, gazeData.Left.GazeDirection, out hitLeft, 20.0f)) {
                    //     _file.WriteAttributeString("HitPointLeft", hitLeft.point.ToString("F6"));
                    //     _file.WriteAttributeString("HitPointLeftValidity", "True");
                    // } else {
                    //     _file.WriteAttributeString("HitPointLeft", "(NaN, NaN, NaN)");
                    //     _file.WriteAttributeString("HitPointLeftValidity", "False");
                    // }
                    // RaycastHit hitRight;
                    // if (Physics.Raycast(gazeData.Left.GazeOrigin, gazeData.Left.GazeDirection, out hitRight, 20.0f)) {
                    //     _file.WriteAttributeString("HitPointRight", hitRight.point.ToString("F6"));
                    //     _file.WriteAttributeString("HitPointRightValidity", "True");
                    // } else {
                    //     _file.WriteAttributeString("HitPointRight", "(NaN, NaN, NaN)");
                    //     _file.WriteAttributeString("HitPointRightValidity", "False");
                    // }

                    // Use GRW 
                    RaycastHit hitLeftGRW;
                    if (Physics.Raycast(gazeData.Left.GazeRayWorld.origin, gazeData.Left.GazeRayWorld.direction, out hitLeftGRW, 20.0f)) {
                        _file.WriteAttributeString("HitPointLeftGRW", hitLeftGRW.point.ToString("F6"));
                        _file.WriteAttributeString("HitPointLeftGRWValidity", "True");
                    } else {
                        _file.WriteAttributeString("HitPointLeftGRW", "(NaN, NaN, NaN)");
                        _file.WriteAttributeString("HitPointLeftGRWValidity", "False");
                    }
                    RaycastHit hitRightGRW;
                    if (Physics.Raycast(gazeData.Right.GazeRayWorld.origin, gazeData.Right.GazeRayWorld.direction, out hitRightGRW, 20.0f)) {
                        _file.WriteAttributeString("HitPointRightGRW", hitRightGRW.point.ToString("F6"));
                        _file.WriteAttributeString("HitPointRightGRWValidity", "True");
                    } else {
                        _file.WriteAttributeString("HitPointRightGRW", "(NaN, NaN, NaN)");
                        _file.WriteAttributeString("HitPointRightGRWValidity", "False");
                    }
                    }
                //{
                //var fixate = GameObject.Find("HMD");
                //Vector3 targetUnity = gameObj.transform.position;
                //Vector3 targetHMD = fixate.transform.localPosition;
                //Debug.Log("HMD"+ targetHMD);
                //Vector3 targetdif = gameObj.transform.position - fixate.transform.position;
                //float angleLeft = Vector3.Angle(gazeData.Left.GazeDirection, targetHMD);
                //Debug.Log("angleLeft" + angleLeft);
                //_file.WriteAttributeString("angleLeft", angleLeft.ToString());
                //float angleRight = Vector3.Angle(gazeData.Right.GazeDirection, targetHMD);
                //_file.WriteAttributeString("angleRight", angleRight.ToString());
                // float angleCombined = Vector3.Angle(gazeData.CombinedGazeRayWorld.direction, targetHMD);
                //_file.WriteAttributeString("angleCombined", angleCombined.ToString());
                //_file.WriteAttributeString("VectorTarget", targetHMD.ToString());

                // }
                // for analysis of free movement target

                //else if (gameObj.name == "FreeMovement") { 
                // Vector3 target = gameObj.transform.localPosition;
                // float anglegazeleftfree = Vector3.Angle(gazeData.Left.GazeDirection, target);
                //float anglegazerightfree = Vector3.Angle(gazeData.Right.GazeDirection, target);
                //var angledifffree = (anglegazeleftfree + anglegazerightfree) / 2;
                //_file.WriteAttributeString("angleLeftfree", anglegazeleftfree.ToString());
                //_file.WriteAttributeString("angleRightfree", anglegazerightfree.ToString());
                //_file.WriteAttributeString("angledifffree", angledifffree.ToString());
                //}
                //}


                // Head Fixed
                //float anglegaze = Vector3.Angle((gazeData.Left.GazeDirection + gazeData.Right.GazeDirection) / 2, target);
                //float anglegazeLeft = Vector3.Angle(gazeData.Left.GazeDirection, target);
                //float anglegazeRight = Vector3.Angle(gazeData.Right.GazeDirection, target);
                //  - Vector3.Angle(gazeData.CombinedGazeRayWorld.direction, target)

                _file.HMDWritePose(gazeData.Pose);
                _file.HMDWriteEye(gazeData.Left, "Left");
                _file.HMDWriteEye(gazeData.Right, "Right");
                _file.WriteRay(gazeData.CombinedGazeRayWorld, gazeData.CombinedGazeRayWorldValid, "CombinedGazeRayWorld");
            }
            if (_saveRawData) {
                _file.HMDWriteRawGaze(gazeData.OriginalGaze);
            }

            _file.WriteEndElement();
        }
    }
}
//public Image LoadingBar;
//public GameObject Canvas;

//LoadingBar.GetComponent<Image>().FillAmount += Time.DeltaTime * 1/TotalTimeInSeconds

//if(LoadingBar.GetComponent<Image>().FillAmount >= 1)
//{Canvas.SetActive(false)}
