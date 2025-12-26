using UnityEngine;

public class SniperEnemy : Enemy
{
    public float aimTime = 2.5f;
    private float aimTimer = 0f;

    protected override void CombatBehaviour()
    {
        agent.isStopped = true;

        if (!HasLineOfFire()) return;

        aimTimer -= Time.deltaTime;
        if (aimTimer <= 0f)
        {
            aimTimer = aimTime;
            Shoot();
        }
    }

    void Shoot()
    {
        weapon.TryShoot(player.position);
        Debug.Log("Sniper fires!");
    }
}
