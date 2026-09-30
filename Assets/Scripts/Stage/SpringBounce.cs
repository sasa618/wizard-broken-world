using UnityEngine;

public class SpringBounce : MonoBehaviour
{
    [SerializeField] private float bounceSpeed = 20f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBouncePlayer(collision, bounceSpeed);
    }

    public static bool TryBouncePlayer(Collision2D collision, float bounceSpeed)
    {
        if(!collision.gameObject.CompareTag("Player")) return false;

        Rigidbody2D playerRb = collision.rigidbody;
        if(playerRb == null) return false;

        bool landedOnTop = false;
        foreach(ContactPoint2D contact in collision.contacts)
        {
            if(contact.normal.y < -0.5f)
            {
                landedOnTop = true;
                break;
            }
        }

        if(!landedOnTop) return false;

        playerRb.linearVelocityY = bounceSpeed;
        SoundManager.Instance?.PlaySE(SEType.SpringBounce);
        return true;
    }
}
