using System;
using System.Collections.Generic;

public enum QuestionType
{
    FourPics,
    TrueFalse,
    MultipleChoice
}

[Serializable]
public class QuestionData
{
    public QuestionType type;
    public string prompt;
    public List<string> clues = new List<string>();
    public List<string> options = new List<string>();
    public string correctAnswer;
    public bool correctBool;
}
