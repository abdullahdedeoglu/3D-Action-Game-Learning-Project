using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCaster : MonoBehaviour
{
    private Collider damageCasterCollider;
    public int damage = 30;
    public string targetTag;
    private List<Collider> damageTargetList = new List<Collider>();

    private void Awake()
    {
        damageCasterCollider = GetComponent<Collider>();
        damageCasterCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == targetTag && !damageTargetList.Contains(other))
        {
            Character targetCharacter = other.GetComponent<Character>();

            if (targetCharacter != null)
            {
                targetCharacter.ApplyDamage(damage,transform.parent.position);

                PlayerVfxManager playerVfxManager = transform.parent.GetComponent<PlayerVfxManager>();
                if (playerVfxManager != null)
                {
                    RaycastHit hit;

                    Vector3 originalPos = transform.position + damageCasterCollider.bounds.extents.z * transform.forward;

                    bool isHit = Physics.BoxCast(originalPos, damageCasterCollider.bounds.extents / 2, transform.forward, out hit, Quaternion.identity, damageCasterCollider.bounds.extents.z, 1 << 6);

                    if (isHit)
                    {
                        playerVfxManager.PlaySlash(hit.point + new Vector3(0, 0.5f, 0));
                    }
                }
            }

            damageTargetList.Add(other);
        }
    }

    public void EnableDamageCaster()
    {
        damageTargetList.Clear();
        damageCasterCollider.enabled=true;
    }

    public void DisableDamageCaster()
    {
        damageTargetList.Clear();
        damageCasterCollider.enabled = false;
    }

}
