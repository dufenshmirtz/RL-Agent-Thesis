using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool roundTelemetryClosed = false;
    static int player1Wins = 0;
    static int player2Wins = 0;
    public GameObject[] stages;
    string stageName;
    public TextMeshProUGUI winner;
    public TextMeshProUGUI finalWinner;
    public TextMeshProUGUI p1ProfileNameText;
    public TextMeshProUGUI p2ProfileNameText;
    string p1, p2;
    static int roundNumber = 1;
    static int roundCounter = 1;
    public CharacterManager p1Manager, p2Manager;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    public GameObject saveReplayButton;
    public GameObject victoryScreenNavigation;
    public AudioManager audioManager;
    public GameObject p1R1, p1R2, p1R3;
    public GameObject p2R1, p2R2, p2R3;
    bool tie = false;
    static string c1Name, c2Name;
    static bool p1Random = false;
    static bool p2Random = false;
    bool gameEnd = false;
    static int portalNumber;
    public GameObject[] portalPairs;
    bool chanChan;
    public int maxHealth = -1;

    //training
    public bool trainingMode = false;           // tick this for training scene
    public FighterAgent agentP1, agentP2;       // drag the two FighterAgent components
    public Transform p1Spawn, p2Spawn;          // empty transforms as spawn points

    public float tScale = 1f;

    public bool roundOn = false;
    public bool trainingRoundOn = false;
    public TrainingOpponentDirector opponentDirector;


    // Start is called before the first frame update
    void Start()
    {
        roundTelemetryClosed = false;

        stageName = PlayerPrefs.GetString("SelectedStage", "Stage 1");
        if (stageName == "Stage 1")
        {
            stages[0].SetActive(true);
        }
        else if (stageName == "Stage 2")
        {
            stages[1].SetActive(true);
        }
        else if (stageName == "Stage 3")
        {
            stages[2].SetActive(true);
        }



        TelemetryManager.Instance?.StartSession();

        TelemetryManager.Instance?.SetMatchMeta(new TelemetryMatchMeta
        {
            map = stageName,
            mode = trainingMode ? "training" : "1v1",
            roundNumber_ = roundCounter,
            trainingMode = trainingMode
        });

        TelemetryManager.Instance?.SetPlayers(
            "P1", p1Manager ? p1Manager.GetCharacterName(1) : "",
            "P2", p2Manager ? p2Manager.GetCharacterName(1) : ""
        );

        // Profile telemetry
        var p1Profile = ProfileManager.I?.GetTelemetryIdentity(1) ?? ("NONE", "None");
        var p2Profile = ProfileManager.I?.GetTelemetryIdentity(2) ?? ("NONE", "None");
        if (p1ProfileNameText != null)
            p1ProfileNameText.text = p1Profile.name;

        if (p2ProfileNameText != null)
            p2ProfileNameText.text = p2Profile.name;
        TelemetryManager.Instance?.SetMatchMeta(new TelemetryMatchMeta
        {
            p1ProfileId = p1Profile.id,
            p1ProfileName = p1Profile.name,
            p2ProfileId = p2Profile.id,
            p2ProfileName = p2Profile.name
        });


        int selectedSlot = RulesetSelectionState.SelectedSlot;

        if (selectedSlot > 0)
        {
            CustomRuleset loadedRuleset = RulesetManager.Instance.LoadCustomRuleset(selectedSlot);

            if (loadedRuleset != null)
            {

                roundNumber = loadedRuleset.rounds;
                portalNumber = loadedRuleset.portals;
                chanChan = loadedRuleset.chanChan;
                maxHealth = loadedRuleset.health;

                ApplyRulesetToCurrentCharacters(loadedRuleset);
            }
            else
            {
                Debug.LogWarning("No custom ruleset found for selected slot: " + selectedSlot);
            }
        }
        else
        {
        }

        if (chanChan)
        {
            portalNumber = Random.Range(0, 5);
        }

        if (trainingMode)
        {
            roundOn = true;
            portalNumber = 0;
            Time.timeScale = tScale;
        }

        switch (portalNumber)
        {
            case 0:
                break;
            case 1:
                portalPairs[0].SetActive(true);
                break;
            case 2:
                portalPairs[0].SetActive(true);
                portalPairs[1].SetActive(true);
                break;
            case 3:
                portalPairs[2].SetActive(true);
                break;
            case 4:
                portalPairs[2].SetActive(true);
                portalPairs[3].SetActive(true);
                break;
        }

        ActivateIndicators();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
            //I keep that useless awake in case I did need it for some reason and I should know that this is the reason behind a bug
        }
        else
        {
            Destroy(gameObject);
        }

        QualitySettings.vSyncCount = 0;

        if (trainingMode)
            Application.targetFrameRate = -1;   // unlimited
        else
            Application.targetFrameRate = 60;
    }

    private void ApplyRulesetToCurrentCharacters(CustomRuleset ruleset)
    {
        if (p1Manager != null)
        {
            Character p1 = p1Manager.GetCurrentCharacter();
            if (p1 != null)
                p1.ApplyCustomRuleset(ruleset);
        }

        if (p2Manager != null)
        {
            Character p2 = p2Manager.GetCurrentCharacter();
            if (p2 != null)
                p2.ApplyCustomRuleset(ruleset);
        }
    }


    public void RoundEnd(int playerNum, string winnerName)
    {

        if (trainingMode)//training
        {
            SoftResetRound(playerNum);
            return;
        }

        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = winnerName + " prevails!";

        ShortWins(playerNum, winnerName);

        if (!roundTelemetryClosed)
        {
            string p1Char = p1Manager ? p1Manager.GetCharacterName(1) : "";
            string p2Char = p2Manager ? p2Manager.GetCharacterName(1) : "";
            string winnerId = (playerNum == 1) ? "P1" : "P2";
            string winnerCharacter = (playerNum == 1) ? p1Char : p2Char;

            // Finalize winner metadata immediately before writing telemetry.
            TelemetryManager.Instance?.SetMatchMeta(new TelemetryMatchMeta
            {
                map = stageName,
                mode = trainingMode ? "training" : "1v1",
                roundNumber_ = roundCounter,
                trainingMode = trainingMode,

                p1Id = "P1",
                p1Character = p1Char,
                p2Id = "P2",
                p2Character = p2Char,

                winnerId = winnerId,
                winnerCharacter = winnerCharacter
            });

            TelemetryManager.Instance?.EndSession($"RoundEnded_KO_winner={winnerName}");
            roundTelemetryClosed = true;
        }

        StartCoroutine(WaitAndCheck(playerNum, winnerName));
        roundOn = false;
    }

    public void RoundEndTie(int playerNum)
    {
        if (trainingMode) //training
        {
            // Remove the short-match adjustment when the round ends in a tie.
            SoftResetRound(0);
            return;
        }

        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = "Tie?\nDEATH PREVAILS...";
        tie = true;

        if (!roundTelemetryClosed)
        {
            // Finalize tie metadata immediately before writing telemetry.
            TelemetryManager.Instance?.SetMatchMeta(new TelemetryMatchMeta
            {
                map = stageName,
                mode = trainingMode ? "training" : "1v1",
                roundNumber_ = roundCounter,
                trainingMode = trainingMode,

                p1Id = "P1",
                p1Character = p1Manager ? p1Manager.GetCharacterName(1) : "",
                p2Id = "P2",
                p2Character = p2Manager ? p2Manager.GetCharacterName(1) : "",

                winnerId = "",
                winnerCharacter = ""
            });

            TelemetryManager.Instance?.EndSession("RoundEnded_Tie");
            roundTelemetryClosed = true;
        }

        if (playerNum == 1)
        {
            player1Wins--;
        }
        else
        {
            player2Wins--;
        }

        ActivateIndicators();
        CheckForRandomCharacters();
        StartCoroutine(WaitAndrestart());
        roundOn = false;
    }

    public void RoundEndFlawless(int playerNum, string winnerName)
    {

        if (trainingMode)
        {
            SoftResetRound(playerNum);
            return;
        }

        winner.gameObject.SetActive(true);
        DisableGamePlay();
        winner.text = "FLAWLESS\n" + winnerName + " prevails!";

        ShortWins(playerNum, winnerName);

        if (!roundTelemetryClosed)
        {
            string p1Char = p1Manager ? p1Manager.GetCharacterName(1) : "";
            string p2Char = p2Manager ? p2Manager.GetCharacterName(1) : "";
            string winnerId = (playerNum == 1) ? "P1" : "P2";
            string winnerCharacter = (playerNum == 1) ? p1Char : p2Char;

            // Finalize winner metadata immediately before writing telemetry.
            TelemetryManager.Instance?.SetMatchMeta(new TelemetryMatchMeta
            {
                map = stageName,
                mode = trainingMode ? "training" : "1v1",
                roundNumber_ = roundCounter,
                trainingMode = trainingMode,

                p1Id = "P1",
                p1Character = p1Char,
                p2Id = "P2",
                p2Character = p2Char,

                winnerId = winnerId,
                winnerCharacter = winnerCharacter
            });

            TelemetryManager.Instance?.EndSession($"RoundEnded_Flawless_winner={winnerName}");
            roundTelemetryClosed = true;
        }

        StartCoroutine(WaitAndCheck(playerNum, winnerName));
        roundOn = false;
    }

    public void ShortWins(int playerNum, string winnerName)
    {
        if (playerNum == 1)
        {
            player1Wins++;
        }
        else if (playerNum == 2)
        {
            {
                player2Wins++;
            }
        }
        ActivateIndicators();
    }

    public void CheckForEnd(int playerNum, string winnerName)
    {
        if (tie && roundNumber != 1)
        {
            tie = false;
            return;
        }

        if (player1Wins > roundNumber / 2 || player2Wins > roundNumber / 2 || roundNumber==1)
        {
            if (trainingMode)//training
            {
                // Training rounds reset without displaying end-of-match UI.
                SoftResetRound(playerNum);
                return;
            }

            finalWinner.text = "Victory belongs to " + winnerName + "!\n Chan Chan smiles...";
            winner.gameObject.SetActive(false);
            finalWinner.gameObject.SetActive(true);
            roundCounter = 1;
            player1Wins = 0;
            player2Wins = 0;

            audioManager.PlaySFX(audioManager.dramaticDrums, audioManager.doubleVol);

            gameEnd = true;
            if (victoryScreenNavigation != null)
                victoryScreenNavigation.SetActive(true);

            playAgainButton.SetActive(true);
            mainMenuButton.SetActive(true);
            saveReplayButton.SetActive(true);

            CheckForRandomCharacters();
        }
        else
        {
            if (trainingMode) //training
            {
                SoftResetRound(playerNum);
                return;
            }

            roundCounter++;
            CheckForRandomCharacters();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }



    private IEnumerator WaitAndCheck(int playerNum, string winnerName)
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);


        // Call the ShortWins method after the delay
        CheckForEnd(playerNum, winnerName);
    }

    private IEnumerator WaitAndrestart()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        tie = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public int GetRoundCounter()
    {
        return roundCounter;
    }

    public void EnableGamePlay()
    {
        p1Manager.Resume();
        p2Manager.Resume();
    }

    public void DisableGamePlay()
    {
        p1Manager.Pause();
        p2Manager.Pause();
    }

    void ActivateIndicators()
    {
        if (player1Wins == 1)
        {
            p1R1.SetActive(true);
        }
        if (player2Wins == 1)
        {
            p2R1.SetActive(true);
        }
        if (player1Wins == 2)
        {
            p1R1.SetActive(true);
            p1R2.SetActive(true);
        }
        if (player2Wins == 2)
        {
            p2R1.SetActive(true);
            p2R2.SetActive(true);
        }
        if (player1Wins == 3)
        {
            p1R1.SetActive(true);
            p1R2.SetActive(true);
            p1R3.SetActive(true);
        }
        if (player2Wins == 3)
        {
            p2R1.SetActive(true);
            p2R2.SetActive(true);
            p2R3.SetActive(true);
        }
    }

    public void CheckForRandomCharacters()
    {
        if (PlayerPrefs.GetString("Player1Choice") == "Random" && roundCounter > 1)
        {
            c1Name = p1Manager.GetCharacterName(1);
            PlayerPrefs.SetString("Player1Choice", c1Name);
            p1Random = true;
        }

        if (PlayerPrefs.GetString("Player2Choice") == "Random" && roundCounter > 1)
        {
            c2Name = p2Manager.GetCharacterName(1);
            PlayerPrefs.SetString("Player2Choice", c2Name);
            p2Random = true;
        }

        if (p1Random && gameEnd)
        {
            PlayerPrefs.SetString("Player1Choice", "Random");
        }

        if (p2Random && gameEnd)
        {
            PlayerPrefs.SetString("Player2Choice", "Random");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (PvESelectionState.IsRLAgentDemo && Input.GetKeyDown(KeyCode.Backspace))
        {
            ReturnToRLAgentDemoMenu();
            return;
        }

        // Restart anytime during normal gameplay (not training)
        if (!trainingMode && Input.GetKeyDown(KeyCode.Return))
        {
            QuickRestart();
        }
    }

    private void ReturnToRLAgentDemoMenu()
    {
        tie = false;
        gameEnd = false;
        roundCounter = 1;
        player1Wins = 0;
        player2Wins = 0;
        Time.timeScale = 1f;

        TelemetryManager.Instance?.EndSession("ReturnedToRLAgentDemoMenu");
        SceneManager.LoadScene("RLAgentDemo");
    }

    private void QuickRestart()
    {
        // Reset basic state
        tie = false;
        gameEnd = false;
        roundCounter = 1;
        player1Wins = 0;
        player2Wins = 0;

        if (trainingMode)
        {
            SoftResetRound();
            return;
        }
        // IMPORTANT: reset telemetry properly (optional but cleaner)
        TelemetryManager.Instance?.EndSession("ManualRestart");

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ResetStatics()
    {
        roundCounter = 1;
        player1Wins = 0;
        player2Wins = 0;
    }

    // Training
    public void SoftResetRound(int winnerPlayerNum = 0)
    {
        if (trainingRoundOn)
        {
            trainingRoundOn = false;
            StartCoroutine(SoftResetRound_Co());
        }
        
    }

    private IEnumerator SoftResetRound_Co()
    {
        DisableGamePlay();
        // Hide UI
        if (victoryScreenNavigation != null)
            victoryScreenNavigation.SetActive(false);
        winner.gameObject.SetActive(false);
        finalWinner.gameObject.SetActive(false);
        playAgainButton.SetActive(false);
        mainMenuButton.SetActive(false);
        saveReplayButton.SetActive(false);

        tie = false;
        gameEnd = false;

        if (trainingMode)
        {
            // End both active episodes before changing training state.
            if (agentP1 != null && agentP1.enabled)
            {
                agentP1.ApplyTerminalReward();
                agentP1.DebugEndEpisode("SOFT_RESET");
                agentP1.EndEpisode();
            }

            if (agentP2 != null && agentP2.enabled)
            {
                agentP2.ApplyTerminalReward();
                agentP2.DebugEndEpisode("SOFT_RESET");
                agentP2.EndEpisode();
            }
                
            // Allow animation, coroutine, and deferred destruction state to settle.
            yield return null;

            // Select the opponent mode for the next episode.
            if (opponentDirector != null)
                opponentDirector.PrepareNextEpisode();

            yield return null;

            // Reroll both characters and wait for completion.
            if (p1Manager) yield return StartCoroutine(p1Manager.RerollRandomCharacter_TrainingOnly_Co());
            if (p2Manager) yield return StartCoroutine(p2Manager.RerollRandomCharacter_TrainingOnly_Co());

            yield return null;

            // Rebind opponents after both character swaps.
            var p1 = p1Manager ? p1Manager.GetCurrentCharacter() : null;
            var p2 = p2Manager ? p2Manager.GetCurrentCharacter() : null;
            if (p1 && p2) p1.ChangeEnemy(p2);
            if (p2 && p1) p2.ChangeEnemy(p1);

            if (opponentDirector != null)
                opponentDirector.RebindAfterCharacterSwap();

            yield return null;
        }

        // Resolve the current character instances after any training reroll.
        var c1 = p1Manager ? p1Manager.GetCurrentCharacter() : null;
        var c2 = p2Manager ? p2Manager.GetCurrentCharacter() : null;


        // Reset both characters for the next round.
        if (c1) c1.ResetForEpisode2();
        if (c2) c2.ResetForEpisode2();


        // Re-enable gameplay after reset completes.
        EnableGamePlay();

    }
}
