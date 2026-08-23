using UnityEngine;


public class CharacterMLProfileDatabase : MonoBehaviour
{
    public static CharacterMLProfileDatabase Instance;

    [SerializeField] private CharacterMLProfile[] profiles;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public CharacterMLProfile GetProfileByID(int id)
    {
        foreach (var p in profiles)
        {
            if (p.characterID == id)
                return p;
        }

        Debug.LogWarning($"No ML profile found for character ID {id}");
        return null;
    }
}