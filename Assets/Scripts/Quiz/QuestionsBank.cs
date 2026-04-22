using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class QuestionsBank : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string questionId;
        public string prompt;
        public Texture2D imageTexture;
        [TextArea(2, 4)] public string hintText;
        [TextArea(2, 5)] public string factText;
        public int correctIndex;
        public List<string> options = new List<string> { string.Empty, string.Empty, string.Empty, string.Empty };
        public int level = 1;
    }

    public List<Entry> questions = new List<Entry>();

    private void Reset()
    {
        PopulateDefaultsIfEmpty(true);
    }

    private void OnValidate()
    {
        EnsureIntegrity();
        PopulateDefaultsIfEmpty(false);
    }

    [ContextMenu("Populate Default Questions")]
    public void PopulateDefaultQuestions()
    {
        questions.Clear();
        PopulateDefaultsIfEmpty(true);
    #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
    #endif
    }

    private void PopulateDefaultsIfEmpty(bool overwriteExisting)
    {
        if (!overwriteExisting && questions.Count > 0) return;
        if (overwriteExisting) questions.Clear();
        if (questions.Count > 0) return;

        // Level 1: Image-based Landmark Questions
        AddDefault(1, "", "aguinaldo_shrine", "It is located in Kawit, Cavite.", "The correct answer is Aguinaldo Shrine. This is where independence from Spain was proclaimed in 1898.", 0, "Aguinaldo Shrine", "Rizal Shrine", "Mabini House", "Quezon Hall");
        AddDefault(1, "", "barasoain_church", "It is located in Malolos, Bulacan.", "The correct answer is Barasoain Church. It hosted the Malolos Congress and the first Philippine Republic.", 1, "Manila Cathedral", "Barasoain Church", "Quiapo Church", "San Agustin Church");
        AddDefault(1, "", "bonifacio_monument", "It is located at a major rotunda in Caloocan.", "The correct answer is Bonifacio Monument. It was designed by Guillermo Tolentino to honor Andres Bonifacio.", 2, "Rizal Monument", "Lapu-Lapu Shrine", "Bonifacio Monument", "Leyte Landing");
        AddDefault(1, "", "corregidor_ruins", "It served as a key coastal defense during World War II.", "The correct answer is Corregidor. This island fortress was the last to fall to Japanese forces in 1942.", 3, "Intramuros", "Fort Santiago", "Bataan Shrine", "Corregidor Ruins");
        AddDefault(1, "", "Lunetaa", "It is formally known as Rizal Park.", "The correct answer is Luneta. It is one of the largest urban parks in Asia and a center for Philippine history.", 0, "Luneta", "Paco Park", "Quezon Circle", "Eco Park");
        AddDefault(1, "", "mactan_shrine", "It features a statue of the chieftain who defeated Magellan.", "The correct answer is Mactan Shrine. It honors Lapu-Lapu, the first Filipino hero who resisted Spanish colonization.", 1, "Magellan's Cross", "Mactan Shrine", "Fort San Pedro", "Basilica del Sto. Niño");

        // Level 1: Text-based History Questions
        AddDefault(1, "Who wrote the national anthem's lyrics?", null, "Lupang Hinirang", "Jose Palma wrote 'Filipinas' in 1899.", 2, "Julian Felipe", "Juan Luna", "Jose Palma", "Nick Joaquin");
        AddDefault(1, "What is the capital of the Philippines?", null, "NCR", "Manila is the seat of government.", 0, "Manila", "Quezon City", "Cebu", "Davao");
        AddDefault(1, "Who is the national hero of the Philippines?", null, "He wrote 'Noli Me Tangere'.", "The correct answer is Jose Rizal.", 0, "Jose Rizal", "Andres Bonifacio", "Emilio Aguinaldo", "Apolinario Mabini");

        // Level 2 Questions (Heroes Gallery) - Based on Hero Stories
        AddDefault(2, "Who is known as the 'Brains of the Revolution'?", null, "He was a advisor to Aguinaldo.", "Apolinario Mabini earned this title due to his intellect despite his paralysis.", 3, "Jose Rizal", "Andres Bonifacio", "Emilio Aguinaldo", "Apolinario Mabini");
        AddDefault(2, "In which province was Emilio Aguinaldo born?", null, "Kawit", "Emilio Aguinaldo was born on March 22, 1869, in Kawit, Cavite.", 0, "Cavite", "Laguna", "Batangas", "Bulacan");
        AddDefault(2, "Who founded the Katipunan, a secret revolutionary society?", null, "The 'Supremo'", "Andres Bonifacio founded the Katipunan to fight for independence through armed struggle.", 1, "Jose Rizal", "Andres Bonifacio", "Emilio Aguinaldo", "Antonio Luna");
        AddDefault(2, "Which hero was a doctor specializing in ophthalmology?", null, "He studied in Spain and Germany.", "Jose Rizal became an ophthalmologist to treat his mother's eye condition.", 0, "Jose Rizal", "Apolinario Mabini", "Antonio Luna", "Juan Luna");
        AddDefault(2, "Who was the chieftain of Mactan that defeated Ferdinand Magellan?", null, "1521", "Lapu-Lapu defeated Magellan in the Battle of Mactan on April 27, 1521.", 2, "Humabon", "Sikatuna", "Lapu-Lapu", "Dagohoy");
        AddDefault(2, "True or False: Antonio Luna was a highly educated Filipino who studied pharmacy in Spain.", null, "Heneral Luna", "True. Antonio Luna studied pharmacy and was also a scientist.", 0, "True", "False", "", "");
        AddDefault(2, "True or False: Andres Bonifacio was born into a wealthy and highly educated family.", null, "The Supremo", "False. Bonifacio came from a humble background and worked hard to support his siblings.", 1, "True", "False", "", "");
        AddDefault(2, "Where was Antonio Luna assassinated?", null, "Nueva Ecija", "Antonio Luna was assassinated on June 5, 1899, in Cabanatuan, Nueva Ecija.", 1, "Manila", "Cabanatuan", "Kawit", "Dapitan");
        AddDefault(2, "Which hero was exiled to Dapitan for four years?", null, "The National Hero", "Jose Rizal was exiled to Dapitan where he served as a doctor and teacher.", 0, "Jose Rizal", "Andres Bonifacio", "Emilio Aguinaldo", "Apolinario Mabini");
        AddDefault(2, "Who is the 'Father of the Philippine Revolution'?", null, "Katipunan Founder", "Andres Bonifacio is honored with this title for his leadership in the revolution.", 1, "Jose Rizal", "Andres Bonifacio", "Emilio Aguinaldo", "Lapu-Lapu");
        AddDefault(2, "True or False: Apolinario Mabini was known as the 'Brains of the Revolution'.", null, "Advisor to Aguinaldo", "True. Despite his paralysis, Mabini's sharp intellect was crucial to the revolutionary government.", 0, "True", "False", "", "");
        AddDefault(2, "When was José Rizal executed at Bagumbayan?", null, "1896", "José Rizal was executed on December 30, 1896.", 2, "June 12, 1898", "November 30, 1863", "December 30, 1896", "August 23, 1896");
        AddDefault(2, "Which hero wrote the novels 'Noli Me Tangere' and 'El Filibusterismo'?", null, "National Hero", "Jose Rizal wrote these novels to expose the abuses of Spanish authorities.", 0, "Jose Rizal", "Marcelo H. del Pilar", "Graciano Lopez Jaena", "Juan Luna");
        AddDefault(2, "True or False: Lapu-Lapu is considered one of the first Filipino heroes to resist foreign colonization.", null, "Battle of Mactan", "True. Lapu-Lapu chose to defend his land and people against Ferdinand Magellan.", 0, "True", "False", "", "");
        AddDefault(2, "What was the secret revolutionary society founded by Bonifacio called?", null, "KKK", "The Katipunan (Kataastaasan, Kagalanggalangang Katipunan ng mga Anak ng Bayan) was founded in 1892.", 3, "La Liga Filipina", "Propaganda Movement", "Hukbalahap", "Katipunan");
}

    private void AddDefault(int level, string prompt, string imageResourceName, string hintText, string factText, int correctIndex, params string[] options)
    {
        Entry entry = new Entry();
        entry.questionId = Guid.NewGuid().ToString("N");
        entry.prompt = prompt;
        entry.level = level;
// Search in Resources or specific path if needed, but for defaults we assume they might be in Resources/QuestionsAssets/
        if (!string.IsNullOrEmpty(imageResourceName))
        {
            entry.imageTexture = Resources.Load<Texture2D>("QuestionsAssets/" + imageResourceName);
            if (entry.imageTexture == null)
            {
                // Try alternate path if not in Resources
                #if UNITY_EDITOR
                entry.imageTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Images/QuestionsLandmarkImage/" + imageResourceName + ".png");
                #endif
            }
        }
        entry.hintText = hintText;
        entry.factText = factText;
        entry.correctIndex = correctIndex;
        entry.options = new List<string>(options);
        questions.Add(entry);
    }

    private void EnsureIntegrity()
    {
        if (questions == null)
        {
            questions = new List<Entry>();
            return;
        }

        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i] == null)
            {
                questions[i] = new Entry();
            }

            if (string.IsNullOrWhiteSpace(questions[i].questionId))
            {
                questions[i].questionId = Guid.NewGuid().ToString("N");
            }

            if (questions[i].options == null)
{
                questions[i].options = new List<string>();
            }

            while (questions[i].options.Count < 4)
            {
                questions[i].options.Add(string.Empty);
            }

            if (questions[i].correctIndex < 0)
            {
                questions[i].correctIndex = 0;
            }
        }
    }
}
