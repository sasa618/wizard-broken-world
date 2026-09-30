using UnityEngine;

//その場から動かず、一定間隔で弾を撃ち続けるスライム(弾は向いている方向へまっすぐ飛ぶ)
//aimAtPlayerをONにすると、検知範囲(sightRange)内のプレイヤーへ向きを変え、ゆっくり近づきながら撃つ
//浄化するとバネブロックになる(跳ね返しはSpringBounce、接触ダメージはEnemyContactDamage側で行う)
public class SlimeEnemy : EnemyBase
{
    [SerializeField] private bool aimAtPlayer = false;      //ONにするとプレイヤーの方を向いて撃ち、ゆっくり近づく
    [SerializeField] private bool attackOutOfRange = false; //ONにすると視野(sightRange)外にプレイヤーがいても攻撃を続ける。OFFなら静止して待機
    [SerializeField] private float stopDistance = 1.5f;     //プレイヤーとの横距離がこれ以下なら近づくのをやめる
    [SerializeField] private float cliffCheckOffset = 0.6f; //進行方向のどれだけ先の足元を調べるか
    [SerializeField] private float cliffCheckDistance = 1f; //足元の地面を下方向に探す距離
    [SerializeField] private LayerMask groundMask;          //崖チェックで地面とみなすレイヤー

    private bool playerInRange; //Gizmos表示の色分け用

    protected override bool CanTurnOnEnemyContact => aimAtPlayer;

    protected override void UpdateAlive()
    {
        if(aimAtPlayer)
        {
            UpdateAimBehavior();
        }
        else
        {
            //チェックOFF: その場から動かず、プレイヤーに関係なく一定間隔で撃つ
            TickFireTimer();
        }
    }

    //チェックON: 検知範囲(sightRange)内にプレイヤーがいる間は、向きを変え・近づき・撃つ
    //範囲外にいる間はattackOutOfRangeに応じて「静止」か「その場で攻撃継続」を選べる
    private void UpdateAimBehavior()
    {
        bool playerFound = TryGetPlayerTransform(out Transform player);
        bool inRange = playerFound &&
            Vector2.Distance(player.position, transform.position) <= sightRange;

        playerInRange = inRange;

        if(inRange)
        {
            FacePlayer(player);
            ApproachPlayer(player);
            TickFireTimer();
            return;
        }

        //視野外: 移動はしない。attackOutOfRangeがONのときだけ向きと攻撃を続ける
        rb.linearVelocityX = 0f;

        if(attackOutOfRange)
        {
            if(playerFound)
            {
                FacePlayer(player);
            }

            TickFireTimer();
        }
    }

    private void TickFireTimer()
    {
        fireTimer += Time.deltaTime;
        if(fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shoot();
        }
    }

    //プレイヤーのいる側へ左右の向きを変える(FacingDirectionに合わせてlocalScale.x > 0 が左向き)
    private void FacePlayer(Transform player)
    {
        float offsetX = player.position.x - transform.position.x;

        //ほぼ真上・真下にいるときは向きを変えない(毎フレーム反転するのを防ぐ)
        if(Mathf.Abs(offsetX) < 0.1f) return;

        float scaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(
            offsetX < 0f ? scaleX : -scaleX,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    //停止距離・崖の手前まで、moveSpeedでゆっくりプレイヤーへ近づく(壁は物理衝突で止まる)
    private void ApproachPlayer(Transform player)
    {
        float offsetX = player.position.x - transform.position.x;

        //十分近ければ動かない(攻撃は続ける)
        if(Mathf.Abs(offsetX) <= stopDistance)
        {
            rb.linearVelocityX = 0f;
            return;
        }

        float directionX = Mathf.Sign(offsetX);

        //進行方向の足元に地面がなければ崖なので止まる
        if(!GroundAhead(directionX))
        {
            rb.linearVelocityX = 0f;
            return;
        }

        rb.linearVelocityX = directionX * moveSpeed;
    }

    //進行方向の少し先の足元に地面があるか
    private bool GroundAhead(float directionX)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(directionX * cliffCheckOffset, 0f);
        return Physics2D.Raycast(origin, Vector2.down, cliffCheckDistance, groundMask);
    }

    protected override void TurnAroundForEnemyContact()
    {
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }

    //Gizmos表示ON(デバッグ)のとき、検知範囲の円をSceneビュー/Gameビューに描画する
    private void OnDrawGizmos()
    {
        //検知範囲を使うのはaimAtPlayerがONのときだけ
        if(!aimAtPlayer) return;

        //検知中は赤、それ以外は黄色
        Gizmos.color = playerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
