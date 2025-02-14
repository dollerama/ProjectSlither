using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public Vector3 follow;
    public float damp;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, follow, Time.deltaTime*damp);
    }
}
