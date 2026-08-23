using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class TrainingOpponentDirector : MonoBehaviour
{
    [Header("Core References")]
    public GameManager gameManager;

    [Header("P1 References")]
    public FighterAgent agentP1;
    public BehaviorParameters behaviorP1;
    public DecisionRequester decisionP1;

    [Header("P2 References")]
    public FighterAgent agentP2;
    public BehaviorParameters behaviorP2;
    public DecisionRequester decisionP2;
    public SimpleBotController botP2;

    [Header("Models")]
    public NNModel inferenceModel1;
    public NNModel inferenceModel2;
    public NNModel inferenceModel3;

    [Header("Progressive Mixing Phases")]
    [SerializeField] private int phase2StartEpisode = 3000;
    [SerializeField] private int phase3StartEpisode = 5000;
    [SerializeField] private int phase4StartEpisode = 7000;

    [Header("Current Weights (debug)")]
    [Range(0f, 1f)] public float scriptedWeight = 0.55f;
    [Range(0f, 1f)] public float inference1Weight = 0.15f;
    [Range(0f, 1f)] public float inference2Weight = 0.10f;
    [Range(0f, 1f)] public float inference3Weight = 0.10f;
    [Range(0f, 1f)] public float mirrorWeight = 0.10f;

    [Header("Scripted Curriculum")]
    [Range(0f, 1f)] public float minScriptedSkill = 0.5f;
    [Range(0f, 1f)] public float maxScriptedSkill = 0.99f;
    public int curriculumEpisodes = 1000;

    [Header("Debug")]
    public OpponentMode currentMode;
    [SerializeField] private int episodeIndex = 0;
    public float currentScriptedSkill = 0.35f;

    private void Awake()
    {
        episodeIndex = 0;
        ValidateReferences();
        PrepareNextEpisode();
    }

    public OpponentMode SelectNextMode()
    {
        float total = scriptedWeight + inference1Weight + inference2Weight + inference3Weight + mirrorWeight;

        if (total <= 0f)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Weights sum to 0. Falling back to ScriptedBot.");
            currentMode = OpponentMode.ScriptedBot;
            return currentMode;
        }

        float r = Random.value * total;

        if (r < scriptedWeight)
        {
            currentMode = OpponentMode.ScriptedBot;
            return currentMode;
        }

        r -= scriptedWeight;

        if (r < inference1Weight)
        {
            currentMode = OpponentMode.InferenceModel1;
            return currentMode;
        }

        r -= inference1Weight;

        if (r < inference2Weight)
        {
            currentMode = OpponentMode.InferenceModel2;
            return currentMode;
        }

        r -= inference2Weight;

        if (r < inference3Weight)
        {
            currentMode = OpponentMode.InferenceModel3;
            return currentMode;
        }

        currentMode = OpponentMode.MirrorSelfPlay;
        return currentMode;
    }

    public float EvaluateScriptedSkill()
    {
        float t = curriculumEpisodes <= 0 ? 1f : Mathf.Clamp01((float)episodeIndex / curriculumEpisodes);
        currentScriptedSkill = Mathf.Lerp(minScriptedSkill, maxScriptedSkill, t);
        return currentScriptedSkill;
    }

    public void PrepareNextEpisode()
    {
        if (!gameManager.trainingMode)
        {
            return;
        }

        episodeIndex++;

        UpdateProgressiveWeights();
        SelectNextMode();
        EvaluateScriptedSkill();

        Debug.Log(
            $"[TrainingOpponentDirector] Episode {episodeIndex} | OpponentMode={currentMode} | " +
            $"Weights=({scriptedWeight:F2}, {inference1Weight:F2}, {inference2Weight:F2}, {inference3Weight:F2}, {mirrorWeight:F2}) | " +
            $"ScriptedSkill={currentScriptedSkill:F2}"
        );

        ApplyCurrentMode();
    }

    public void ApplyCurrentMode()
    {
        switch (currentMode)
        {
            case OpponentMode.ScriptedBot:
            {
                if (CanApplyScriptedMode())
                {
                    ApplyScriptedBotMode();
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] Scripted mode unavailable. Falling back to MirrorSelfPlay.");
                    currentMode = OpponentMode.MirrorSelfPlay;
                    ApplyMirrorMode();
                }
                break;
            }

            case OpponentMode.InferenceModel1:
            {
                if (CanApplyInferenceMode(inferenceModel1))
                {
                    ApplyInferenceMode(inferenceModel1);
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] InferenceModel1 unavailable. Falling back to ScriptedBot.");
                    currentMode = OpponentMode.ScriptedBot;
                    ApplyScriptedBotMode();
                }
                break;
            }

            case OpponentMode.InferenceModel2:
            {
                if (CanApplyInferenceMode(inferenceModel2))
                {
                    ApplyInferenceMode(inferenceModel2);
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] InferenceModel2 unavailable. Falling back to ScriptedBot.");
                    currentMode = OpponentMode.ScriptedBot;
                    ApplyScriptedBotMode();
                }
                break;
            }

            case OpponentMode.InferenceModel3:
            {
                if (CanApplyInferenceMode(inferenceModel3))
                {
                    ApplyInferenceMode(inferenceModel3);
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] InferenceModel3 unavailable. Falling back to ScriptedBot.");
                    currentMode = OpponentMode.ScriptedBot;
                    ApplyScriptedBotMode();
                }
                break;
            }

            case OpponentMode.MirrorSelfPlay:
            {
                if (CanApplyMirrorMode())
                {
                    ApplyMirrorMode();
                }
                else
                {
                    Debug.LogWarning("[TrainingOpponentDirector] Mirror mode unavailable. Falling back to ScriptedBot.");
                    currentMode = OpponentMode.ScriptedBot;
                    ApplyScriptedBotMode();
                }
                break;
            }
        }
    }

    public void RebindAfterCharacterSwap()
    {
        Character p1 = gameManager != null && gameManager.p1Manager != null
            ? gameManager.p1Manager.GetCurrentCharacter()
            : null;

        Character p2 = gameManager != null && gameManager.p2Manager != null
            ? gameManager.p2Manager.GetCurrentCharacter()
            : null;

        if (currentMode == OpponentMode.ScriptedBot)
        {
            if (botP2 != null && p2 != null && p1 != null)
            {
                botP2.Rebind(p2, p1);
            }
        }

        if (agentP1 != null)
        {
            agentP1.ForceRebind();
        }

        if (agentP2 != null && agentP2.enabled)
        {
            agentP2.ForceRebind();
        }
    }

    private void ApplyScriptedBotMode()
    {
        if (botP2 != null)
        {
            botP2.enabled = true;
            botP2.SetSkill(currentScriptedSkill);
        }

        if (agentP2 != null)
        {
            agentP2.ClearInput();
            agentP2.enabled = false;
        }

        if (decisionP2 != null)
        {
            decisionP2.enabled = false;
        }

        if (behaviorP2 != null)
        {
            behaviorP2.BehaviorType = BehaviorType.Default;
            behaviorP2.Model = null;
        }
    }

    private void ApplyMirrorMode()
    {
        if (botP2 != null)
        {
            botP2.enabled = false;
        }

        if (agentP2 != null)
        {
            agentP2.ClearInput();
            agentP2.enabled = true;
        }

        if (decisionP2 != null)
        {
            decisionP2.enabled = true;
        }

        if (behaviorP2 != null)
        {
            behaviorP2.BehaviorType = BehaviorType.Default;
            behaviorP2.Model = null;
        }
    }

    private void ApplyInferenceMode(NNModel model)
    {
        if (botP2 != null)
        {
            botP2.enabled = false;
        }

        if (agentP2 != null)
        {
            agentP2.ClearInput();
            agentP2.enabled = true;
        }

        if (decisionP2 != null)
        {
            decisionP2.enabled = true;
        }

        if (behaviorP2 != null)
        {
            behaviorP2.Model = model;
            behaviorP2.BehaviorType = BehaviorType.InferenceOnly;
        }
    }

    private void UpdateProgressiveWeights()
    {
        if (episodeIndex >= phase4StartEpisode)
        {
            scriptedWeight = 0.05f;
            inference1Weight = 0.25f;
            inference2Weight = 0.10f;
            inference3Weight = 0.10f;
            mirrorWeight = 0.50f;
        }
        else if (episodeIndex >= phase3StartEpisode)
        {
            scriptedWeight = 0.05f;
            inference1Weight = 0.35f;
            inference2Weight = 0.10f;
            inference3Weight = 0.10f;
            mirrorWeight = 0.40f;
        }
        else if (episodeIndex >= phase2StartEpisode)
        {
            scriptedWeight = 0.20f;
            inference1Weight = 0.20f;
            inference2Weight = 0.20f;
            inference3Weight = 0.15f;
            mirrorWeight = 0.25f;
        }
        else
        {
            scriptedWeight = 0.30f;
            inference1Weight = 0.15f;
            inference2Weight = 0.20f;
            inference3Weight = 0.25f;
            mirrorWeight = 0.10f;
        }
    }

    private void ValidateReferences()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing GameManager reference.");
        }

        if (agentP1 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing agentP1 reference.");
        }

        if (behaviorP1 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing behaviorP1 reference.");
        }

        if (decisionP1 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing decisionP1 reference.");
        }

        if (agentP2 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing agentP2 reference.");
        }

        if (behaviorP2 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing behaviorP2 reference.");
        }

        if (decisionP2 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing decisionP2 reference.");
        }

        if (botP2 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing botP2 reference.");
        }

        if (inferenceModel1 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing inferenceModel1 reference.");
        }

        if (inferenceModel2 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing inferenceModel2 reference.");
        }

        if (inferenceModel3 == null)
        {
            Debug.LogWarning("[TrainingOpponentDirector] Missing inferenceModel3 reference.");
        }
    }

    private bool CanApplyScriptedMode()
    {
        return botP2 != null;
    }

    private bool CanApplyInferenceMode(NNModel model)
    {
        return agentP2 != null && decisionP2 != null && behaviorP2 != null && model != null;
    }

    private bool CanApplyMirrorMode()
    {
        return agentP2 != null && decisionP2 != null && behaviorP2 != null;
    }

    public OpponentMode GetCurrentMode()
    {
        return currentMode;
    }

    public string GetCurrentModeName()
    {
        return currentMode.ToString();
    }

    public bool IsScriptedMode()
    {
        return currentMode == OpponentMode.ScriptedBot;
    }

    public bool IsInferenceMode()
    {
        return currentMode == OpponentMode.InferenceModel1
            || currentMode == OpponentMode.InferenceModel2
            || currentMode == OpponentMode.InferenceModel3;
    }

    public bool IsMirrorMode()
    {
        return currentMode == OpponentMode.MirrorSelfPlay;
    }
}