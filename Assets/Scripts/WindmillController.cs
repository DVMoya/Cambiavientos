using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillController : MonoBehaviour
{
    public float velocity = 10f;
    // 10f --> idle velocity
    // 600f --> tornado velocity

    void Update()
    {
        transform.Rotate(Vector3.forward * velocity * Time.deltaTime);
    }
}
