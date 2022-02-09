using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydManager : MonoBehaviour
{
    [Header("Boyd Count")]
    public List<BoydMovement> mBoydList;
    public int mBoydCount;

    [Header("Path Nodes")]
    public int mPathNodeCount;
    public GameObject[] mPathNodeArray;
    private GameObject mSelectedNode;
    private Vector3 mClickOffset;

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
        else if(mSelectedNode)
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
