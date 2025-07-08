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

    private void Start()
    {
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

        StartCoroutine(MoveMovable(duration));
    }

    IEnumerator MoveMovable(float duration)
    {
        Debug.Log("Moving piston");

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            int i = 0;
            foreach (var movable in movables)
            {
                Vector3 pos = movable.transform.position;
                float initialPos = pos.y;
                float targetPos = movePos[i];
                if (isOff) targetPos = startingPos[i];

                pos = new Vector3(pos.x, Mathf.Lerp(initialPos, targetPos, t), pos.z);

                movable.transform.position = pos;

                i++;
            }

            yield return null;
        }

        isMoving = false;
    }

}
