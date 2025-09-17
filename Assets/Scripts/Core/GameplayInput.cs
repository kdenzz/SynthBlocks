using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class GameplayInput : MonoBehaviour
{
    [SerializeField] private InputRouter inputRouter;
    private InputSystem_Actions inputActions;
    private bool useNewInputSystem = false;
    private bool isMultiplayer = false;

    void Awake()
    {
        Debug.Log("GameplayInput: Awake called");
        
        // Check if we're in multiplayer mode
        isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
        Debug.Log($"GameplayInput: isMultiplayer = {isMultiplayer}");
        
        // Try to initialize the new Input System
        try
        {
            inputActions = new InputSystem_Actions();
            useNewInputSystem = true;
            Debug.Log("GameplayInput: InputSystem_Actions created successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"GameplayInput: Failed to create InputSystem_Actions: {e.Message}. Using fallback input.");
            useNewInputSystem = false;
        }
    }

    void OnEnable()
    {
        // Find the correct InputRouter for this player
        FindCorrectInputRouter();
        
        if (useNewInputSystem && inputActions != null)
        {
            Debug.Log("GameplayInput: Enabling new input system...");
            try
            {
                inputActions.Gameplay.Enable();
                
                // Add debugging to see if input is being received
                inputActions.Gameplay.MoveLeft.performed += _ => {
                    Debug.Log("New Input System: MoveLeft");
                    if (inputRouter != null) inputRouter.MoveLeft();
                };
                inputActions.Gameplay.MoveRight.performed += _ => {
                    Debug.Log("New Input System: MoveRight");
                    if (inputRouter != null) inputRouter.MoveRight();
                };
                inputActions.Gameplay.SoftDrop.performed += _ => {
                    Debug.Log("New Input System: SoftDrop");
                    if (inputRouter != null) inputRouter.SoftDrop();
                };
                inputActions.Gameplay.HardDrop.performed += _ => {
                    Debug.Log("New Input System: HardDrop");
                    if (inputRouter != null) inputRouter.HardDrop();
                };
                if (inputActions.Gameplay.RotateCW != null)
                {
                    inputActions.Gameplay.RotateCW.performed += _ => {
                        Debug.Log("New Input System: RotateCW");
                        if (inputRouter != null) inputRouter.RotateCW();
                    };
                }
                if (inputActions.Gameplay.RotateCCW != null)
                {
                    inputActions.Gameplay.RotateCCW.performed += _ => {
                        Debug.Log("New Input System: RotateCCW");
                        if (inputRouter != null) inputRouter.RotateCCW();
                    };
                }
                
                Debug.Log("GameplayInput: New input system enabled successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"GameplayInput: Failed to enable new input system: {e.Message}. Using fallback input.");
                useNewInputSystem = false;
            }
        }
        else
        {
            Debug.Log("GameplayInput: Using fallback input system");
        }
    }

    void OnDisable()
    {
        if (useNewInputSystem && inputActions != null)
        {
            try
            {
                inputActions.Gameplay.Disable();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"GameplayInput: Error disabling input system: {e.Message}");
            }
        }
    }

    void Update()
    {
        // Find the correct InputRouter periodically in multiplayer
        if (isMultiplayer && inputRouter == null)
        {
            FindCorrectInputRouter();
        }
        
        // Only use fallback input if new Input System is not available
        if (!useNewInputSystem)
        {
            // Fallback input using old Input class
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("Fallback input: MoveLeft");
                if (inputRouter != null) inputRouter.MoveLeft();
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Debug.Log("Fallback input: MoveRight");
                if (inputRouter != null) inputRouter.MoveRight();
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("Fallback input: SoftDrop");
                if (inputRouter != null) inputRouter.SoftDrop();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Fallback input: HardDrop");
                if (inputRouter != null) inputRouter.HardDrop();
            }
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("Fallback input: RotateCW");
                if (inputRouter != null) inputRouter.RotateCW();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Fallback input: RotateCCW");
                if (inputRouter != null) inputRouter.RotateCCW();
            }
        }
    }
    
    private void FindCorrectInputRouter()
    {
        if (inputRouter != null) return; // Already found
        
        Debug.Log("GameplayInput: Searching for correct InputRouter...");
        
        if (isMultiplayer)
        {
            // In multiplayer, find the InputRouter that belongs to this client
            var allRouters = FindObjectsByType<InputRouter>(FindObjectsSortMode.None);
            Debug.Log($"GameplayInput: Found {allRouters.Length} InputRouter objects");
            
            foreach (var router in allRouters)
            {
                Debug.Log($"GameplayInput: Checking InputRouter on {router.gameObject.name}");
                
                // Check if this router has a local grid (single player) or is for multiplayer
                if (router.GetComponent<InputRouter>() != null)
                {
                    // Check if this router is assigned to the current client's grid
                    var gridManager = router.GetComponent<InputRouter>().GetComponentInChildren<GridManager>();
                    if (gridManager != null)
                    {
                        Debug.Log($"GameplayInput: Found InputRouter with GridManager: {gridManager.gameObject.name}");
                        inputRouter = router;
                        Debug.Log($"GameplayInput: Assigned InputRouter: {inputRouter.gameObject.name}");
                        return;
                    }
                }
            }
            
            // Fallback: use any InputRouter if we can't find the right one
            if (allRouters.Length > 0)
            {
                inputRouter = allRouters[0];
                Debug.LogWarning($"GameplayInput: Using fallback InputRouter: {inputRouter.gameObject.name}");
            }
        }
        else
        {
            // Single player - use the assigned router or find one
            if (inputRouter == null)
            {
                inputRouter = FindObjectOfType<InputRouter>();
                if (inputRouter != null)
                {
                    Debug.Log($"GameplayInput: Found single player InputRouter: {inputRouter.gameObject.name}");
                }
            }
        }
        
        if (inputRouter == null)
        {
            Debug.LogError("GameplayInput: No InputRouter found!");
        }
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            try
            {
                inputActions.Dispose();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"GameplayInput: Error disposing input actions: {e.Message}");
            }
        }
    }
}