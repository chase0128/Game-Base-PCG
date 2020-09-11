using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Replay : MonoBehaviour
{
    private void Update()
    {

        if (Input.anyKeyDown)
        {
            GameManager.instance.ReBegin();
        }
    }
}
