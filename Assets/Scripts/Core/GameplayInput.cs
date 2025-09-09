using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayInput : MonoBehaviour
{
    [SerializeField] private InputRouter inputRouter;
    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Gameplay.Enable();
        inputActions.Gameplay.MoveLeft.performed += _ => inputRouter.MoveLeft();
        inputActions.Gameplay.MoveRight.performed += _ => inputRouter.MoveRight();
        inputActions.Gameplay.SoftDrop.performed += _ => inputRouter.SoftDrop();
        inputActions.Gameplay.HardDrop.performed += _ => inputRouter.HardDrop();
        if (inputActions.Gameplay.RotateCW != null)
            inputActions.Gameplay.RotateCW.performed += _ => inputRouter.RotateCW();
        if (inputActions.Gameplay.RotateCCW != null)
            inputActions.Gameplay.RotateCCW.performed += _ => inputRouter.RotateCCW();
    }

    void OnDisable()
    {
        inputActions.Gameplay.Disable();
    }

    void OnDestroy()
    {
        inputActions?.Dispose();
    }
}