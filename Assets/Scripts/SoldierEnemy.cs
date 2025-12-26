using UnityEngine;

public class SoldierEnemy : Enemy
{
    public float fireRate = 0.3f;
    public float minCombatDistance = 15f;
    private float fireTimer = 0f;

    protected override void CombatBehaviour()
    {
        if (agent.remainingDistance < 0.5f && Quaternion.Angle(Quaternion.LookRotation(player.position - transform.position), Quaternion.LookRotation(transform.forward)) < 2f) agent.isStopped = true;
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        if (!HasLineOfFire()) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if(Quaternion.Angle(Quaternion.LookRotation(player.position - transform.position), Quaternion.LookRotation(transform.forward)) < 10f)
            weapon.TryShoot(player.position);
        Debug.Log("Soldier fires");
    }
}
