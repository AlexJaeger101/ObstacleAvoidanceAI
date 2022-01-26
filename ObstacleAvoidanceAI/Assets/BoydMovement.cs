using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydMovement : MonoBehaviour
{
    public float mSpeed = 5.0f;

    public float mRotSpeed = 1.0f;

    public float mCircleDist = 3.0f;
    public float mCircleRadius = 5.0f;

    public float mRayDist = 8.0f;
    public float mRayAngle = 45.0f;

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
        Vector2 rayAngleDir = new Vector2(-Mathf.Cos(mRayAngle * Mathf.Deg2Rad), Mathf.Sin(mRayAngle * Mathf.Deg2Rad)).normalized;

        RaycastHit2D middleRay = Physics2D.Raycast(transform.position, mRB.velocity, mRayDist);
        RaycastHit2D leftRay = Physics2D.Raycast(transform.position, -rayAngleDir, mRayDist);
        RaycastHit2D rightRay = Physics2D.Raycast(transform.position, rayAngleDir, mRayDist);

        Debug.DrawRay(transform.position, mRB.velocity, Color.red);
        Debug.DrawRay(transform.position, -rayAngleDir, Color.blue);
        Debug.DrawRay(transform.position, rayAngleDir, Color.blue);


        if (middleRay) //If obsticle detected
        {
            Debug.Log("OH GOD OH FRICK OBJECT IN THE WAY!!!!!!!");

            //Boyd will try and avoid hit object by "fleeing" it
            Vector2 desiredVel = ((Vector2)transform.position - (Vector2)middleRay.collider.transform.position).normalized * mSpeed;
            Vector2 target = desiredVel - mRB.velocity;
            
            //Rotate based on the new target
            Quaternion newRot = Quaternion.LookRotation(Vector3.forward, target);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRot, Time.deltaTime * mRotSpeed);
        }

        mRB.velocity = transform.up * mSpeed;
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
