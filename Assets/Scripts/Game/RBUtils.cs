
using UnityEngine;

public class RBUtils
{

    public static void XDecelRB(Rigidbody2D rb, float decelAmount)
    {
        float forceToStop = Mathf.Abs(rb.linearVelocityX) * rb.mass / Time.fixedDeltaTime;

        float attemptDecelForce = decelAmount * Time.deltaTime;
        float decelSign = -Mathf.Sign(rb.linearVelocityX);
        float decelForce = attemptDecelForce * decelSign;

        if (forceToStop <= attemptDecelForce)
        {
            rb.linearVelocityX = 0;
        } else
        {
            rb.AddForceX(decelForce);
        }
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
