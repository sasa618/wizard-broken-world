using UnityEngine;

public class FastSlimeEnemy : SlimeEnemyBase
{
    protected override void UpdateAlive()
    {
        MoveWithTurn();
    }
}
