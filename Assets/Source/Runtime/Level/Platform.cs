using UnityEngine;
using UnityEngine.Serialization;

public class Platform : MonoBehaviour
{
    [HideInInspector] public int _id;
    
    public Transform platformExit;
    public Transform[] trackPoints;
    
    public int GetID()
    {
        return _id;
    }

    public void SetID(int id)
    {
        _id = id;
    }

    public int Count
    {
        get
        {
            int _count = 0;
            Platform current = this.First();
            while (current != null)
            {
                _count++;
                current = current.next;
            }

            return _count;
        }
    }

    public Platform previous;
    public Platform next;

    public Platform First()
    {
        if (previous != null)
        {
            return previous.First();
        }

        return this;
    }

    public Platform Last()
    {
        if (next != null)
        {
            return next.Last();
        }

        return this;
    }
}