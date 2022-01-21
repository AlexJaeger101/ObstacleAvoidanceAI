using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydMovement : MonoBehaviour
{
    public float mSpeed = 5.0f;
    Rigidbody2D mRb;

    // Start is called before the first frame update
    void Start()
    {
        mRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        WrapAroundCamera();
    }

    //For Physics movement
    private void FixedUpdate()
    {
        mRb.velocity = transform.up * mSpeed;
    }

    private void WrapAroundCamera()
    {
        if (transform.position.x < -14.0f || transform.position.x > 14.0f)
        {
            float newPos = Mathf.Clamp(transform.position.x, 14.0f, -14.0f);
            transform.position = new Vector2(newPos, transform.position.y);
        }
        else if (transform.position.y < -6.0f || transform.position.y > 6.0f)
        {
            float newPos = Mathf.Clamp(transform.position.y, 6.0f, -6.0f);
            transform.position = new Vector2(transform.position.x, newPos);
        }
    }
}
