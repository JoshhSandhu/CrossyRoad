using System;
using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public static event Action OnPlayerMovedForward;  //player moved forward event

    // public feilds
    [Tooltip("hight of the player hops")]
    [SerializeField]
    private float hopHeight = 0.5f;
    [Tooltip("duration of the player hops")]
    [SerializeField]
    private float hopDuration = 0.2f;

    public static event Action<int> OnScoreChanged; //event for score change

    //private feilds
    private Rigidbody playerRb;
    private bool isMoving = false; //to check if the player is moving
    //private Vector3 targetPosition;
    //private float moveSpeed = 10f; //speed of movement
    private int forwardPosZ = 0; //to track the forward position of the player

    //for the new input system var
    private Vector2 touchStartPos;
    private float minSwipeDist = 50f; //minimum distance for a swipe to be registered
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }
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

    private void OnMove(InputAction.CallbackContext context)
    {
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
        if (isMoving)
        {
            return;
        }

        Vector3 destination = transform.position + direction;
        StartCoroutine(HopCoroutine(destination));
    
        if (direction == Vector3.forward && (int)destination.z > forwardPosZ)
        {
            forwardPosZ = (int)destination.z;
            OnPlayerMovedForward?.Invoke();
            OnScoreChanged?.Invoke(forwardPosZ);
        } 
    }

    private IEnumerator HopCoroutine(Vector3 destination)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < hopDuration)
        {
            Vector3 newPosition = Vector3.Lerp(startPos, destination, (elapsedTime / hopDuration)); //lerp is to handel the movement start -> finish 

            //to make the player jump and come down
            float offsetY = hopHeight * 4 * (elapsedTime/hopDuration) * ( 1 - (elapsedTime/hopDuration)); //claculates the height using a simple parabola eq
            playerRb.MovePosition(newPosition + new Vector3(0, offsetY, 0));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerRb.MovePosition(destination);
        isMoving = false;
    }

    //game over logic
    private void GameOver()
    {
        Debug.Log("Game Over!");
        // The method in your UIManager is likely named ShowGameOverPanel, based on previous scripts
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
        this.enabled = false; // Disable this script to stop movement
    }
    //collision detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameOver();
        }
        else if (other.CompareTag("Coin"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins();
            }
            other.gameObject.SetActive(false);
        }
    }

    // This is for physical Collisions (like trees and rocks) that only block you
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bumped into a solid object: " + collision.gameObject.name);
    }
}