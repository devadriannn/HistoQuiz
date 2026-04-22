using System;
using System.Collections.Generic;

[Serializable]
public class HeroCollection
{
    public List<HeroData> heroes;
}

[Serializable]
public class HeroData
{
    public string name;
    public List<string> aliases;
    public string birthPlace;
    public List<string> knownFor;
    public List<string> titles;
    public List<string> clues;
    public List<string> trueFacts;
    public List<string> falseFacts;
    public string bio;
}