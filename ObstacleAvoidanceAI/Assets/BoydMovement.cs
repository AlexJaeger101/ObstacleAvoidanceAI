using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydMovement : MonoBehaviour
{
    public float mSpeed = 5.0f;
    public float mRotSpeed = 5.0f;
    public float mCircleDist = 3.0f;
    public float mCircleRadius = 5.0f;

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
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, mRB.velocity, 5.0f);
        Debug.DrawRay(transform.position, mRB.velocity * 5.0f, Color.red);

        if (rayHit) //If obsticle detected
        {
            Debug.Log("OH GOD OH FRICK OBJECT IN THE WAY!!!!!!!");

            //Boyd will try and avoid hit object by "fleeing" it
            Vector2 desiredVel = ((Vector2)transform.position - (Vector2)rayHit.collider.transform.position).normalized * mSpeed;
            mRB.velocity = desiredVel - mRB.velocity;
        }
        else //Move using Wander behavior if nothing in the way
        {
            //Get the center of the circle
            Vector2 circleCenter = mRB.velocity;
            if (circleCenter == Vector2.zero)
            {
                circleCenter = mRB.velocity;
            }
            circleCenter = (Vector2)circleCenter.normalized;
            circleCenter *= mCircleDist;
            circleCenter = circleCenter + (Vector2)transform.position;

            // Generate a point on the circle by using a random angle
            // Angle is converted into radian form (angle * pi / 180)
            // That new point will be the target
            int randAngle = Random.Range(0, 360);
            float circlePosX = circleCenter.x + ((Mathf.Cos(randAngle * Mathf.PI / 180.0f) * mCircleRadius));
            float circlePosY = circleCenter.y + ((Mathf.Sin(randAngle * Mathf.PI / 180.0f) * mCircleRadius));
            Vector2 randPoint = new Vector2(circlePosX, circlePosY);

            mRB.velocity = randPoint - mRB.velocity;
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
