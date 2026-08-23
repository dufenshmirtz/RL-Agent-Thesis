using UnityEngine;
using System.Text;

public class FighterAgentRewardDebugger : MonoBehaviour
{
    [Header("Debug Toggles")]
    [SerializeField] private bool debugRewardBreakdown = false;
    [SerializeField] private bool debugProfileLogging = false;
    [SerializeField] private int debugLogEveryNEpisodes = 10;

    private int localEpisodeCounter = 0;

    // totals
    private float epStepPenaltyTotal;
    private float epDamageDealtRewardTotal;
    private float epDamageTakenRewardTotal;
    private float epSpacingRewardTotal;
    private float epStackPenaltyTotal;
    private float epMashPenaltyTotal;
    private float epAirJumpPenaltyTotal;
    private float epEdgeCampPenaltyTotal;
    private float epApproachRewardTotal;
    private float epExtremeFarHeavyPenaltyTotal;
    private float epExtremeFarChargePenaltyTotal;
    private float epFarMeleeLightPenaltyTotal;
    private float epFarMeleeSpecialPenaltyTotal;
    private float epDashNoDirectionPenaltyTotal;
    private float epWrongFacingSpecialPenaltyTotal;
    private float epBlockHoldPenaltyTotal;
    private float epBlockStartPenaltyTotal;
    private float epAimlessBlockPenaltyTotal;
    private float epBlockedAdvancePenaltyTotal;
    private float epBlockInsteadParryPenaltyTotal;
    private float epRepeatSameMovePenaltyTotal;
    private float epChargeSpamPenaltyTotal;
    private float epEmptyChargeReleasePenaltyTotal;
    private float epWinRewardTotal;
    private float epLossRewardTotal;

    // counts
    private int epStepPenaltyCount;
    private int epDamageDealtCount;
    private int epDamageTakenCount;
    private int epSpacingCount;
    private int epStackPenaltyCount;
    private int epMashPenaltyCount;
    private int epAirJumpPenaltyCount;
    private int epEdgeCampPenaltyCount;
    private int epApproachRewardCount;
    private int epExtremeFarHeavyPenaltyCount;
    private int epExtremeFarChargePenaltyCount;
    private int epFarMeleeLightPenaltyCount;
    private int epFarMeleeSpecialPenaltyCount;
    private int epDashNoDirectionPenaltyCount;
    private int epWrongFacingSpecialPenaltyCount;
    private int epBlockHoldPenaltyCount;
    private int epBlockStartPenaltyCount;
    private int epAimlessBlockPenaltyCount;
    private int epBlockedAdvancePenaltyCount;
    private int epBlockInsteadParryPenaltyCount;
    private int epRepeatSameMovePenaltyCount;
    private int epChargeSpamPenaltyCount;
    private int epEmptyChargeReleasePenaltyCount;
    private int epWinRewardCount;
    private int epLossRewardCount;

    public void BeginEpisode(string playerSuffix, int? characterId, ReachType lightReach, ReachType specialReach, bool profileLoaded)
    {
        localEpisodeCounter++;
        ResetEpisodeRewardDebug();

        if (debugProfileLogging)
        {
            Debug.Log(
                $"[Agent {playerSuffix}] Episode {localEpisodeCounter} BEGIN | " +
                $"CharacterID={(characterId.HasValue ? characterId.Value.ToString() : "?")} | " +
                $"LightReach={lightReach} | SpecialReach={specialReach} | ProfileLoaded={profileLoaded}"
            );
        }
    }

    public void LogStepPenalty(float value)
    {
        epStepPenaltyTotal += value;
        epStepPenaltyCount++;
    }

    public void LogDamageDealt(float value)
    {
        epDamageDealtRewardTotal += value;
        if (value != 0f)
        {
            epDamageDealtCount++;
        }
    }

    public void LogDamageTaken(float value)
    {
        epDamageTakenRewardTotal += value;
        if (value != 0f)
        {
            epDamageTakenCount++;
        }
    }

    public void LogSpacing(float value)
    {
        epSpacingRewardTotal += value;
        epSpacingCount++;
    }

    public void LogStackPenalty(float value)
    {
        epStackPenaltyTotal += value;
        epStackPenaltyCount++;
    }

    public void LogMashPenalty(float value)
    {
        epMashPenaltyTotal += value;
        epMashPenaltyCount++;
    }

    public void LogAirJumpPenalty(float value)
    {
        epAirJumpPenaltyTotal += value;
        epAirJumpPenaltyCount++;
    }

    public void LogEdgeCampPenalty(float value)
    {
        epEdgeCampPenaltyTotal += value;
        epEdgeCampPenaltyCount++;
    }

    public void LogApproachReward(float value)
    {
        epApproachRewardTotal += value;
        epApproachRewardCount++;
    }

    public void LogExtremeFarHeavyPenalty(float value)
    {
        epExtremeFarHeavyPenaltyTotal += value;
        epExtremeFarHeavyPenaltyCount++;
    }

    public void LogExtremeFarChargePenalty(float value)
    {
        epExtremeFarChargePenaltyTotal += value;
        epExtremeFarChargePenaltyCount++;
    }

    public void LogFarMeleeLightPenalty(float value)
    {
        epFarMeleeLightPenaltyTotal += value;
        epFarMeleeLightPenaltyCount++;
    }

    public void LogFarMeleeSpecialPenalty(float value)
    {
        epFarMeleeSpecialPenaltyTotal += value;
        epFarMeleeSpecialPenaltyCount++;
    }

    public void LogDashNoDirectionPenalty(float value)
    {
        epDashNoDirectionPenaltyTotal += value;
        epDashNoDirectionPenaltyCount++;
    }

    public void LogWrongFacingSpecialPenalty(float value)
    {
        epWrongFacingSpecialPenaltyTotal += value;
        epWrongFacingSpecialPenaltyCount++;
    }

    public void LogBlockHoldPenalty(float value)
    {
        epBlockHoldPenaltyTotal += value;
        epBlockHoldPenaltyCount++;
    }

    public void LogBlockStartPenalty(float value)
    {
        epBlockStartPenaltyTotal += value;
        epBlockStartPenaltyCount++;
    }

    public void LogAimlessBlockPenalty(float value)
    {
        epAimlessBlockPenaltyTotal += value;
        epAimlessBlockPenaltyCount++;
    }

    public void LogBlockedAdvancePenalty(float value)
    {
        epBlockedAdvancePenaltyTotal += value;
        epBlockedAdvancePenaltyCount++;
    }

    public void LogBlockInsteadParryPenalty(float value)
    {
        epBlockInsteadParryPenaltyTotal += value;
        epBlockInsteadParryPenaltyCount++;
    }

    public void LogRepeatSameMovePenalty(float value)
    {
        epRepeatSameMovePenaltyTotal += value;
        epRepeatSameMovePenaltyCount++;
    }

    public void LogChargeSpamPenalty(float value)
    {
        epChargeSpamPenaltyTotal += value;
        epChargeSpamPenaltyCount++;
    }

    public void LogEmptyChargeReleasePenalty(float value)
    {
        epEmptyChargeReleasePenaltyTotal += value;
        epEmptyChargeReleasePenaltyCount++;
    }

    public void LogWinReward(float value)
    {
        epWinRewardTotal += value;
        epWinRewardCount++;
    }

    public void LogLossReward(float value)
    {
        epLossRewardTotal += value;
        epLossRewardCount++;
    }

    public void EndEpisode(
        string playerSuffix,
        string endReason,
        int? characterId,
        ReachType lightReach,
        ReachType specialReach,
        bool profileLoaded)
    {
        if (!debugRewardBreakdown)
        {
            return;
        }

        if (debugLogEveryNEpisodes <= 0)
        {
            debugLogEveryNEpisodes = 1;
        }

        if (localEpisodeCounter % debugLogEveryNEpisodes != 0)
        {
            return;
        }

        float total =
            epStepPenaltyTotal +
            epDamageDealtRewardTotal +
            epDamageTakenRewardTotal +
            epSpacingRewardTotal +
            epStackPenaltyTotal +
            epMashPenaltyTotal +
            epAirJumpPenaltyTotal +
            epEdgeCampPenaltyTotal +
            epApproachRewardTotal +
            epExtremeFarHeavyPenaltyTotal +
            epExtremeFarChargePenaltyTotal +
            epFarMeleeLightPenaltyTotal +
            epFarMeleeSpecialPenaltyTotal +
            epDashNoDirectionPenaltyTotal +
            epWrongFacingSpecialPenaltyTotal +
            epBlockHoldPenaltyTotal +
            epBlockStartPenaltyTotal +
            epAimlessBlockPenaltyTotal +
            epBlockedAdvancePenaltyTotal +
            epBlockInsteadParryPenaltyTotal +
            epRepeatSameMovePenaltyTotal +
            epChargeSpamPenaltyTotal +
            epEmptyChargeReleasePenaltyTotal +
            epWinRewardTotal +
            epLossRewardTotal;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            $"[Agent {playerSuffix}] Episode {localEpisodeCounter} END={endReason} | " +
            $"CharID={(characterId.HasValue ? characterId.Value.ToString() : "?")} | " +
            $"LightReach={lightReach} | SpecialReach={specialReach} | ProfileLoaded={profileLoaded}"
        );

        sb.AppendLine($"Total={total:F4}");

        AppendLine(sb, "DamageDealt", epDamageDealtRewardTotal, epDamageDealtCount);
        AppendLine(sb, "DamageTaken", epDamageTakenRewardTotal, epDamageTakenCount);
        AppendLine(sb, "Win", epWinRewardTotal, epWinRewardCount);
        AppendLine(sb, "Loss", epLossRewardTotal, epLossRewardCount);
        AppendLine(sb, "Step", epStepPenaltyTotal, epStepPenaltyCount);
        AppendLine(sb, "Spacing", epSpacingRewardTotal, epSpacingCount);
        AppendLine(sb, "Stack", epStackPenaltyTotal, epStackPenaltyCount);
        AppendLine(sb, "Mash", epMashPenaltyTotal, epMashPenaltyCount);
        AppendLine(sb, "AirJump", epAirJumpPenaltyTotal, epAirJumpPenaltyCount);
        AppendLine(sb, "EdgeCamp", epEdgeCampPenaltyTotal, epEdgeCampPenaltyCount);
        AppendLine(sb, "Approach", epApproachRewardTotal, epApproachRewardCount);
        AppendLine(sb, "ExtremeFarHeavy", epExtremeFarHeavyPenaltyTotal, epExtremeFarHeavyPenaltyCount);
        AppendLine(sb, "ExtremeFarCharge", epExtremeFarChargePenaltyTotal, epExtremeFarChargePenaltyCount);
        AppendLine(sb, "FarMeleeLight", epFarMeleeLightPenaltyTotal, epFarMeleeLightPenaltyCount);
        AppendLine(sb, "FarMeleeSpecial", epFarMeleeSpecialPenaltyTotal, epFarMeleeSpecialPenaltyCount);
        AppendLine(sb, "DashNoDirection", epDashNoDirectionPenaltyTotal, epDashNoDirectionPenaltyCount);
        AppendLine(sb, "WrongFacingSpecial", epWrongFacingSpecialPenaltyTotal, epWrongFacingSpecialPenaltyCount);
        AppendLine(sb, "BlockHold", epBlockHoldPenaltyTotal, epBlockHoldPenaltyCount);
        AppendLine(sb, "BlockStart", epBlockStartPenaltyTotal, epBlockStartPenaltyCount);
        AppendLine(sb, "AimlessBlock", epAimlessBlockPenaltyTotal, epAimlessBlockPenaltyCount);
        AppendLine(sb, "BlockedAdvance", epBlockedAdvancePenaltyTotal, epBlockedAdvancePenaltyCount);
        AppendLine(sb, "BlockInsteadParry", epBlockInsteadParryPenaltyTotal, epBlockInsteadParryPenaltyCount);
        AppendLine(sb, "RepeatSameMove", epRepeatSameMovePenaltyTotal, epRepeatSameMovePenaltyCount);
        AppendLine(sb, "ChargeSpam", epChargeSpamPenaltyTotal, epChargeSpamPenaltyCount);
        AppendLine(sb, "EmptyChargeRelease", epEmptyChargeReleasePenaltyTotal, epEmptyChargeReleasePenaltyCount);

        Debug.Log(sb.ToString());
    }

    private void AppendLine(StringBuilder sb, string label, float total, int count)
    {
        if (count <= 0 && Mathf.Approximately(total, 0f))
        {
            return;
        }

        sb.AppendLine($"  {label}: total={total:F4}, count={count}");
    }

    private void ResetEpisodeRewardDebug()
    {
        epStepPenaltyTotal = 0f;
        epDamageDealtRewardTotal = 0f;
        epDamageTakenRewardTotal = 0f;
        epSpacingRewardTotal = 0f;
        epStackPenaltyTotal = 0f;
        epMashPenaltyTotal = 0f;
        epAirJumpPenaltyTotal = 0f;
        epEdgeCampPenaltyTotal = 0f;
        epApproachRewardTotal = 0f;
        epExtremeFarHeavyPenaltyTotal = 0f;
        epExtremeFarChargePenaltyTotal = 0f;
        epFarMeleeLightPenaltyTotal = 0f;
        epFarMeleeSpecialPenaltyTotal = 0f;
        epDashNoDirectionPenaltyTotal = 0f;
        epWrongFacingSpecialPenaltyTotal = 0f;
        epBlockHoldPenaltyTotal = 0f;
        epBlockStartPenaltyTotal = 0f;
        epAimlessBlockPenaltyTotal = 0f;
        epBlockedAdvancePenaltyTotal = 0f;
        epBlockInsteadParryPenaltyTotal = 0f;
        epRepeatSameMovePenaltyTotal = 0f;
        epChargeSpamPenaltyTotal = 0f;
        epEmptyChargeReleasePenaltyTotal = 0f;
        epWinRewardTotal = 0f;
        epLossRewardTotal = 0f;

        epStepPenaltyCount = 0;
        epDamageDealtCount = 0;
        epDamageTakenCount = 0;
        epSpacingCount = 0;
        epStackPenaltyCount = 0;
        epMashPenaltyCount = 0;
        epAirJumpPenaltyCount = 0;
        epEdgeCampPenaltyCount = 0;
        epApproachRewardCount = 0;
        epExtremeFarHeavyPenaltyCount = 0;
        epExtremeFarChargePenaltyCount = 0;
        epFarMeleeLightPenaltyCount = 0;
        epFarMeleeSpecialPenaltyCount = 0;
        epDashNoDirectionPenaltyCount = 0;
        epWrongFacingSpecialPenaltyCount = 0;
        epBlockHoldPenaltyCount = 0;
        epBlockStartPenaltyCount = 0;
        epAimlessBlockPenaltyCount = 0;
        epBlockedAdvancePenaltyCount = 0;
        epBlockInsteadParryPenaltyCount = 0;
        epRepeatSameMovePenaltyCount = 0;
        epChargeSpamPenaltyCount = 0;
        epEmptyChargeReleasePenaltyCount = 0;
        epWinRewardCount = 0;
        epLossRewardCount = 0;
    }
}