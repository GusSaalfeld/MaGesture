using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorShower : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private float spawnRadius;
    [SerializeField] private int spawnNumber;
    [SerializeField] private float duration;
    [SerializeField] private float startDelay;
    [SerializeField] private GameObject meteor;
#pragma warning restore 0649

    private void Start()
    {
        StartCoroutine(spawner());
    }

    private IEnumerator spawner()
    {
        yield return new WaitForSeconds(startDelay);
        for(float i = 0; i < spawnNumber; i ++)
        {
            GameObject tmp = Instantiate(meteor);
            Vector2 pt = Random.insideUnitCircle * spawnRadius;
            tmp.transform.position = new Vector3(transform.position.x + pt.x, transform.position.y, transform.position.z + pt.y);
            tmp.SetActive(true);
            yield return new WaitForSeconds(duration / spawnNumber);
        }

        //TODO
        //Run ending animation
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
