using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Level : MonoBehaviour
{
    public NavMeshData navMeshData;
    public Transform startPoint;
    public Transform finishPoint;
    public int botAmount;
    public Stage[] stages;

    public void OnInit()
    {
        foreach (var stage in stages)
        {
            stage.OnInit();
        }
    }
}
