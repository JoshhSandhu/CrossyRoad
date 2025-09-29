using System;
using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

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
    [SerializeField]
    private LayerMask blockingMask; //solid obstacles and Boundary
    [SerializeField]
    private LayerMask groundMask; //for the ground masks (ground, water, platform)
    [SerializeField]
    private Vector3 blockCastHalfExtents = new Vector3(0.3f, 0.5f, 0.3f);

    [Header("Death Scatter Settings")]
    [SerializeField]
    private ParticleSystem scatterParticleSystem;

    //private feilds
    private Rigidbody playerRb;
    private bool isMoving = false;  //to check if the player is moving
    private int forwardPosZ = 0;    //to track the forward position of the player
    private bool isDead = false;    //to keep track if the player is alive or dead
    private bool hasScattered = false;

    private Vector2 touchStartPos;      //for the new input system var
    private float minSwipeDist = 50f;   //minimum distance for a swipe to be registered
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        playerRb.freezeRotation = true;
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

        Vector3 snappedPos = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y + 0.5f,
            Mathf.Round(transform.position.z)
        );

        Vector3 destination = snappedPos + direction;

        if(Mathf.Abs(destination.x) > xBoundary)
        {
            return;
        }

        //compute expected landing height, and then using it for accurate blocking vs rocks while on logs
        float candidateLandingY = transform.position.y;
        Vector3 probeStart = destination + Vector3.up * 5f;
        if (Physics.Raycast(probeStart, Vector3.down, out var probeHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            candidateLandingY = probeHit.point.y;
        }

        //pre hop check to avoid false positives from nearby solid obstacles
        Vector3 overlapCenter = new Vector3(destination.x, candidateLandingY + 0.6f, destination.z);
        Collider[] hits = Physics.OverlapBox(overlapCenter, blockCastHalfExtents, Quaternion.identity, blockingMask, QueryTriggerInteraction.Ignore);
        //prehop box check with boxcast to catch walls and boundaries reliably
        if (hits != null && hits.Length > 0)
        {
            return;
        }
        StartCoroutine(HopCoroutine(destination));
        transform.SetParent(null);

        if (direction == Vector3.forward && (int)destination.z > forwardPosZ)
        {
            forwardPosZ = (int)destination.z;
            OnPlayerMovedForward?.Invoke();
            OnScoreChanged?.Invoke(forwardPosZ);
        } 
    }

    private IEnumerator HopCoroutine(Vector3 destinationXZ)
    {
        isMoving = true;
        float landingY = transform.position.y;

        Vector3 rayStart = destinationXZ + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out var groundHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            landingY = groundHit.point.y;
        }

        Vector3 startPos = playerRb.position;
        Vector3 destination = new Vector3(destinationXZ.x, landingY, destinationXZ.z);
        float elapsedTime = 0f;

        while (elapsedTime < hopDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float a = Mathf.Clamp01(elapsedTime / hopDuration);
            float arc = Mathf.Sin(a * Mathf.PI) * hopHeight;
            Vector3 pos = Vector3.Lerp(startPos, destination, a);
            pos.y += arc;
            playerRb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }
        //RaycastHit landinghit;
        //Physics.Raycast(transform.position, Vector3.down, out landinghit, 1f);

        playerRb.MovePosition(destination);
        //if we landed on a log
        if (Physics.Raycast(destination + Vector3.up * 0.5f, Vector3.down, out var landinghit, 1.5f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (landinghit.collider.CompareTag("Platform"))
            {
                transform.SetParent(landinghit.transform);
            }
            else if (landinghit.collider.CompareTag("Water"))
            {
                GameOver(true);
            }
            else
            {
                transform.SetParent(null);
            }
        }
        cameraFollow.Shake(0.15f); //camera shake at the end of an hop
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
        else
        {
            ScatterIntoBlocks();
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

    private void ScatterIntoBlocks()
    {
        if (hasScattered) return;
        hasScattered = true;

        // disable visuals and interaction of the player
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        if (cameraFollow != null)
        {
            cameraFollow.Shake(0.6f);
        }

        ParticleSystem ps = Instantiate(scatterParticleSystem, transform.position, transform.rotation);
        var main = ps.main;
        ps.Play();
        Destroy(ps.gameObject, main.duration + main.startLifetime.constantMax + 0.5f);

    }
}