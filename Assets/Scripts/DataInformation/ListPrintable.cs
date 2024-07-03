using System.Collections.Generic;

namespace Source.DataInformation
{
    
    /// <summary>
    /// An Abstract class for any data recording; use this to create custom logFiles which should be stored by the DataWritingInterface 
    /// </summary>
    public abstract class ListPrintable
    {
        public string UserId { get; set; }
        public int TrialId { get; set; }
        public int ConditionId { get; set; }

        /// <summary>
        /// Returns the Header of the File
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetHeaderOfFile();
        
        /// <summary>
        /// Returns the recording of the Trial
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetResultsOfRecording();

    }
}