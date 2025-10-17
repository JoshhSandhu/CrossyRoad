using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class StartScreenManager : MonoBehaviour
{
    public static StartScreenManager Instance { get; private set; }

    [Header("Strat screen UI")]
    [SerializeField] private GameObject startScreenPanel;
    [SerializeField] private GameObject startScreenButtons;

    [Header("left side buttons")]
    [SerializeField] private Button[] leftButtons;

    [Header("right side buttons")]
    [SerializeField] private Button[] rightButtons;

    [Header("animation settings")]
    [SerializeField] private float slideDist = 2000f;
    [SerializeField] private float slideDuration = 1.0f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game start settings")]
    [SerializeField] private GameObject[] gameStartObjects;
    [SerializeField] private PlayerController playerController;

    private bool hasGameStarted = false;
    private Vector3[] leftbuttonsStartPos;
    private Vector3[] rightbuttonsStartPos;

    private PlayerInputActions playerInputActions;

    public static System.Action OnGameStart;

    private void Awake()
    {
        if(Instance != null && Instance == this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        storeButtonPos();

        SetGameObjActive(true);
    }

    private void Start()
    {
        if (playerInputActions == null)
        {
            playerInputActions = new PlayerInputActions();
        }
        playerInputActions.Enable();

        ShowStartScreen();

        StartCoroutine(WaitForPlayerMovement());
    }

    private void storeButtonPos()
    {
        leftbuttonsStartPos = new Vector3[leftButtons.Length];
        rightbuttonsStartPos = new Vector3[rightButtons.Length];

        for (int i = 0; i < leftButtons.Length; i++)
        {
            leftbuttonsStartPos[i] = leftButtons[i].transform.localPosition;
        }
        for (int i = 0; i < rightButtons.Length; i++)
        {
            rightbuttonsStartPos[i] = rightButtons[i].transform.localPosition;    
        }
    }

    private IEnumerator WaitForPlayerMovement()
    {
        while (!hasGameStarted)
        {
            if (AuthenticationFlowManager.Instance != null)
            {
                Debug.Log("Authentication flow active - StartScreenManager bypassed");
                yield return null;
                continue;
            }
            Vector2 moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();
            bool hasMoveInput = Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f;

            //check for touch input
            bool hasTouchInput = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;

            if (hasMoveInput || hasTouchInput)
            {
                StartGame();
                yield break;
            }

            yield return null;
        }
    }

    public void StartGame() 
    {
        if (hasGameStarted)
        {
            return;
        }
        hasGameStarted = true;
        HideStartScreen();
        OnGameStart?.Invoke();

        SetGameObjActive(true);
    }

    public void ShowStartScreen()
    {
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(true);
        }
        ResetButtonPos();
    }

    private void HideStartScreen()
    {
        StartCoroutine(SlideButtonsOut());
    }

    private void ResetButtonPos()
    {
        for (int i = 0; i < leftButtons.Length; i++)
        {
            leftButtons[i].transform.localPosition = leftbuttonsStartPos[i];
        }

        for (int i = 0; i < rightButtons.Length; i++)
        {
            rightButtons[i].transform.localPosition = rightbuttonsStartPos[i];
        }
    }


    private IEnumerator SlideButtonsOut()
    {
        Vector3[] leftTargetPositions = new Vector3[leftButtons.Length];
        Vector3[] rightTargetPositions = new Vector3[rightButtons.Length];

        for (int i = 0; i < leftButtons.Length; i++)
        {
            leftTargetPositions[i] = leftbuttonsStartPos[i] + new Vector3(-slideDist, 0, 0);
        }

        for (int i = 0; i < rightButtons.Length; i++)
        {
            rightTargetPositions[i] = rightbuttonsStartPos[i] + new Vector3(slideDist, 0, 0);
        }

        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideDuration;
            float easedProgress = slideCurve.Evaluate(progress);

            for (int i = 0; i < leftButtons.Length; i++)
            {
                leftButtons[i].transform.localPosition = Vector3.Lerp(
                    leftbuttonsStartPos[i],
                    leftTargetPositions[i],
                    easedProgress
                );
            }

            for (int i = 0; i < rightButtons.Length; i++)
            {
                rightButtons[i].transform.localPosition = Vector3.Lerp(
                    rightbuttonsStartPos[i],
                    rightTargetPositions[i],
                    easedProgress
                );
            }

            yield return null;
        }

        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(false);
        }
    }

    public void SetGameObjActive(bool active)
    {
        if (gameStartObjects != null)
        {
            foreach (GameObject obj in gameStartObjects)
            {
                if (obj != null)
                    obj.SetActive(active);
            }
        }
    }
    public void RestartToStartScreen()
    {
        hasGameStarted = false;

        ShowStartScreen();

        SetGameObjActive(false);

        StartCoroutine(WaitForPlayerMovement());
    }
    public void SlideButtonsBackIn()
    {
        Debug.Log("SlideButtonsBackIn() called");
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(true);
            Debug.Log("StartScreenPanel re-enabled for slide-in animation");
        }
        StartCoroutine(SlideButtonsIn());
    }

    private IEnumerator SlideButtonsIn()
    {
        Debug.Log("SlideButtonsIn coroutine started");
        Vector3[] leftCurrentPositions = new Vector3[leftButtons.Length];
        Vector3[] rightCurrentPositions = new Vector3[rightButtons.Length];

        // Store current positions
        for (int i = 0; i < leftButtons.Length; i++)
        {
            leftCurrentPositions[i] = leftButtons[i].transform.localPosition;
        }

        for (int i = 0; i < rightButtons.Length; i++)
        {
            rightCurrentPositions[i] = rightButtons[i].transform.localPosition;
        }

        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideDuration;
            float easedProgress = slideCurve.Evaluate(progress);

            for (int i = 0; i < leftButtons.Length; i++)
            {
                leftButtons[i].transform.localPosition = Vector3.Lerp(
                    leftCurrentPositions[i],
                    leftbuttonsStartPos[i],
                    easedProgress
                );
            }

            for (int i = 0; i < rightButtons.Length; i++)
            {
                rightButtons[i].transform.localPosition = Vector3.Lerp(
                    rightCurrentPositions[i],
                    rightbuttonsStartPos[i],
                    easedProgress
                );
            }

            yield return null;
        }
        hasGameStarted = false;
        ResetButtonPos();
        if (!hasGameStarted)
        {
            StartCoroutine(WaitForPlayerMovement());
        }
    }
    public bool IsGameStarted()
    {
        return hasGameStarted;
    }

    private void OnDestroy()
    {
        //clean up input system
        if (playerInputActions != null)
        {
            playerInputActions.Disable();
            playerInputActions.Dispose();
        }
    }
}
