using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbElementSelector : MonoBehaviour
{
    public ISpellCaster spellCaster;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0, 0, 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        FingerElement fingerElement = other.GetComponent<FingerElement>();

        if (fingerElement != null)
        {
            Debug.Log("FINGERTIP collided " + fingerElement);

            this.GetComponent<SpriteRenderer>().color = fingerElement.color;

            if (spellCaster != null)
            {
                Debug.Log("FINGERTIP: Element selected: " + fingerElement.type);
                spellCaster.PublishGesture(new Gesture(fingerElement.type, fingerElement.hand));
            }
        }
    }
}
