using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydMovement : MonoBehaviour
{
    //Boyd Speeds
    [Header("Boyd Speeds")]
    public float mSpeed = 5.0f;
    public float mRotSpeed = 1.0f;

    //Obsticle Avoidance Raycast data
    [Header("Obsticle Avoidance Raycast Data")]
    public float mRayDist = 8.0f;
    public float mRayAngle = 90.0f;
    public int mNumOfRays = 3;
    const float mANGLE_OFFSET = -0.3f;

    //Screen Offsets
    const float mSCREEN_MIN_X = -13.0f;
    const float mSCREEN_MAX_X = 13.0f;
    const float mSCREEN_MIN_Y = -5.0f;
    const float mSCREEN_MAX_Y = 5.0f;

    //Wander Data
    [Header("Wander Data")]
    public float mCircleDist = 5.0f;
    public float mCircleRadius = 2.0f;

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
        Quaternion newBoydRot = Quaternion.identity;

        for (int i = 0; i < mNumOfRays; ++i)
        {
            //Calculate based on the number of rays we want to create around a point (the point is our player in this case
            //Algorithm: (angle * number of the rays + angleOffset) * rayAngle = New Rotation 
            //Axis will be the boyds forward direction
            Quaternion newRayRot = Quaternion.AngleAxis((i / (float)mNumOfRays + mANGLE_OFFSET) * mRayAngle, transform.forward);
            Vector2 newRayDir = transform.rotation * newRayRot * Vector2.up;

            //If the current ray collides with anything, change to "flee" behavoir 
            RaycastHit2D currentRay = Physics2D.Raycast(transform.position, newRayDir * mRayDist, mRayDist);
            if (currentRay)
            {
                Debug.DrawLine(transform.position, currentRay.point, Color.red);
                Debug.Log("Obsticle Spotted");

                newBoydRot = FleeBehavoir((Vector2)currentRay.collider.transform.position);
                break;
            }
        }

        //If we are not currently trying to avoid an object, then we will move using the "Wander" behavoir
        if (newBoydRot == Quaternion.identity)
        {
            //Rotate based on random value on the sphere
            newBoydRot = WanderBehavior();
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, newBoydRot, Time.deltaTime * mRotSpeed);
        mRB.velocity = transform.up * mSpeed;
    }

    //Wander Behavior: Calculate and return rotation towards a random location
    //Random location is generated using a circle a given distance away from the boyd
    Quaternion WanderBehavior()
    {
        Debug.Log("Wandering around");

        //Get the center of the circle a fixed distance away from the boyd
        //Algorithm: currentVel.norm * cirlceDist + currentPos = circleCenter
        Vector2 circleCenter = (((Vector2)mRB.velocity).normalized * mCircleDist) + (Vector2)transform.position;

        // Generate a point on the circle by using a random angle
        // Angle is converted into radian form (angle * pi / 180)
        // That new point will be the target
        int randAngle = Random.Range(0, 360);
        float circlePosX = circleCenter.x + ((Mathf.Cos(randAngle * Mathf.PI / 180.0f) * mCircleRadius));
        float circlePosY = circleCenter.y + ((Mathf.Sin(randAngle * Mathf.PI / 180.0f) * mCircleRadius));
        Vector2 randPoint = new Vector2(circlePosX, circlePosY);

        //Rotate based on random value on the sphere
        return Quaternion.LookRotation(Vector3.forward, randPoint);
    }

    //Flee Behavior: Calculate and return the rotation away from a given point
    Quaternion FleeBehavoir(Vector2 fleeFromPos)
    {
        Debug.Log("Obsticle Spotted");

        //Desired veolocity is based on the current position and the normalized position of the collided object
        Vector2 desiredVel = ((Vector2)transform.position - fleeFromPos).normalized * mSpeed;
        Vector2 target = desiredVel - mRB.velocity;

        //Return rotation based on the new target
        return Quaternion.LookRotation(Vector3.forward, target);
    }

    //When the boyd is out of view of the camera, wrap it to the other side
    private void WrapAroundCamera()
    {
        if (transform.position.x < mSCREEN_MIN_X || transform.position.x > mSCREEN_MAX_X)
        {
            float newPosX = Mathf.Clamp(transform.position.x, mSCREEN_MAX_X, mSCREEN_MIN_X);
            transform.position = new Vector2(newPosX, transform.position.y);
        }
        else if (transform.position.y < mSCREEN_MIN_Y || transform.position.y > mSCREEN_MAX_Y)
        {
            float newPosY = Mathf.Clamp(transform.position.y, mSCREEN_MAX_Y, mSCREEN_MIN_Y);
            transform.position = new Vector2(transform.position.x, newPosY);
        }
    }
}
