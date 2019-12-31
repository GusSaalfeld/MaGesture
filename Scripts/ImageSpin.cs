using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed;
    

    private void Update()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + spinSpeed);
        if (transform.rotation.z > 720 || transform.rotation.z < -720)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z % 360);

    }
}
