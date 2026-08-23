using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSetup : MonoBehaviour
{

    //movement keys
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

    public GameObject[] stages;

    public Transform attackPoint;

    public GameObject blockDisabledIndicator;
    public GameObject poison;
    public GameObject Stack1Poison;
    public GameObject Stack2Poison;
    public GameObject Stack3Poison;
    public GameObject stun;
    public GameObject shield;

    public TextMeshPro robberyCountIndicator;
    public GameManager gameManager;

    public Rigidbody2D rb;
    public Animator animator;

    public LayerMask enemyLayer;

    

    public Image cdbarimage;
    public Sprite activeSprite, ogSprite;
    public Slider cooldownSlider;
    public AudioManager audioManager;
    public HealthBar healthbar;
    public TextMeshProUGUI P1Name, winner;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    public GameObject saveReplayButton;
    public GameObject quickAttackIndicator;
    public TextMeshProUGUI damageCounter;
    public Transform groundCheck;

    public int playerNum;

    private void OnDrawGizmosSelected()
    {
        Vector2 groundCheckSize = new Vector2(0.8f, 0.3f);
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

}
