using UnityEngine;

public class SimpleBotController : MonoBehaviour
{
    [Header("Runtime References")]
    [SerializeField] private Character character;
    [SerializeField] private CharacterSetup setup;
    [SerializeField] private Character target;

    [Header("Skill")]
    [Range(0f, 1f)]
    [SerializeField] private float skill = 0.6f;
    [SerializeField] private bool randomizePersonality = true;

    [Header("Movement")]
    [SerializeField] private float preferredRange = 1.25f;
    [SerializeField] private float tooCloseRange = 0.55f;
    [SerializeField] private float moveCommitMin = 0.10f;
    [SerializeField] private float moveCommitMax = 0.24f;
    [SerializeField] private float idleShimmyChance = 0.10f;

    [Header("Timing")]
    [SerializeField] private float thinkIntervalMin = 0.18f;
    [SerializeField] private float thinkIntervalMax = 0.28f;
    [Header("Base Action Chances")]
    [SerializeField] private float quickChance = 0.62f;
    [SerializeField] private float heavyChance = 0.20f;
    [SerializeField] private float specialChance = 0.08f;
    [SerializeField] private float jumpChance = 0.04f;
    [SerializeField] private float blockChance = 0.05f;

    [Header("Level")]
    [SerializeField] private float sameLevelTolerance = 1.5f;
    [SerializeField] private float engageRange = 1.35f;

    [Header("Platform Drop")]
    [SerializeField] private float dropHoldDuration = 0.20f;

    [Header("Anti-Charge Reaction")]
    [SerializeField] private bool antiChargeEnabled = true;
    [SerializeField] private float antiChargeRange = 2.0f;
    [SerializeField] private float antiChargeHeightTolerance = 1.6f;
    [SerializeField] private float antiChargeReactionCooldown = 0.45f;
    [SerializeField] private float antiChargeBaseChance = 0.35f;
    [SerializeField] private float antiChargeSkillBonus = 0.50f;
    [SerializeField] private float antiChargeParryWeight = 0.42f;
    [SerializeField] private float antiChargeJumpWeight = 0.26f;
    [SerializeField] private float antiChargeRetreatWeight = 0.22f;
    [SerializeField] private float antiChargeBlockWeight = 0.10f;
    [SerializeField] private float retreatDuration = 0.22f;

    [SerializeField] private float antiChargeSpecialChance = 0.92f;
    [SerializeField] private float antiChargeSpecialRange = 2f;
    [SerializeField] private float antiChargeSpecialCooldown = 0.30f;

    [SerializeField] private float antiChargeParryDelay = 0.5f;

    [Header("Post Action Reposition")]
    [SerializeField] private float repositionChance = 0.28f;
    [SerializeField] private float repositionMinDuration = 0.08f;
    [SerializeField] private float repositionMaxDuration = 0.18f;

    [Header("Vertical / Head-Stack Avoidance")]
    [SerializeField] private float headStackXThreshold = 0.55f;
    [SerializeField] private float headStackMinY = 0.65f;
    [SerializeField] private float headStackMaxY = 2.2f;
    [SerializeField] private float headStackEscapeMin = 0.18f;
    [SerializeField] private float headStackEscapeMax = 0.32f;
    [SerializeField] private float crossLevelFollowXThreshold = 1.6f;

    [Header("Anti-Charge Danger")]
    [SerializeField] private float antiChargeDangerRange = 1.35f;
    [SerializeField] private float antiChargeDangerHeightTolerance = 1.4f;
    [SerializeField] private float antiChargeEmergencyRetreatTime = 0.30f;
    [SerializeField] private float antiChargeEmergencyJumpChance = 0.75f;
    [SerializeField] private float antiChargeEmergencyParryChance = 0.55f;

    // Personality traits vary behavior without changing the configured skill level.
    private float aggression;
    private float defenseBias;
    private float mobilityBias;
    private float patience;

    private bool trackingEnemyCharge;
    private float enemyChargeTimer;

    private BotInputProvider botInput;
    private float thinkTimer;
    private float actionTimer;
    private float blockHoldTimer;

    private float forcedVerticalTimer;
    private float forcedVerticalValue;
    private bool isDroppingThroughPlatform;

    private float antiChargeReactionTimer;

    private float forcedHorizontalTimer;
    private float forcedHorizontalValue;

    private float moveCommitTimer;
    private float committedMove;

    private bool initialized;

    private Transform selfTr;
    private Transform targetTr;

    // =========================================================
    // INIT
    // =========================================================

    private void Start()
    {
        selfTr = transform;
        TryInitialize();
        RandomizePersonality();
    }

    private void OnEnable()
    {
        thinkTimer = 0f;
        actionTimer = 0f;
        blockHoldTimer = 0f;

        forcedVerticalTimer = 0f;
        forcedVerticalValue = 0f;
        isDroppingThroughPlatform = false;

        antiChargeReactionTimer = 0f;
        forcedHorizontalTimer = 0f;
        forcedHorizontalValue = 0f;

        moveCommitTimer = 0f;
        committedMove = 0f;

        initialized = false;

        trackingEnemyCharge = false;
        enemyChargeTimer = 0f;
    }

    private void OnDisable()
    {
        if (botInput != null)
            botInput.ClearFrameState();
    }

    private void Update()
    {
        if (!initialized)
        {
            TryInitialize();
            return;
        }

        if (character == null || setup == null || target == null)
        {
            target = FindOpponent();
            targetTr = target != null ? target.transform : null;
            if (character == null || setup == null || target == null)
                return;
        }

        botInput.ClearFrameState();

        float dt = Time.deltaTime;

        thinkTimer -= dt;
        actionTimer -= dt;
        blockHoldTimer -= dt;
        antiChargeReactionTimer -= dt;

        UpdateForcedVertical(dt);
        UpdateForcedHorizontal(dt);
        UpdateEnemyChargeTracking(dt);

        if (blockHoldTimer <= 0f)
            botInput.SetKey(setup.block, false);

        // Resolve urgent reactions before selecting a general action.
        if (TryReactToCharge())
        {
            HandleMovement(dt);
            return;
        }

        HandleMovement(dt);

        if (thinkTimer <= 0f)
        {
            thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
            DecideAction();
        }
    }

    private void TryInitialize()
    {
        if (setup == null) setup = GetComponent<CharacterSetup>();
        if (character == null) character = GetComponent<Character>();

        if (setup == null || character == null) return;

        if (selfTr == null)
            selfTr = transform;

        if (botInput == null)
        {
            botInput = new BotInputProvider();
            character.SetInput(botInput);
        }

        if (target == null)
            target = FindOpponent();

        targetTr = target != null ? target.transform : null;

        if (target != null)
            initialized = true;
    }

    // =========================================================
    // PERSONALITY
    // =========================================================

    private void RandomizePersonality()
    {
        if (!randomizePersonality)
        {
            aggression = 0.5f;
            defenseBias = 0.5f;
            mobilityBias = 0.5f;
            patience = 0.5f;
            return;
        }

        aggression = Random.Range(0.25f, 0.85f);
        defenseBias = Random.Range(0.20f, 0.80f);
        mobilityBias = Random.Range(0.20f, 0.80f);
        patience = Random.Range(0.20f, 0.80f);
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement(float dt)
    {
        if (targetTr == null)
        {
            ApplyMovement(0f);
            return;
        }

        Vector3 myPos = selfTr.position;
        Vector3 targetPos = targetTr.position;

        float dxSigned = targetPos.x - myPos.x;
        float dySigned = targetPos.y - myPos.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= sameLevelTolerance;
        bool targetBelow = dySigned < -sameLevelTolerance;
        bool inEngageRange = dx <= engageRange && sameLevel;

        bool headStacking = IsHeadStackingTarget(dx, dySigned);

        if (headStacking)
        {
            float escapeDir = -Mathf.Sign(dxSigned);

            if (Mathf.Abs(escapeDir) < 0.01f)
                escapeDir = Random.value < 0.5f ? -1f : 1f;

            forcedHorizontalValue = escapeDir;
            forcedHorizontalTimer = Random.Range(headStackEscapeMin, headStackEscapeMax);
            moveCommitTimer = 0f;
            committedMove = 0f;

            ApplyMovement(forcedHorizontalValue);
            return;
        }

        // forced retreat / burst movement
        if (forcedHorizontalTimer > 0f)
        {
            ApplyMovement(forcedHorizontalValue);
            return;
        }

        if (moveCommitTimer > 0f)
        {
            moveCommitTimer -= dt;
            ApplyMovement(committedMove);
            return;
        }

        float move = 0f;

        // Anti-top-camp
        if (targetBelow && dx < 0.6f && !isDroppingThroughPlatform && !headStacking)
        {
            bool enemyChargingOrReady =
                target != null &&
                (target.IsCharging || target.IsCharged);

            if (!enemyChargingOrReady)
            {
                float dropChance = Mathf.Lerp(0.4f, 0.75f, skill);

                if (Random.value < dropChance)
                {
                    TriggerDropPlatform();
                }
                else
                {
                    move = Random.value < 0.5f ? -1f : 1f;
                }
            }
            else
            {
                // Move away horizontally instead of dropping into a charged attack.
                move = -Mathf.Sign(dxSigned);

                if (Mathf.Abs(move) < 0.01f)
                    move = Random.value < 0.5f ? -1f : 1f;
            }
        }
        else
        {
            float desiredRange =
                preferredRange
                + Mathf.Lerp(0.22f, -0.18f, aggression)
                + Mathf.Lerp(-0.05f, 0.18f, defenseBias);

            float retreatRange =
                tooCloseRange
                + Mathf.Lerp(-0.04f, 0.08f, defenseBias);

            desiredRange = Mathf.Max(retreatRange + 0.08f, desiredRange);

            if (!sameLevel)
            {
                // On separate platforms, only chase substantial horizontal separation.
                if (dx > crossLevelFollowXThreshold)
                {
                    move = Mathf.Sign(dxSigned);
                }
                else
                {
                    move = 0f;
                }
            }
            else if (!inEngageRange)
            {
                if (dx > desiredRange)
                    move = Mathf.Sign(dxSigned);
                else if (dx < retreatRange)
                    move = -Mathf.Sign(dxSigned);
            }
            else if (!inEngageRange)
            {
                if (dx > desiredRange)
                    move = Mathf.Sign(dxSigned);
                else if (dx < retreatRange)
                    move = -Mathf.Sign(dxSigned);
            }
            else
            {
                // Add small neutral-position adjustments.
                float shimmyChance = idleShimmyChance * Mathf.Lerp(0.7f, 1.5f, mobilityBias);

                if (Random.value < shimmyChance)
                {
                    if (dx < retreatRange + 0.05f)
                        move = -Mathf.Sign(dxSigned);
                    else
                        move = Random.value < 0.55f ? Mathf.Sign(dxSigned) : -Mathf.Sign(dxSigned);
                }
            }
        }

        committedMove = Mathf.Clamp(move, -1f, 1f);
        moveCommitTimer = Random.Range(moveCommitMin, moveCommitMax);

        ApplyMovement(committedMove);
    }

    private void ApplyMovement(float move)
    {
        string suffix = GetAxisSuffix();

        botInput.SetAxis("Horizontal" + suffix, move);
        botInput.SetAxis("Horizontal" + setup.playerNum, move);
        botInput.SetAxis("Horizontal", move);

        float vertical = forcedVerticalTimer > 0f ? forcedVerticalValue : 0f;

        botInput.SetAxis("Vertical" + suffix, vertical);
        botInput.SetAxis("Vertical" + setup.playerNum, vertical);
        botInput.SetAxis("Vertical", vertical);
    }

    // =========================================================
    // ACTIONS
    // =========================================================

    private void DecideAction()
    {
        if (actionTimer > 0f || isDroppingThroughPlatform || targetTr == null)
            return;

        Vector3 myPos = selfTr.position;
        Vector3 targetPos = targetTr.position;

        float dxSigned = targetPos.x - myPos.x;
        float dySigned = targetPos.y - myPos.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= sameLevelTolerance;
        bool inEngageRange = dx <= engageRange && sameLevel;
        bool inCloseRange = dx <= tooCloseRange && sameLevel;
        bool inPokeRange = dx <= engageRange * 1.20f && sameLevel;

        // Add a short reaction delay for less mechanical timing.
        float hesitateChance = Mathf.Lerp(0.03f, 0.18f, patience);
        if (Random.value < hesitateChance)
        {
            actionTimer = Random.Range(0.06f, 0.14f);
            return;
        }

        // Defensive reaction in close combat
        float defendChance =
            Mathf.Lerp(blockChance, blockChance + 0.28f, defenseBias) *
            Mathf.Lerp(0.75f, 1.15f, skill);

        if (inEngageRange && Random.value < defendChance)
        {
            botInput.SetKey(setup.block, true);
            blockHoldTimer = Random.Range(0.10f, 0.24f);
            actionTimer = Random.Range(0.16f, 0.28f);
            MaybeRepositionAfterAction(dxSigned, false);
            return;
        }

        // FAR
        if (!inPokeRange)
        {
            float jumpInChance = jumpChance * Mathf.Lerp(0.7f, 2.0f, mobilityBias);

            if (character.IsGrounded && !character.JumpDisabled && Random.value < jumpInChance)
            {
                PressOneFrame(setup.up);
                actionTimer = Random.Range(0.22f, 0.38f);
                MaybeRepositionAfterAction(dxSigned, true);
            }

            return;
        }

        // CLOSE SCRAMBLE
        if (inCloseRange)
        {
            float quickW = quickChance * Mathf.Lerp(0.85f, 1.25f, aggression);
            float retreatW = Mathf.Lerp(0.06f, 0.26f, defenseBias);
            float specialW = CanUseSpecialNow() ? specialChance * Mathf.Lerp(0.5f, 1.0f, aggression) : 0f;
            float waitW = Mathf.Lerp(0.04f, 0.18f, patience);

            float total = quickW + retreatW + specialW + waitW;
            float roll = Random.value * total;

            if (roll < quickW)
            {
                PressOneFrame(setup.lightAttack);
                actionTimer = Random.Range(0.18f, 0.32f);
                MaybeRepositionAfterAction(dxSigned, true);
                return;
            }
            roll -= quickW;

            if (roll < retreatW)
            {
                ForceRetreatFromTarget(dxSigned, 0.14f);
                actionTimer = 0.14f;
                return;
            }
            roll -= retreatW;

            if (roll < specialW)
            {
                PressOneFrame(setup.ability);
                actionTimer = Random.Range(0.28f, 0.50f);
                MaybeRepositionAfterAction(dxSigned, true);
                return;
            }

            actionTimer = Random.Range(0.08f, 0.16f);
            return;
        }

        // NORMAL ENGAGE / POKE RANGE
        {
            float quickW =
                quickChance * Mathf.Lerp(0.80f, 1.20f, aggression);

            float heavyW =
                heavyChance * Mathf.Lerp(0.70f, 1.15f, aggression) * Mathf.Lerp(0.85f, 1.15f, patience);

            float specialW =
                CanUseSpecialNow()
                ? specialChance * Mathf.Lerp(0.75f, 1.35f, aggression)
                : 0f;

            float jumpW =
                jumpChance * Mathf.Lerp(0.60f, 1.80f, mobilityBias);

            float blockW =
                blockChance * Mathf.Lerp(0.80f, 1.80f, defenseBias);

            float waitW =
                Mathf.Lerp(0.05f, 0.22f, patience);

            float total = quickW + heavyW + specialW + jumpW + blockW + waitW;
            float roll = Random.value * total;

            if (roll < quickW)
            {
                PressOneFrame(setup.lightAttack);
                actionTimer = Random.Range(0.20f, 0.36f);
                MaybeRepositionAfterAction(dxSigned, true);
                return;
            }
            roll -= quickW;

            if (roll < heavyW)
            {
                PressOneFrame(setup.heavyAttack);
                actionTimer = Random.Range(0.28f, 0.46f);
                MaybeRepositionAfterAction(dxSigned, true);
                return;
            }
            roll -= heavyW;

            if (roll < specialW)
            {
                PressOneFrame(setup.ability);
                actionTimer = Random.Range(0.32f, 0.56f);
                MaybeRepositionAfterAction(dxSigned, true);
                return;
            }
            roll -= specialW;

            if (roll < jumpW)
            {
                if (character.IsGrounded && !character.JumpDisabled)
                {
                    PressOneFrame(setup.up);
                    actionTimer = Random.Range(0.20f, 0.34f);
                    MaybeRepositionAfterAction(dxSigned, true);
                    return;
                }
            }
            roll -= jumpW;

            if (roll < blockW)
            {
                botInput.SetKey(setup.block, true);
                blockHoldTimer = Random.Range(0.10f, 0.22f);
                actionTimer = Random.Range(0.15f, 0.26f);
                MaybeRepositionAfterAction(dxSigned, false);
                return;
            }

            actionTimer = Random.Range(0.08f, 0.18f);
        }
    }

    // =========================================================
    // ANTI-CHARGE
    // =========================================================

    private bool TryReactToCharge()
    {
        if (!antiChargeEnabled) return false;
        if (antiChargeReactionTimer > 0f) return false;
        if (character == null || target == null || targetTr == null) return false;

        bool emergencyChargeThreat = target.IsCharging || target.IsCharged;

        if (actionTimer > 0f && !emergencyChargeThreat)
            return false;

        if (character.IsCasting || character.IsStunned || character.IsKnocked)
            return false;

        Vector3 myPos = selfTr.position;
        Vector3 targetPos = targetTr.position;

        float dxSigned = targetPos.x - myPos.x;
        float dySigned = targetPos.y - myPos.y;

        float dx = Mathf.Abs(dxSigned);
        float dy = Mathf.Abs(dySigned);

        bool sameLevel = dy <= antiChargeHeightTolerance;
        bool closeEnough = dx <= antiChargeRange;
        bool opponentCharging = target.IsCharging;

        if (!opponentCharging || !closeEnough || !sameLevel)
            return false;

        bool chargingTowardsMe = IsTargetChargingTowardsMe(dxSigned);

        bool inDangerZone =
            dx <= antiChargeDangerRange &&
            dy <= antiChargeDangerHeightTolerance &&
            chargingTowardsMe;

        if (inDangerZone)
        {
            // Interrupt the charge with a special when possible.
            if (dx <= antiChargeSpecialRange && CanUseSpecialNow())
            {
                PressOneFrame(setup.ability);
                antiChargeReactionTimer = antiChargeSpecialCooldown;
                actionTimer = 0.16f;
                return true;
            }

            // Prefer jumping away when grounded.
            if (character.IsGrounded && !character.JumpDisabled && !character.IsCasting)
            {
                if (trackingEnemyCharge && enemyChargeTimer >= antiChargeParryDelay &&
                    character.CanParry && Random.value < antiChargeEmergencyParryChance)
                {
                    PressOneFrame(setup.parry);
                    antiChargeReactionTimer = antiChargeReactionCooldown;
                    actionTimer = 0.18f;
                    return true;
                }

                if (Random.value < antiChargeEmergencyJumpChance)
                {
                    PressOneFrame(setup.up);
                    ForceRetreatFromTarget(dxSigned, antiChargeEmergencyRetreatTime);
                    antiChargeReactionTimer = antiChargeReactionCooldown * 0.75f;
                    actionTimer = 0.18f;
                    return true;
                }
            }

            // 3. fallback = hard retreat
            ForceRetreatFromTarget(dxSigned, antiChargeEmergencyRetreatTime);
            antiChargeReactionTimer = antiChargeReactionCooldown * 0.75f;
            actionTimer = 0.16f;
            return true;
        }

        // Prioritize a special when an incoming charge is within range.
        if (chargingTowardsMe &&
            dx <= antiChargeSpecialRange &&
            CanUseSpecialNow())
        {
            if (Random.value < antiChargeSpecialChance)
            {
                PressOneFrame(setup.ability);
                antiChargeReactionTimer = antiChargeSpecialCooldown;
                actionTimer = 0.20f;
                return true;
            }
        }

        float reactChance = antiChargeBaseChance + antiChargeSkillBonus * skill;

        if (target.IsCharged)
            reactChance += 0.15f;

        reactChance = Mathf.Clamp01(reactChance);

        if (Random.value > reactChance)
            return false;

        antiChargeReactionTimer = antiChargeReactionCooldown;

        float totalWeight =
            antiChargeParryWeight +
            antiChargeJumpWeight +
            antiChargeRetreatWeight +
            antiChargeBlockWeight;

        float roll = Random.value * totalWeight;

        // 1) PARRY
        if (roll < antiChargeParryWeight)
        {
            bool parryTimingReady = trackingEnemyCharge && enemyChargeTimer >= antiChargeParryDelay;

            if (parryTimingReady && character.CanParry && !character.IsCasting)
            {
                PressOneFrame(setup.parry);
                actionTimer = 0.22f;
                return true;
            }
        }
        else
        {
            roll -= antiChargeParryWeight;
        }

        // 2) JUMP
        if (roll < antiChargeJumpWeight)
        {
            if (character.IsGrounded && !character.JumpDisabled && !character.IsCasting)
            {
                PressOneFrame(setup.up);
                ForceRetreatFromTarget(dxSigned, retreatDuration * 0.75f);
                actionTimer = 0.25f;
                return true;
            }
        }
        else
        {
            roll -= antiChargeJumpWeight;
        }

        // 3) RETREAT
        if (roll < antiChargeRetreatWeight)
        {
            ForceRetreatFromTarget(dxSigned, retreatDuration);
            actionTimer = 0.18f;
            return true;
        }
        else
        {
            roll -= antiChargeRetreatWeight;
        }

        // 4) BLOCK fallback
        botInput.SetKey(setup.block, true);
        blockHoldTimer = Random.Range(0.18f, 0.32f);
        actionTimer = 0.22f;
        return true;
    }

    private void ForceRetreatFromTarget(float dxSigned, float duration)
    {
        forcedHorizontalValue = -Mathf.Sign(dxSigned);

        if (Mathf.Abs(forcedHorizontalValue) < 0.01f)
            forcedHorizontalValue = Random.value < 0.5f ? -1f : 1f;

        forcedHorizontalTimer = duration;
        moveCommitTimer = 0f;
        committedMove = 0f;
    }

    private void MaybeRepositionAfterAction(float dxSigned, bool allowToward)
    {
        float chance = repositionChance * Mathf.Lerp(0.7f, 1.5f, mobilityBias);

        if (Random.value > chance)
            return;

        float dir;

        if (allowToward && Random.value < 0.38f)
            dir = Mathf.Sign(dxSigned);   // Re-engage at close range.
        else
            dir = -Mathf.Sign(dxSigned);  // Create space at longer range.

        if (Mathf.Abs(dir) < 0.01f)
            dir = Random.value < 0.5f ? -1f : 1f;

        forcedHorizontalValue = dir;
        forcedHorizontalTimer = Random.Range(repositionMinDuration, repositionMaxDuration);
        moveCommitTimer = 0f;
    }

    private void UpdateForcedHorizontal(float dt)
    {
        if (forcedHorizontalTimer > 0f)
        {
            forcedHorizontalTimer -= dt;
            if (forcedHorizontalTimer <= 0f)
            {
                forcedHorizontalTimer = 0f;
                forcedHorizontalValue = 0f;
            }
        }
    }

    // =========================================================
    // PLATFORM DROP
    // =========================================================

    private void TriggerDropPlatform()
    {
        isDroppingThroughPlatform = true;

        PressOneFrame(setup.down);

        forcedVerticalValue = -1f;
        forcedVerticalTimer = dropHoldDuration;
    }

    private void UpdateForcedVertical(float dt)
    {
        if (forcedVerticalTimer > 0f)
        {
            forcedVerticalTimer -= dt;

            if (forcedVerticalTimer <= 0f)
            {
                forcedVerticalValue = 0f;
                isDroppingThroughPlatform = false;
            }
        }
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private string GetAxisSuffix()
    {
        return setup.playerNum == 1 ? "_P1" : "_P2";
    }

    private void PressOneFrame(KeyCode key)
    {
        botInput.PressKeyOneFrame(key);
    }

    private Character FindOpponent()
    {
        Character[] all = FindObjectsOfType<Character>();

        foreach (Character c in all)
        {
            if (c != null && c != character)
                return c;
        }

        return null;
    }

    public void SetTarget(Character newTarget)
    {
        target = newTarget;
        targetTr = target != null ? target.transform : null;
    }

    public void SetSkill(float newSkill)
    {
        skill = Mathf.Clamp01(newSkill);
    }

    public void Rebind(Character self, Character enemy)
    {
        character = self;
        target = enemy;
        targetTr = target != null ? target.transform : null;
        setup = GetComponent<CharacterSetup>();

        if (botInput == null)
            botInput = new BotInputProvider();

        if (character != null)
            character.SetInput(botInput);

        selfTr = transform;
        initialized = (character != null && setup != null && target != null);
    }

    private bool IsTargetChargingTowardsMe(float dxSigned)
    {
        if (target == null) return false;

        float targetFacing = Mathf.Sign(target.transform.localScale.x);
        float dirToMe = -Mathf.Sign(dxSigned);

        if (Mathf.Abs(dirToMe) < 0.01f)
            return true;

        return targetFacing == dirToMe;
    }

    private bool CanUseSpecialNow()
    {
        if (character == null) return false;

        return
            !character.IsCasting &&
            !character.IsStunned &&
            !character.IsKnocked &&
            character.CanCast &&
            !character.OnAbilityCD &&
            !character.SpecialDisabled;
    }

    private void UpdateEnemyChargeTracking(float dt)
    {
        if (target == null)
        {
            trackingEnemyCharge = false;
            enemyChargeTimer = 0f;
            return;
        }

        if (target.IsCharging)
        {
            if (!trackingEnemyCharge)
            {
                trackingEnemyCharge = true;
                enemyChargeTimer = 0f;
            }
            else
            {
                enemyChargeTimer += dt;
            }
        }
        else
        {
            trackingEnemyCharge = false;
            enemyChargeTimer = 0f;
        }
    }

    private bool IsHeadStackingTarget(float dx, float dySigned)
    {
        float absDy = Mathf.Abs(dySigned);

        bool iAmAboveTarget = dySigned < 0f;
        bool closeInX = dx <= headStackXThreshold;
        bool badYBand = absDy >= headStackMinY && absDy <= headStackMaxY;

        return iAmAboveTarget && closeInX && badYBand;
    }
}
