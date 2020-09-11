using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTrigger : MonoBehaviour
{
    private LootBox lootBox;

    private void Start()
    {
        
    }
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        lootBox = transform.parent.GetComponent<LootBox>();
        lootBox.CollisionEnter(other);        
    }

    private void OnTriggerExit(Collider other)
    {
        lootBox = transform.parent.GetComponent<LootBox>();
        lootBox.CollisionExit(other);
    }
}
