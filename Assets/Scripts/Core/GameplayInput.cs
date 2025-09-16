using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayInput : MonoBehaviour
{
    [SerializeField] private InputRouter inputRouter;
    private InputSystem_Actions inputActions;

    void Awake()
    {
        Debug.Log("GameplayInput: Awake called");
        inputActions = new InputSystem_Actions();
        Debug.Log("GameplayInput: InputSystem_Actions created successfully");
    }

    void OnEnable()
    {
        Debug.Log("GameplayInput: Enabling input actions...");
        inputActions.Gameplay.Enable();
        
        // Add debugging to see if input is being received
        inputActions.Gameplay.MoveLeft.performed += _ => {
            Debug.Log("Input received: MoveLeft");
            inputRouter.MoveLeft();
        };
        inputActions.Gameplay.MoveRight.performed += _ => {
            Debug.Log("Input received: MoveRight");
            inputRouter.MoveRight();
        };
        inputActions.Gameplay.SoftDrop.performed += _ => {
            Debug.Log("Input received: SoftDrop");
            inputRouter.SoftDrop();
        };
        inputActions.Gameplay.HardDrop.performed += _ => {
            Debug.Log("Input received: HardDrop");
            inputRouter.HardDrop();
        };
        if (inputActions.Gameplay.RotateCW != null)
        {
            inputActions.Gameplay.RotateCW.performed += _ => {
                Debug.Log("Input received: RotateCW");
                inputRouter.RotateCW();
            };
        }
        if (inputActions.Gameplay.RotateCCW != null)
        {
            inputActions.Gameplay.RotateCCW.performed += _ => {
                Debug.Log("Input received: RotateCCW");
                inputRouter.RotateCCW();
            };
        }
        
        Debug.Log("GameplayInput: Input actions enabled successfully");
    }

    void OnDisable()
    {
        inputActions.Gameplay.Disable();
    }

    void Update()
    {
        // Fallback input using old Input class for builds
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Fallback input: MoveLeft");
            inputRouter.MoveLeft();
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Fallback input: MoveRight");
            inputRouter.MoveRight();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("Fallback input: SoftDrop");
            inputRouter.SoftDrop();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Fallback input: HardDrop");
            inputRouter.HardDrop();
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Fallback input: RotateCW");
            inputRouter.RotateCW();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Fallback input: RotateCCW");
            inputRouter.RotateCCW();
        }
    }

    void OnDestroy()
    {
        inputActions?.Dispose();
    }
}