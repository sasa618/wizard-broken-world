using UnityEngine;

public class ShooterSlimeEnemy : SlimeEnemyBase
{
    protected override void UpdateAlive()
    {
        MoveWithTurn();
        TickFireTimer();
    }
}
