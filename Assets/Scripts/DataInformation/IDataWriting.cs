using System.Collections;
using System.Collections.Generic;

namespace Source.DataInformation
{
    public interface IDataWriting
    {
        IEnumerator WriteTrialsIntoCsvFile(ListPrintable[] trialsToWrite, string postAddress);

        IEnumerator WriteQuestionnaireAsCoroutine(string userID, string folderPath, string conditionName, Dictionary<string, string> input, string nameOfQuestionnaire);
    }
}