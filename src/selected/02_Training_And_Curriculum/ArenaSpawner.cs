using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
    public bool enableForTraining = true;
    public GameObject arenaPrefab;
    public int rows = 1, cols = 3;
    public Vector2 spacing = new Vector2(30f, 18f);

    public GameManager gm;             // drag it in

    void Start()
    {
        if (!enableForTraining || gm == null || !gm.trainingMode) return;

        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            var pos = new Vector3(c * spacing.x, r * spacing.y, 0f);
            Instantiate(arenaPrefab, pos, Quaternion.identity, transform);
        }
    }
}
