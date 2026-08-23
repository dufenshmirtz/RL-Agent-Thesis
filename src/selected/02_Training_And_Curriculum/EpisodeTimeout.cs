using UnityEngine;
using Unity.MLAgents;

[DisallowMultipleComponent]
public class EpisodeTimeout : MonoBehaviour
{
    public float maxSeconds = 800f;
    FighterAgent agent;
    GameManager gm;
    float t;

    void Awake()
    {
        agent = GetComponent<FighterAgent>();
        gm = FindObjectOfType<GameManager>();
    }

    void OnEnable() { t = 0f; }

    void Update()
    {
        if (gm != null && !gm.trainingMode) return; // only in training
        t += Time.deltaTime;
        if (t >= maxSeconds)
        {
            if (agent != null)
            {
                Debug.Log("[Timeout] Ending episode due to time limit.");
                agent.EndEpisode();
            }
            t = 0f;
        }
    }
}
