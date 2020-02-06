using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class patrol : AI_Agent
{
    Vector3[] waypoints;
    Transform target;
    public Transform player;
    public int maxWaypoints = 10;
    int actualWaypoint = 0;

    public float angularVelocity;
    public float angleGoTo;
    public float vel;
    public float halfAngle;
    public float coneDistance;

    Color gizmoColor;

    void initPositions()
    {
        List<Vector3> waypointsList = new List<Vector3>();
        float anglePartition = 360.0f / (float)maxWaypoints;
        for (int i = 0; i < maxWaypoints; ++i)
        {
            Vector3 v = transform.position + 5 *  Vector3.forward * Mathf.Cos(i* anglePartition) 
                + 5* Vector3.right * Mathf.Sin(i*anglePartition);
            waypointsList.Add(v);

        }
        waypoints = waypointsList.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (waypoints.Length > 0)
        {
            for (int i = 0; i < maxWaypoints; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 1.0f);
            }
        }
        Gizmos.DrawSphere(waypoints[actualWaypoint], 2.0f);

        Vector3 rightSide = Quaternion.Euler(Vector3.up * halfAngle) * transform.forward * coneDistance;
        Vector3 leftSide = Quaternion.Euler(Vector3.up * -halfAngle) * transform.forward * coneDistance;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * coneDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightSide);
        Gizmos.DrawLine(transform.position, transform.position + leftSide);

        Gizmos.DrawLine(transform.position + rightSide, transform.position + transform.forward * coneDistance);
        Gizmos.DrawLine(transform.position + leftSide, transform.position + transform.forward * coneDistance);

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position + (Vector3.up * 3), 1f);
    }

    void idle()
    {
        
        if(Input.GetKeyDown(KeyCode.A))
        {
            setState(getState("goto")) ;
        }

        gizmoColor = Color.gray;
    }


    
    void goToWaypoint()
    {
        rotateTo(waypoints[actualWaypoint]);
        if(Vector3.Distance(player.position, transform.position) < coneDistance &&
        Vector3.Angle(transform.forward, player.position - transform.position) <= halfAngle)
            setState(getState("goplayer"));
        else if (Vector3.Distance(transform.position, waypoints[actualWaypoint]) <= 2)
            setState(getState("nextwp"));
        gizmoColor = Color.blue;

    }

    void calculateNextWaypoint()
    {
        actualWaypoint++;
        if (actualWaypoint >= maxWaypoints)
            actualWaypoint = 0;

        setState(getState("goto"));
        gizmoColor = Color.green;

    }

    void playerDetected()
    {
        rotateTo(player.position);
        gizmoColor = Color.red;
    }

    // Start is called before the first frame update
    void Start()
    {
        initPositions();
        actualWaypoint = 0;
        initState("idle", idle);
        initState("goto", goToWaypoint);
        initState("nextwp", calculateNextWaypoint);
        initState("goplayer", playerDetected);
        
        setState(getState("idle"));
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Vector3.Distance(transform.position, waypoints[actualWaypoint]));
    }

    void rotateTo(Vector3 target)
    {
        float maxAngle = Vector3.SignedAngle(transform.forward, target - transform.position, Vector3.up);
        angleGoTo = Mathf.Min(angularVelocity, Mathf.Abs(maxAngle));
        angleGoTo *= Mathf.Sign(maxAngle);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angleGoTo, transform.rotation.eulerAngles.z);
        transform.position += transform.forward * vel;
    }
}
