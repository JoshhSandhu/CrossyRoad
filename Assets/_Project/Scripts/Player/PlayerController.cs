using System;
using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public static event Action OnPlayerMovedForward;  //player moved forward event
    public static event Action<int> OnScoreChanged;   //event for score change

    [Header("Dependancies")]
    [SerializeField]
    private CamFollow cameraFollow;

    // public feilds
    [Tooltip("hight of the player hops")]
    [SerializeField]
    private float hopHeight = 0.5f;

    [Tooltip("duration of the player hops")]
    [SerializeField]
    private float hopDuration = 0.2f;

    [SerializeField]
    private float xBoundary = 5f;

    //private feilds
    private Rigidbody playerRb;
    private bool isMoving = false;  //to check if the player is moving
    private int forwardPosZ = 0;    //to track the forward position of the player
    private bool isDead = false;    //to keep track if the player is alive or dead

    private Vector2 touchStartPos;      //for the new input system var
    private float minSwipeDist = 50f;   //minimum distance for a swipe to be registered
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

    private void Update()
    {
        CheckForOutOfBounds();
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
        float swipeDistX = Mathf.Abs(TouchendPos.x - touchStartPos.x);
        float swipeDistY = Mathf.Abs(TouchendPos.y - touchStartPos.y);

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
        if (isMoving || isDead)
        {
            return;
        }
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f))
        {
            //if the ray hits a solid obstacle, cancel the move.
            if (hit.collider.CompareTag("SolidObstacles"))
            {
                Debug.Log("Move blocked by a solid obstacle!");
                return; //exit the function, do not hop.
            }
        }
        //Detaching the parent if the player is on a log
        transform.SetParent(null);

        //rounding the position to avoid floating point errors
        Vector3 snappedPos = new Vector3(
            Mathf.Round(transform.position.x),
            0.5f,
            Mathf.Round(transform.position.z)
        );

        Vector3 destination = snappedPos + direction;
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
        RaycastHit landinghit;
        playerRb.MovePosition(destination);
        Physics.Raycast(transform.position, Vector3.down, out landinghit, 1f);
        //if we landed on a log
        if (Physics.Raycast(transform.position, Vector3.down, out landinghit, 2f)) // Increased distance to be safe
        {
            Debug.Log("Landed on: " + landinghit.collider.name + " | Tag: " + landinghit.collider.tag);
            if (landinghit.collider.CompareTag("Platform"))
            {
                // We landed safely on a log. Parent to it.
                transform.SetParent(landinghit.transform);
            }
            else if (landinghit.collider.CompareTag("Water"))
            {
                // The first thing our ray hit was water. Game Over.
                GameOver(true);
            }
        }
        isMoving = false;
    }

    //game over logic
    private void GameOver(bool isDrowning)
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Game Over!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        if (isDrowning) {
            transform.SetParent(null); //detach from the log if we are attched to it
            if (cameraFollow != null) 
            {
                cameraFollow.enabled = false;
            }
            StartCoroutine(DrownCoroutine());
        }
    }

    private IEnumerator DrownCoroutine()
    {
        float Duration = 1f;
        float elapsedTime = 0f;
        Vector3 startpos = transform.position;
        Vector3 endpos = startpos - new Vector3(0, 4, 0); //to sink the player

        while (elapsedTime < Duration)
        {
            Vector3 newpos = Vector3.Lerp(startpos, endpos, elapsedTime / Duration);
            playerRb.MovePosition(newpos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void CheckForOutOfBounds()
    {
        //check if we are parented to the log or not
        if(transform.parent != null && transform.parent.CompareTag("Platform"))
        {
            //when the player's X pos exceeds the XBoundary value
            if(Mathf.Abs(transform.position.x) > xBoundary)
            {
                //then its game over for the player
                GameOver(true);
            }
        }
    }
    //collision detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameOver(false);
        }
        else if (other.CompareTag("Coin"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins();
            }
            other.gameObject.SetActive(false);
        }
        //else if(other.CompareTag("Water"))
        //{
        //    if(transform.parent == null)
        //    {
        //        GameOver();
        //    }
        //}
        //else if(other.CompareTag("Boundary"))
        //{
        //    if(transform.parent != null && transform.parent.CompareTag("Platform"))
        //    {
        //        GameOver();
        //    }
        //}
    }

    //this is for physical Collisions (like trees and rocks) that only block you
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bumped into a solid object: " + collision.gameObject.name);
    }
}