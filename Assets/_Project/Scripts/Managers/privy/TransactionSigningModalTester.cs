using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

/// <summary>
/// Simple test script to verify the Transaction Signing Modal works
/// Attach this to a GameObject with a Button to test the modal
/// </summary>
public class TransactionSigningModalTester : MonoBehaviour
{
    [Header("Test Button (Optional)")]
    [SerializeField] private Button testButton;

    private void Start()
    {
        // If test button is assigned, set up click listener
        if (testButton != null)
        {
            testButton.onClick.AddListener(TestModal);
        }
    }

    /// <summary>
    /// Test method - call this from a button or other trigger
    /// </summary>
    public async void TestModal()
    {
        if (TransactionSigningModal.Instance == null)
        {
            Debug.LogError("TransactionSigningModal.Instance is null! Make sure the modal is set up in the scene.");
            return;
        }

        Debug.Log("Testing Transaction Signing Modal...");

        // Test with sample data
        string testMessage = "Test transaction message";
        string testWalletAddress = "7xKXtg2CW87d97TXJSDpbD5jBkheTqA83TZRuJosgAsU";

        bool approved = await TransactionSigningModal.Instance.ShowSigningModal(
            testMessage,
            testWalletAddress
        );

        if (approved)
        {
            Debug.Log("User approved the transaction!");
            
            // Simulate loading state
            TransactionSigningModal.Instance.ShowLoadingState();
            
            // Simulate some processing time
            await Task.Delay(2000);
            
            // Hide loading and close
            TransactionSigningModal.Instance.HideLoadingState();
            await TransactionSigningModal.Instance.CloseModal();
        }
        else
        {
            Debug.Log("User rejected the transaction.");
        }
    }

    /// <summary>
    /// Test method that can be called from Unity Inspector or other scripts
    /// </summary>
    [ContextMenu("Test Signing Modal")]
    public void TestModalFromContextMenu()
    {
        TestModal();
    }
}

