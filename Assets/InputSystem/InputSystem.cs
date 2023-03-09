using UnityEngine;
using UnityEngine.InputSystem;


public class InputSystem: MonoBehaviour
{

    public InputActionReference actionReference;

    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public float Switch;
    public bool jump;
    public bool sprint;
    public bool dodge;
    public bool fire;
    public bool fireCharge;
    public bool fireRelease;
    public bool aiming;
    public bool reload;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    private void Awake()
    {
        actionReference.action.performed += _x => Switch = _x.action.ReadValue<float>();
    }
    public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		if(cursorInputForLook)
		{
			LookInput(value.Get<Vector2>());
		}
	}

	public void OnJump(InputValue value)
	{
		JumpInput(value.isPressed);
	}

	public void OnSprint(InputValue value)
	{
		SprintInput(value.isPressed);
	}

    public void OnDodge(InputValue value)
    {
        DodgeInput(value.isPressed);
    }

    public bool OnFire(InputValue value)
    {
        FireInput(value.isPressed);
        if (value.isPressed)
            return true;
        else return false;
    }

    public void OnFireCharge(InputValue value)
    {
        FireChargeInput(value.isPressed);
    }

    public void OnFireRelease(InputValue value)
    {
        FireReleaseInput(value.isPressed);
    }

    public void OnAiming(InputValue value)
    {
        AimingInput(value.isPressed);
    }
    public void OnReload(InputValue value)
    {
        ReloadInput(value.isPressed);
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }
    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }
    
    public void DodgeInput(bool newDodgeState)
    {
        dodge = newDodgeState;
    }
    public void FireInput(bool newFireState)
    {
        fire = newFireState;
    }

    public void FireChargeInput(bool newFireState)
    {
        fireCharge = newFireState;
    }

    public void FireReleaseInput(bool newFireState)
    {
        fireRelease = newFireState;
    }
    public void AimingInput(bool newAimingState)
    {
        aiming = newAimingState;
    }
    public void ReloadInput(bool newReloadState)
    {
        reload = newReloadState;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public int GetSwitchWeaponInput()
    {
        if (Switch > 0f)
            return -1;
        else if (Switch < 0f) 
            return 1;
        else
            return 0;
    }

    public int GetSelectWeaponInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            return 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            return 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            return 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            return 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            return 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            return 6;
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            return 7;
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            return 8;
        else if (Input.GetKeyDown(KeyCode.Alpha9))
            return 9;
        else
            return 0;
    }
}
