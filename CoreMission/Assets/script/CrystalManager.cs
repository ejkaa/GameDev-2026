using UnityEngine;
using TMPro;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance;

    public int totalCrystals = 3;
    public int collectedCrystals = 0;
    public TMP_Text crystalText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCrystal()
    {
        collectedCrystals++;
        UpdateUI();
    }

    public int GetCrystal()
    {
        return collectedCrystals;
    }
    
    void UpdateUI()
    {
        if (crystalText != null)
        {
            crystalText.text = "Crystals: " + collectedCrystals + "/" + totalCrystals;
        }
    }
}