using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
#pragma warning disable
    [SerializeField] private GameOver gameOver;
#pragma warning restore


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grunt"))
        {
            gameOver.StartGameOver();
        }
    }
}
