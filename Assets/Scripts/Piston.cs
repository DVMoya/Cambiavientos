using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piston : MonoBehaviour
{

    [SerializeField] AudioSource audioMove;
    [SerializeField] AudioSource audioStay;

    public void PlayMove() 
    {
        if (!audioMove.isPlaying)
        {
            audioMove.spatialBlend = 1f;
            audioMove.Play();
        }
    }
    public void PlayStay() 
    { 
        if(!audioStay.isPlaying) 
        {
            audioStay.spatialBlend = 1f;
            audioStay.Play(); 
        }
    }
}
