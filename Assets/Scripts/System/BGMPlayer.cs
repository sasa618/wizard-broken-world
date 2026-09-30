using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private BGM _bgm;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SwitchBGM();
    }

    private void Start()
    {
        SwitchBGM();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SwitchBGM()
    {
        SoundManager.Instance?.SwitchBGM(_bgm);
    }
}
