using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] // para poder trabajar con esto en el editor
public class CloudGenerator : MonoBehaviour
{
    public int horizontalStackSize = 20;
    public float cloudHeight;
    public Mesh quadMesh;
    public Material cloudMaterial;
    float offset;

    public int layer;
    public Camera cam;
    private Matrix4x4 matrix;

    void LateUpdate()
    {
        cloudMaterial.SetFloat("_MidYvalue", transform.position.y);
        cloudMaterial.SetFloat("_CloudHeight", cloudHeight);

        offset = cloudHeight / horizontalStackSize / 2f;
        Vector3 startPosition = transform.position + (Vector3.up * (offset * horizontalStackSize / 2f));

        for (int i = 0; i < horizontalStackSize; i++)
        {
            matrix = Matrix4x4.TRS(startPosition - (Vector3.up * offset * i), transform.rotation, transform.localScale * 10);
            Graphics.DrawMesh(quadMesh, matrix, cloudMaterial, layer, cam, 0, null, true, false, false);
        }
    }
}
