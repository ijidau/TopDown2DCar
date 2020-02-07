using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    // 'torque' is just the wheel traction force, not rotational force
    [SerializeField]
    [Range(0f, 100f)]
    float power = 1;

    [SerializeField]
    bool steerable = false;

    [SerializeField]
    [Range(0f, 10f)]
    float steerPower = 1;

    [SerializeField]
    [Range(0f, 1f)]
    float surfaceFriction = 1;

    [SerializeField]
    [Range(0f, 20f)]
    float maxTyreFriction = 1;

    private Rigidbody2D rb;
    private TrailRenderer skidmarks;
    private Vector2 acceleration, driftForce, frictionForce;
    float h, v, driftPercentage;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skidmarks = GetComponent<TrailRenderer>();
    }

    void FixedUpdate()
    {
        // Inverted horizontal value so that it maps to clockwise (positive) and counter-clockwise (negative)
        h = -Input.GetAxis("Horizontal");
        Debug.DrawLine(new Vector2(0, 0), new Vector2(-h * 5, 0), Color.blue);
        v = Input.GetAxis("Vertical");

        // Steering
        if (steerable)
        {
            //rb.AddTorque(h * steerPower);
            rb.rotation += h * steerPower;

            // TODO
            // Drifting sort of works, but the steering is not 'stiff' enough and so it swings to easily.
            // Input.GetAxis is also all or nothing with a keyboard, how could a partial steer be simulated?
        }

        acceleration = transform.up * v * power;
        rb.AddForce(acceleration);

        // Calculate the sideways drift velocity
        driftForce = new Vector2(transform.InverseTransformVector(rb.velocity).x, 0);
        Debug.DrawLine(rb.position, rb.GetRelativePoint(driftForce * 5), Color.red);

        // Calculate an opposing friction force to counteract drift, limited by a maximum type grip
        frictionForce = Vector2.ClampMagnitude(driftForce * -1 * surfaceFriction, maxTyreFriction);
        Debug.DrawLine(rb.position, rb.GetRelativePoint(frictionForce * 5), Color.green);

        // Apply the friction force to counteract drift, i.e. so wheels roll forwards/backwards but not sideways
        rb.AddForce(rb.GetRelativeVector(frictionForce), ForceMode2D.Impulse);

        // Draw skidmarks when drift force exceeds maximum friction
        skidmarks.emitting = driftForce.sqrMagnitude > frictionForce.sqrMagnitude ? true : false;
    }
}