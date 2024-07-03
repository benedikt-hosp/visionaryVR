// Author: Martin Dechant

using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Source.Interfaces
{
    public enum StopSignalType
    {
        None,
        Audio,
        Haptic,
        AudioAndHaptic
    }
    
    public enum StoryVersion
    {
        Approach, 
        Avoid
    }
    
    public enum CharacterVersion
    {
        Strong, 
        Weak
    }
    

    public enum Gender
    {
        None,
        Female,
        Male,
    }

    public enum Language
    {
        None, 
        German, 
        English
    }

    public enum CameraMovement
    {
        FirstPerson, 
        ThirdPerson
    }

    public enum SlotTypes
    {
        Hair, 
        Accessory,
        Shirt, 
        Pants, 
        Shoes,
        Beard
    }

    public class ConstantValues
    {
        public const float DistanceToStartStaringAtPlayer = 15f;
        public const string PathToConfigSettings = "/Config/Settings/";
        public const string PathToConfigQuestionnaires = "/Resources/Questionnaire/";
        public const string PathToConfigLanguage = "/Config/Language/";

        public const string PathToDefaultExperimentProtocolObject = "ExperimentProtocol/Protocol";
        public const string PathToDefaultRiddles = "ExperimentProtocol/Riddles";
        public const string PathToLogFiles = ("/Logfiles/");

        
        // Setup Variables for our experiment states
        public const string ValueNotWritten = "NOT_ANSWERED";

        public static readonly Dictionary<string, Color> HairColours = new Dictionary<string, Color>()
        {
            {"Red",new Color(0.54f, 0.11f, 0.04f, 0.75f)}, 
            {"Black",new Color(0.13f, 0.12f, 0.12f, 0.93f)}, 
            {"Brown",new Color(0.28f, 0.12f, 0.05f, 0.76f)}, 
            {"Blond",new Color(0.73f, 0.55f, 0.3f, 0.48f)},
            {"Gray",new Color(1f, 1f, 1f, 1f)},
            {"Blue",new Color(0.05f, 0.45f, 1f, 0.56f)},

        };
        
        public static readonly string[] HairColoursNames = new string [] {"Red", "Black", "Brown", "Blond", "Gray","Blue"};

        /// <summary>
        /// Returns the HairColour by the sleected String
        /// </summary>
        /// <param name="searchedColour"></param>
        /// <returns></returns>
        public static Color ConvertStringTohairColour(string searchedColour)
        {
            return HairColours[searchedColour];
        }
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