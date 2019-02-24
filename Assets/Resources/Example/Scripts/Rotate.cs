using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    public float degsPerSecond = -90f;
    public Vector3 direction = new Vector3(0, 0, 1);

    void Update()
    {
        transform.Rotate(direction, degsPerSecond * Time.deltaTime);
    }
}
