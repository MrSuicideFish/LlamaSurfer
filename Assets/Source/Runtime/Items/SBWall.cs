using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBWall : WorldObjectBase
{
    public void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;
        if (GameManager.Instance.gameHasStarted 
            && !GameManager.Instance.gameHasEnded
            && other.CompareTag("Player"))
        {
            if (other.transform.position.y <= this.transform.position.y + 0.3f)
            {
                SurfBlockView sb = other.GetComponent<SurfBlockView>();
                GameManager.Instance.RemovePlayerBlock(sb);
            }
        }
    }
}