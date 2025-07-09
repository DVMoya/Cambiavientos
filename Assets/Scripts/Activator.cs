using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Activator : MonoBehaviour
{
    bool isOff = true;
    [HideInInspector] public bool isMoving = false;
    public float[] movePos;
    private float[] startingPos;

    [SerializeField]
    GameObject[] movables;

    private AudioSource audioThunder;

    private void Start()
    {
        audioThunder = GetComponentInChildren<AudioSource>();

        InstantReset();
    }

    private void InstantReset()
    {
        isOff = true;

        startingPos = new float[movables.Length + 1];

        int i = 0;
        foreach (var movable in movables)
        {
            startingPos[i] = movable.transform.position.y;
            i++;
        }
    }

    public void SoftMove(float duration)
    {
        isOff = !isOff;

        audioThunder.volume = 2f;
        audioThunder.pitch = Random.Range(0.9f, 1.1f);
        audioThunder.spatialBlend = 1f;
        audioThunder.Play();

        StartCoroutine(MoveMovable(duration-0.5f)); 
        // --> Hago que dure un poco para que el movimiento no coincida exáctamente con el impacto del rayo
    }

    IEnumerator MoveMovable(float duration)
    {
        float time = 0f;
        isMoving = true;

        // Coge la posicion inicial y objetivo
        List<float> initialPositions = new List<float>();
        List<float> targetPositions = new List<float>();

        for (int i = 0; i < movables.Length; i++)
        {
            float initialY = movables[i].transform.position.y;
            float targetY = isOff ? startingPos[i] : movePos[i];

            initialPositions.Add(initialY);
            targetPositions.Add(targetY);
        }

        // Mueve el objeto activado a la nueva posición en duration segundos
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            for (int i = 0; i < movables.Length; i++)
            {
                Vector3 pos = movables[i].transform.position;
                float newY = Mathf.Lerp(initialPositions[i], targetPositions[i], t);
                movables[i].transform.position = new Vector3(pos.x, newY, pos.z);

                movables[i].GetComponent<Piston>()?.PlayMove();
            }

            yield return null;
        }

        // Asegura que la posición final es exacta
        for (int i = 0; i < movables.Length; i++)
        {
            Vector3 pos = movables[i].transform.position;
            float finalY = targetPositions[i];
            movables[i].transform.position = new Vector3(pos.x, finalY, pos.z);

            movables[i].GetComponent<Piston>()?.PlayStay();
        }

        isMoving = false;
    }

}
