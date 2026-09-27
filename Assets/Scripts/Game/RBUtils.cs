
using UnityEngine;

public class RBUtils
{
    public static float GetDecelSpeed(Rigidbody2D rb, float decelAmount)
    {
        float decelMin = Mathf.Min(Mathf.Abs(rb.linearVelocityX) * rb.mass / Time.fixedDeltaTime, decelAmount * Time.deltaTime);
        float decelSign = -Mathf.Sign(rb.linearVelocityX);
        return decelMin * decelSign;
    }

    public static void LimitXSpeed(Rigidbody2D rb, float maxSpeed)
    {
        if (Mathf.Abs(rb.linearVelocityX) > maxSpeed)
        {
            rb.linearVelocityX = Mathf.Sign(rb.linearVelocityX) * maxSpeed;
        }
    }

    public static void SetRBFreeze(Rigidbody2D rb, bool frozen)
    {
        if (frozen)
        {
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            rb.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        }
    }
}
