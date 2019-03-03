using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Text healthText;
    private PlayerStatsManager playerStats;

    void Start()
    {
        playerStats = GameObject.FindWithTag("GameManager").GetComponent<PlayerStatsManager>();
    }

    void Update()
    {
        healthText.text = playerStats.health.ToString();
    }
}
