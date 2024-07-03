// Author: Martin Dechant

using System.Collections.Generic;
using Random = System.Random;

namespace Source.DataInformation
{
    
    public enum Gender
    {
        NONE,
        Female,
        Male,
        Other
    }

    public enum Language
    {
        NONE, 
        German, 
        English
    }

    public class ConstantValues
    {
        public const float DistanceToStartStaringAtPlayer = 5f;
        public const string PathToConfigSettings = "/Config/Settings/";
        public const string PathToConfigQuestionnaires = "/Resources/Questionnaire/";
        public const string PathToConfigLanguage = "/Config/Language/";

        public const string PathToDefaultExperimentProtocolObject = "ExperimentProtocol/Protocol";
        public const string PathToDefaultRiddles = "ExperimentProtocol/Riddles";
        public const string PathToLogFiles = ("/Logfiles/");
        
        public static string ValueNotWritten { get; set; } = "VALUE_NOT_WRITTEN";
    }
    
    /// <summary>
    /// Helper Classes for any purposes
    /// </summary>
    public static class HelperFunctions
    {
        /// <summary>
        /// Shuffle a given list
        /// </summary>
        /// <param name="inputList"></param>
        /// <typeparam name="T"></typeparam>
        public static void Shuffle<T>(this IList<T> inputList)
        {
            Random random = new Random();
            if (inputList.Count > 1)
            {
                for (int i = inputList.Count - 1; i >= 0; i--)
                {
                    T tmp = inputList[i];
                    int randomIndex = random.Next(i + 1);

                    //Swap elements
                    inputList[i] = inputList[randomIndex];
                    inputList[randomIndex] = tmp;
                }
            }
        }
    }
}