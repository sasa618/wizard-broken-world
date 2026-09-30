using UnityEngine;

//プレイヤーのHPをHPバーに反映させるスクリプト（HPバーのUIにアタッチする）
//プレイヤーはPlayerSpawnerが実行時に生成するのでInspectorから参照できない。
//そのためPlayerHPのイベントを購読して受け取る
[RequireComponent(typeof(HealthBarView))]
public class PlayerHPBar : MonoBehaviour
{
    private HealthBarView view;

    private void Awake()
    {
        view = GetComponent<HealthBarView>();
    }

    private void OnEnable()
    {
        PlayerHP.OnHPChanged += HandleHPChanged;
    }

    private void OnDisable()
    {
        PlayerHP.OnHPChanged -= HandleHPChanged;
    }

    private void HandleHPChanged(float current, float max)
    {
        view.SetHealth(current, max);
    }
}
