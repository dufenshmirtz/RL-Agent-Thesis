public enum PvEBotType
{
    MLAgent,
    ScriptedBot
}

public enum PvEDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum PvEBotSide
{
    Player1,
    Player2
}

public static class PvESelectionState
{
    public static bool IsPvE = false;
    public static bool IsRLAgentDemo = false;
    public static bool IsRLAgentDemoAgentVsAgent = false;
    public static PvEDifficulty RLAgentDemoAgent1Difficulty = PvEDifficulty.Hard;
    public static PvEDifficulty RLAgentDemoAgent2Difficulty = PvEDifficulty.Hard;
    public static PvEBotType SelectedBotType = PvEBotType.ScriptedBot;
    public static PvEDifficulty SelectedDifficulty = PvEDifficulty.Easy;
    public static PvEBotSide SelectedBotSide = PvEBotSide.Player1;

    public static void ResetToDefaults()
    {
        IsPvE = false;
        IsRLAgentDemo = false;
        IsRLAgentDemoAgentVsAgent = false;
        RLAgentDemoAgent1Difficulty = PvEDifficulty.Hard;
        RLAgentDemoAgent2Difficulty = PvEDifficulty.Hard;
        SelectedBotType = PvEBotType.ScriptedBot;
        SelectedDifficulty = PvEDifficulty.Easy;
        SelectedBotSide = PvEBotSide.Player1;
    }
}
