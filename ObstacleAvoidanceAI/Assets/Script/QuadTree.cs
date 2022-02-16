using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuadTreeIndex
{
    TopLeft = 0,
    BotRight = 1,
    TopRight = 2,
    BotLeft = 3
}

public class QuadTree<TType>
{
    private QuadNode<TType> mNode;
    private int mQuadDepth;

    public QuadTree(Vector2 pos, float size, int depth)
    {
        mNode = new QuadNode<TType>(pos, size);
        mNode.subDiv(depth);
    }

    private int GetIndexPos(Vector2 lookUpPos, Vector2 nodePos)
    {
        int iter = 0;

        iter |= lookUpPos.y > nodePos.y ? 2 : 0;
        iter |= lookUpPos.x > nodePos.x ? 1 : 0;

        return iter;
    }

    public QuadNode<TType> GetRoot()
    {
        return mNode;
    }

}

public class QuadNode<TType>
{
    Vector2 mPos;
    float mSize;
    QuadNode<TType>[] mSubNodeArray;
    IList<TType> mVal;
        
    public QuadNode(Vector3 pos, float size)
    {
        mPos = pos;
        mSize = size;
    }

    public void subDiv(int depth)
    {
        mSubNodeArray = new QuadNode<TType>[4];

        for (int i = 0; i < mSubNodeArray.Length; ++i)
        {
            Vector2 newPos = mPos;
            if ((i & 2) == 2)
            {
                newPos.y -= mSize * 0.25f;
            }
            else
            {
                newPos.y += mSize * 0.25f;
            }

            if ((i & 2) == 1)
            {
                newPos.x += mSize * 0.25f;
            }
            else
            {
                newPos.x -= mSize * 0.25f;
            }

            mSubNodeArray[i] = new QuadNode<TType>(newPos, mSize * 0.25f);
            if (depth > 0)
            {
                mSubNodeArray[i].subDiv(depth - 1);
            }

        }
    }

    public bool IsLeaf()
    {
        return mSubNodeArray == null;
    }
}
