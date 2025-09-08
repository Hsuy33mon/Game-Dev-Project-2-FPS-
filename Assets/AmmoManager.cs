using UnityEngine;
using TMPro;

public class AmmoManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static AmmoManager Instance { get; set; }

    // UI
    public TextMeshProUGUI ammoDisplay;

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
