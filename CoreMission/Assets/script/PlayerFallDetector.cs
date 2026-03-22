using UnityEngine;

public class PlayerFallDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PenaltyGround"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseGame();
            }
        }
    }
}