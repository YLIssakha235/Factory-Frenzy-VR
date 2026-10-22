using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 0.15f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.03f);
    }
}