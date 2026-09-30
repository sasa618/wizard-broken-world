using UnityEngine;
using UnityEngine.InputSystem;

public class RepairDebugKey : MonoBehaviour
{
    [SerializeField] private RepairableVisualSwitcher target;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            target.SetRepaired();
        }

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            target.SetBroken();
        }
    }
}