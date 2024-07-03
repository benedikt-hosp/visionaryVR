using System.Collections.Generic;
using UnityEngine;

namespace Content.Source.DataInformation
{
    public abstract class QuestionUI : MonoBehaviour
    {

        protected Question _RawQuestionInformation;
        
        /// <summary>
        /// Returns the Answers of all Questions
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<string, string> GetAnswersOfQuestion();

        public virtual void SetupQuestionUI(Question question, Transform parentAnchor)
        {
            this.transform.SetParent(parentAnchor);
            _RawQuestionInformation = question;
        }

        public abstract bool isQuestionValid();
    }
}