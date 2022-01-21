using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydMovement : MonoBehaviour
{
    public float mSpeed = 5.0f;
    public float mRotSpeed = 5.0f;
    private Rigidbody2D mRB;

    // Start is called before the first frame update
    void Start()
    {
        mRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        WrapAroundCamera();
    }

    private void FixedUpdate()
    {
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, Vector2.up, 5.0f);
        Debug.DrawRay(transform.position, Vector2.up * 5.0f, Color.red);
        if (rayHit)
        {
            Debug.Log("OH GOD OH FRICK OBJECT IN THE WAY!!!!!!!");

            //Boyd will try and avoid hit object by "fleeing" it
            Vector2 desiredVel = ((Vector2)transform.position - (Vector2)rayHit.collider.transform.position).normalized * mSpeed;
            mRB.velocity = desiredVel - mRB.velocity;
        }
        else
        {
            mRB.velocity = transform.up * mSpeed;
        }

        //Rotate Boyd in the direction of its current velocity
        Quaternion newRotation = Quaternion.LookRotation(Vector3.forward, mRB.velocity);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, mRotSpeed * Time.deltaTime);
    }

    private void WrapAroundCamera()
    {
        //TO DO: Hard Coded edges of camera, should change later
        if (transform.position.x < -14.0f || transform.position.x > 14.0f)
        {
            float newPosX = Mathf.Clamp(transform.position.x, 14.0f, -14.0f);
            transform.position = new Vector2(newPosX, transform.position.y);
        }
        else if (transform.position.y < -6.0f || transform.position.y > 6.0f)
        {
            float newPosY = Mathf.Clamp(transform.position.y, 6.0f, -6.0f);
            transform.position = new Vector2(transform.position.x, newPosY);
        }
    }
    
    //Gets Mouse World Position (THIS IS ONLY FOR TESTING THE FLEE BEHAVIOR)
    private Vector2 GetMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
