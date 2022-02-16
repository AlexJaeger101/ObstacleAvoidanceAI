using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydMovement : MonoBehaviour
{
    public enum BehavoirTypes
    {
        BASE,
        FLOCK,
        PATH,
    }

    //Speeds
    private float mHalfSpeed;
    private float mCurrentSpeed;

    //Screen Offsets
    const float mSCREEN_MIN_X = -14.0f;
    const float mSCREEN_MAX_X = 14.0f;
    const float mSCREEN_MIN_Y = -6.0f;
    const float mSCREEN_MAX_Y = 6.0f;

    //Wander Data
    [Header("Wander Data")]
    public float mWanderCircleDist = 5.0f;
    public float mWanderCircleRadius = 2.0f;

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
    public List<BoydMovement> mInRangeList;
    

    // Start is called before the first frame update
    void Start()
    {
        mHalfSpeed = mBoydManager.mMaxSpeed / 2.0f;
        mCurrentSpeed = mBoydManager.mMaxSpeed;

        mRB = GetComponent<Rigidbody2D>();
        mLR = GetComponent<LineRenderer>();
        mInRangeList = new List<BoydMovement>();
        mBoydManager = GameObject.FindGameObjectWithTag("BoydManager").GetComponent<BoydManager>();

        InitLineRenderer();
    }

    private void Update()
    {
        //Wrap around screen
        WrapAroundCamera();

        Quaternion lastRotation = transform.rotation;
        Quaternion newBoydRot = lastRotation;

        newBoydRot = ObstacleAvoidance(lastRotation);

        //If we are not currently trying to avoid an object, then we will move using the selected movement behavoir
        if (newBoydRot == lastRotation)
        {
            switch (mBoydManager.mMovementType)
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

            if (mCurrentSpeed < mBoydManager.mMaxSpeed)
            {
                mCurrentSpeed += Time.deltaTime;
            }
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, newBoydRot, Time.deltaTime * mBoydManager.mRotSpeed);
        mRB.velocity = transform.up * mCurrentSpeed;

        if (mLR.enabled)
        {
            mLR.enabled = false;
        }

    }

    Quaternion ObstacleAvoidance(Quaternion currentRot)
    {
        Quaternion newRot = currentRot;

        RaycastHit2D rightRay = Physics2D.Raycast(transform.position, transform.right, mBoydManager.mSideRayDist);
        RaycastHit2D leftRay = Physics2D.Raycast(transform.position, -transform.right, mBoydManager.mSideRayDist);

        if (rightRay && !rightRay.collider.gameObject.transform.CompareTag("Boyd")
                           && !rightRay.collider.gameObject.transform.CompareTag("PathNode")) //Move away from object to the right
        {
            Debug.Log("Obstacle Spotted, right");

            CreateLine(transform.position, transform.right, mBoydManager.mSideRayDist);
            mLR.enabled = true;

            newRot = AvoidBehavoir(rightRay.point);
        }
        else if (leftRay && !leftRay.collider.gameObject.transform.CompareTag("Boyd")
                           && !leftRay.collider.gameObject.transform.CompareTag("PathNode")) //Move away from object to the left
        {
            Debug.Log("Obstacle Spotted, left");

            CreateLine(transform.position, -transform.right, mBoydManager.mSideRayDist);
            mLR.enabled = true;

            newRot = AvoidBehavoir(leftRay.point);
        }
        else
        {
            //Avoid any obstacles in the way ahead
            for (int i = 0; i < mBoydManager.mNumOfRays; ++i)
            {
                //(angle * number of the rays + angleOffset) * rayAngle = New Rotation 
                //Axis will be the boyds forward direction
                Quaternion newRayRot = Quaternion.AngleAxis((i / (float)mBoydManager.mNumOfRays + mBoydManager.GetAngleOffset()) * mBoydManager.mRayAngle, transform.forward);
                Vector2 newRayDir = transform.rotation * newRayRot * Vector2.up;

                RaycastHit2D currentRay = Physics2D.Raycast(transform.position, newRayDir * mBoydManager.mRayDist, mBoydManager.mRayDist);
                if (currentRay && !currentRay.collider.gameObject.transform.CompareTag("Boyd")
                               && !currentRay.collider.gameObject.transform.CompareTag("PathNode"))
                {
                    Debug.Log("Obstacle Spotted, forward");

                    CreateLine(transform.position, newRayDir, mBoydManager.mRayDist);
                    mLR.enabled = true;

                    newRot = AvoidBehavoir(currentRay.point);

                    if (mCurrentSpeed > mHalfSpeed)
                    {
                        mCurrentSpeed -= Time.deltaTime;
                    }
                    return newRot;
                }
            }
        }

        return newRot;
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
                if (Vector3.Distance(this.transform.position, boyd.transform.position) > mBoydManager.mMaxCohesionDist)
                {
                    neighborPos += (Vector2)boyd.transform.position;
                }
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
        Debug.Log("Flocking");
        List<BoydMovement> inRange = mBoydManager.GetBoydsInRange(this, mBoydManager.mNeighborDist);

        if (inRange.Count > 1)
        {
            Vector2 seperateVec = SeperationSteer(inRange) * mBoydManager.mSeperateStrength;
            Vector2 cohesionVec = CohesionSteer(inRange) * mBoydManager.mCohesionStrength;
            Vector2 allignVec = AlignmentSteer(inRange) * mBoydManager.mAlligmentStrength;

            Vector2 flockingVec = (seperateVec + cohesionVec + allignVec).normalized;
            return Quaternion.LookRotation(Vector3.forward, flockingVec);
        }

        return WanderBehavior();
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("PathNode") && !collision.CompareTag("Boyd"))
        {
            ++mBoydManager.mCollisionCount;
        }
    }

}