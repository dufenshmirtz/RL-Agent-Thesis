using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class PvEMLBotRuntimeSetup : MonoBehaviour
{
    [Header("Difficulty Models (drag & drop)")]
    [SerializeField] private NNModel easyModel;
    [SerializeField] private NNModel mediumModel;
    [SerializeField] private NNModel hardModel;

    [Header("Behavior")]
    [SerializeField] private bool useInferenceOnly = true;

    void Start()
    {
        if (!PvESelectionState.IsPvE) return;
        if (PvESelectionState.SelectedBotType != PvEBotType.MLAgent) return;

        CharacterManager[] managers = FindObjectsOfType<CharacterManager>();

        CharacterManager p1 = null;
        CharacterManager p2 = null;

        foreach (var m in managers)
        {
            if (m.playerNum == 1) p1 = m;
            if (m.playerNum == 2) p2 = m;
        }

        if (p1 == null || p2 == null)
        {
            Debug.LogError("PvE ML Setup: Could not find both players.");
            return;
        }

        if (PvESelectionState.IsRLAgentDemo && PvESelectionState.IsRLAgentDemoAgentVsAgent)
        {
            SetupMLAgent(p1, p2, PvESelectionState.RLAgentDemoAgent1Difficulty);
            SetupMLAgent(p2, p1, PvESelectionState.RLAgentDemoAgent2Difficulty);
            return;
        }

        CharacterManager botManager =
            PvESelectionState.SelectedBotSide == PvEBotSide.Player1 ? p1 : p2;
        CharacterManager humanManager =
            PvESelectionState.SelectedBotSide == PvEBotSide.Player1 ? p2 : p1;

        SetupMLBot(botManager, humanManager);
    }

    void SetupMLBot(CharacterManager botManager, CharacterManager enemyManager)
    {
        GameObject humanObj = enemyManager.gameObject;

        if (!SetupMLAgent(botManager, enemyManager, PvESelectionState.SelectedDifficulty))
        {
            return;
        }

        // HUMAN SIDE
        Character humanCharacter = enemyManager.GetCurrentCharacter();
        if (humanCharacter != null)
        {
            humanCharacter.SetInput(new KeyboardInputProvider());
        }

        FighterAgent humanML = humanObj.GetComponent<FighterAgent>();
        if (humanML != null)
        {
            humanML.ClearInput();
            humanML.enabled = false;
        }

        Unity.MLAgents.DecisionRequester humanDecision = humanObj.GetComponent<Unity.MLAgents.DecisionRequester>();
        if (humanDecision != null)
        {
            humanDecision.enabled = false;
        }

        SimpleBotController humanScripted = humanObj.GetComponent<SimpleBotController>();
        if (humanScripted != null)
        {
            humanScripted.enabled = false;
        }
    }

    bool SetupMLAgent(CharacterManager agentManager, CharacterManager enemyManager, PvEDifficulty difficulty)
    {
        GameObject agentObj = agentManager.gameObject;

        FighterAgent ml = agentObj.GetComponent<FighterAgent>();
        if (ml == null)
        {
            Debug.LogError($"PvE ML Setup: No FighterAgent found on Player {agentManager.playerNum}.");
            return false;
        }

        Unity.MLAgents.DecisionRequester decision = agentObj.GetComponent<Unity.MLAgents.DecisionRequester>();
        BehaviorParameters behavior = agentObj.GetComponent<BehaviorParameters>();
        SimpleBotController scripted = agentObj.GetComponent<SimpleBotController>();

        if (scripted != null)
        {
            scripted.enabled = false;
        }

        if (behavior == null)
        {
            Debug.LogError($"PvE ML Setup: No BehaviorParameters found on Player {agentManager.playerNum}.");
            return false;
        }

        NNModel selectedModel = GetModelFromDifficulty(difficulty);

        if (selectedModel == null)
        {
            Debug.LogError($"PvE ML Setup: No model assigned for difficulty {difficulty}.");
            return false;
        }

        ml.selfManager = agentManager;
        ml.enemyManager = enemyManager;

        behavior.Model = selectedModel;
        behavior.BehaviorType = useInferenceOnly ? BehaviorType.InferenceOnly : BehaviorType.Default;

        ml.enabled = true;

        if (decision != null)
        {
            decision.enabled = true;
        }

        ml.ForceRebind();
        ml.ClearInput();

        Debug.Log(
            $"ML Agent Enabled on Player {agentManager.playerNum} | Difficulty: {difficulty} | Model: {selectedModel.name}"
        );

        return true;
    }

    NNModel GetModelFromDifficulty(PvEDifficulty difficulty)
    {
        if (PvESelectionState.IsRLAgentDemo)
        {
            NNModel demoModel = RLAgentDemoModelOverrides.GetModel(difficulty);
            if (demoModel != null)
            {
                return demoModel;
            }
        }

        switch (difficulty)
        {
            case PvEDifficulty.Easy:
            {
                return easyModel;
            }

            case PvEDifficulty.Medium:
            {
                return mediumModel;
            }

            case PvEDifficulty.Hard:
            {
                return hardModel;
            }
        }

        return mediumModel;
    }
}
