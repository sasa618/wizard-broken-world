using System.Collections;
using UnityEngine;

//左右に浮遊しながら巡回し、扇状の視界にプレイヤーが入ると止まって一定間隔で撃ち続けるゴースト
//修理(浄化)されると昇天(上にふわふわ浮きながらフェードアウト)して消える
public class GhostEnemy : EnemyBase
{
    [SerializeField] private float patrolDistance = 3f;    //開始位置から左右にどこまで動くか
    [SerializeField] private float sightAngle = 90f;       //扇状視界の全体角度(向いている方向が中心)
    [SerializeField] private float firstShotDelay = 0.5f;  //視界に入ってから1発目を撃つまでの秒数
    [SerializeField] private float ascendSpeed = 1.5f;     //昇天の上昇速度
    [SerializeField] private float disappearDuration = 2f; //昇天してから消えるまでの秒数
    [SerializeField] private ParticleSystem repairedParticle; //浄化パーティクル(昇天時にその場へ切り離す)

    private float startX;
    private bool wasPlayerInSight;

    protected override bool CanTurnOnEnemyContact => true;

    protected override void Start()
    {
        base.Start();
        startX = transform.position.x;
    }

    protected override void UpdateAlive()
    {
        //視界に入った瞬間から数えてfirstShotDelay秒後に1発目が出るようタイマーを合わせる
        bool inSight = PlayerInSight();
        if(inSight && !wasPlayerInSight)
        {
            fireTimer = fireInterval - firstShotDelay;
        }
        wasPlayerInSight = inSight;

        base.UpdateAlive();
    }

    //扇状の視界判定(距離+角度+地形の遮蔽)
    protected override bool PlayerInSight()
    {
        if(!TryGetPlayerTransform(out Transform player)) return false;

        Vector2 origin = muzzle.position;
        Vector2 toPlayer = (Vector2)player.position - origin;
        float distance = toPlayer.magnitude;

        if(distance > sightRange || distance <= 0f) return false;

        //向いている方向を中心とした扇の外なら見えない
        if(Vector2.Angle(FacingDirection, toPlayer) > sightAngle * 0.5f) return false;

        //間に地形があると見えない
        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer / distance, distance, sightMask);
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    //開始位置から一定距離離れたら折り返す
    protected override void Move()
    {
        float offsetX = transform.position.x - startX;

        if((offsetX <= -patrolDistance && FacingDirection.x < 0f) ||
           (offsetX >= patrolDistance && FacingDirection.x > 0f))
        {
            TurnAroundForEnemyContact();
        }

        base.Move();
    }

    protected override void TurnAroundForEnemyContact()
    {
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }

    public override void SetRepaired()
    {
        base.SetRepaired();
        PrepareAscendAndDisappearPhysics();
        StartCoroutine(AscendAndDisappear());
    }

    private void PrepareAscendAndDisappearPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }

    //上にふわふわ浮かびながらフェードアウトし、消滅する
    private IEnumerator AscendAndDisappear()
    {
        //浄化パーティクルは幽霊と一緒に浮き上がらないよう、その場に切り離す
        //(切り離した後は再生が終わる頃に自動で削除する)
        if(repairedParticle != null)
        {
            repairedParticle.transform.SetParent(null, true);

            ParticleSystem.MainModule main = repairedParticle.main;
            Destroy(repairedParticle.gameObject, main.duration + main.startLifetime.constantMax);
        }

        //RepairableVisualSwitcherが浄化後の見た目に切り替えるのを1フレーム待つ
        yield return null;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        float timer = 0f;
        while(timer < disappearDuration)
        {
            timer += Time.deltaTime;
            transform.position += Vector3.up * (ascendSpeed * Time.deltaTime);

            float alpha = Mathf.Clamp01(1f - timer / disappearDuration);
            foreach(SpriteRenderer sr in renderers)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    //Gizmos表示ON(デバッグ)のとき、扇状の視界をSceneビュー/Gameビューに描画する
    private void OnDrawGizmos()
    {
        if(muzzle == null) return;

        //視認中は赤、それ以外は黄色
        Gizmos.color = wasPlayerInSight ? Color.red : Color.yellow;

        Vector3 origin = muzzle.position;
        float halfAngle = sightAngle * 0.5f;
        int segments = 20;

        Vector3 prevPoint = origin;
        for(int i = 0; i <= segments; i++)
        {
            float angle = -halfAngle + sightAngle * i / segments;
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * (Vector3)FacingDirection;
            Vector3 point = origin + direction * sightRange;

            //扇の縁(最初と最後)と弧を線で描く
            if(i == 0 || i == segments)
            {
                Gizmos.DrawLine(origin, point);
            }
            if(i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }
    }
}
