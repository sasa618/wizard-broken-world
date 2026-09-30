using UnityEngine;

public class TurretSlimeEnemy : SlimeEnemyBase
{
    protected override bool CanTurnOnEnemyContact => false;

    protected override void UpdateAlive()
    {
        rb.linearVelocityX = 0f;

        TickFireTimer();
    }
}
