using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Text healthText;
    private PlayerStatsManager playerStats;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GameObject.FindWithTag("GameManager").GetComponent<PlayerStatsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerStats.health);
        healthText.text = playerStats.health.ToString();
    }
}
