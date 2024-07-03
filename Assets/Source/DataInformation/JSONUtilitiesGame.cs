using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Source.Interfaces;
using UnityEngine;

namespace Content.Source.DataInformation
{
    public static class JSONUtilitiesGame
    {
        /// <summary>
        /// Reads a Config File
        /// </summary>
        /// <returns></returns>
        public static string[] GetQuestionnaireNamesFromConfig()
        {
            string pathToJson = 
                string.Concat(Application.dataPath,ConstantValues.PathToConfigSettings,"Settings_Used_Questionnaire",".JSON");
            QuestionnaireSettings questionnaires = new QuestionnaireSettings();
            
            if (File.Exists(pathToJson))
            {
                var jsonContent = File.ReadAllText(pathToJson);
                Debug.Log(jsonContent);
                questionnaires = (QuestionnaireSettings) Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent,typeof(QuestionnaireSettings));
            }

            return questionnaires.questionnaires;
        }
        
        /// <summary>
        /// Reads a Questionnairefile
        /// </summary>
        /// <param name="nameQuestionnaire"></param>
        /// <param name="questionnaire"></param>
        /// <returns></returns>
        public static bool GetQuestionnaireByString(string nameQuestionnaire, out Questionnaire questionnaire)
        {
            string pathToJson = 
                string.Concat(Application.dataPath,ConstantValues.PathToConfigQuestionnaires,nameQuestionnaire,".JSON");
            Debug.Log("Path: " + pathToJson);
            
            var textFile = Resources.Load<TextAsset>(pathToJson);
            questionnaire = null;

            return ConvertTextAssetToQuestionnaire(textFile, out questionnaire);
        }

        public static bool ConvertTextAssetToQuestionnaire(TextAsset textFile, out Questionnaire questionnaire)
        {
            questionnaire = null;
            if (textFile != null)
            {
                var jsonContent = textFile.text;
                Debug.Log(jsonContent);
                questionnaire = (Questionnaire) Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent,typeof(Questionnaire));
            }

            return (questionnaire != null);
        }

        /// <summary>
        /// Reads a JSON file containing language phrases. Use this to get a dictionary to 
        /// </summary>
        /// <param name="selectedLanguage"></param>
        /// <param name="resultingDictionary"></param>
        /// <returns></returns>
        public static bool GetStringLibraryByString(Language selectedLanguage, out Dictionary<string,string> resultingDictionary)
        {
            string pathToJson = 
                string.Concat(Application.dataPath,ConstantValues.PathToConfigLanguage,selectedLanguage.ToString(),".JSON");
            Debug.Log("Path: " + pathToJson);
            resultingDictionary = null;
            if (File.Exists(pathToJson))
            {
                var jsonContent = File.ReadAllText(pathToJson);
                resultingDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            }
            
            return (resultingDictionary !=null);
        }




        public class QuestionnaireSettings
        {
            public string[] questionnaires { get; set; }
        }
    }
}