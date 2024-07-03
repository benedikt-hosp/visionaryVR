using UnityEngine;

namespace Source.ExperimentManagement
{
    /// <summary>
    /// A simple Class for Setting up Experiment Condition information of the experiment
    /// </summary>
    [CreateAssetMenu(menuName = "StudySettings/ExperimentCondition")]
    public class ConditionParameters : ScriptableObject
    {
        [Header("Basic Settings")] 
        [SerializeField] public string condition = "Default";
        
        [Header("The Experiment Protocol for this Condition")]
        [SerializeField] public ProtocolInformation experimentProtocolForCondition;

        
        
        
        
        /// <summary>
        /// Use this to determine how often all trigger need to be repeated.
        /// </summary>
        //[SerializeField] public int repetitionPerTrigger = 6;

        //[SerializeField] public CameraMovement cameraMovement = CameraMovement.ThirdPerson;

        //[SerializeField] public bool useCharacterCustomizer;

    }
}