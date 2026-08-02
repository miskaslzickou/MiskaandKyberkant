using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class StatsDisplayUI : MonoBehaviour
{
    public UIDocument uiDocument;
    public PlayerStats playerStats; // Odkaz na tvůj nový zautomatizovaný skript

    private VisualElement statsContainer;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        // Najdeme pouze náš jeden hlavní prázdný kontejner
        statsContainer = root.Q<VisualElement>("stats-container");

        if (playerStats != null)
        {
            // Přihlásíme se k odběru změn
            playerStats.OnStatsChanged += UpdateUI;
            UpdateUI(); // Vykreslíme hned při startu
        }
    }

    private void UpdateUI()
    {
        if (statsContainer == null) return;

        // 1. DŮLEŽITÉ: Než vykreslíme nové staty, smažeme ty staré,
        // jinak by se nám texty donekonečna kopírovaly pod sebe.
        statsContainer.Clear();

        // 2. Projdeme všechny existující staty z našeho Enumu
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {

            // Vytáhneme si hodnotu z našeho slovníku v PlayerStats
            float value = playerStats.GetStat(stat);

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween; // Tohle roztáhne texty od sebe!
            row.style.width = new Length(100, LengthUnit.Percent); // Řádek zabere celou šířku panelu
            row.style.marginBottom = 5; // Malá mezera pod řádkem

            
            

        
            string formatedName = Regex.Replace(stat.ToString(), "([a-z])([A-Z])", "$1 $2");

            
            Label nameLabel = new Label();
            nameLabel.text = formatedName;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            
            Label valueLabel = new Label();
            valueLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            valueLabel.text = value.ToString("0.##"); // Formátování na 2 desetinná místa
            nameLabel.style.color = new StyleColor(Color.white);
            valueLabel.style.color = new StyleColor(Color.white);
            nameLabel.style.fontSize = 14;
            valueLabel.style.fontSize = 14;
            row.Add(nameLabel);
            row.Add(valueLabel);
            statsContainer.Add(row);

        }
    }
}