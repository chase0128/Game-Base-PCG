using UnityEngine;

public class LargeEnemyView : MonoBehaviour
{
    private LargeEnemy largeEnemy;

    private void Start()
    {
        largeEnemy = transform.parent.GetComponent<LargeEnemy>();
        largeEnemy.SetValue(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        largeEnemy = transform.parent.GetComponent<LargeEnemy>();
        largeEnemy.SetWakeDuration(0);
        largeEnemy.SetTarget(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        largeEnemy = transform.parent.GetComponent<LargeEnemy>();
        largeEnemy.SetWakeDuration(0);
    }

    private void OnTriggerStay(Collider other)
    {
        largeEnemy = transform.parent.GetComponent<LargeEnemy>();
        largeEnemy.SetWakeDuration(largeEnemy.GetWakeDuration() + Time.deltaTime);
    }
}
