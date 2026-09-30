using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SoundVolumeSlider : MonoBehaviour
{
    private const float DefaultPreviewCooldown = 0.15f;

    private enum VolumeType
    {
        BGM,
        SE,
    }

    [SerializeField] private VolumeType volumeType;
    [SerializeField] private float sePreviewCooldown = DefaultPreviewCooldown;

    private Slider slider;
    private float lastSEPreviewTime = -DefaultPreviewCooldown;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void OnEnable()
    {
        SyncSliderValue();
        slider.onValueChanged.AddListener(HandleValueChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(HandleValueChanged);
    }

    private void SyncSliderValue()
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        float value = volumeType == VolumeType.BGM
            ? SoundManager.Instance.BGMVolume
            : SoundManager.Instance.SEVolume;

        slider.SetValueWithoutNotify(value);
    }

    private void HandleValueChanged(float value)
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        if (volumeType == VolumeType.BGM)
        {
            SoundManager.Instance.SetBGMVolume(value);
        }
        else
        {
            SoundManager.Instance.SetSEVolume(value);
            PlaySEPreview();
        }
    }

    private void PlaySEPreview()
    {
        if (Time.unscaledTime - lastSEPreviewTime < sePreviewCooldown)
        {
            return;
        }

        lastSEPreviewTime = Time.unscaledTime;
        SoundManager.Instance.PlayUIButtonSE();
    }
}
