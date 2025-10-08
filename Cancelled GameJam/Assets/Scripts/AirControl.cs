using UnityEngine;

public class AirControl : MonoBehaviour
{
    public float airControl = 0.3f;
    
    private Rigidbody2D rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void ApplyMovement(float moveInput, float maxSpeed, bool isGrounded)
    {
        if (isGrounded)
        {
            //full control on ground
            rb.linearVelocity = new Vector2(moveInput * maxSpeed, rb.linearVelocity.y);
        }
        else
        {
            //limited directional control in air
            float newHorizontalVelocity = CalculateAirMovement(moveInput, maxSpeed);
            rb.linearVelocity = new Vector2(newHorizontalVelocity, rb.linearVelocity.y);
        }
    }
    
    private float CalculateAirMovement(float moveInput, float maxSpeed)
    {
        float currentVelocity = rb.linearVelocity.x;
        float desiredVelocity = moveInput * maxSpeed;
        
        return Mathf.Lerp(currentVelocity, desiredVelocity, airControl);
    }
}