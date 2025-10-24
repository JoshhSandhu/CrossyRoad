using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Burst.CompilerServices;
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

    [Header("Rotation")]
    //[SerializeField]
    //private bool smoothTurn = true;
    //[SerializeField]
    //private float turnSpeed = 20f;

    //private feilds
    private Rigidbody playerRb;
    private bool isMoving = false;  //to check if the player is moving
    private int forwardPosZ = 0;    //to track the forward position of the player
    private bool isDead = false;    //to keep track if the player is alive or dead
    private bool hasScattered = false;
    private Quaternion targetRotation; //to store the rotation of the player
    private Vector2 touchStartPos;      //for the new input system var
    private float minSwipeDist = 50f;   //minimum distance for a swipe to be registered
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        playerRb.freezeRotation = true;
        TouchSimulation.Enable();
    }

    //subscribe the events
    private void OnEnable()
    {
        if (playerInputActions == null)
        {
            playerInputActions = new PlayerInputActions();
        }
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.PrimaryTouch.started += OnTouchStart;
        playerInputActions.Player.PrimaryTouch.canceled += OnTouchEnd;
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
        float deadZone = 0.1f;
        if (Mathf.Abs(inputDirection.x) < deadZone) inputDirection.x = 0;
        if (Mathf.Abs(inputDirection.y) < deadZone) inputDirection.y = 0;

        //vertical movement takes priority over horizontal movement
        if (Mathf.Abs(inputDirection.y) > Mathf.Abs(inputDirection.x))
        {
            if (inputDirection.y > 0)
            {
                movePlayer(Vector3.forward);
            }
            else if (inputDirection.y < 0)
            {
                movePlayer(Vector3.back);
            }
        }
        else if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))  //remove this condition to allow diagonal movement
        {
            if (inputDirection.x > 0)
            {
                movePlayer(Vector3.right);
            }
            else if (inputDirection.x < 0)
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

        //check if authentication flow is active and game is not ready
        if (AuthenticationFlowManager.Instance != null && !AuthenticationFlowManager.Instance.IsGameReady())
        {
            return;
        }
        //if authentication is complete but game hasn't started yet, start the game
        if (AuthenticationFlowManager.Instance != null && AuthenticationFlowManager.Instance.IsGameReady() && !GameManager.Instance.IsGameActive())
        {
            AuthenticationFlowManager.Instance.StartGame();
            return;
        }
        //if no authentication flow but game is not active, start the game
        if (AuthenticationFlowManager.Instance == null && !GameManager.Instance.IsGameActive())
        {
            if (StartScreenManager.Instance != null)
            {
                StartScreenManager.Instance.StartGame();
            }
            return;
        }

        Vector3 snappedPos = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y + 0.5f,
            Mathf.Round(transform.position.z)
        );

        Vector3 destination = snappedPos + direction;

        if (Mathf.Abs(destination.x) > xBoundary)
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

        //function to turn the player the ideration we are moving
        transform.SetParent(null);
        FaceDirection(direction);
        StartCoroutine(HopCoroutine(destination));

        if (direction == Vector3.forward && (int)destination.z > forwardPosZ)
        {
            forwardPosZ = (int)destination.z;
            OnPlayerMovedForward?.Invoke();
            OnScoreChanged?.Invoke(forwardPosZ);
        }

        // Send Solana transaction for player movement
        SendMovementTransaction(direction);
    }

    private IEnumerator HopCoroutine(Vector3 destinationXZ)
    {
        //Debug.Log($"HopCoroutine started for destination: {destinationXZ}");
        isMoving = true;
        transform.SetParent(null);
        float landingY = destinationXZ.y;

        Vector3 rayStart = destinationXZ + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out var groundHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            landingY = groundHit.point.y;
        }

        Vector3 startPos = playerRb.position;
        Vector3 destination = new Vector3(destinationXZ.x, landingY, destinationXZ.z);
        float elapsedTime = 0f;
        //Debug.Log($"HopCoroutine: startPos={startPos}, destination={destination}, parent={transform.parent?.name ?? "null"}, rotation={transform.rotation.eulerAngles}");
        while (elapsedTime < hopDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float a = Mathf.Clamp01(elapsedTime / hopDuration);
            float arc = Mathf.Sin(a * Mathf.PI) * hopHeight;
            Vector3 pos = Vector3.Lerp(startPos, destination, a);
            pos.y += arc;
            playerRb.MovePosition(pos);
            //if (smoothTurn) { transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime); }
            //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        playerRb.MovePosition(destination);
        transform.rotation = targetRotation;
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
        //Debug.Log($"HopCoroutine completed, isMoving set to false");
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

        if (isDrowning)
        {
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
        if (transform.parent != null && transform.parent.CompareTag("Platform"))
        {
            //when the player's X pos exceeds the XBoundary value
            if (Mathf.Abs(transform.position.x) > xBoundary)
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

    private void FaceDirection(Vector3 dir)
    {
        Vector3 flat = new Vector3(dir.x, 0f, dir.z);
        if (dir == Vector3.zero)
        {
            return;
        }
        Quaternion rawTarget = Quaternion.LookRotation(flat, Vector3.up);
        float yaw = rawTarget.eulerAngles.y;
        float snappedYaw = Mathf.Round(yaw / 90f) * 90f;
        targetRotation = Quaternion.Euler(0f, snappedYaw, 0f);
        //Debug.Log($"FaceDirection: dir={dir}, yaw={yaw}, snappedYaw={snappedYaw}, targetRotation={targetRotation.eulerAngles}");
        //if (smoothTurn)
        //{
        //    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        //}
        //else
        //{
        //    transform.rotation = targetRotation;
        //}
        transform.rotation = targetRotation;
        //Debug.Log($"FaceDirection: Set rotation to {transform.rotation.eulerAngles}");
    }

    public void ResetPlayer()
    {
        // Reset death state
        isDead = false;
        hasScattered = false;
        isMoving = false;
        forwardPosZ = 0;

        // Reset position
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.SetParent(null);

        // Re-enable camera follow
        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
            cameraFollow.ResetCamera();
        }

        // Re-enable renderers and colliders (in case they were disabled by scatter)
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = true;
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = true;

        // Reset rigidbody
        if (playerRb != null)
        {
            if (!playerRb.isKinematic)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
        }
    }

    //this sends a transaction for the player movement
    private async void SendMovementTransaction(Vector3 direction)
    {
        Debug.Log("SendMovementTransaction called with direction: " + direction);
        if (AuthenticationFlowManager.Instance == null)
        {
            Debug.LogWarning("AuthenticationFlowManager instance is null. Cannot send movement transaction.");
            return;
        }

        try
        {
            Debug.Log("Checking if user has Solana wallet...");
            //first ensure the user has a solana wallet
            if (!await AuthenticationFlowManager.Instance.EnsureSolanaWallet())
            {
                Debug.LogWarning("No solana wallet avilable, skipping transaction");
                return;
            }
            Debug.Log("Solana wallet check passed");
            //create movement message
            string directionText = GetDirectionText(direction);
            string message = $"Crossy Road: Player moved {directionText} at {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            Debug.Log($"Sending Solana transaction: {message}");

            string walletAddress = await AuthenticationFlowManager.Instance.GetSolanaWalletAddress();
            Debug.Log($"From Solana wallet: {walletAddress}");

            Debug.Log("Attempting to sign message...");
            var signature = await AuthenticationFlowManager.Instance.SignSolanaMessage(message);
            Debug.Log($"SignSolanaMessage returned: {(string.IsNullOrEmpty(signature) ? "NULL/EMPTY" : "SUCCESS")}");

            if (!string.IsNullOrEmpty(signature))
            {
                Debug.Log("Solana transaction sent successfully!");
                Debug.Log($"View on Solana Explorer: https://explorer.solana.com/tx/{signature}?cluster=devnet");
            }
            else
            {
                Debug.LogWarning("Failed to send Solana transaction");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending Solana transaction: {ex.Message}");
        }
    }

    private string GetDirectionText(Vector3 direction)
    {
        if (direction == Vector3.forward) return "FORWARD";
        if (direction == Vector3.back) return "BACKWARD";
        if (direction == Vector3.left) return "LEFT";
        if (direction == Vector3.right) return "RIGHT";
        return "UNKNOWN";
    }
}