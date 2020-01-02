using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRNGForce : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Rigidbody>().AddTorque(Random.Range(0, 200), Random.Range(0, 200), Random.Range(0, 200), ForceMode.Force);
    }
}
