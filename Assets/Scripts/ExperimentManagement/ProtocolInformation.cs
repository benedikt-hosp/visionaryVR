using UnityEngine;

namespace Source.ExperimentManagement
{
    /// <summary>
    ///     A simple Class for describing the Protocol of the experiment.
    ///     NOTE: This class is condition independent and gets applied for the complete experiment!
    /// </summary>
    [CreateAssetMenu(menuName = "StudySettings/ProtocolInformation")]
    public class ProtocolInformation : ScriptableObject
    {
        [Header("Protocol")] [SerializeField]
        public string ProtocolName;
        
        // Use the commands in special scenes to adjust loading paths etc.
        [Header("Scene AdditionalCommands")] [SerializeField]
        public string[] sceneCommands;

        [Header("Scene Order")] [SerializeField]
        public string[] ScenesToLoad;


    }
}