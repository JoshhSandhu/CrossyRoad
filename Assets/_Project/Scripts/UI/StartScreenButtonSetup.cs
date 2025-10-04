using UnityEngine.UI;
using UnityEngine;

public class StartScreenButtonSetup : MonoBehaviour
{
    [Header("Button references")]
    [SerializeField] private Button[] allbuttons;

    private void Start()
    {
        foreach (Button button in allbuttons)
        {
            if (button != null)
            {
                button.interactable = false;
                button.onClick.RemoveAllListeners();
            }
        }
    }
}
