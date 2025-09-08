using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static SoundManager Instance { get; set; }

    public AudioSource shootingSoundM1911;
    public AudioSource reloadingSoundM1911;
    public AudioSource emptyMagazineSoundM1911;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}
