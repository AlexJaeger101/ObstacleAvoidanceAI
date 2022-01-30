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
        for (int i = 0; i < mNumOfRays; ++i)
        {
            Quaternion currentRot = transform.rotation;

            //Calculate based on the number of rays we want to create around a point (the point is our player in this case
            //Algorithm: (angle * number of the rays + angleOffset) * rayAngle = New Rotation 
            //Axis will be the boyds forward direction
            Quaternion newRayRot = Quaternion.AngleAxis((i / (float)mNumOfRays + mANGLE_OFFSET) * mRayAngle, transform.forward);
            Vector2 newRayDir = currentRot * newRayRot * Vector2.up;


            RaycastHit2D currentRay = Physics2D.Raycast(transform.position, newRayDir * mRayDist, mRayDist);
            if (currentRay)
            {
                Debug.DrawLine(transform.position, currentRay.point, Color.red);
                Debug.Log("Obsticle Spotted");
                Vector2 desiredVel = ((Vector2)transform.position - (Vector2)currentRay.collider.transform.position).normalized * mSpeed;
                Vector2 target = desiredVel - mRB.velocity;

                //Rotate based on the new target
                Quaternion newBoydRot = Quaternion.LookRotation(Vector3.forward, target);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, newBoydRot, Time.deltaTime * mRotSpeed);
                break;
            }
        }

        mRB.velocity = transform.up * mSpeed;
    }

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
    
    //Gets Mouse World Position (THIS IS ONLY FOR TESTING THE FLEE BEHAVIOR)
    private Vector2 GetMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
