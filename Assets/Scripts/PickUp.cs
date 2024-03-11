using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum itemType
    {
        heal,
        coin
    }

    public itemType item;

    public int value=20;

    public ParticleSystem collectedVFX;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.GetComponent<Character>().PickUpItem(this);
            
            if (collectedVFX != null)
            {
                Instantiate(collectedVFX, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }

        
    }
}
