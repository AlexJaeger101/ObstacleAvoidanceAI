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
