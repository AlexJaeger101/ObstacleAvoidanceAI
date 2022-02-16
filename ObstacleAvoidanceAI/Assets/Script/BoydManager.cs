using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoydManager : MonoBehaviour
{
    public int mFPSCap = 120;

    //Boyd Speeds
    [Header("Boyd Movement")]
    public float mMaxSpeed = 5.0f;
    public float mRotSpeed = 200.0f;
    public BoydMovement.BehavoirTypes mMovementType;

    //Boyds
    [Header("Boyd Count")]
    public List<BoydMovement> mBoydList;
    public int mBoydCount;

    //Obsticle Avoidance Raycast data
    [Header("Obsticle Avoidance Raycast Data")]
    public float mRayDist = 2.0f;
    public float mSideRayDist = 3.0f;
    public float mRayAngle = 90.0f;
    public int mNumOfRays = 10;
    const float mANGLE_OFFSET = -0.4f;

    //Path Nodes
    [Header("Path Nodes")]
    public int mPathNodeCount;
    public GameObject[] mPathNodeArray;
    private GameObject mSelectedNode;
    private Vector3 mClickOffset;

    //Flocking Data
    [Header("Flocking Data")]
    public float mNeighborDist = 2.5f;
    public float mMaxCohesionDist = 0.25f;
    [Range(0.0f, 10.0f)] public float mCohesionStrength = 5.0f;
    [Range(0.0f, 10.0f)] public float mSeperateStrength = 5.0f;
    [Range(0.0f, 10.0f)] public float mAlligmentStrength = 5.0f;

    //UI data
    [Header("UI Data")]
    public int mCollisionCount = 0;
    public Text mCollisionCountText;
    public GameObject mFlockingUI;
    public GameObject mUI;
    public Text mFPSCountText;

    // Start is called before the first frame update
    void Start()
    {
        //Init Boyds
        foreach(GameObject boyd in GameObject.FindGameObjectsWithTag("Boyd"))
        {
            mBoydList.Add(boyd.GetComponent<BoydMovement>());
        }

        mBoydCount = mBoydList.Count;

        //Init PathNodes
        mPathNodeArray = GameObject.FindGameObjectsWithTag("PathNode");
        mPathNodeCount = mPathNodeArray.Length;

        //Init Proper UI
        if (mMovementType != BoydMovement.BehavoirTypes.FLOCK)
        {
            mFlockingUI.SetActive(false);
        }

        //Setup FPS Counter
        InvokeRepeating("GetCurrentFPS", 1.0f, 1.0f);
        Application.targetFrameRate = mFPSCap;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            mUI.active = mUI.active ? false : true;
        }

        MoveWorldElements();

        mCollisionCountText.text = mCollisionCount.ToString();
    }

    private void MoveWorldElements()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);
            if (targetObject && (targetObject.CompareTag("PathNode") || targetObject.CompareTag("Wall")))
            {
                mSelectedNode = targetObject.transform.gameObject;
                mClickOffset = mSelectedNode.transform.position - mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0) && mSelectedNode != null)
        {
            mSelectedNode = null;
        }
        else if (mSelectedNode)
        {
            mSelectedNode.transform.position = mousePosition + mClickOffset;
        }
    }

    public List<BoydMovement> GetBoydsInRange(BoydMovement originBoyd, float range)
    {
        List<BoydMovement> inRangeList = new List<BoydMovement>();
        foreach (BoydMovement boyd in mBoydList)
        {
            if (Vector3.Distance(originBoyd.transform.position, boyd.transform.position) < range)
            {
                inRangeList.Add(boyd);
            }
        }

        return inRangeList;
    }

    public void GetCurrentFPS()
    {
        mFPSCountText.text = ((int)(1.0f / Time.unscaledDeltaTime)).ToString();
    }

    public float GetAngleOffset()
    {
        return mANGLE_OFFSET;
    }

    public void setNewCohesion(float newVal)
    {
        mCohesionStrength = newVal;
    }

    public void setNewSeperate(float newVal)
    {
        mSeperateStrength = newVal;
    }

    public void setNewAllign(float newVal)
    {
        mAlligmentStrength = newVal;
    }
}