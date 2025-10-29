using UnityEngine;

/// <summary>
/// Handles player collision detection and responses
/// </summary>
public class PlayerCollisionHandler : ICollisionHandler
{
    private readonly Transform playerTransform;
    private readonly float xBoundary;
    private readonly System.Action<bool> onGameOver;
    private readonly System.Action onAddCoins;

    public PlayerCollisionHandler(Transform playerTransform, float xBoundary, System.Action<bool> onGameOver, System.Action onAddCoins)
    {
        this.playerTransform = playerTransform;
        this.xBoundary = xBoundary;
        this.onGameOver = onGameOver;
        this.onAddCoins = onAddCoins;
    }

    public void HandleTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            onGameOver?.Invoke(false);
        }
        else if (other.CompareTag("Coin"))
        {
            onAddCoins?.Invoke();
            other.gameObject.SetActive(false);
        }
        //else if(other.CompareTag("Water"))
        //{
        //    if(playerTransform.parent == null)
        //    {
        //        onGameOver?.Invoke();
        //    }
        //}
        //else if(other.CompareTag("Boundary"))
        //{
        //    if(playerTransform.parent != null && playerTransform.parent.CompareTag("Platform"))
        //    {
        //        onGameOver?.Invoke();
        //    }
        //}
    }

    public void HandleCollisionEnter(Collision collision)
    {
        Debug.Log("Bumped into a solid object: " + collision.gameObject.name);
    }

    public void CheckForOutOfBounds()
    {
        //check if we are parented to the log or not
        if (playerTransform.parent != null && playerTransform.parent.CompareTag("Platform"))
        {
            //when the player's X pos exceeds the XBoundary value
            if (Mathf.Abs(playerTransform.position.x) > xBoundary)
            {
                //then its game over for the player
                onGameOver?.Invoke(true);
            }
        }
    }
}
