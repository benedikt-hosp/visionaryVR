using System.Collections.Generic;

namespace Content.Source.DataInformation
{
    /// <summary>
    /// Container class for all our Questions of ONE Questionnaire. Combine these objects to a questionnaire view.
    /// </summary>
    public class Questionnaire
    {
        public string title { get; set; }
        public string instructions{ get; set; } 
        public string code{ get; set; }

        // A list of Questions
        public List<Question> questions { get; set; }
    }

    /// <summary>
    /// The raw experiment Item
    /// </summary>
    public class Question
    {
        public string questiontype { get; set; }
        
        public string instructions { get; set; }
        
        public string id{ get; set; }
        public bool shuffle{ get; set; }
        public string[] labels { get; set; }

        public string[] items { get; set; }
        public List<QuestionItem> q_text { get; set; }

        public bool required { get; set; }

        // slider
        public string left { get; set; }
        public string right { get; set; }

        public int tick_count { get; set; }
    }

    public class Questionscontainer
    {
        public List<QuestionItem> qText;
    }

    /// <summary>
    /// A simple Question item which stores the ID and a explanation text of the question. Use this to identify sub elements in a question (e.g. possible answers)
    /// </summary>
    public class QuestionItem
    {
        public string id { get; set; }
        public string text { get; set; }
    }
}