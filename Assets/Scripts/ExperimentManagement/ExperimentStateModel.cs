// Author: Martin Dechant

using System;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Source.ExperimentManagement
{

    public delegate void OnModelChanged();

    /// <summary>
    /// Holds the Participant information about the experiment 
    /// </summary>
    public sealed class ExperimentStateModel
    {
        private int _activeState;
        private readonly ProtocolInformation[] _experimentProtocolOptions;
        private ProtocolInformation _experimentProtocol;
        private string userFolder;

        private int nTrials = 10;
        private int Trial = 0;
        private float matchRate = 0.5f;
        public ETController Zero;
        private latinsquaregroups _selectedLatinGroupId;  // Protocol group
        public GameObject interior;
        public GameObject calibrationRoom;
        public XTAL_ControllerInput xtalControllerController;
        public string currentCondition;



        private int _roundId = 0;  // Round means repetition of the same task

        private string _userId = "TEST_" + (Random.Range((int)1, (int)9999).ToString()).ToString(); // We create a random ID at the beginning in case no id will be set

        private string _userAge = "-1"; // We create a random ID at the beginning in case no id will be set


        private string _gender = "male";
        private string _vrEx = "yes";
        private string _etEx = "no";
        internal bool isHardwarCalibrated;

        public event OnModelChanged ModelChangedHandler;



        public ExperimentStateModel()
        {
            _experimentProtocolOptions = Resources.LoadAll<ProtocolInformation>("Protocols");

            // For Debug purpose: take the first one you find
            _experimentProtocol = _experimentProtocolOptions[0];
        }

        public void UpdateUserId(string userId)
        {
            _userId = userId;
            OnOnModelChanged();
        }

        public void UpdateUserAge(string userAge)
        {
            _userAge = userAge;
            OnOnModelChanged();
        }

        public void UpdateUserFolder()
        {
          
            userFolder = createUserFolder(_userId);

            OnOnModelChanged();

        }

        private string createUserFolder(string userID)
        {
            string oldName;
            string userFolder;
            int fileCounter = 1;
            userFolder = "D:\\SFB_Subjects\\" + userID;

            oldName = userFolder;

            while (Directory.Exists(userFolder))
            {
                userFolder = oldName + fileCounter.ToString();
                fileCounter += 1;
            }

            Directory.CreateDirectory(userFolder);

            // Add BaselineFolder, Condition 1, Condition 2, Condition 3, and Condition 4 folders
            string baselineF = userFolder + "/Baseline/";
            if (!Directory.Exists(baselineF))
                Directory.CreateDirectory(baselineF);

            string con1F = userFolder + "/Manual/";
            if (!Directory.Exists(con1F))
                Directory.CreateDirectory(con1F);

            string con2F = userFolder + "/Gaze/";
            if (!Directory.Exists(con2F))
                Directory.CreateDirectory(con2F);

            string con3F = userFolder + "/Vergence/";
            if (!Directory.Exists(con3F))
                Directory.CreateDirectory(con3F);



            var userFilePath = userFolder + "/" + userID+".txt";
            using (StreamWriter sw = File.AppendText(userFilePath))
            {
                sw.WriteLine("Name\tAge\tGender\tGroupID\tET-Experience?\tVR-Experience?");
                sw.Flush();
                sw.WriteLine(userID + "\t" + _userAge + "\t" + _gender + "\t" + _selectedLatinGroupId.ToString() + "\t" + _etEx + "\t" + _vrEx);
                sw.Flush();
            }

            return userFolder;
        }

        public string GetUserFolder()
        {
            if (userFolder == null)
                userFolder = "D:\\SFB_Subjects\\TEST";
            return userFolder;
        }

        public string GetUserId()
        {
            return _userId;
        }

        public void SetCurrentCondition(string conditionName)
        {
            currentCondition = conditionName;
        }

        public string GetCurrentConditionName()
        {
            return currentCondition;
        }

        public void UpdateConditionSelection(int groupProtocol)
        {
            _selectedLatinGroupId = (latinsquaregroups)groupProtocol;
            string nameOfCondition = _selectedLatinGroupId.ToString();

            foreach (ProtocolInformation protocolOption in _experimentProtocolOptions)
            {
                //Debug.Log(protocolOption.ProtocolName);
                //Debug.Log("Current selected condition" + nameOfCondition);

                if (protocolOption.ProtocolName == nameOfCondition)
                {
                    _experimentProtocol = protocolOption;
                    OnOnModelChanged();
                    return;
                }
            }

            Debug.Log("Condition " + nameOfCondition + " not found! Make sure the name of the condition matches your condition enum options.");
        }

        public void UpdateGenderSelection(int gender)
        {
            if (gender == 0) _gender = "Male";

            if (gender == 1) _gender = "Female";

            if (gender == 2) _gender = "Divers";


            OnOnModelChanged();
        }

        public void UpdateVREXSelection(int vrex)
        {
            if (vrex == 0) _vrEx = "yes";

            if (vrex == 1) _vrEx = "no";

            
        }

        public void UpdateETEXSelection(int etEx)
        {
            if (etEx == 1) _etEx = "yes";

            if (etEx == 0) _etEx = "no";

            OnOnModelChanged();
        }

        /// <summary>
        /// Returns the selected Protocol
        /// </summary>
        /// <returns></returns>
        public latinsquaregroups GetProtocolID()
        {
            return _selectedLatinGroupId;
        }

        public void StartNextRoundOfSameTask()
        {
            _roundId += 1;
        }

        public int GetRoundId()
        {
            return _roundId;
        }

        /// <summary>
        /// Returns the next scene of the Stack
        /// </summary>
        /// <returns></returns>
        public string GetNextSceneNameToLoad()
        {
            _activeState++;
            if (_activeState >= _experimentProtocol.ScenesToLoad.Length)
            {
                _activeState = 0;
                return "END";
            }


            return _experimentProtocol.ScenesToLoad[_activeState];
        }

        public string GetLastSceneNameToLoad()
        {
            _activeState--;
            if (_activeState <= 1)
            {
                _activeState = 1;
            }
            return _experimentProtocol.ScenesToLoad[_activeState];

        }


        public string GetCurrentSceneNameToLoad()
        {
            if (_activeState >= _experimentProtocol.ScenesToLoad.Length)
            {
                _activeState = 0;
                return "END";
            }


            return _experimentProtocol.ScenesToLoad[_activeState];
        }



        /// <summary>
        /// Returns the sceneCommands for the running Scene
        /// </summary>
        /// <returns></returns>
        public string ReturnCommandOfRunningScene()
        {
            return _experimentProtocol.sceneCommands[_activeState];
        }

        /// <summary>
        /// Calls the Event OnModelChanged
        /// </summary>
        private void OnOnModelChanged()
        {
            ModelChangedHandler?.Invoke();
        }

        internal float GetMatchRate()
        {
            return matchRate;
        }

        internal int GetTrial()
        {
            return Trial;
        }

        internal int GetNrOfTrials()
        {
            return nTrials;
        }
    }
}