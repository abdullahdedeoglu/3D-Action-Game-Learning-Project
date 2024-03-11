using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed = 80f;
    void Update()
    {
        transform.Rotate(new Vector3(0, rotationSpeed*Time.deltaTime, 0), Space.World);
    }
}
