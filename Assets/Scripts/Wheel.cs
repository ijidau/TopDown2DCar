using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    // 'torque' is just the wheel traction force, not rotational force
    [SerializeField]
    [Range(0f, 100f)]
    float power;

    [SerializeField]
    bool steerable;

    [SerializeField]
    [Range(0f, 10f)]
    float steerPower;

    [SerializeField]
    [Range(0f, 1f)]
    float grip;

    [SerializeField]
    [Range(0f, 50f)]
    float maxFriction;

    Rigidbody2D rb;
    TrailRenderer skidmarks;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skidmarks = GetComponent<TrailRenderer>();
    }

    void FixedUpdate()
    {
        // Inverted horizontal value so that it maps to clockwise (positive) and counter-clockwise (negative)
        float h = -Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Steering
        if (steerable)
        {
            //rb.AddTorque(h * steerPower);
            rb.rotation += h * steerPower;
        }

        Vector2 acceleration = transform.up * v * power;
        rb.AddForce(acceleration);

        // Calculate the percentage amount of sideways velocity compared to forwards velocity 
        float driftPercentage = Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.left));
        // Create a vector that represents this drifting force
        Vector2 driftForce = Vector2.left * driftPercentage;
        // Draw a line (exaggerated) to demonstrate the drifting force
        Debug.DrawLine(rb.position, rb.GetRelativePoint(driftForce * 10), Color.red);

        // Calculate an opposing friction force to counteract drift
        Vector2 frictionForce = Vector2.ClampMagnitude(driftForce * -1 * grip, maxFriction);
        // Draw a line (exaggerated) to demonstrate the friction force
        Debug.DrawLine(rb.position, rb.GetRelativePoint(frictionForce * 10), Color.green);

        // Apply the friction force to counteract drift, i.e. so wheels roll forwards/backwards but not sideways
        rb.AddForce(rb.GetRelativeVector(frictionForce), ForceMode2D.Impulse);

        // Draw skidmarks when drift force exceeds maximum friction
        if(driftForce.sqrMagnitude > frictionForce.sqrMagnitude)
        {
            skidmarks.emitting = true;
        }
        else
        {
            skidmarks.emitting = false;
        }
    }
}