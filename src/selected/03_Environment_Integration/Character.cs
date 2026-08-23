using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Playables;
//using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;
using System.Text;

public enum MoveType
{
    Quick,
    Heavy,
    Special,
    Charge,
    Projectile,
    ParryCounter,
    PoisonTick
}
public enum SourceType
{
    Melee,
    Spell,
    Projectile,
    Dot,
    Parry
}
public abstract class Character : MonoBehaviour
{


    public string PlayerId => playerNum == 1 ? "P1" : "P2";
    public bool CanDropPlatform => isonpad > 0;
    protected string incomingAttackerId;
    protected MoveType incomingMoveType;
    protected SourceType incomingSourceType;
    // --- Telemetry: movement logging (edge-triggered) ---
    private int lastMoveDir = 0; // -1 = left, 0 = idle, +1 = right
    [SerializeField] private float moveLogCooldown = 0.20f; // seconds, anti-spam
    private float nextMoveLogTime = 0f;
    protected Character enemy;
    protected Animator animator;
    protected AudioManager audioManager;
    protected Rigidbody2D rb;
    protected CharacterResources resources;
    

    //Charge
    protected bool charged = false;
    protected bool charging = false;
    private Coroutine chargeCoroutine;
    float chargeTime = 0.5f;
    protected int chargeDmg = 31;

    //Flags
    public bool isBlocking = false;
    public bool ignoreDamage = false;
  
    public bool isStatic = false;
    public bool casting = false;
    public bool canCast = true;
    public bool knocked = false;
    public bool canRotate = true;
    public bool usingAbility;
    public int currHealth;
    public bool isGrounded;
    protected bool isRolling = false;
    public bool IsRolling => isRolling;

    //knockback

    protected float KBForce;
    protected float KBCounter;
    protected float KBTotalTime;
    protected bool knockfromright;
    protected bool knockbackXaxis;
    public bool knockable = true;

    //parry
    public bool canParry = true;
    protected bool safety = true;
    protected bool ignoreCounterOff = false;
    protected int parryDamage = 16;
    public bool counterIsOn = false;
    protected bool counterDone = false;

    //cd
    public bool onCooldown = false;
    public float cdTimer = 0f;

    public bool ignoreUpdate = false;

    // Player-specific UI references.
    protected TextMeshProUGUI P1Name, winner;
    protected string P2Name;
    string playerEnemysString;
    protected GameObject playAgainButton;
    protected GameObject mainMenuButton;
    protected GameObject saveReplayButton;
    protected Slider cooldownSlider;
    protected TextMeshProUGUI damageCounter;
    protected HealthBar healthbar;

    // Core combat configuration.
    public int characterID = -1;
    public float moveSpeed = 4f;
    protected float heavySpeed;
    protected float OGMoveSpeed;
    protected float jumpForce = 10f;
    public int maxHealth = 100;
    protected int heavyDamage = 14;
    protected int playerNum;
    protected float attackRange = 0.5f;
    protected float ogRange = 0.5f;

    // Stage state.
    string stageName;
    protected GameObject[] stages;

    private bool jumpAxisHeld;

    protected Transform attackPoint;



    protected LayerMask enemyLayer;

    protected bool ignoreMovement = false;
    protected bool ignoreSlow = false;
    public bool blockDisabled;
    public bool canAlterSpeed = true;

    // Cooldown visuals.
    protected Image cdbarimage;
    protected Sprite activeSprite, ogSprite;

    // Status indicators.
    protected GameObject blockDisabledIndicator;
    protected GameObject poison;
    protected GameObject Stack1Poison;
    protected GameObject Stack2Poison;
    protected GameObject Stack3Poison;
    protected GameObject quickAttackIndicator;
    protected GameObject stun;
    protected GameObject shield;

    protected TextMeshPro robberyCountIndicator;
    protected bool stunned = false;

    // Input bindings.
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;
    public KeyCode lightAttack;
    public KeyCode heavyAttack;
    public KeyCode block;
    public KeyCode ability;
    public KeyCode charge;
    public KeyCode parry;

    protected CharacterSetup characterSetup;
    protected CharacterManager characterChoiceHandler;
    protected GameManager gameManager;


    protected bool damageShield = false;

    protected AudioClip blockSound;
    protected AudioClip characterJump;
    protected AudioClip chargeHitSound;
    protected AudioClip winQuip;


    public string playerString;

    protected bool quickDisable = false;
    protected bool heavyDisable = false;
    protected bool blockDisable = false;
    protected bool specialDisable = false;
    public bool chargeDisable = false;
    bool ignoreStats = false;


    public bool overrideDeath = false;

    //handling variables
    public int grounds = 0;
    int isonpad = 0;

    int controllerCount = 0;

    protected bool controller = false;
    protected bool chargeReset = false;
    protected bool chargeAttackActive = false;

    protected bool jumpDisabled = false;

    public bool chanChan;

    //teleport
    public bool justTeleported = false;

    public Transform spawn;

    protected float originalGravityScale=1.8f;

    private bool debugControllers = false;

    // --- Episode spawn ---
    private Vector3 _spawnPos;
    public void SetSpawnPosition(Vector3 pos) => _spawnPos = pos;

    //helpers
    public bool isLightAttacking=false;
    public bool heavyAttacking=false;

    //new grounded logic experiement
    private Transform groundCheck;
    private float groundCheckRadius = 0.15f;
    private LayerMask solidGroundLayers;
    private LayerMask platformLayers;
    private LayerMask playerGroundLayers;

    protected Collider2D feetTrigger;

    private Vector2 groundCheckSize = new Vector2(0.8f, 0.3f);

    private static int spawnIndexP1 = -1;
    private static int spawnIndexP2 = -1;

    Coroutine flashRedCoroutine;

    Coroutine cdCoroutine;

    

    public void SetIncomingDamageContext(string attackerId, MoveType moveType, SourceType sourceType)
    {
        incomingAttackerId = attackerId;
        incomingMoveType = moveType;
        incomingSourceType = sourceType;
    }
    #region Base
    public virtual void Start()
    {
        characterSetup = GetComponent<CharacterSetup>();
        characterChoiceHandler = GetComponent<CharacterManager>();

        string[] connectedControllers = Input.GetJoystickNames();

        // Count how many controllers are connected (non-empty entries in the array)
        foreach (string controller in connectedControllers)
        {
            if (!string.IsNullOrEmpty(controller))
            {
                controllerCount++;
            }
        }

        InitializeCharacter();

        string json = PlayerPrefs.GetString("SelectedRuleset", null);

        if (!string.IsNullOrEmpty(json))
        {
            // Convert the JSON string back to a CustomRuleset object
            CustomRuleset loadedRuleset = JsonUtility.FromJson<CustomRuleset>(json);

            chanChan = loadedRuleset.chanChan;

            if (chanChan)
            {
                StartCoroutine(WaitForMaxHealth());
            }
            else
            {
                maxHealth = loadedRuleset.health;
                currHealth = maxHealth;
                healthbar.SetMaxHealth(maxHealth);
            }


            moveSpeed = loadedRuleset.playerSpeed;

            quickDisable = loadedRuleset.quickDisabled;
            heavyDisable = loadedRuleset.heavyDisabled;
            blockDisable = loadedRuleset.blockDisabled;
            specialDisable = loadedRuleset.specialDisabled;
            chargeDisable = loadedRuleset.chargeDisabled;

            if (loadedRuleset.hideHealth)
            {
                healthbar.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("No ruleset found in PlayerPrefs.");
        }

        //basic variables assignment
        rb = GetComponent<Rigidbody2D>();
        resources = GetComponent<CharacterResources>();

        winner.gameObject.SetActive(false);

        playAgainButton.SetActive(false);
        mainMenuButton.SetActive(false);
        saveReplayButton.SetActive(false);

        OGMoveSpeed = moveSpeed;
        heavySpeed = moveSpeed / 2;

        cooldownSlider.maxValue = 1f;

        _spawnPos = transform.position;

        originalGravityScale = rb.gravityScale;

        Collider2D[] colliders = GetComponents<Collider2D>();

        feetTrigger = colliders[0];

        solidGroundLayers = LayerMask.GetMask("Ground");
        platformLayers    = LayerMask.GetMask("PlatformLayer");

        //Disable Indicators
        shield.gameObject.SetActive(false);
        poison.gameObject.SetActive(false);
        Stack1Poison.gameObject.SetActive(false);
        Stack2Poison.gameObject.SetActive(false);
        Stack3Poison.gameObject.SetActive(false);
        stun.gameObject.SetActive(false);
        blockDisabledIndicator.gameObject.SetActive(false);
        robberyCountIndicator.gameObject.SetActive(false);

    }


    public void InitializeCharacter()
    {
        up = characterSetup.up;
        down = characterSetup.down;
        left = characterSetup.left;
        right = characterSetup.right;
        lightAttack = characterSetup.lightAttack;
        heavyAttack = characterSetup.heavyAttack;
        block = characterSetup.block;
        ability = characterSetup.ability;
        charge = characterSetup.charge;
        parry = characterSetup.parry;
        attackPoint = characterSetup.attackPoint;
        blockDisabledIndicator = characterSetup.blockDisabledIndicator;
        poison = characterSetup.poison;
        Stack1Poison = characterSetup.Stack1Poison;
        Stack2Poison = characterSetup.Stack2Poison;
        Stack3Poison = characterSetup.Stack3Poison;
        robberyCountIndicator = characterSetup.robberyCountIndicator;
        stun = characterSetup.stun;
        enemyLayer = characterSetup.enemyLayer;
        shield = characterSetup.shield;
        gameManager = characterSetup.gameManager;
        cdbarimage = characterSetup.cdbarimage;
        activeSprite = characterSetup.activeSprite;
        ogSprite = characterSetup.ogSprite;
        playerNum = characterSetup.playerNum;
        healthbar = characterSetup.healthbar;
        P1Name = characterSetup.P1Name;
        winner = characterSetup.winner;
        playAgainButton = characterSetup.playAgainButton;
        mainMenuButton = characterSetup.mainMenuButton;
        saveReplayButton = characterSetup.saveReplayButton;
        cooldownSlider = characterSetup.cooldownSlider;
        damageCounter = characterSetup.damageCounter;
        audioManager = characterSetup.audioManager;
        quickAttackIndicator = characterSetup.quickAttackIndicator;
        groundCheck = characterSetup.groundCheck;

        playerGroundLayers = enemyLayer;
        P1Name.text = characterChoiceHandler.GetCharacterName(1);
        P2Name = characterChoiceHandler.GetCharacterName(2);
        enemy = characterChoiceHandler.CharacterChoice(2);


        if (playerNum == 1)
        {
            playerString = "_P1";
            if (controllerCount >= 2)
            {
                controller = true;
            }
        }
        else if (playerNum == 2)
        {
            playerString = "_P2";
            if (controllerCount >= 1)
            {
                controller = true;
            }

        }

        animator = GetComponent<Animator>();

        if (gameManager != null && gameManager.trainingMode)
        {
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
    }

    // inside Character (fields)
    protected IInputProvider input = new KeyboardInputProvider();

    // optional: allow swapping to AI later
    public void SetInput(IInputProvider provider)
    {
        input = provider ?? new KeyboardInputProvider();
    }

    // inside Character
    public IInputProvider GetInputProvider() => input;

    int ControllerNum(int pNum)
    {
        if (pNum == 1)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    public virtual void Update()
    {
        GroundedSafeguard();
        StaticSafeguard();
        //UpdateGroundedState(); in the future
        //self knockback mechanic
        if (knockable)
        {
            if (KBCounter > 0)
            {
                if (knockfromright == true)
                {
                    if (!knockbackXaxis)
                    {
                        rb.velocity = new Vector2(-KBForce, KBForce);
                    }
                    else
                    {
                        rb.velocity = new Vector2(-KBForce, rb.velocity.y);
                    }
                }
                else
                {
                    if (!knockbackXaxis)
                    {
                        rb.velocity = new Vector2(KBForce, KBForce);
                    }
                    else
                    {
                        rb.velocity = new Vector2(KBForce, rb.velocity.y);
                    }
                }

                KBCounter -= Time.deltaTime;
                return;
            }
        }
        //animator.SetBool("knocked", false);  oldKnocked*

        if (ignoreUpdate)
        {
            return;
        }

        if (stunned)
        {
            animator.SetBool("IsRunning", false);
            rb.velocity = new Vector2(0, rb.velocity.y);

            animator.SetBool("cWalk", false);
            isBlocking = false;
            animator.SetTrigger("tookDmg");
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
            {
                // If not, reset the trigger so it doesn't stay set
                animator.ResetTrigger("tookDmg");
            }
            return;
        }

        if (!canRotate)
        {
            return;
        }
        //charge attack specifics
        if (ChargeCheck(charge))
        {
            return;
        }

        if (isStatic)
        {
            return;
        }

        float moveDirection = input.GetAxis("Horizontal" + playerString);
        int dir = (moveDirection > 0.1f) ? 1 : (moveDirection < -0.1f) ? -1 : 0;

        
        if (!ignoreMovement && !knocked && !isStatic && !stunned && !ignoreUpdate)
        {
            LogMoveIfChanged(dir);
        }
        // Running animations...
        if (Mathf.Abs(moveDirection) > 0.1f && !isStatic)
        {
            if (ignoreMovement || knocked)
            {
                animator.SetBool("IsRunning", false);
                animator.SetBool("cWalk", false);
                return;
            }

            if (isBlocking)
            {
                animator.SetBool("IsRunning", false);
                animator.SetBool("cWalk", isGrounded);
            }
            else if (isGrounded)
            {
                animator.SetBool("IsRunning", true);
                animator.SetBool("cWalk", false);
            }
            else
            {
                animator.SetBool("IsRunning", false);
                animator.SetBool("cWalk", false);
                animator.SetTrigger("Jump");
            }


            if (isBlocking)
            {
                animator.SetBool("cWalk", true);
                rb.velocity = new Vector2(moveDirection * heavySpeed, rb.velocity.y);
                transform.localScale = new Vector3(Mathf.Sign(moveDirection), 1, 1); // Flip sprite according to movement direction

            }
            else
            {
                rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
                transform.localScale = new Vector3(Mathf.Sign(moveDirection), 1, 1); // Flip sprite according to movement direction
            }
        }
        else
        {
            animator.SetBool("IsRunning", false);
            rb.velocity = new Vector2(0, rb.velocity.y);

            animator.SetBool("cWalk", false);
        }

        float v = input.GetAxis("Vertical" + playerString);
        bool axisUp = v > 0.5f;

        // Jumping
        if (input.GetKeyDown(up) || (axisUp && !jumpAxisHeld))
        {
            if (isGrounded && !jumpDisabled && !casting)
            {
                Jump();
            }
        }
        jumpAxisHeld = axisUp;

        // Heavy Punching
        if (input.GetKeyDown(heavyAttack) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 2")))
        {
            if (!heavyDisable && !casting)
            {
                Unblock();
                heavyAttacking=true;
                HeavyAttack();
            }
        }

        //Blocking
        if (input.GetKeyDown(block) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 5")))
        {
            if (!blockDisable && !casting)
            {
                Block();
            }
        }
        else if (input.GetKeyUp(block) || (controller && Input.GetKeyUp("joystick "+ControllerNum(playerNum)+" button 5")))
        {
            if (!blockDisable && !casting)
            {
                Unblock();
            }
        }

        //ChargeAttack
        if (input.GetKeyDown(charge) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 1")))
        {
            if (isGrounded && !chargeDisable && !casting && !charging)
            {
                Unblock();
                ChargeAttack();
            }

        }

        //Get down from pad
        if (input.GetKeyDown(down) || (controller && input.GetAxis("Vertical" + playerString) < -0.5f))
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            if (CanDropPlatform)
                TelemetryManager.Instance?.LogAction(PlayerId, "DropPlatform");
            colliders[3].enabled = false;
        }

        //LightAttack
        if (input.GetKeyDown(lightAttack) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 0")))
        {
            if (!quickDisable && !casting)
            {
                Unblock();
                LightAttack();
                StartCoroutine(ResetLightAttackIndicator());
            }
        }

        //Spells
        if (input.GetKeyDown(ability) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 3")))
        {
            if (!onCooldown && canCast && !casting && !specialDisable)
            {
                Unblock();
                Spell();
            }
        }

        //Parry
        if (input.GetKeyDown(parry) || (controller && Input.GetKeyDown("joystick "+ControllerNum(playerNum)+" button 4")))
        {
            if (canParry && canCast && !casting)
            {
                Unblock();
                Parry();
            }
        }

        // Animation control for jumping, falling, and landing
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);

    }

    IEnumerator ResetLightAttackIndicator()
    {
        yield return null;
        isLightAttacking=false;
    }

    private void LogMoveIfChanged(int dir)
    {
        if (TelemetryManager.Instance == null) return;

        // log only on changes: idle->move, move->idle, left<->right
        if (dir == lastMoveDir) return;

        // extra safety: anti-spam cooldown
        if (Time.time < nextMoveLogTime) return;
        nextMoveLogTime = Time.time + moveLogCooldown;

        if (dir == -1)
            TelemetryManager.Instance.LogAction(PlayerId, "MoveLeft");
        else if (dir == 1)
            TelemetryManager.Instance.LogAction(PlayerId, "MoveRight");
        else
            TelemetryManager.Instance.LogAction(PlayerId, "MoveStop");

        lastMoveDir = dir;
    }


    IEnumerator WaitForMaxHealth()
    {
        // Wait until gameManager.maxHealth is no longer -1
        yield return new WaitUntil(() => gameManager.maxHealth != -1);

        // Now it's safe to assign the value
        maxHealth = gameManager.maxHealth;
        currHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
    }


    #endregion

    #region Colliders
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
            grounds++;
        }

        if (other.CompareTag("Platform"))
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
            isonpad++;
            grounds++;
            Collider2D[] colliders = GetComponents<Collider2D>();

            colliders[3].enabled = true;
        }

        if (other.CompareTag("Player"))  //--here
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
            grounds++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {

        if (other.CompareTag("Ground"))
        {
            grounds--;

            if (grounds <= 0)
            {
                grounds=0;
                isGrounded = false;
            }
        }

        if (other.CompareTag("Platform"))
        {
            isonpad--;
            grounds--;

            if (isonpad == 0)
            {
                isGrounded = false;
                Collider2D[] colliders = GetComponents<Collider2D>();

                colliders[3].enabled = false;
            }
        }

        if (other.CompareTag("Player"))  //--here
        {
            grounds--;

            if (grounds <= 0)
            {
                grounds=0;
                isGrounded = false;
            }
        }
    }
    public void ActivateColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        colliders[0].enabled = true; //feetTrigger
        colliders[1].enabled = true; //head
        colliders[2].enabled = true; //body
        colliders[3].enabled = false; //exclude enemylayer featTrigger
        colliders[4].enabled = false; //bodytrigger
        colliders[5].enabled = false; //Border-only Collider
    }

    public void DeactivateColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
    }

    public void stayStatic()
    {
        StaticStateTracker.Record(this);
        isStatic = true;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale=originalGravityScale; //safety
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
        if(animator != null)
        {
            animator.SetBool("IsRunning",false);
        }  
    }

    public void stayDynamic()
    {
        isStatic = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            Debug.LogWarning("Cannot reset charge state without a Rigidbody2D.");
        }
        chargeReset = false;
        chargeAttackActive = false;
    }

    public Collider2D[] GetColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        return colliders;
    }

    private void UpdateGroundedState()
    {
        if (groundCheck == null) return;

        bool onSolidGround = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            solidGroundLayers
        );

        bool onPlatform = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            platformLayers
        );

        bool onPlayer = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            playerGroundLayers
        );

        isGrounded = onSolidGround || onPlatform || onPlayer;

        animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded)
        {
            animator.SetBool("Jump", false);
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length > 3)
        {
            colliders[3].enabled = onPlatform;
        }
    }

    private void GroundedSafeguard()
    {
        
        if (groundCheck == null) return;

        bool touchingGround = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            solidGroundLayers | platformLayers | enemyLayer
        );

        if (touchingGround && !isGrounded)
        {
            isGrounded = true;
            animator.SetBool("IsGrounded", true);
            animator.SetBool("Jump", false);
        }
        else if (!touchingGround && isGrounded)
        {
            isGrounded = false;
            animator.SetBool("IsGrounded", false);
        }
    }

    public void StaticSafeguard()
    {
        if(rb.bodyType == RigidbodyType2D.Static && !isStatic)
        {
            stayDynamic();
            Debug.LogWarning("Static Error.");
        }
    }
    #endregion

    #region Abilities and Cooldowns
    public void OnCooldown(float cd)
    {
        ignoreDamage = false;
        ignoreMovement = false;
        if(enemy != null)
        {
            EnemyAbilityEnable();
        }      
        knockable = true;
        cdbarimage.sprite = ogSprite;
        animator.SetBool("isUsingAbility", false);
        animator.SetBool("Casting", false);
        casting = false;

        stayDynamic();
        cdTimer = cd;
        onCooldown = true;
        cdCoroutine = StartCoroutine(AbilityCooldown(cd));
    }

    public IEnumerator AbilityCooldown(float duration)
    {
        if(cdCoroutine == null)
        {
            // cdTimer already set in OnCooldown()
            while (cdTimer > 0f)
            {
                cdTimer -= Time.deltaTime;
                UpdateCooldownSlider(duration);
                yield return null; // next frame
            }

            onCooldown = false;
            cdTimer = 0f;
            cdCoroutine = null;
            UpdateCooldownSlider(duration);
        }  
    }

    void UpdateCooldownSlider(float duration)
    {
        float progress = Mathf.Clamp01(1f - cdTimer / duration);
        cooldownSlider.value = progress;
    }

    public void EnemyAbilityBlock()
    {
        if (enemy == null) return;
        enemy.AbilityDisabled();
    }

    public void EnemyAbilityEnable()
    {
        enemy.AbilityEnabled();
    }

    public void AbilityDisabled()
    {
        canCast = false;
    }

    public void AbilityEnabled()
    {
        canCast = true;
    }

    public void UsingAbility(float cd)
    {
        casting = true;
        ignoreDamage = true;
        knockable = false;
        animator.SetBool("Casting", true);
        EnemyAbilityBlock();
        animator.SetBool("isUsingAbility", true);
        cdbarimage.sprite = activeSprite;
        isBlocking = false;
        UpdateCooldownSlider(cd);

        lastAbilityCD = cd; //ML
    }

    public virtual IEnumerator SpellSafety(float time, float cd)
    {
        yield return new WaitForSeconds(time);

        if (!onCooldown)
        {
            OnCooldown(cd);
        }     
    }

    public IEnumerator ChargeSafety(float time)
    {
        yield return new WaitForSeconds(time);

        chargeReset = false;
        knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
        
        stayDynamic();
        ignoreMovement = false;    
    }

    public void Casting(bool castin)
    {
        casting = castin;
    }
    #endregion

    #region Knockback
    public void Knockback(float force, float time, bool axis)
    {
        if (knockable)
        {
            knockbackXaxis = axis;
            audioManager.PlaySFX(audioManager.knockback, audioManager.lessVol);
            bool enemyOnRight = enemy.transform.position.x > this.transform.position.x;
            //This if must be removed when knockback tranfers to playerscript, its used for a Stellger Passive Function
            if (time == 0.3333f)
            {
                enemyOnRight = !enemyOnRight;
                knocked = true;
                StartCoroutine(ResetKnockedAfterDelay(0.3333f));
            }
            else
            {
                //animator.SetBool("knocked", true); oldKnocked*
                animator.SetTrigger("tookDmg");
            }
            KBForce = force;
            KBCounter = time;
            knockfromright = enemyOnRight;
        }
    }

    private IEnumerator ResetKnockedAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Reset the knocked variable
        knocked = false;
    }

    public void Knockable(bool update)
    {
        knockable = update;
    }
    #endregion

    #region Purely Virtual
    public virtual void HeavyAttack() { }

    public virtual void DealHeavyDamage() { }

    public virtual void Spell() { }

    public virtual void LightAttack() { }
    #endregion

    #region ChargeAttack
    public virtual void ChargeAttack() {
        TelemetryManager.Instance?.LogAction(PlayerId, "ChargeStart");
        knockable = false;
        charging = true;
        animator.SetBool("Charging", true);
        StartCharge();
    }

    private void StartCharge()
    {
        // If there is an existing charge coroutine, stop it
        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
        }

        // Start a new charge coroutine
        chargeCoroutine = StartCoroutine(Charge());
    }

    private IEnumerator Charge()
    {
        yield return new WaitForSeconds(chargeTime);  // Waits for 2 seconds
        if (charging)
        {
            charged = true;
            audioManager.PlaySFX(audioManager.charged, 0.7f);
            animator.SetTrigger("Charged");
        }
    }

    public virtual void DealChargeDmg()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "ChargeRelease");
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            enemy.StopPunching();
            if (!enemy.counterIsOn) {
                enemy.BreakCharge();
            }
            TelemetryManager.Instance?.LogHitAttempt(PlayerId, enemy.PlayerId, MoveType.Charge);
            enemy.SetIncomingDamageContext(PlayerId, MoveType.Charge, SourceType.Melee);
            enemy.TakeDamage(chargeDmg, false);
            enemy.Knockback(13f, 0.4f, false);
            audioManager.PlaySFX(audioManager.smash, audioManager.doubleVol);
            if (chargeHitSound != null)
            {
                audioManager.PlaySFX(chargeHitSound, 1.5f);
            }
        }
        else
        {
            if (chargeHitSound != null)
            {
                audioManager.PlaySFX(chargeHitSound, 1.5f);
            }
            else
            {
                TelemetryManager.Instance?.LogMiss(PlayerId, MoveType.Charge);
                audioManager.PlaySFX(audioManager.swoosh, audioManager.swooshVolume);
            }

        }
        chargeReset = true;
        knockable = true;
        charging = false;
        animator.SetBool("Casting", false);
        animator.SetBool("Charging", false);
    }

    public void ApplyCustomRuleset(CustomRuleset ruleset)
    {
        if (ruleset == null) return;

        maxHealth = ruleset.health;
        currHealth = ruleset.health;

        moveSpeed = ruleset.playerSpeed;
        OGMoveSpeed = ruleset.playerSpeed;

        quickDisable = ruleset.quickDisabled;
        heavyDisable = ruleset.heavyDisabled;
        blockDisable = ruleset.blockDisabled;
        specialDisable = ruleset.specialDisabled;
        chargeDisable = ruleset.chargeDisabled;

        chanChan = ruleset.chanChan;

       

        Debug.Log(
            $"Applied custom ruleset to {PlayerId} | " +
            $"HP={currHealth}/{maxHealth}, Speed={moveSpeed}, " +
            $"Q={quickDisable}, H={heavyDisable}, B={blockDisable}, S={specialDisable}, C={chargeDisable}"
        );
    }
    public virtual bool ChargeCheck(KeyCode charge)
    {
        if (charging)
        {
            chargeAttackActive = true;
            stayStatic();
            ignoreMovement = true;
            if (charged)
            {
                if (input.GetKeyUp(charge) || (controller && Input.GetKeyUp("joystick "+ControllerNum(playerNum)+" button 1")))
                {
                    //stayDynamic();
                    animator.SetTrigger("ChargedHit");
                    charged = false;
                    charging = false;
                    animator.SetBool("Casting", true);
                    animator.ResetTrigger("tookDmg");
                    StartCoroutine(ChargeSafety(0.83f));
                }
                return true;
            }
            else
            {
                if (input.GetKeyUp(charge) || (controller && Input.GetKeyUp("joystick "+ControllerNum(playerNum)+" button 1")))
                {
                    stayDynamic();
                    ignoreMovement = false;
                    animator.SetBool("Charging", false);
                    charging = false;
                    knockable = true;
                    animator.ResetTrigger("tookDmg");
                    chargeAttackActive = false;
                }
                return true;
            }
        }
        return false;
    }

    public void StopCHarge()
    {
        if (!casting)
        {
            chargeAttackActive = false;
            ignoreMovement = false;
            animator.SetBool("Charging", false);
            charging = false;
            knockable = true;
            charged = false;
            stayDynamic();
            animator.SetBool("Casting", false);
            animator.ResetTrigger("ChargedHit");
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
            }
        }
    }
    public void BreakCharge()
    {
        StopCHarge();
    }
    #endregion

    #region Block
    public void Block()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "BlockStart");
        if (blockDisabled)
        {
            return;
        }

        animator.SetTrigger("critsi");
        animator.SetBool("Crouch", true);
        animator.SetBool("IsRunning", false);
        PlayerBlock(true);
        isBlocking = true;
        ResetQuickPunch();
    }
    public void Unblock()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "BlockEnd");
        animator.SetBool("cWalk", false);
        animator.SetBool("Crouch", false);
        isBlocking = false;

        ResetQuickPunch();
    }

    public void blockBreaker()
    {
        isBlocking = false;
    }

    public void PlayerBlock(bool blck)
    {
        if (blck && animator != null)
        {
            animator.SetBool("IsRunning", false);
        }

        isBlocking = blck;
    }

    protected void ClearChargeState()
    {
        charging = false;
        charged = false;
        chargeAttackActive = false;
        chargeReset = false;

        ignoreMovement = false;
        knockable = true;

        animator.SetBool("Charging", false);
        animator.SetBool("Casting", false);
        animator.ResetTrigger("ChargedHit");

        stayDynamic();
    }

    #endregion

    #region General
    public void Jump()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Jump");
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        animator.SetBool("Jump", true);
        if (characterJump != null)
        {
            audioManager.PlaySFX(characterJump, audioManager.normalVol);
        }
        else
        {
            audioManager.PlaySFX(audioManager.jump, audioManager.jumpVolume);
        }


        ResetQuickPunch();
    }

    public Collider2D HitEnemy()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);
        return hitEnemy;
    }

    virtual public void HeavyAttackStart()
    {
        if (canAlterSpeed)
        {
            moveSpeed = heavySpeed;
            StartCoroutine(WaitAndSetSpeed());
        }
        animator.SetBool("IsHeavyAttacking", true);
    }

    public void HeavyAttackEnd()
    {
        if (canAlterSpeed)
        {
            moveSpeed = OGMoveSpeed;
        }
        animator.SetBool("IsHeavyAttacking", false);
        heavyAttacking=false;
    }

    private IEnumerator WaitAndSetSpeed()
    {

        yield return new WaitForSeconds(0.49f);  // Waits for 0.49 seconds
        moveSpeed = OGMoveSpeed;
        heavyAttacking=false;

    }

    void Parry() {
        TelemetryManager.Instance?.LogAction(PlayerId, "ParryAttempt");
        counterIsOn = true;
        safety = true;
        canParry = false;
        knockable = false;
        ignoreUpdate = true;
        stayStatic();
        StartCoroutine(ResetParry());
        StartCoroutine(CounterOffSafety());
        audioManager.PlaySFX(audioManager.counterScream, 2.5f);
        animator.SetTrigger("Parry");
    }

    IEnumerator ResetParry()
    {

        yield return new WaitForSeconds(5f);
        audioManager.PlaySFX(audioManager.rollReady, audioManager.lessVol);
        canParry = true;
    }

    public bool DetectCounter()
    {
        if (counterIsOn)
        {
            if (!counterDone)
            {
                Countered();
                return true;
            };
        }
        return false;
    }

    private IEnumerator CounterOffSafety()
    {
        yield return new WaitForSeconds(0.25f);
        if (!counterDone)
        {
            CounterVariablesOff();
        }
    }

    private IEnumerator CounterSuccessOff()
    {
        yield return new WaitForSeconds(0.7f);
        ClearParryState();
    }

    public void Countered()
    {
        TelemetryManager.Instance?.LogAction(PlayerId, "Parry");

        animator.SetTrigger("counterHit");
        audioManager.PlaySFX(audioManager.counterSucces, 1.5f);
        enemy.BreakCharge();
        enemy.stayStatic();
        stayStatic();
        ignoreCounterOff = true;
        counterDone = true;
        ignoreUpdate = true;
        StartCoroutine(CounterSuccessOff());
    }

    virtual public void DealCounterDmg()
    {
        enemy.StopPunching();
        enemy.BreakCharge();

        audioManager.PlaySFX(audioManager.counterClong, 0.5f);

        enemy.TakeDamage(parryDamage, true);

        stayDynamic();
        enemy.stayDynamic();

        enemy.Knockback(10f, .3f, false);

    }

    public void CounterVariablesOff()
    {
        counterDone = false;
        counterIsOn = false;
        knockable = true;
        safety = true;
        ignoreCounterOff = false;
        ignoreUpdate = false;
        stayDynamic();
    }

    protected void ClearParryState()
    {
        counterDone = false;
        counterIsOn = false;
        knockable = true;
        safety = true;
        ignoreCounterOff = false;
        ignoreUpdate = false;
        enemy.stayDynamic();
        stayDynamic();
    }

    protected void ClearTemporaryCombatState()
    {
        ignoreMovement = false;
        ignoreDamage = false;
        knockable = true;
        casting = false;
        isBlocking = false;

        animator.SetBool("Casting", false);
        animator.SetBool("Crouch", false);
        animator.SetBool("IsRunning", false);

        stayDynamic();
    }

    protected void QuickAttackIndicatorEnable()
    {
        quickAttackIndicator.SetActive(true);
    }

    protected void QuickAttackIndicatorDisable()
    {
        quickAttackIndicator?.SetActive(false);
    }

    public Character GetEnemy()
    {
        return enemy;
    }

    public void SetEnemy(Character changeEnemy)
    {
        enemy = changeEnemy;
    }

    public bool AmICasting()
    {
        return casting;
    }

    public IEnumerator TeleportCooldown()
    {
        justTeleported = true;
        yield return new WaitForSeconds(2f);
        justTeleported = false;
    }

    public bool IsCooldownBarActiveSprite
    {
        get
        {
            return cdbarimage != null && activeSprite != null && cdbarimage.sprite == activeSprite;
        }
    }

    public void DamagedAnimation()
    {
        animator.SetTrigger("tookDmg");
    }

    #endregion

    #region Passive and Damage
    // --- Telemetry helper ---
    protected float GetDistanceToEnemy()
    {
        if (enemy == null || gameManager.trainingMode) return -1f;
        return Vector2.Distance(transform.position, enemy.transform.position);
    }
    virtual public void TakeDamage(int dmg, bool blockable, bool parryable = true, bool canCrit = true)
    {
        if (parryable)
        {
            if (DetectCounter())
            {
                return;
            }
        }

        // cache distance once for this damage call
        float distance = GetDistanceToEnemy();

        // Invulnerability / i-frames (e.g., roll)
        if (ignoreDamage)
        {
            int hpBeforeInv = currHealth;
            int hpAfterInv = currHealth;

            TelemetryManager.Instance?.LogDamageApplied(
                incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                0,
                hpBeforeInv,
                hpAfterInv,
                distance,
                false,
                true
            );
            return;
        }

        if (dmg == chargeDmg)
        {
            StopCHarge();
        }

        if (chargeAttackActive)
        {
            if (chargeReset)
            {
                stayDynamic();
                ignoreMovement = false;
                chargeReset = false;
            }
            else
            {
                CheckForCrit(canAlterSpeed);
                TakeDamageNoAnimation(dmg, blockable);
                return;
            }
        }

        ResetQuickPunch();

        int hpBefore = currHealth;

        if (isBlocking && blockable)
        {
            if (blockSound != null)
            {
                audioManager.PlaySFX(blockSound, audioManager.normalVol);
            }

            if (dmg == heavyDamage)
            {
                currHealth -= 5;
                healthbar.SetHealth(currHealth);
                StartCoroutine(TriggerDamageCounter(5));
            }

            if (dmg == chargeDmg)
            {
                currHealth -= dmg;
                healthbar.SetHealth(currHealth);
                moveSpeed = OGMoveSpeed;
                StartCoroutine(TriggerDamageCounter(dmg));
            }

            // Blocked light attacks do not reduce health.
        }
        else
        {
            if (damageShield)
            {
                damageShield = false;
                shield.gameObject.SetActive(false);

                int hpBeforeShield = currHealth;
                int hpAfterShield = currHealth;

                // Treat shield as negated / dodged
                TelemetryManager.Instance?.LogDamageApplied(
                    incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                    0,
                    hpBeforeShield,
                    hpAfterShield,
                    distance,
                    false,
                    true
                );

                return;
            }

            currHealth -= dmg;
            CheckForCrit(canCrit);
            animator.SetTrigger("tookDmg");
            healthbar.SetHealth(currHealth);
            StartCoroutine(TriggerDamageCounter(dmg));

        }

        int hpAfter = currHealth;
        int actualDamage = hpBefore - hpAfter;

        // Log outcome (always meaningful: damage, or blocked 0)
        if (actualDamage > 0)
        {
            TelemetryManager.Instance?.LogDamageApplied(
                incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                actualDamage,
                hpBefore,
                hpAfter,
                distance,
                (isBlocking && blockable),
                false
            );
        }
        else if (isBlocking && blockable)
        {
            // blocked 0 damage (important for defense metrics)
            TelemetryManager.Instance?.LogDamageApplied(
                incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                0,
                hpBefore,
                hpAfter,
                distance,
                true,
                false
            );
        }

        if (currHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        int winnerNum = playerNum == 1 ? 2 : 1;
        if (overrideDeath) {
            return;
        }
        animator.SetBool("isDead", true);
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            stayStatic();
        }

        ignoreDamage = true;
        knockable = false;

        ActivateHealthBars(); //In case they are hidden

        enemy.Win();
        enemy.stayStatic();

        audioManager.StopMusic();
        audioManager.PlaySFX(audioManager.dearth, audioManager.doubleVol);

        if (enemy.currHealth == maxHealth)
        {
            gameManager.RoundEndFlawless(winnerNum, P2Name);
            KeepStats(P2Name,enemy.GetCharID(), P1Name.text,characterID);
        }
        else if (enemy.currHealth <= 0)
        {
            gameManager.RoundEndTie(playerNum);
        }
        else
        {
            KeepStats(P2Name,enemy.GetCharID(), P1Name.text,characterID);
            gameManager.RoundEnd(winnerNum, P2Name);
        }

    }

    public void PermaDeath()
    {
        animator.SetBool("permanentDeath", true);
        this.enabled = false;
    }

    public void Win()
    {
        if (winQuip != null)
        {
            audioManager.PlaySFX(winQuip, 2);
        }
        animator.SetTrigger("Win");
    }

    public void ActivateHealthBars()
    {
        healthbar.gameObject.SetActive(true);
        enemy.healthbar.gameObject.SetActive(true);
    }

    virtual public void TakeDamageNoAnimation(int dmg, bool blockable, bool parryable = true)
    {
        if (parryable)
        {
            if (DetectCounter())
            {
                return;
            }
        }

        float distance = GetDistanceToEnemy();

        // Invulnerability / i-frames
        if (ignoreDamage)
        {
            int hpBeforeInv = currHealth;
            int hpAfterInv = currHealth;

            TelemetryManager.Instance?.LogDamageApplied(
                incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                0,
                hpBeforeInv,
                hpAfterInv,
                distance,
                false,
                true
            );
            return;
        }

        int hpBefore = currHealth;

        if (isBlocking && blockable)
        {
            if (blockSound != null)
            {
                audioManager.PlaySFX(blockSound, audioManager.normalVol);
            }
            // No HP change here (blocked)
        }
        else
        {
            if (damageShield)
            {
                damageShield = false;
                shield.gameObject.SetActive(false);

                int hpBeforeShield = currHealth;
                int hpAfterShield = currHealth;

                TelemetryManager.Instance?.LogDamageApplied(
                    incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                    0,
                    hpBeforeShield,
                    hpAfterShield,
                    distance,
                    false,
                    true
                );

                return;
            }

            currHealth -= dmg;

            healthbar.SetHealth(currHealth);
            StartCoroutine(TriggerDamageCounter(dmg));
        }

        int hpAfter = currHealth;
        int actualDamage = hpBefore - hpAfter;

        if (actualDamage > 0)
        {
            TelemetryManager.Instance?.LogDamageApplied(
                incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                actualDamage,
                hpBefore,
                hpAfter,
                distance,
                (isBlocking && blockable),
                false
            );
        }
        else if (isBlocking && blockable)
        {
            TelemetryManager.Instance?.LogDamageApplied(
                incomingAttackerId, this.PlayerId, incomingMoveType, incomingSourceType,
                0,
                hpBefore,
                hpAfter,
                distance,
                true,
                false
            );
        }

        if (currHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator TriggerDamageCounter(int damage) {

        if (damageCounter.gameObject.activeSelf) {
            damage += int.Parse(damageCounter.text);
        }
        damageCounter.text = damage.ToString();
        damageCounter.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        damageCounter.gameObject.SetActive(false);

    }

    public void DealDamageToEnemy(int amount)
    {
        enemy.TakeDamageNoAnimation(amount, false);
    }

    public IEnumerator InterruptMovement(float time)
    {
        rb.velocity = Vector2.zero; // Stop the enemy's movement
        ignoreMovement = true;

        yield return new WaitForSeconds(time);

        ignoreMovement = false;
    }

    public void Stun(float time)
    {
        StartCoroutine(StunCoroutine(time));
    }

    public IEnumerator StunCoroutine(float time)
    {
        StopCHarge();

        stun.gameObject.SetActive(true);
        rb.velocity = Vector2.zero; // Stop the enemy's movement
        stunned = true;

        yield return new WaitForSeconds(time);

        stunned = false;
        stun.gameObject.SetActive(false);
    }

    public void Slow(float time, float amount)
    {
        if (canAlterSpeed)
        {
            StartCoroutine(SlowCoroutine(time, amount));
        }
    }

    public IEnumerator SlowCoroutine(float time, float amount)
    {

        stun.gameObject.SetActive(true);
        moveSpeed = moveSpeed - amount;

        yield return new WaitForSeconds(time);

        moveSpeed = OGMoveSpeed;
        stun.gameObject.SetActive(false);
    }


    public void DisableBlock(bool whileKnocked)
    {
        Unblock();
        moveSpeed = OGMoveSpeed;
        blockDisabled = true;
        blockDisabledIndicator.gameObject.SetActive(true);
    }

    public void DisableJump(bool choice)
    {
        jumpDisabled = choice;
    }

    public void EnableBlock()
    {
        blockDisabled = false;
        blockDisabledIndicator.gameObject.SetActive(false);
    }

    public void IgnoreMovement(bool boolean)
    {
        ignoreMovement = boolean;
    }

    public void IgnoreSlow(bool boolean)
    {
        ignoreSlow = boolean;
    }

    public void IgnoreUpdate(bool boolean)
    {
        ignoreUpdate = boolean;
    }

    public void ChangeSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void ChangeOGSpeed(float speed)
    {
        OGMoveSpeed = speed;
    }

    public void StopPunching()
    {
        animator.SetBool("IsHeavyAttacking", false);
    }

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public bool IsEnemyClose()
    {
        return Vector3.Distance(this.transform.position, enemy.transform.position) <= 4f;
    }

    public int GetCurrentHealth()
    {
        return currHealth;
    }

    public void SetCurrentHealth(int value)
    {
        currHealth = value;
        healthbar.SetHealth(value);
    }
    #endregion

    #region Special Functions
    //Rager
    public void ResetQuickPunch()
    {
        if(this is Rager)
        {
            animator.SetBool("QuickPunch", false);
        }       
    }

    public void Grabbed()
    {
        audioManager.PlaySFX(audioManager.grab, audioManager.heavyAttackVolume);
        animator.SetTrigger("grabbed");
    }

    public void StackPoison1(bool on)
    {
        Stack1Poison.gameObject.SetActive(on);
    }
    public void StackPoison2(bool on)
    {
        Stack2Poison.gameObject.SetActive(on);
    }
    public void StackPoison3(bool on)
    {
        Stack3Poison.gameObject.SetActive(on);
    }

    public bool IsPoisoned()
    {
        return poison.gameObject.activeSelf;
    }

    public void ActivatePoison(bool on)
    {
        poison.gameObject.SetActive(on);
    }

    public void ActivateStun(bool on)
    {
        poison.gameObject.SetActive(on);
    }
    public void ActivateblockBreaker(bool on)
    {
        blockDisabledIndicator.gameObject.SetActive(on);
    }

    public void Heal(int amount)
    {
        if (!animator.GetBool("isDead")) {

            currHealth += amount;
            if (currHealth > maxHealth)
            {
                currHealth = maxHealth;
            }
            healthbar.SetHealth(currHealth);
        }

    }

    void CheckForCrit(bool canCrit = true)
    {
        if (CriticalChance() && !gameManager.trainingMode && canCrit)
        {
            FlashRed();
            TakeDamageNoAnimation(10,false);
            audioManager.PlaySFX(audioManager.critical, 2.6f);
        }
    }
    virtual protected bool CriticalChance()
    {
        return UnityEngine.Random.value < 0.1f;
    }

    public void FlashRed()
    {
        if (flashRedCoroutine != null)
        {
            StopCoroutine(flashRedCoroutine);
        }

        flashRedCoroutine = StartCoroutine(FlashRedCoroutine(0.3f));
    }

    private IEnumerator FlashRedCoroutine(float duration)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (renderers == null || renderers.Length == 0)
            yield break;

        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
            renderers[i].color = Color.red;
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }

        flashRedCoroutine = null;
    }

    public void ChangeEnemy(Character newEnemy)
    {
        enemy = newEnemy;
    }

    #endregion

    #region PowerUps
    public void SpeedBoost()
    {
        moveSpeed = moveSpeed + 2;

        StartCoroutine(SpeedBoostCoroutine());
    }

    private IEnumerator SpeedBoostCoroutine()
    {
        yield return new WaitForSeconds(5f);

        moveSpeed = OGMoveSpeed;
    }

    public void DamageShield()
    {
        damageShield = true;
        shield.gameObject.SetActive(true);
    }

    public void RefreshCD()
    {
        cdTimer -= 5f;
    }

    public void HealUp()
    {
        StartCoroutine(HealCoroutine(2, 2f, 5));
    }

    private IEnumerator HealCoroutine(int amount, float interval, int times)
    {

        for (int i = 0; i < times; i++)
        {
            yield return new WaitForSeconds(interval);

            Heal(amount);
        }

    }

    public void KeepStats(string winner,int winnerID, string loser,int loserID)
    {
        if (gameManager.trainingMode)
        {
            return;
        }
        
        if (winner == loser || ignoreStats)
        {
            return;
        }

        if (CharacterStatsManager.Instance != null)
        {
            CharacterStatsManager.Instance.KeepStats(winner, loser);
        }
        else
        {
            Debug.LogWarning("CharacterStatsManager is not available; match statistics were not recorded.");
        }

        KeepData(winnerID,loserID,playerNum);
    }

    public void KeepData(int winnerID, int loserID, int setupNum)
    {
        if (MatchDataLogger.Instance == null)
        {
            Debug.LogWarning("MatchDataLogger not found.");
            return;
        }

        int charA, charB;
        int aWins;

        if (setupNum == 2)
        {
            charA = winnerID;
            charB = loserID;
            aWins = 1;
        }
        else
        {
            charA = loserID;
            charB = winnerID;
            aWins = 0;
        }

        CharacterSpecsBase specsA = resources.GetSpecsByID(charA);
        CharacterSpecsBase specsB = resources.GetSpecsByID(charB);

        if (specsA == null || specsB == null)
        {
            Debug.LogError("Could not load specs.");
            return;
        }

        MatchDataLogger.Instance.LogMatchRow(
            charA,
            charB,
            specsA.damage,
            specsA.cooldown,
            specsA.utility,
            specsB.damage,
            specsB.cooldown,
            specsB.utility,
            aWins
        );
    }

    public int GetCharID()
    {
        return characterID;
    }

    #endregion

    #region RL
    // --- Public read-only state for RL ---
    public bool IsGrounded => isGrounded;
    public bool IsBlocking => isBlocking;
    public bool IsCasting => casting;
    public bool IsStunned => stunned;
    public bool IsKnocked => knocked;
    public bool IsCharging => charging;
    public bool IsCharged => charged;
    public bool OnAbilityCD => onCooldown;
    public bool CanCast => canCast;
    public bool CanParry => canParry;

    public bool QuickDisabled => quickDisable;
    public bool HeavyDisabled => heavyDisable;
    public bool BlockDisabled => blockDisable;
    public bool SpecialDisabled => specialDisable;
    public bool ChargeDisabled => chargeDisable;
    public bool JumpDisabled => jumpDisabled;
    public bool Parrying => counterIsOn;
    public bool HeavyAttacking => heavyAttacking;
    public bool LightAttacking => isLightAttacking;

    // Normalized ability cooldown: zero is ready and one is newly activated.
    private float lastAbilityCD = 0f;
    public float AbilityCooldown01
    {
        get
        {
            if (!onCooldown || lastAbilityCD <= 0f) return 0f;
            // cdTimer counts down each second; normalize remaining
            return Mathf.Clamp01(cdTimer / lastAbilityCD);
        }
    }
    private static readonly Vector3[] spawnPoints =
    {
        new Vector3(-7.3f, -2.50f, 0f),   // existing P1
        new Vector3(7.4f, -2.50f, 0f),    // existing P2

        new Vector3(-2.33f, 1.78f, 0f),
        new Vector3(0.89f, -2.44f, 0f),
        new Vector3(6.84f, 3.51f, 0f),
        new Vector3(3.43f, 0.52f, 0f),
        new Vector3(0.02f, 3.93f, 0f)
    };

    private static void ChooseSpawnPoints()
    {
        int count = spawnPoints.Length;

        spawnIndexP1 = UnityEngine.Random.Range(0, count);

        do
        {
            spawnIndexP2 = UnityEngine.Random.Range(0, count);
        }
        while (spawnIndexP2 == spawnIndexP1);
    }


    public virtual void ResetForEpisode2()
    {
        StopAllCoroutines();

        // Position & physics
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1.8f;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (playerNum == 1)
        {
            ChooseSpawnPoints(); // choose once

            transform.position = spawnPoints[spawnIndexP1];
        }
        else
        {
            transform.position = spawnPoints[spawnIndexP2];
        }
        
        // Animator sanity
        if (animator == null) animator = GetComponent<Animator>();
        animator.Rebind();
        animator.Update(0f);
        animator.SetBool("isDead", false);
        animator.ResetTrigger("tookDmg");
        animator.ResetTrigger("ChargedHit");
        animator.SetBool("Charging", false);
        animator.SetBool("Casting", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("Crouch", false);
        //animator.SetBool("Jump", false);
        //animator.SetBool("isGrounded", true);

        // Core flags
        ignoreUpdate = false;
        isBlocking = false;
        casting = false;
        stunned = false;
        knocked = false;
        knockable = true;
        justTeleported = false;
        isonpad=0;
        onCooldown = false;
        ignoreUpdate = false;
        ignoreDamage = false;
        counterDone = false;
        counterIsOn = false;
        canParry = true;
        charging = false;
        charged = false;
        ActivateColliders();
    }

    public void ClearDynamicScripts()
    {
        // Remove LupenSpirit if it exists
        LupenSpirit spirit = GetComponent<LupenSpirit>();
        if (spirit != null)
        {
            Destroy(spirit);
        }
        // Remove Lupen if it exists
        Lupen lup = GetComponent<Lupen>();
        if (lup != null)
        {
            Destroy(lup);
        }
    }

    public void DebugDumpState(string context = "CharacterStateDump")
    {
        StringBuilder sb = new StringBuilder(2048);

        sb.AppendLine("===================[t]=====================");
        sb.AppendLine($"[DEBUG STATE DUMP] {context}");
        sb.AppendLine($"Character: {gameObject.name}");
        sb.AppendLine($"Type: {GetType().Name}");
        sb.AppendLine($"PlayerId: {PlayerId}");
        sb.AppendLine($"playerNum: {playerNum}");
        sb.AppendLine($"characterID: {characterID}");
        sb.AppendLine($"Time.time: {Time.time:F3}");
        sb.AppendLine($"Time.frameCount: {Time.frameCount}");
        sb.AppendLine("-------------- Transform / Physics --------------");
        sb.AppendLine($"Position: {transform.position}");
        sb.AppendLine($"LocalScale: {transform.localScale}");
        sb.AppendLine($"Rotation: {transform.rotation.eulerAngles}");
        sb.AppendLine($"SpawnPos: {_spawnPos}");

        if (rb != null)
        {
            sb.AppendLine($"Rigidbody bodyType: {rb.bodyType}");
            sb.AppendLine($"Rigidbody velocity: {rb.velocity}");
            sb.AppendLine($"Rigidbody angularVelocity: {rb.angularVelocity}");
            sb.AppendLine($"Rigidbody gravityScale: {rb.gravityScale}");
            sb.AppendLine($"Rigidbody mass: {rb.mass}");
            sb.AppendLine($"Rigidbody simulated: {rb.simulated}");
            sb.AppendLine($"Rigidbody constraints: {rb.constraints}");
        }
        else
        {
            sb.AppendLine("Rigidbody: NULL");
        }

        sb.AppendLine("-------------- Core State Flags --------------");
        sb.AppendLine($"isStatic: {isStatic}");
        sb.AppendLine($"ignoreUpdate: {ignoreUpdate}");
        sb.AppendLine($"ignoreMovement: {ignoreMovement}");
        sb.AppendLine($"ignoreDamage: {ignoreDamage}");
        sb.AppendLine($"ignoreSlow: {ignoreSlow}");
        sb.AppendLine($"canRotate: {canRotate}");
        sb.AppendLine($"knockable: {knockable}");
        sb.AppendLine($"stunned: {stunned}");
        sb.AppendLine($"knocked: {knocked}");
        sb.AppendLine($"casting: {casting}");
        sb.AppendLine($"usingAbility: {usingAbility}");
        sb.AppendLine($"canCast: {canCast}");
        sb.AppendLine($"onCooldown: {onCooldown}");
        sb.AppendLine($"cdTimer: {cdTimer:F3}");
        sb.AppendLine($"lastAbilityCD: {lastAbilityCD:F3}");
        sb.AppendLine($"AbilityCooldown01: {AbilityCooldown01:F3}");

        sb.AppendLine("-------------- Health / Combat --------------");
        sb.AppendLine($"currHealth: {currHealth}");
        sb.AppendLine($"maxHealth: {maxHealth}");
        sb.AppendLine($"damageShield: {damageShield}");
        sb.AppendLine($"isBlocking: {isBlocking}");
        sb.AppendLine($"blockDisabled: {blockDisabled}");
        sb.AppendLine($"quickDisable: {quickDisable}");
        sb.AppendLine($"heavyDisable: {heavyDisable}");
        sb.AppendLine($"specialDisable: {specialDisable}");
        sb.AppendLine($"chargeDisable: {chargeDisable}");
        sb.AppendLine($"jumpDisabled: {jumpDisabled}");
        sb.AppendLine($"overrideDeath: {overrideDeath}");

        sb.AppendLine("-------------- Movement / Grounding --------------");
        sb.AppendLine($"isGrounded: {isGrounded}");
        sb.AppendLine($"grounds: {grounds}");
        sb.AppendLine($"isonpad: {isonpad}");
        sb.AppendLine($"CanDropPlatform: {CanDropPlatform}");
        sb.AppendLine($"moveSpeed: {moveSpeed:F3}");
        sb.AppendLine($"OGMoveSpeed: {OGMoveSpeed:F3}");
        sb.AppendLine($"heavySpeed: {heavySpeed:F3}");
        sb.AppendLine($"jumpForce: {jumpForce:F3}");
        sb.AppendLine($"attackRange: {attackRange:F3}");
        sb.AppendLine($"ogRange: {ogRange:F3}");
        sb.AppendLine($"jumpAxisHeld: {jumpAxisHeld}");
        sb.AppendLine($"lastMoveDir: {lastMoveDir}");
        sb.AppendLine($"nextMoveLogTime: {nextMoveLogTime:F3}");

        sb.AppendLine("-------------- Charge State --------------");
        sb.AppendLine($"charging: {charging}");
        sb.AppendLine($"charged: {charged}");
        sb.AppendLine($"chargeAttackActive: {chargeAttackActive}");
        sb.AppendLine($"chargeReset: {chargeReset}");
        sb.AppendLine($"chargeTime: {chargeTime:F3}");
        sb.AppendLine($"chargeDmg: {chargeDmg}");
        sb.AppendLine($"chargeCoroutine running?: {chargeCoroutine != null}");

        sb.AppendLine("-------------- Parry / Counter State --------------");
        sb.AppendLine($"canParry: {canParry}");
        sb.AppendLine($"counterIsOn: {counterIsOn}");
        sb.AppendLine($"counterDone: {counterDone}");
        sb.AppendLine($"safety: {safety}");
        sb.AppendLine($"ignoreCounterOff: {ignoreCounterOff}");
        sb.AppendLine($"parryDamage: {parryDamage}");

        sb.AppendLine("-------------- Knockback State --------------");
        sb.AppendLine($"KBForce: {KBForce:F3}");
        sb.AppendLine($"KBCounter: {KBCounter:F3}");
        sb.AppendLine($"KBTotalTime: {KBTotalTime:F3}");
        sb.AppendLine($"knockfromright: {knockfromright}");
        sb.AppendLine($"knockbackXaxis: {knockbackXaxis}");

        sb.AppendLine("-------------- Input / Control --------------");
        sb.AppendLine($"playerString: {playerString}");
        sb.AppendLine($"controller: {controller}");
        sb.AppendLine($"controllerCount: {controllerCount}");
        sb.AppendLine($"debugControllers: {debugControllers}");
        sb.AppendLine($"input provider: {(input != null ? input.GetType().Name : "NULL")}");

        sb.AppendLine("-------------- Teleport / Misc --------------");
        sb.AppendLine($"justTeleported: {justTeleported}");
        sb.AppendLine($"chanChan: {chanChan}");
        sb.AppendLine($"originalGravityScale: {originalGravityScale:F3}");

        sb.AppendLine("-------------- Attack Flags --------------");
        sb.AppendLine($"isLightAttacking: {isLightAttacking}");
        sb.AppendLine($"heavyAttacking: {heavyAttacking}");

        sb.AppendLine("-------------- References --------------");
        sb.AppendLine($"enemy: {(enemy != null ? enemy.gameObject.name : "NULL")}");
        sb.AppendLine($"animator: {(animator != null ? "OK" : "NULL")}");
        sb.AppendLine($"audioManager: {(audioManager != null ? "OK" : "NULL")}");
        sb.AppendLine($"resources: {(resources != null ? "OK" : "NULL")}");
        sb.AppendLine($"characterSetup: {(characterSetup != null ? "OK" : "NULL")}");
        sb.AppendLine($"characterChoiceHandler: {(characterChoiceHandler != null ? "OK" : "NULL")}");
        sb.AppendLine($"gameManager: {(gameManager != null ? "OK" : "NULL")}");
        sb.AppendLine($"healthbar: {(healthbar != null ? "OK" : "NULL")}");
        sb.AppendLine($"attackPoint: {(attackPoint != null ? attackPoint.position.ToString() : "NULL")}");
        sb.AppendLine($"groundCheck: {(groundCheck != null ? groundCheck.position.ToString() : "NULL")}");

        sb.AppendLine("-------------- Layers / Ground Check --------------");
        sb.AppendLine($"enemyLayer: {enemyLayer.value}");
        sb.AppendLine($"solidGroundLayers: {solidGroundLayers.value}");
        sb.AppendLine($"platformLayers: {platformLayers.value}");
        sb.AppendLine($"playerGroundLayers: {playerGroundLayers.value}");
        sb.AppendLine($"groundCheckRadius: {groundCheckRadius:F3}");

        if (groundCheck != null)
        {
            bool onSolidGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, solidGroundLayers);
            bool onPlatform = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, platformLayers);
            bool onPlayer = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, playerGroundLayers);
            bool touchingAny = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, solidGroundLayers | platformLayers | enemyLayer);

            sb.AppendLine($"Overlap onSolidGround: {onSolidGround}");
            sb.AppendLine($"Overlap onPlatform: {onPlatform}");
            sb.AppendLine($"Overlap onPlayer: {onPlayer}");
            sb.AppendLine($"Overlap touchingAny: {touchingAny}");
        }

        sb.AppendLine("-------------- Animator State --------------");
        if (animator != null)
        {
            AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(0);
            sb.AppendLine($"Animator enabled: {animator.enabled}");
            sb.AppendLine($"Animator speed: {animator.speed}");
            sb.AppendLine($"Animator updateMode: {animator.updateMode}");
            sb.AppendLine($"Animator cullingMode: {animator.cullingMode}");
            sb.AppendLine($"Animator state shortHash: {st.shortNameHash}");
            sb.AppendLine($"Animator normalizedTime: {st.normalizedTime:F3}");
            sb.AppendLine($"Animator IsInTransition: {animator.IsInTransition(0)}");

            sb.AppendLine($"Anim Bool isDead: {SafeGetAnimatorBool("isDead")}");
            sb.AppendLine($"Anim Bool Charging: {SafeGetAnimatorBool("Charging")}");
            sb.AppendLine($"Anim Bool Casting: {SafeGetAnimatorBool("Casting")}");
            sb.AppendLine($"Anim Bool IsRunning: {SafeGetAnimatorBool("IsRunning")}");
            sb.AppendLine($"Anim Bool Crouch: {SafeGetAnimatorBool("Crouch")}");
            sb.AppendLine($"Anim Bool IsGrounded: {SafeGetAnimatorBool("IsGrounded")}");
            sb.AppendLine($"Anim Bool Jump: {SafeGetAnimatorBool("Jump")}");
            sb.AppendLine($"Anim Bool IsHeavyAttacking: {SafeGetAnimatorBool("IsHeavyAttacking")}");
            sb.AppendLine($"Anim Bool QuickPunch: {SafeGetAnimatorBool("QuickPunch")}");
        }

        sb.AppendLine("-------------- Colliders --------------");
        Collider2D[] cols = GetComponents<Collider2D>();
        sb.AppendLine($"Collider count: {cols.Length}");
        for (int i = 0; i < cols.Length; i++)
        {
            Collider2D c = cols[i];
            if (c == null)
            {
                sb.AppendLine($"Collider[{i}]: NULL");
                continue;
            }

            sb.AppendLine(
                $"Collider[{i}]: type={c.GetType().Name}, " +
                $"enabled={c.enabled}, isTrigger={c.isTrigger}, boundsCenter={c.bounds.center}, boundsSize={c.bounds.size}"
            );
        }

        StaticStateTracker.PrintLastInfo(this, "Before reset");

        sb.AppendLine("========================================");

        Debug.Log(sb.ToString(), this);
    }

    private string SafeGetAnimatorBool(string paramName)
    {
        if (animator == null) return "Animator NULL";

        foreach (var p in animator.parameters)
        {
            if (p.name == paramName && p.type == AnimatorControllerParameterType.Bool)
            {
                return animator.GetBool(paramName).ToString();
            }
        }

        return "MISSING";
    }

    #endregion
}
