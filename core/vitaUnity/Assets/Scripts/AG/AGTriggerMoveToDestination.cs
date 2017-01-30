/* Move game object entering collider to target point.
 * 
 * Used in VITA to make sure that characters reach the exact point we want them to pla y certain
 * animations from. For example sitting down on a chair with a table next to it.
*/

// TODO: Read up on smooth damp; does it need be in OnTriggerStay
// TODO: Is tolerance needed?


using UnityEngine;


public class AGTriggerMoveToDestination : MonoBehaviour
{
    #region variables
    public float smoothTime = 1.00f;
    public float distanceTolerance = .1f;

    private Transform target;
    private Transform destination;

    private Vector3 velocity = Vector3.zero;    // What is this for?
    #endregion


    void Start()
    {
        destination = transform;
    }


    void OnTriggerEnter(Collider col)
    {
        target = col.gameObject.transform;

        Debug.Log("Moving <color=white>"+target.name+ "</color> to match position of <color=white>" + destination.name+"</color>");
    }


    void OnTriggerStay()
    {
        MoveToPoint();
    }


    void MoveToPoint()
    {
        float dist = Vector3.Distance(target.position, destination.position);
        if (dist > distanceTolerance)
        {
            target.position = Vector3.SmoothDamp(target.position, destination.position, ref velocity, smoothTime);
        }
    }

}
