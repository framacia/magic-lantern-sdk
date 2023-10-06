using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogAgentController : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform crateTransform;
    [SerializeField, Range(0, 1)] private float distancePercentage = 0.5f;

    private NavMeshAgent agent;
    private Animator anim;
    private Vector2 smoothDeltaPosition = Vector2.zero;
    private Vector2 velocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        //Don't update position automatically
        agent.updatePosition = false;
    }

    // Update is called once per frame
    void Update()
    {
        //GoToClick();

        //Calculate speed/animation
        CalculateSpeedAnimation();

        if (agent)
        {
            //Place agent in between the target and the crate
            agent.SetDestination(targetTransform.position + (crateTransform.position - targetTransform.position) *  (1 - distancePercentage));
        }
    }

    void GoToClick()
    {
        if (Camera.main == null) return;

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (agent)
                    agent.SetDestination(hit.point);
            }
        }
    }

    void CalculateSpeedAnimation()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.005f && agent.remainingDistance >= agent.stoppingDistance; //> agent.radius; //Commenting so agent doesn't "hover"

        // Update animation parameters
        anim.SetBool("moving", shouldMove);
        //anim.SetFloat("velx", velocity.x);
        //anim.SetFloat("vely", velocity.y);

        // Pull character towards agent
        if (worldDeltaPosition.magnitude > agent.radius)
            transform.position = agent.nextPosition - 0.9f * worldDeltaPosition;
    }

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = anim.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
    }
}