using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Collider> RagdollParts = new List<Collider>();
    private List<Rigidbody> RagdollBodies = new List<Rigidbody>();
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        SetRagdollParts();
        SetRagdoll(false);
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    private void SetRagdollParts()
    {

        RagdollParts.Clear();
        RagdollBodies.Clear();
        Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();
        Rigidbody[] bodies = this.gameObject.GetComponentsInChildren<Rigidbody>();

        foreach(Rigidbody body in bodies)
        {
            if (body.gameObject != this.gameObject)
            {
                //collider.isTrigger = true;
                RagdollBodies.Add(body);
            }
        }


        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != this.gameObject)
            {
                //collider.isTrigger = true;
                RagdollParts.Add(collider);
            }
            

        }
    }

    public void SetRagdoll(bool active)
    {
        
        if (animator != null)
            animator.enabled = !active;

        
        foreach (var rb in RagdollBodies)
            rb.isKinematic = !active;

        
        foreach (var col in RagdollParts)
            col.isTrigger = false;
    }


}
