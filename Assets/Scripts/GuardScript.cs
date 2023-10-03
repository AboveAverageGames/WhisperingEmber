using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardScript : MonoBehaviour
{
    //PlayerSpotted
    public static event System.Action OnGuardHasSpottedPlayer;

    //Guard Movement
    public float waitTime = 0.5f;
    public float speed = 5;
    public float turnSpeed;




    //Pathing system
    public Transform pathHolder;


    //Vision
    public Light spotlight;
    public float viewDistance;
    float viewAngle;
    public LayerMask viewMask;
    Color originalColor;

    //Get Player
    Transform player;

    //TimeToSpot
    public float timeToSpotPlayer = 1f;
    float playerVisibleTimer;

    //Alert bar reference
    [SerializeField] AlertBar alertbar;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = spotlight.color;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotlight.spotAngle;

        //Positions of path
        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance){
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle (transform.forward, dirToPlayer); 
            if (angleBetweenGuardAndPlayer < viewAngle /2f) {
                if (!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {

        //Detecting player
        if (CanSeePlayer()) {
            playerVisibleTimer += Time.deltaTime;
        }
        else
        {
            playerVisibleTimer -= Time.deltaTime;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0f, timeToSpotPlayer);
        spotlight.color = Color.Lerp(originalColor, Color.red, playerVisibleTimer / timeToSpotPlayer);
        alertbar.UpdateAlertBar(playerVisibleTimer, timeToSpotPlayer);

        //Static event when player is spotted begins here 
        if (playerVisibleTimer >= timeToSpotPlayer)
            {
            if (OnGuardHasSpottedPlayer != null)
            {  OnGuardHasSpottedPlayer();
           }
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            //Makes Guard stop if they can see the player before resuming their "Patrol"
            while (CanSeePlayer())
            {
                yield return new WaitForSeconds(timeToSpotPlayer);
            }
            float angle = Mathf.MoveTowardsAngle (transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];
        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while (true)
        {
            //Makes Guard stop if they can see the player before resuming their "Patrol"
            while (CanSeePlayer())
            {
                yield return new WaitForSeconds(timeToSpotPlayer);
            }
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if (transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex +1) % waypoints.Length; //MODULUS HERE IS SO SMART, not my code but I use it to learn. It divides and returns the remainder.
                //Meaning when it hits the max waypoints.length, it returns a value of 0. so the Array will go back to the start at 0. And begin again.
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            yield return null;

        }
    }

    private void WaitUntil(bool v)
    {
        throw new NotImplementedException();
    }

    private void OnDrawGizmos()
    {
        //Visualize Ray / sight of gaurd
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);

        //Visualize path
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        //Draw line back to start position
        Gizmos.DrawLine (previousPosition, startPosition);
    }
}
