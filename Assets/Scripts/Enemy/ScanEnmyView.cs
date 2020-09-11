
using System.Collections.Generic;
using UnityEngine;

public class ScanEnmyView : MonoBehaviour
{
    private List<GameObject> patrolEnemiesAround;

    private void Awake()
    {
        patrolEnemiesAround = new List<GameObject>();
    }

}
