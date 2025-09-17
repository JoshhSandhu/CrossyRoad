using System;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Rendering;

public class PlayerControler : MonoBehaviour
{
    public static event Action OnPlayerMovedForward;  //player moved forward event

    //private feilds
    private Vector3 targetPosition;
    private float moveSpeed = 10f; //speed of movement
    private int forwardPosZ = 0; //to track the forward position of the player

    //for the new input system var
    private Vector2 touchStartPos;
    private float minSwipeDist = 50f; //minimum distance for a swipe to be registered
    private PlayerInputActions playerInputActions;

    //subscribe the events
    private void OnEnable()
    {
        if (playerInputActions == null)
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Move.performed += OnMove;
            playerInputActions.Player.PrimaryTouch.started += ctx => OnTouchStart(ctx);
            playerInputActions.Player.PrimaryTouch.canceled += ctx => OnTouchEnd(ctx);
        }
        playerInputActions.Enable();
    }

    //unsubscribe the events
    private void OnDisable()
    {
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.PrimaryTouch.started -= OnTouchStart;
        playerInputActions.Player.PrimaryTouch.canceled -= OnTouchEnd;
        playerInputActions.Player.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if(Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
        return;
        }
        Vector2 inputDirection = context.ReadValue<Vector2>();


        //vertical movement takes priority over horizontal movement
        if (Mathf.Abs(inputDirection.y) > Mathf.Abs(inputDirection.x))
        {
            if(inputDirection.y > 0)
            {
                movePlayer(Vector3.forward);
            }
            else if(inputDirection.y < 0)
            {
                movePlayer(Vector3.back);
            }
        }
        else if(Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))  //remove this condition to allow diagonal movement
        {
            if(inputDirection.x > 0)
            {
                movePlayer(Vector3.right);
            }
            else if(inputDirection.x < 0)
            {
                movePlayer(Vector3.left);
            }
        }
    }

    private void OnTouchStart(InputAction.CallbackContext context)
    {
        touchStartPos = playerInputActions.Player.PrimaryContact.ReadValue<Vector2>();
    }

    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        //only accept input if the player is not moving
        if ( Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            return;
        }
        Vector2 touchEndPos = playerInputActions.Player.PrimaryContact.ReadValue<Vector2>();
        ProcessSwipe(touchEndPos);
    }

    private void ProcessSwipe(Vector2 TouchendPos)
    {
        float swipeDistX = Mathf.Abs(TouchendPos.x - TouchendPos.x);
        float swipeDistY = Mathf.Abs(TouchendPos.y - TouchendPos.y);

        if (swipeDistX < minSwipeDist && swipeDistY < minSwipeDist)
        {
            return;
        }
        if (swipeDistX > swipeDistY)
        {
            if (TouchendPos.x > touchStartPos.x)
            {
                movePlayer(Vector3.right);
            }
            else
            {
                movePlayer(Vector3.left);
            }
        }
        else
        {
            if (TouchendPos.y > touchStartPos.y)
            {
                movePlayer(Vector3.forward);
            }
            else
            {
                movePlayer(Vector3.back);
            }
        }
    }

    private void movePlayer(Vector3 direction)
    {
        targetPosition += direction;
        if (direction == Vector3.forward && (int)targetPosition.z > forwardPosZ)
        {
            forwardPosZ = (int)targetPosition.z;
            OnPlayerMovedForward?.Invoke();
        }
    }
}