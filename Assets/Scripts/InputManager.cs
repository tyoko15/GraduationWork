using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private InputSystem_Actions inputActions;

    public Vector2 move { get; private set; }
    public bool jump { get; private set; }

    public bool click { get; private set; }

    //public void OnMove(InputValue value) => move = value.Get<Vector2>();
    //public void OnJump(InputValue value) => jump = value.isPressed;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Move.started += OnMove;
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.started += OnJump;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Jump.canceled += OnJump;
        inputActions.Player.Click.started += OnClick;
        inputActions.Player.Click.performed += OnClick;
        inputActions.Player.Click.canceled += OnClick;
    }
    private void OnEnable()
    {
        // “ü—Í‚ð—LŒø‰»
        inputActions.Enable();
    }
    private void OnDisable()
    {
        // “ü—Í‚ð–³Œø‰»
        inputActions.Disable();
    }
    public void OnMove(InputAction.CallbackContext context)
    {    
        move = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jump = true;
        }
        if (context.performed)
        {
        }
        if (context.canceled)
        {
            jump = false;
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            click = true;
        }
        if (context.performed)
        {

        }
        if (context.canceled)
        {
            click = false;
        }
    }
}
