using UnityEngine;

public class PlayerStatsManager : MonoBehaviour {
    public float health = 100;
    
    [Tooltip("After player is attacked, player will start regenning after this many seconds (if he's not attacked anymore).")]
    public float regenWaitAfterDamange = 5;

    [Tooltip("Player regen per second.")]
    public float regenRate = 5;

    private float timeOfLastDamage = 0;
    private float previousFrameHealth;

    private void Update() {
        if(health < previousFrameHealth) {
            timeOfLastDamage = Time.time;
        }
        
        if(Time.time - timeOfLastDamage > regenWaitAfterDamange) {
            health += regenRate * Time.deltaTime;
        }

        if(health > 100) {
            health = 100;
        }

        previousFrameHealth = health;
    }
}
