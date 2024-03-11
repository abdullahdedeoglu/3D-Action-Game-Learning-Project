using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class EnemyVfxManager : MonoBehaviour
{
    public VisualEffect burstFootStep;
    public VisualEffect enemyAttack;
    public ParticleSystem beingHitParticle;
    public VisualEffect beingHitSplashVFX;

    public void EnemyAttack()
    {
        enemyAttack.SendEvent("OnPlay");
    }
    public void BurstFootStep()
    {
        burstFootStep.SendEvent("OnPlay");
    }

    public void BeingHit(Vector3 attackerPos)
    {
        Vector3 hitPos = attackerPos - transform.position;
        hitPos.Normalize();
        hitPos.y = 0;
        beingHitParticle.transform.rotation = Quaternion.LookRotation(hitPos);
        beingHitParticle.Play();

        Vector3 splashPos = transform.position + new Vector3(0, 2f, 0);
        VisualEffect newSplashVFX = Instantiate(beingHitSplashVFX, splashPos, Quaternion.identity);
        newSplashVFX.Play();
        Destroy(newSplashVFX, 10f);

    }
}
