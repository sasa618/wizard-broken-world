using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverTest : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private GameOverPanelManager gameoverManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            gameoverManager.GameOver();
        }
    }
}
