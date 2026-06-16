using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    public Vector2 move { get; private set; }
    public bool jump { get; private set; }

    public void OnMove(InputValue value) => move = value.Get<Vector2>();
    public void OnJump(InputValue value) => jump = value.isPressed;

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("‰Ÿ‚µ‚½uŠÔ");
        }
        if (context.performed)
        {
            Debug.Log("‰Ÿ‚µ‚Ä‚¢‚éŠÔ or ’l‚ª•Ï‰»‚µ‚½uŠÔ");
        }
        if (context.canceled)
        {
            Debug.Log("—£‚µ‚½uŠÔ");
        }

        move = context.ReadValue<Vector2>();
    }
}
