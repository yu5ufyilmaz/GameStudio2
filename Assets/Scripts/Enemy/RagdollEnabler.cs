using UnityEngine;
using UnityEngine.AI;

public class RagdollEnabler : MonoBehaviour
{
    private Animator Animator;

    [SerializeField]
    private Transform RagdollRoot;

    private NavMeshAgent Agent;

    [SerializeField]
    private bool StartRagdoll = false;
    private Rigidbody[] Rigidbodies;
    private CharacterJoint[] Joints;

    private void Awake()
    {
        Rigidbodies = RagdollRoot.GetComponentsInChildren<Rigidbody>();
        Joints = RagdollRoot.GetComponentsInChildren<CharacterJoint>();
    }

    private void Start()
    {
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        if (StartRagdoll)
        {
            EnableRagdoll();
        }
        else
        {
            EnableAnimator();
        }
    }

    public void EnableRagdoll()
    {
        Animator.enabled = false;
        Agent.enabled = false;
        foreach (CharacterJoint joint in Joints)
        {
            joint.enableCollision = true;
        }
        foreach (Rigidbody rigidbody in Rigidbodies)
        {
            // rigidbody.linearVelocity = Vector3.zero;
            rigidbody.detectCollisions = true;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
        }
    }

    public void DisableAllRigidbodies()
    {
        foreach (Rigidbody rigidbody in Rigidbodies)
        {
            rigidbody.detectCollisions = false;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = true;
        }
    }

    public void EnableAnimator()
    {
        Animator.enabled = true;
        Agent.enabled = true;
        foreach (CharacterJoint joint in Joints)
        {
            joint.enableCollision = false;
        }
        foreach (Rigidbody rigidbody in Rigidbodies)
        {
            //rigidbody.detectCollisions = false;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }
    }
}
