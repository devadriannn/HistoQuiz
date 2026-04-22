using TMPro;
using UnityEngine;

public class HeroesGalleryUI : MonoBehaviour
{
    public TMP_Dropdown heroDropdown;
    public TMP_Text heroNameText;
    public TMP_Text heroBioText;
    public SceneLoader loader;

    private HeroCollection heroCollection;

    private void Start()
    {
        LoadHeroes();
        PopulateDropdown();
        ShowHero(0);
    }

    private void LoadHeroes()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("hero_data");
        if (jsonFile == null) return;

        heroCollection = JsonUtility.FromJson<HeroCollection>(jsonFile.text);
    }

    private void PopulateDropdown()
    {
        if (heroDropdown == null || heroCollection == null || heroCollection.heroes == null) return;

        heroDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        foreach (var hero in heroCollection.heroes)
        {
            options.Add(hero.name);
        }

        heroDropdown.AddOptions(options);
        heroDropdown.onValueChanged.AddListener(ShowHero);
    }

    public void ShowHero(int index)
    {
        if (heroCollection == null || heroCollection.heroes == null || heroCollection.heroes.Count == 0) return;
        if (index < 0 || index >= heroCollection.heroes.Count) return;

        var hero = heroCollection.heroes[index];

        if (heroNameText != null) heroNameText.text = hero.name;
        if (heroBioText != null)
        {
            heroBioText.text =
                $"Birth Place: {hero.birthPlace}\n\n" +
                $"Known For: {string.Join(", ", hero.knownFor)}\n\n" +
                $"{hero.bio}";
        }
    }

    public void Back()
    {
        loader.LoadScene("StudentDashboard");
    }
}