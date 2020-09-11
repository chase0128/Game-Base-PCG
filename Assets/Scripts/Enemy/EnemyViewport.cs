using UnityEngine;

public class EnemyViewport : MonoBehaviour
{
    private GameObject m_parent;
    private PatrolEnemy m_patrolEnemy;
    private float rayLength = 6f;
    private Ray r;
    private bool flag = false;
    private void Start()
    {
        m_parent = transform.parent.gameObject;
        m_patrolEnemy = m_parent.GetComponent<PatrolEnemy>();
    }

    private void Update()
    {
        if(flag)
            Debug.DrawLine(r.origin,r.origin + r.direction * 20,Color.red);

    }

    private void OnTriggerEnter(Collider other)
    {
        m_parent = transform.parent.gameObject;
        m_patrolEnemy = m_parent.GetComponent<PatrolEnemy>();
        if (!m_patrolEnemy.IsActive()) return;
        Vector3 target = other.gameObject.transform.position;
        target.y += 0.5f;
        Vector3 origin = gameObject.transform.position;
        origin.y = target.y;
        Ray ray = new Ray(transform.position ,target - origin);
        r = ray;
        flag = true;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, m_patrolEnemy.GetPatrolDistance(),
            LayerMask.GetMask("Environment", "Player")))
        {
            Debug.Log(other.gameObject);
            Debug.Log(hit.collider.gameObject);
            if (other.gameObject == hit.collider.gameObject)
            {
                Debug.Log("Found");
                m_patrolEnemy.FoundPlayer(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //m_patrolEnemy.LossPlayer();
    }
}
