using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] [Range(0f, 500f)] float power = 1;

    [SerializeField] bool steerable = false;

    [SerializeField][Range(0f, 10f)] float steerPower = 1;

    [SerializeField] [Range(0f, 1f)] float surfaceFriction = 1;

    [SerializeField] [Range(0f, 20f)] float maxTyreFriction = 1;

    [SerializeField] [Range(1f, 10f)] float skidmarkThreshold = 1;

    private Rigidbody2D rb;
    private TrailRenderer skidmarks;
    private Vector2 acceleration, driftForce, frictionForce;
    private float h, v;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skidmarks = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        // Inverted horizontal value so that it maps to clockwise (positive) and counter-clockwise (negative)
        h = -Input.GetAxis("Horizontal");
        Debug.DrawLine(new Vector2(0, 0), new Vector2(-h * 5, 0), Color.blue);
        v = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        // Steering
        if (steerable)
        {
            //rb.AddTorque(h * steerPower);
            //rb.rotation += h * steerPower;
            rb.SetRotation(rb.rotation + h * steerPower);

            // TODO
            // Figure out a way to use rb.MoveRotation() so that interpolation is respected
            // Using this will mean that the Friction Joint 2D will be overriden, so a work-around is needed
        }

        acceleration = transform.up * v * power;
        rb.AddForce(acceleration);

        // Calculate the sideways drift velocity
        driftForce = new Vector2(transform.InverseTransformVector(rb.velocity).x, 0);
        //Debug.DrawLine(rb.position, rb.GetRelativePoint(driftForce * 5), Color.red);

        // Calculate an opposing friction force to counteract drift, limited by a maximum type grip
        frictionForce = Vector2.ClampMagnitude(driftForce * -1 * surfaceFriction, maxTyreFriction);
        //Debug.DrawLine(rb.position, rb.GetRelativePoint(frictionForce * 5), Color.green);

        // TODO
        // Improve the friction response to reduce drift at low speeds and stabilise drift at higher speeds

        // Apply the friction force to counteract drift, i.e. so wheels 'roll' forwards/backwards but not sideways
        rb.AddForce(rb.GetRelativeVector(frictionForce), ForceMode2D.Impulse);

        // Draw skidmarks when drift force exceeds maximum friction
        skidmarks.emitting = driftForce.sqrMagnitude > frictionForce.sqrMagnitude * skidmarkThreshold ? true : false;
    }
}