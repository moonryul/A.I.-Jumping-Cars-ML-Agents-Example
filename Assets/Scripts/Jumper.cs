using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;  // Unity.MLAgents namespace contains the definition of a partial Agent class
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Jumper : Agent
{
    [SerializeField] private float jumpForce;
    [SerializeField] private KeyCode jumpKey;
    
    private bool jumpIsReady = true;
    private Rigidbody rBody;
    private Vector3 startingPosition;
    private int score = 0;
    public event Action OnReset;

    // OnReset event Action is set to the EventHandlder in Awake() of Spawner.cs attached to Spawner gameObj
    //   Awake()
    //{
    //        jumper = GetComponentInChildren<Jumper>();
    //        //Subscribes to Reset of Player
    //        jumper.OnReset += DestroyAllSpawnedObjects;

    //        StartCoroutine(nameof(Spawn));
    //}

    //public void Awake()
    //{
    //    rBody = GetComponent<Rigidbody>();
    //    startingPosition = transform.position;
    //}

    public override void Initialize()
    {
        rBody = this.gameObject.GetComponent<Rigidbody>();
        startingPosition = this.gameObject.transform.position;
    }

    private void Jump()
    {
        if (jumpIsReady)
        {
            rBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.VelocityChange);
            jumpIsReady = false;
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKey(jumpKey))
    //        Jump();
    //}


    public override void OnActionReceived(float[] vectorAction)
    {
        if (Mathf.FloorToInt( vectorAction[0] ) == 1)        // vectorAction[0] == 1 means Jump; vectorAction[0]=0 means no action
            Jump();
    }


    public override void Heuristic(float[] actionsOut)         // action selection heuristically
    {
        actionsOut[0] = 0;          // action 0 means do nothing

        if (Input.GetKey(jumpKey))
            actionsOut[0] = 1;      // action 1 means jump
    }

    private void FixedUpdate()     // FixedUpdate() is called  with the fixed time steps to decide whether or
                                  // not request a decision about the action to choose from the brain.
                                  //  The request is performed only when the agent is able to jump, that is, it is on the
                                  // ground rather than in the air.
    {
        if (jumpIsReady)
            RequestDecision();        // this enables m_RequestAction to be True so that the Academy.instance.AgentAct to really executeActions();
                                      // This in turn executes  actuator.OnActionReceived(new ActionBuffers(continuousActions, discreteActions))
                                      // which in turns calls  onActionReceived defined above within Jumper.cs which inherits Agent, which
                                      // inherits IActtionReceiver which defines OnActionReceived()

        //void AgentStep()
        //{
        //    if ((m_RequestAction) && (m_Brain != null))
        //    {
        //        m_RequestAction = false;
        //        m_ActuatorManager.ExecuteActions();
        //    }

        //    if ((m_StepCount >= MaxStep) && (MaxStep > 0))
        //    {
        //        NotifyAgentDone(DoneReason.MaxStepReached);
        //        _AgentReset();
        //    }
        //}

    }

    private void Reset()
    {
        score = 0;
        jumpIsReady = true;

        //Reset Movement and Position
        transform.position = startingPosition;
        rBody.velocity = Vector3.zero;

        OnReset?.Invoke();

      
    }


    //private void OnCollisionEnter(Collision collidedObj)
    //{
    //    if (collidedObj.gameObject.CompareTag("Street"))
    //        jumpIsReady = true;

    //    else if (collidedObj.gameObject.CompareTag("Mover") || collidedObj.gameObject.CompareTag("DoubleMover"))
    //        Reset();   // Destroy all the spawned objects and set score to 0. That is, if the agent is collided with spawned objects,
    //                   // game is over, and everything is reset. 
    //}

    //  OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
    private void OnCollisionEnter(Collision collision)      
        // collision is collision instance, rather than the collider
        //  The Collision class contains information, for example, about contact points and impact velocity.
        //  Notes: Collision events are only sent if one of the colliders also has a non-kinematic rigidbody attached.
        //  Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
    {
        if (collision.gameObject.CompareTag("Street"))
            jumpIsReady = true;

        else if (collision.gameObject.CompareTag("Mover") || collision.gameObject.CompareTag("DoubleMover"))
        {
            AddReward(-1.0f);       // increment reward by -1: Obstacles are considered harmful
            EndEpisode();
        }
    }



    //private void OnTriggerEnter(Collider collidedObj)
    //{
    //    if (collidedObj.gameObject.CompareTag("score"))
    //    {
    //        score++;
    //        ScoreCollector.Instance.AddScore(score);
    //    }
    //}

    //When a GameObject collides with another GameObject, Unity calls OnTriggerEnter.
    // Both GameObjects must contain a Collider component.
    //One must have Collider.isTrigger enabled, and contain a Rigidbody. 
    //If both GameObjects have Collider.isTrigger enabled, no collision happens.
    //The same applies when both GameObjects do not have a Rigidbody component.

    //Each car has a hidden cube behind itself that triggers the OnTriggerEnter() method on collision.
    //OnTriggerEnter is called when a collider enters a trigger(i.e.a collider with "IsTrigger" set).

    //True "collisions", used in conjunction with non-kinematic rigidbodies, 
    //are for when you want to simulate the physical effects of two objects colliding 
    //- the conservation of momentum, forces applied based on the angle of incidence etc.
    //OnCollisionEnter is called when these objects first hit each other and gives you properties of the point of collision, 
    //but the physics engine itself automatically resolves changes in motion of the two bodies etc.

    //OnTriggerEnter is used (with kinematic or non-kinematic rigidbodies) to detect when one object has entered another's trigger.
    //Note that, in this case, the physics engine does not prevent the object from doing so
    // - unlike a collider, you do not "bounce off" a trigger. It can be thought of as an invisible "zone", 
    //    and you can decide what happens to the object having entered that zone.

    //Triggers are effectively ignored by the physics engine, 
     //   and have a unique set of three trigger messages that are sent out when a collision with a Trigger occurs.
   // Either can be used for a fighting game - it really depends how you are planning to control the kinematics of the characters.


    // In this code, we have two games depending on whether OnTriggerEnter() or onCollisionEnter() is used. 

    private void OnTriggerEnter(Collider collidedObj)
    {
        if (collidedObj.gameObject.CompareTag("score"))       // the agent entered the trigger region of the "score obstacle": considered useful for scoring.
        {
            AddReward(0.1f); //New  
            score++;
            ScoreCollector.Instance.AddScore(score);
        }
    }

}
