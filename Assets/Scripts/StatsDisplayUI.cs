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

            // VOLITELNÉ: Pokud chceš vypsat jen staty, které nejsou nulové
            // (Zruš komentář u tohoto IFu, pokud nechceš ukazovat např. "Armor: 0")
            // if (value == 0) continue; 

            // 3. Vytvoříme nový textový element (Label) přímo v kódu
            Label statLabel = new Label();

            // 4. Upravíme název. Regulární výraz "LifeSteal" rozdělí na "Life Steal"
            string formatedName = Regex.Replace(stat.ToString(), "([a-z])([A-Z])", "$1 $2");

            // 5. Poskládáme finální text (např. "Life Steal: 15")
            statLabel.text = $"{formatedName}: {value}";
            statLabel.style.color = Color.white;

            // Přidáme mu CSS třídu (volitelné), abys ho mohl v UI Builderu hromadně stylovat
            statLabel.AddToClassList("stat-row-text");

            // 6. Vložíme tento nový řádek do našeho kontejneru v UI
            statsContainer.Add(statLabel);
        }
    }
}