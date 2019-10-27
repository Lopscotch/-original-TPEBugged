using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour
{
    public Animator animator;

    private bool isScope = false;

    void Update ()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            isScope = !isScope;
            animator.SetBool("Scope", isScope);
        }
    }

}
