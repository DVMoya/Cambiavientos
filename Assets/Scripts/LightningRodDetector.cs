using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LightningRodDetector : MonoBehaviour
{
    // Lista pública para ver qué pararayos están dentro
    public List<GameObject> lightningRodsInRange = new List<GameObject>();

    private CapsuleCollider capsule;

    void Start()
    {
        capsule = GetComponent<CapsuleCollider>();

        // Obtenemos los colliders que están dentro del volumen del capsule
        Vector3 point1 = transform.position + capsule.center + transform.up * (capsule.height / 2 - capsule.radius);
        Vector3 point2 = transform.position + capsule.center - transform.up * (capsule.height / 2 - capsule.radius);
        Collider[] hits = Physics.OverlapCapsule(point1, point2, capsule.radius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("LightningRod") && !lightningRodsInRange.Contains(hit.gameObject))
            {
                lightningRodsInRange.Add(hit.gameObject);
                Debug.Log($"Detectado al iniciar: {hit.name}");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LightningRod") && !lightningRodsInRange.Contains(other.gameObject))
        {
            lightningRodsInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LightningRod") && lightningRodsInRange.Contains(other.gameObject))
        {
            lightningRodsInRange.Remove(other.gameObject);
        }
    }
}
