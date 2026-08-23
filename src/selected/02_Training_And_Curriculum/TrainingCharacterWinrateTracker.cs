using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TrainingCharacterWinrateTracker : MonoBehaviour
{
    public static TrainingCharacterWinrateTracker Instance { get; private set; }

    [Header("Display")]
    [SerializeField] private bool showOnScreen = false;
    [SerializeField] private bool logEveryNMatches = true;
    [SerializeField] private int logInterval = 1;

    [Header("Character Names")]
    [SerializeField]
    private string[] characterNames =
    {
        "Fin", "Skipler", "Lithra", "Lazy Bigus", "Rager",
        "Vander", "Chiback", "Steelager", "Lupen", "Visvia"
    };

    private readonly Dictionary<int, CharacterTrainingStats> statsByCharacter =
        new Dictionary<int, CharacterTrainingStats>();

    private int totalRecordedMatches = 0;

    private GUIStyle guiStyle;
    private string cachedDisplayText = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStats()
    {
        statsByCharacter.Clear();

        for (int i = 0; i < characterNames.Length; i++)
        {
            statsByCharacter[i] = new CharacterTrainingStats
            {
                characterId = i,
                characterName = characterNames[i]
            };
        }

        RebuildDisplayText();
    }

    public void RecordResult(int winnerId, int loserId, int loserPlayerNum)
    {
        EnsureCharacterExists(winnerId);
        EnsureCharacterExists(loserId);

        statsByCharacter[winnerId].wins++;
        statsByCharacter[winnerId].games++;

        statsByCharacter[loserId].losses++;
        statsByCharacter[loserId].games++;

        totalRecordedMatches++;

        RebuildDisplayText();

        if (logEveryNMatches && logInterval > 0 && totalRecordedMatches % logInterval == 0)
        {
            Debug.Log(cachedDisplayText);
        }
    }

    public void RecordTie(int charAId, int charBId)
    {
        EnsureCharacterExists(charAId);
        EnsureCharacterExists(charBId);

        statsByCharacter[charAId].ties++;
        statsByCharacter[charAId].games++;

        statsByCharacter[charBId].ties++;
        statsByCharacter[charBId].games++;

        totalRecordedMatches++;

        RebuildDisplayText();
    }

    private void EnsureCharacterExists(int characterId)
    {
        if (statsByCharacter.ContainsKey(characterId))
        {
            return;
        }

        statsByCharacter[characterId] = new CharacterTrainingStats
        {
            characterId = characterId,
            characterName = GetCharacterName(characterId)
        };
    }

    private string GetCharacterName(int characterId)
    {
        if (characterId >= 0 && characterId < characterNames.Length)
        {
            return characterNames[characterId];
        }

        return "Char " + characterId;
    }

    private void RebuildDisplayText()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== TRAINING CHARACTER WINRATES ===");
        sb.AppendLine("Total matches: " + totalRecordedMatches);
        sb.AppendLine();

        foreach (var pair in statsByCharacter)
        {
            CharacterTrainingStats s = pair.Value;

            if (s.games <= 0)
            {
                continue;
            }

            float winrate = s.GetWinrate() * 100f;

            sb.AppendLine(
                $"{s.characterName} [{s.characterId}] | " +
                $"WR: {winrate:0.0}% | " +
                $"W: {s.wins} | L: {s.losses} | T: {s.ties} | Games: {s.games}"
            );
        }

        cachedDisplayText = sb.ToString();
    }

    public string GetDisplayText()
    {
        return cachedDisplayText;
    }

    public void ResetStats()
    {
        InitializeStats();
        totalRecordedMatches = 0;
        RebuildDisplayText();
    }

    private void OnGUI()
    {
        if (!showOnScreen)
        {
            return;
        }

        if (guiStyle == null)
        {
            guiStyle = new GUIStyle(GUI.skin.box);
            guiStyle.alignment = TextAnchor.UpperLeft;
            guiStyle.fontSize = 16;
            guiStyle.normal.textColor = Color.white;
        }

        GUI.Box(
            new Rect(15, 15, 520, 360),
            cachedDisplayText,
            guiStyle
        );
    }
}

[System.Serializable]
public class CharacterTrainingStats
{
    public int characterId;
    public string characterName;

    public int games;
    public int wins;
    public int losses;
    public int ties;

    public float GetWinrate()
    {
        int decidedGames = wins + losses;

        if (decidedGames <= 0)
        {
            return 0f;
        }

        return (float)wins / decidedGames;
    }
}