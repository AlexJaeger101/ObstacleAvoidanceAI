using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoydManager : MonoBehaviour
{
    //Boyds
    [Header("Boyd Count")]
    public List<BoydMovement> mBoydList;
    public int mBoydCount;

    //Path Nodes
    [Header("Path Nodes")]
    public int mPathNodeCount;
    public GameObject[] mPathNodeArray;
    private GameObject mSelectedNode;
    private Vector3 mClickOffset;

    //Flocking Data
    [Header("Flocking Data")]
    public float mCohesionStrength = 10.0f;
    public float mSeperateStrength = 10.0f;
    public float mAlligmentStrength = 10.0f;

    //UI data
    public int mCollisionCount = 0;
    public Text mCollisionCountText;

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
    }

    // Update is called once per frame
    void Update()
    {
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

    public List<BoydMovement> getBoydsInRange(BoydMovement originBoyd, float range)
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
}
