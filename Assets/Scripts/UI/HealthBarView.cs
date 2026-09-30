using TMPro;
using UnityEngine;
using UnityEngine.UI;

//HPバーの見た目だけを担当するスクリプト（プレイヤー用にもボス用にも使う）
//誰のHPかは知らないので、外から SetHealth を呼んでもらう
public class HealthBarView : MonoBehaviour
{
    [Header("HPを表すバー（Image Type = Filled / Horizontal）")]
    [SerializeField] private Image fillImage;

    [Header("減った分を遅れて追いかけるバー（未設定でも動く）")]
    [SerializeField] private Image delayedImage;

    [Header("「78 / 100」のように数値を出すテキスト（未設定でも動く）")]
    [SerializeField] private TMP_Text valueText;

    [Header("遅延バーが動き出すまでの待ち時間（秒）")]
    [SerializeField] private float delayedWaitTime = 0.3f;

    [Header("遅延バーが減る速さ（1秒あたりに減る割合）")]
    [SerializeField] private float delayedSpeed = 0.5f;

    [Header("この割合以下になったら点滅する")]
    [SerializeField, Range(0f, 1f)] private float blinkThreshold = 0.3f;

    [Header("点滅の速さ")]
    [SerializeField] private float blinkSpeed = 4f;

    [Header("通常時のバーの色")]
    [SerializeField] private Color normalColor = new Color(0.36f, 0.62f, 0.35f, 1f);

    [Header("HPが少ないときのバーの色")]
    [SerializeField] private Color lowColor = new Color(0.76f, 0.29f, 0.26f, 1f);

    //今のHPの割合（0～1）
    private float currentRatio = 1f;

    //遅延バーの割合（currentRatioを追いかける）
    private float delayedRatio = 1f;

    //遅延バーが動き出すまでの残り時間
    private float waitTimer;

    //HPが変わったときに呼ぶ
    public void SetHealth(float current, float max)
    {
        float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        if (ratio >= currentRatio)
        {
            //回復したときは遅延バーもすぐ合わせる
            delayedRatio = ratio;
        }
        else
        {
            //ダメージを受けたときは少し待ってから遅延バーを減らす
            waitTimer = delayedWaitTime;
        }

        currentRatio = ratio;

        SetBarLength(fillImage, currentRatio);

        if (valueText != null)
        {
            //小数だと読みにくいので切り上げた整数で出す
            valueText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    private void Update()
    {
        UpdateDelayedBar();
        UpdateColor();
    }

    //減った分のバーをゆっくり追いつかせる
    private void UpdateDelayedBar()
    {
        if (delayedImage == null)
        {
            return;
        }

        if (delayedRatio <= currentRatio)
        {
            delayedRatio = currentRatio;
            SetBarLength(delayedImage, delayedRatio);
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
        }
        else
        {
            delayedRatio = Mathf.MoveTowards(
                delayedRatio,
                currentRatio,
                delayedSpeed * Time.deltaTime
            );
        }

        SetBarLength(delayedImage, delayedRatio);
    }

    //バーの長さを割合に合わせる
    //Image.fillAmountだと画像の端が引き伸ばされて透けるので、RectTransformの右アンカーを動かして伸縮させる
    private void SetBarLength(Image image, float ratio)
    {
        if (image == null)
        {
            return;
        }

        RectTransform rect = image.rectTransform;
        Vector2 anchorMax = rect.anchorMax;
        anchorMax.x = ratio;
        rect.anchorMax = anchorMax;
    }

    //HPが少なくなったら赤く点滅させる
    private void UpdateColor()
    {
        if (fillImage == null)
        {
            return;
        }

        if (currentRatio > blinkThreshold)
        {
            fillImage.color = normalColor;
            return;
        }

        //PingPongで0↔1を往復させて明るさを変える
        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        Color darkColor = new Color(
            lowColor.r * 0.4f,
            lowColor.g * 0.4f,
            lowColor.b * 0.4f,
            lowColor.a
        );

        fillImage.color = Color.Lerp(darkColor, lowColor, t);
    }
}
