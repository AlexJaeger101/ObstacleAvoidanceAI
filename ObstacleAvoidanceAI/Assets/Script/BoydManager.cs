using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoydManager : MonoBehaviour
{
    public List<BoydMovement> mBoydList;
    public int mBoydCount;

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject boyd in GameObject.FindGameObjectsWithTag("Boyd"))
        {
            mBoydList.Add(boyd.GetComponent<BoydMovement>());
        }

        mBoydCount = mBoydList.Count;
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
