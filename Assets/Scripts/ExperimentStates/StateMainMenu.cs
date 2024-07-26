using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Source.ExperimentManagement;
using UnityEngine;
using UnityEngine.UI;


public enum latinsquaregroups
{
    Group1,
    Group2,
    Group3

}

namespace Source.ExperimentStates
{
    public class StateMainMenu:ExperimentState
    {

        public InputField userIdSetting;
        public InputField userAgeSetting;
        public Dropdown vrexSetting;
        public Dropdown etexSetting;
        public Dropdown genderSetting;
        public Dropdown conditionSettings;
        public Button startButton;

        private bool _isUserIdValid;
        private bool _isConditionValid;
        

        public override void StartState()
        {
            Debug.Log("Start State");

            // Add listener to changes on user ID
            userIdSetting.onValueChanged.AddListener(UpdateUserId);
            userIdSetting.name = ExperimentController._model.GetUserId().ToString();

            // Add listener for changes on age
            userAgeSetting.onValueChanged.AddListener(UpdateUserAge);

            // Add listener for changes on gender
            genderSetting.onValueChanged.AddListener(UpdateGenderSettings);

            // Add listener for changes on ET Experience
            etexSetting.onValueChanged.AddListener(UpdateETEXSettings);

            // Add listener for changes on VR Experience
            vrexSetting.onValueChanged.AddListener(UpdateVREXSettings);


            // Add listener for changes on Group Protocol
            conditionSettings.onValueChanged.AddListener(UpdateConditionSettings);



            startButton.onClick.AddListener(StartExperiment);
            CheckIfExperimentIsReady();

            
            List<latinsquaregroups> conditions = Enum.GetValues(typeof(latinsquaregroups)).Cast<latinsquaregroups>().ToList();
            List<Dropdown.OptionData> options = conditions.Select(option => new Dropdown.OptionData(option.ToString())).ToList();
            conditionSettings.options = options;
           
        }

        private void StartExperiment()
        {
            if (_isConditionValid && _isUserIdValid)
            {
                //ExperimentController._model.UpdateUserFolder();   // moved to ETController

                ExperimentController.ChangeToNextStateOfExperiment();
            }
        }

        private void UpdateConditionSettings(int arg0)
        {
            if(arg0>0)
                ExperimentController._model.UpdateConditionSelection(arg0);
            CheckIfExperimentIsReady();
        }

        private void UpdateGenderSettings(int arg0)
        {
            if (arg0 > 0)
                ExperimentController._model.UpdateGenderSelection(arg0);
            CheckIfExperimentIsReady();
        }

        private void UpdateVREXSettings(int arg0)
        {
            if (arg0 > 0)
                ExperimentController._model.UpdateVREXSelection(arg0);
            CheckIfExperimentIsReady();
        }
        private void UpdateETEXSettings(int arg0)
        {
            if (arg0 > 0)
                ExperimentController._model.UpdateETEXSelection(arg0);
            CheckIfExperimentIsReady();
        }


        private void UpdateUserId(string arg0)
        {
            if (arg0.Length>0)
            {
                ExperimentController._model.UpdateUserId(arg0);
            }

            CheckIfExperimentIsReady();
        }

        private void UpdateUserAge(string arg0)
        {
            if (arg0.Length > 0)
            {
                ExperimentController._model.UpdateUserAge(arg0);
            }

            CheckIfExperimentIsReady();
        }



        public override void EndState()
        {
            // Nothing to do
        }

        private void CheckIfExperimentIsReady()
        {
            _isConditionValid = ExperimentController._model.GetProtocolID() >= 0;
            _isUserIdValid = ExperimentController._model.GetUserId()!= null;
         
            

            startButton.interactable = (_isUserIdValid&&_isConditionValid);
        }

    }
}