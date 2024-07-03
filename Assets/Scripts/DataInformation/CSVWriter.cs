using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Source.DataInformation
{
    public class CSVWriter:IDataWriting
    {
        // We store the Elements before we clear the information in the Model

        private static CSVWriter _instance;
        public const string PathToLogFiles = ("/Logfiles/");
        private string _pathToLogFiles;
        private readonly string _nameLogfile = "TrialRecord.csv";
        private readonly string _nameLogfileQuestionnaire = ".csv";

        private readonly string _delimiter = ";";
        
        // The default Questionnaire Recording
        private readonly string[] _rowNamesQuestionnaire = 
        {
            "PlayerName",
            "QuestionName",
            "Value"
        };
        
        public static CSVWriter Instance()
        {
            if (_instance == null)
            {
                _instance = new CSVWriter();
            }

            return _instance;
        }

        /// <summary>
        /// Writes Results of the Trial into a CSV file
        /// </summary>
        /// <param name="trialsToWrite"></param>
        public void WriteTrialsIntoCsvFile(ListPrintable[] trialsToWrite)
        {
            // Create the Directory for the Logfile
            if (!Directory.Exists(_pathToLogFiles)) Directory.CreateDirectory(_pathToLogFiles);

            string filePath = string.Concat(_pathToLogFiles, $"{GetTimeStampAsString()}_{trialsToWrite[0].UserId}_{SceneManager.GetActiveScene().name}_{_nameLogfile}");
            Debug.Log("filePath:" + filePath);

            if (!File.Exists(filePath))
            {
                StringBuilder sb = new StringBuilder();
                List<string> rowsToWrite = new List<string>();
                
                // Write the Headers
                foreach (string rowName in trialsToWrite[0].GetHeaderOfFile())
                {
                    rowsToWrite.Add(rowName);
                }

                sb.AppendLine(string.Join(_delimiter, rowsToWrite));

                // REWORK HERE TO MAKE SURE IT'S WRITING ROWS and NOT COLLUMNS
                // Write the Rows per Trial
                foreach (ListPrintable informationContainer in trialsToWrite)
                {
                    rowsToWrite.Clear();

                    rowsToWrite = informationContainer.GetResultsOfRecording();

                    var output = string.Join(_delimiter, rowsToWrite);
                    Debug.Log("outPut:" +output);
                    sb.AppendLine(output);
                }
                
                File.WriteAllText(filePath, sb.ToString());
            }
        }

        public IEnumerator WriteTrialsIntoCsvFile(ListPrintable[] trialsToWrite, string postAddress)
        {
            // TODO: FiX Here post address
            _pathToLogFiles = string.Concat(Application.dataPath,postAddress);
            
            // Create the Directory for the Logfile
            if (!Directory.Exists(_pathToLogFiles)) Directory.CreateDirectory(_pathToLogFiles);

            string filePath = string.Concat(_pathToLogFiles, $"{GetTimeStampAsString()}_{trialsToWrite[0].UserId}_{SceneManager.GetActiveScene().name}_{_nameLogfile}");
            Debug.Log("filePath:" + filePath);

            if (!File.Exists(filePath))
            {
                StringBuilder sb = new StringBuilder();
                List<string> rowsToWrite = new List<string>();
                
                // Write the Headers
                foreach (string rowName in trialsToWrite[0].GetHeaderOfFile())
                {
                    rowsToWrite.Add(rowName);
                }

                sb.AppendLine(string.Join(_delimiter, rowsToWrite));

                // REWORK HERE TO MAKE SURE IT'S WRITING ROWS and NOT COLUMNS
                // Write the Rows per Trial
                foreach (ListPrintable informationContainer in trialsToWrite)
                {
                    rowsToWrite.Clear();

                    rowsToWrite = informationContainer.GetResultsOfRecording();

                    var output = string.Join(_delimiter, rowsToWrite);
                    Debug.Log("outPut:" +output);
                    sb.AppendLine(output);
                }
                
                File.WriteAllText(filePath, sb.ToString());
            }
            
            yield return null;
        }

        public IEnumerator WritePositionLogIntoCSVFile(ListPrintable positionInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a CSV File of the Questionnaire to the CSV Folder
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="input"></param>
        /// <param name="nameOfQuestionnaire"></param>
        public void WriteQuestionnaire(string userID, string folderPath, string conditionName, Dictionary<string,string> input,string nameOfQuestionnaire)
        {
            var folder = Path.Combine(folderPath, conditionName);


            // Create the Directory for the Logfile
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filePath = string.Concat(folder, $"{GetTimeStampAsString()}_{nameOfQuestionnaire}_{userID}{_nameLogfileQuestionnaire}");
            Debug.Log("filePath:" + filePath);

            if (!File.Exists(filePath))
            {
                StringBuilder sb = new StringBuilder();
                List<string> rowsToWrite = new List<string>();
                
                // Write the Headers
                foreach (string rowName in _rowNamesQuestionnaire)
                {
                    rowsToWrite.Add(rowName);
                }
                sb.AppendLine(string.Join(_delimiter, rowsToWrite));

                foreach (var infoNode in input) 
                {
                    rowsToWrite.Clear();
                    rowsToWrite.AddRange(new string[]{userID,infoNode.Key,infoNode.Value});
                        
                    var output = string.Join(_delimiter, rowsToWrite);
                    output = output.Replace(" ", "_");
                    output = output.Replace("/", "_");
                    output = output.Replace(":", "-");
                    
                    sb.AppendLine(output);
                }
                
                File.WriteAllText(filePath, sb.ToString());
            }
        }

        public IEnumerator WriteQuestionnaireAsCoroutine(string userID, string folderPath, string conditionName, Dictionary<string, string> input, string nameOfQuestionnaire)
        {
            var folder = Path.Combine(folderPath, conditionName);

            //throw new NotImplementedException();
            // Create the Directory for the Logfile
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filePath = string.Concat(folder, "/", $"{GetTimeStampAsString()}_{nameOfQuestionnaire}_{userID}{_nameLogfileQuestionnaire}");
            Debug.Log("filePath:" + filePath);

            if (!File.Exists(filePath))
            {
                StringBuilder sb = new StringBuilder();
                List<string> rowsToWrite = new List<string>();

                // Write the Headers
                foreach (string rowName in _rowNamesQuestionnaire)
                {
                    rowsToWrite.Add(rowName);
                }
                sb.AppendLine(string.Join(_delimiter, rowsToWrite));

                foreach (var infoNode in input)
                {
                    rowsToWrite.Clear();
                    rowsToWrite.AddRange(new string[] { userID, infoNode.Key, infoNode.Value });

                    var output = string.Join(_delimiter, rowsToWrite);
                    output = output.Replace(" ", "_");
                    output = output.Replace("/", "_");
                    output = output.Replace(":", "-");

                    sb.AppendLine(output);
                }

                File.WriteAllText(filePath, sb.ToString());
                yield return null;
            }
        }

        private string GetTimeStampAsString()
        {
            
            string DateStamp = DateTime.Now.ToString();
            DateStamp = DateStamp.Replace(" ", "_");
            DateStamp = DateStamp.Replace("/", "_");
            DateStamp = DateStamp.Replace(".", "_");
            DateStamp = DateStamp.Replace(",", "_");
            DateStamp = DateStamp.Replace(":", "-");
            return DateStamp;
        }



        public CSVWriter()
        {
            _pathToLogFiles = string.Concat(Application.dataPath,PathToLogFiles);
        }

    }
}