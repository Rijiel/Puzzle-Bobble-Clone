using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    PlayerInputActions playerInputActions;

    public event Action OnClickAction;

    private void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Shoot.performed += Click_performed;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Shoot.performed -= Click_performed;
        playerInputActions.Dispose();
    }

    private void Click_performed(InputAction.CallbackContext context)
    {
        OnClickAction?.Invoke();
    }

    public float GetInputFloat()
    {
        float inputFloat = playerInputActions.Player.Rotate.ReadValue<float>();
        return inputFloat;
    }
}
