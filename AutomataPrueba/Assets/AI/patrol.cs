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
    public float battleDistance;


    float probIdle = 15f;
    float probOrbit = 25f;
    float probFight = 40f;
    float totalProb;
    float rand;

    float actualAngle;
    float randAngle;
    float totalAngle;
    float initAngle;
    int leftRight;

    [SerializeField]
    float seconds = 0f;
    float timerStill = 1f;

    [SerializeField]
    float randFight;
    float probAttack1 = 30f;
    float probAttack2 = 25f;
    float probCombo1 = 10f;
    float totalProbAttack;
    float timerAttack1 = 0.5f;
    float timerAttack2 = 0.8f;

    [SerializeField]
    bool attacked = false;

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
        if (Vector3.Distance(transform.position, player.position) <= battleDistance)
        {
            totalProb = probIdle + probOrbit + probFight;
            setState(getState("battle"));
        }

    }
    
    void runFromPlayer()
    {
        float reversePlayer = -transform.forward.z;
        Vector3 runFrom = new Vector3(player.position.x, player.position.y, reversePlayer);
        rotateTo(runFrom);
        if (Vector3.Distance(transform.position, player.position) > 6)
            setState(getState("nextwp"));

        gizmoColor = Color.yellow;
    }

    void idleBattle()
    {
        rand = Random.Range(0, totalProb);

        if (rand <= probIdle)
        {
            Debug.Log("Waiting...");
            setState(getState("stay"));
        }
        else if (rand <= probOrbit + probIdle)
        {
            Debug.Log("Orbiting");
            initAngle = transform.rotation.eulerAngles.y;
            actualAngle = transform.rotation.eulerAngles.y;
            randAngle = Random.Range(30, 90);
            leftRight = Random.Range(0, 2);
            setState(getState("orbit"));
        }
        else if (rand <= probOrbit + probIdle + probFight)
        {
            //Fight
            Debug.Log("Fighting");
            seconds = 0f;
            attacked = false;
            totalProbAttack = probAttack1 + probAttack2 + probCombo1;
            randFight = Random.Range(0, totalProbAttack);
            setState(getState("fight"));
        }

    }

    void stayStill()
    {
        gizmoColor = Color.cyan;
        if (seconds < timerStill)
            seconds += Time.deltaTime;
        else
            setState(getState("battle"));
    }


    void orbit()
    {
        gizmoColor = Color.magenta;

        if (leftRight == 0)
        {
            orbitLeft();
            totalAngle = initAngle + randAngle;
        }
        else
        {
            orbitRight();
            totalAngle = initAngle - randAngle;
        }

    }
    void orbitRight()
    {
        float angleToGo = Mathf.Min(angularVelocity - 9, randAngle);
        actualAngle = actualAngle - angleToGo;
        transform.position = battleDistance * transform.forward;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, actualAngle, transform.rotation.eulerAngles.z);
        transform.position = battleDistance * -transform.forward;

        if (actualAngle <= totalAngle)
            setState(getState("battle"));

    }

    void orbitLeft()
    {

        float angleToGo = Mathf.Min(angularVelocity - 9, randAngle);
        actualAngle = actualAngle + angleToGo;
        transform.position = battleDistance * transform.forward;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, actualAngle, transform.rotation.eulerAngles.z);
        transform.position = battleDistance * -transform.forward;

        if (actualAngle >= totalAngle)
            setState(getState("battle"));

    }

    void fight()
    {


        attacked = false;

        if (randFight <= probAttack1)
        {
            //Attack1
            attack1();
        }
        else if (randFight <= probAttack1 + probAttack2)
        {
            //Attack2
            attack2();
        }
        else if (randFight <= totalProbAttack)
        {
            //Combo
            combo1();
        }
    }

    void attack1()
    {
        gizmoColor = Color.black;

        if (seconds < timerAttack1)
        {
            seconds += Time.deltaTime;
            Debug.Log("Attack1");
            attacked = true;
            //Attack1
        }
        else if (seconds >= timerAttack1 || attacked)
        {
            Debug.Log("Going idle battle 1");
            setState(getState("battle"));

        }
    }

    void attack2()
    {
        gizmoColor = Color.black;

        if (seconds < timerAttack2)
        {
            seconds += Time.deltaTime;
            Debug.Log("Attack 2");
            attacked = true;
            //Attack2
        }
        else if (seconds >= timerAttack2 || attacked)
        {
            Debug.Log("Going idle battle 2");
            setState(getState("battle"));
        }
    }

    void combo1()
    {
        gizmoColor = Color.black;
        float timerCombo = timerAttack1 + timerAttack2;
        bool attacked1 = attacked;

        if (seconds < timerCombo)
        {
            seconds += Time.deltaTime;

            if(seconds <= 0.3 * timerCombo)
            {
                //Attack1
                Debug.Log("Combo 1");
                attacked1 = true;
            }
            else if (seconds >= 0.5f * timerCombo || attacked1)
            {
                Debug.Log("Combo 2");
                attacked = true;
                //Attack2
            }
        }
        else if (seconds >= timerCombo || attacked)
        {
            Debug.Log("Going idle battle combo");
            setState(getState("battle"));
        }
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
        initState("battle", idleBattle);
        initState("stay", stayStill);
        initState("orbit", orbit);
        initState("fight", fight);

        setState(getState("idle"));
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Vector3.Distance(transform.position, waypoints[actualWaypoint]));
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
