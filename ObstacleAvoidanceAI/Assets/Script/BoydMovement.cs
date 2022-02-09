using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BoydMovement : MonoBehaviour
{
    public enum BehavoirTypes
    {
        BASE,
        FLOCK,
        PATH
    }

    //Boyd Speeds
    [Header("Boyd Speeds")]
    public float mMaxSpeed = 5.0f;
    float mHalfSpeed;
    float mCurrentSpeed;
    public float mRotSpeed = 1.0f;
    public BehavoirTypes mMovementType;

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
    public float mWanderCircleDist = 5.0f;
    public float mWanderCircleRadius = 2.0f;

    //Flocking Data
    [Header("Flocking Data")]
    public float mCohesionStrength = 10.0f;
    public float mSeperateStrength = 10.0f;
    public float mAlligmentStrength = 10.0f;

    //Path Data
    [Header("Path Data")]
    public float mArriveDistOffset = 3.0f;
    [SerializeField] private int mPathIter = 0;

    //Line Renderer Data
    [Header("Line Renderer Data")]
    public float mLineWidth = 0.1f;
    private LineRenderer mLR;

    //Misc Data
    [Header("Misc Data")]
    public BoydManager mBoydManager;
    private Rigidbody2D mRB;
    

    // Start is called before the first frame update
    void Start()
    {
        mHalfSpeed = mMaxSpeed / 2.0f;
        mCurrentSpeed = mMaxSpeed;

        mRB = GetComponent<Rigidbody2D>();
        mLR = GetComponent<LineRenderer>();

        InitLineRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        WrapAroundCamera();
    }

    private void FixedUpdate()
    {
        Quaternion lastRotation = transform.rotation;
        Quaternion newBoydRot = lastRotation;

        //Avoid any obstacles in the way
        for (int i = 0; i < mNumOfRays; ++i)
        {
            //(angle * number of the rays + angleOffset) * rayAngle = New Rotation 
            //Axis will be the boyds forward direction
            Quaternion newRayRot = Quaternion.AngleAxis((i / (float)mNumOfRays + mANGLE_OFFSET) * mRayAngle, transform.forward);
            Vector2 newRayDir = transform.rotation * newRayRot * Vector2.up;

            RaycastHit2D currentRay = Physics2D.Raycast(transform.position, newRayDir * mRayDist, mRayDist);
            if (currentRay && !currentRay.collider.gameObject.transform.CompareTag("Boyd"))
            {
                Debug.Log("Obstacle Spotted");

                CreateLine(transform.position, newRayDir, mRayDist);
                mLR.enabled = true;

                newBoydRot = AvoidBehavoir(currentRay.point);

                if (mCurrentSpeed > mHalfSpeed)
                {
                    mCurrentSpeed -= Time.deltaTime;
                }
                break;
            }
        }

        //If we are not currently trying to avoid an object, then we will move using the selected movement behavoir
        if (newBoydRot == lastRotation)
        {
            switch (mMovementType)
            {
                case BehavoirTypes.BASE:

                    newBoydRot = WanderBehavior();
                    break;

                case BehavoirTypes.FLOCK:

                    newBoydRot = FlockingBehavior();
                    break;

                case BehavoirTypes.PATH:

                    newBoydRot = PathBehavior();
                    break;

                default:

                    Debug.Log("Invalid Behavior Type");
                    break;
            }

            if (mCurrentSpeed < mMaxSpeed)
            {
                mCurrentSpeed += Time.deltaTime;
            }
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, newBoydRot, Time.deltaTime * mRotSpeed);
        mRB.velocity = transform.up * mCurrentSpeed;

    }

    Vector2 SeperationSteer(List<BoydMovement> boydsInRange)
    {
        Vector2 neighborPos = Vector2.zero;

        if (boydsInRange.Count > 1)
        {
            foreach(BoydMovement boyd in boydsInRange)
            {
                neighborPos += (Vector2)boyd.transform.position;
            }

            Vector2 averagePos = neighborPos / boydsInRange.Count;

            //Seperate Vector = (averagePos - currentPos).normalized * -1 (must be negated for seperation)
            return new Vector2(averagePos.x - transform.position.x, averagePos.y - transform.position.y).normalized * -1;
        }

        return Vector2.zero;
    }

    Vector2 CohesionSteer(List<BoydMovement> boydsInRange)
    {
        Vector2 neighborPos = Vector2.zero;
        if (boydsInRange.Count > 1)
        {
            foreach(BoydMovement boyd in boydsInRange)
            {
                neighborPos += (Vector2)boyd.transform.position;
            }

            Vector2 averagePos = neighborPos / boydsInRange.Count;

            //Same as seperation except no negating
            //Cohesion Vector = (averagePos - currentPos).normalized
            return new Vector2(averagePos.x - transform.position.x, averagePos.y - transform.position.y).normalized;
        }

        return Vector2.zero;
    }

    Vector2 AlignmentSteer(List<BoydMovement> boydsInRange)
    {
        Vector2 averageHeading = Vector2.zero;
        if (boydsInRange.Count > 1)
        {
            foreach (BoydMovement boyd in boydsInRange)
            {
                averageHeading += boyd.mRB.velocity;
            }
        }

        return averageHeading.normalized;
    }

    //Flocking is all three vectors combined
    Quaternion FlockingBehavior()
    {
        Vector2 seperateVec = SeperationSteer(mBoydManager.getBoydsInRange(this, 5.0f)) * mSeperateStrength;
        Vector2 cohesionVec = CohesionSteer(mBoydManager.getBoydsInRange(this, 5.0f)) * mCohesionStrength;
        Vector2 allignVec = AlignmentSteer(mBoydManager.getBoydsInRange(this, 5.0f)) * mAlligmentStrength;

        Vector2 flockingVec = (seperateVec + cohesionVec + allignVec).normalized;
        return Quaternion.LookRotation(Vector3.forward, flockingVec);
    }

    Quaternion PathBehavior()
    {
        if (Vector2.Distance(transform.position, mBoydManager.mPathNodeArray[mPathIter].transform.position) < mArriveDistOffset)
        {
            ++mPathIter;

            if (mPathIter > (mBoydManager.mPathNodeCount - 1))
            {
                mPathIter = 0;
            }
        }
        Vector2 currentSeekPos = mBoydManager.mPathNodeArray[mPathIter].transform.position;

        mLR.SetPosition(0, transform.position);
        mLR.SetPosition(1, currentSeekPos);
        mLR.enabled = true;

        Vector2 desiredVel = (currentSeekPos - (Vector2)transform.position).normalized;
        Vector2 target = desiredVel - mRB.velocity;

        return Quaternion.LookRotation(Vector3.forward, target);
    }

    //Random location is generated using a circle a given distance away from the boyd
    Quaternion WanderBehavior()
    {
        Debug.Log("Wandering around");
        mLR.enabled = false;

        //currentVel.norm * cirlceDist - currentPos = circleCenter
        Vector2 circleCenter = (((Vector2)mRB.velocity).normalized * mWanderCircleDist) - (Vector2)transform.position;


        // Angle is converted into radian form (angle * pi / 180)
        float randAngle = Random.Range(0, 360) * Mathf.PI / 180.0f;

        float circlePosX = circleCenter.x + ((Mathf.Cos(randAngle) * mWanderCircleRadius));
        float circlePosY = circleCenter.y + ((Mathf.Sin(randAngle) * mWanderCircleRadius));
        Vector2 randPoint = new Vector2(circlePosX, circlePosY);

        return Quaternion.LookRotation(Vector3.forward, randPoint);
    }

    Quaternion AvoidBehavoir(Vector2 fleeFromPos)
    {
        mLR.enabled = true;

        //Desired veolocity is based on the current position and the normalized position of the collided object
        Vector2 desiredVel = ((Vector2)transform.position - fleeFromPos).normalized * mCurrentSpeed;
        Vector2 target = desiredVel - mRB.velocity;


        return Quaternion.LookRotation(Vector3.forward, target);
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


    private void InitLineRenderer()
    {
        Vector3[] initLinePos = new Vector3[2] { Vector3.zero, Vector3.zero };
        mLR.SetPositions(initLinePos);

        mLR.startWidth = mLineWidth;
        mLR.endWidth = mLineWidth;
    }


    void CreateLine(Vector3 start, Vector3 dir, float length)
    {
        Ray ray = new Ray(start, dir);
        RaycastHit raycastHit;
        Vector3 endPosition = start + (length * dir);

        if (Physics.Raycast(ray, out raycastHit, length))
        {
            endPosition = raycastHit.point;
        }

        mLR.SetPosition(0, start);
        mLR.SetPosition(1, endPosition);
    }
}