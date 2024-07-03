using Source.ExperimentManagement.Management;
using UnityEngine;

namespace Source.ExperimentManagement
{
    /// <summary>
    /// Use this Template to Model any Application State for this experiment. Every Scene needs to have one basic LevelManager
    /// </summary>
    public abstract class ExperimentState :MonoBehaviour
    {
        protected ExperimentStateModel ExperimentStateModel;
        protected ExperimentController ExperimentController;


        public void SetupState(ExperimentStateModel stateModelReference, ExperimentController controllerReference)
        {
            ExperimentStateModel = stateModelReference;
            ExperimentController = controllerReference;
        }

        /// <summary>
        /// Starts the State. Use this to Initialize a State
        /// </summary>
        public abstract void StartState();
        
        /// <summary>
        /// Ends the state.
        /// </summary>
        public abstract void EndState();
        
    }
}