using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    private CharacterController controller;

    // Movement Variables
    public float moveSpeed = 5f;
    public float slideSpeed = 9f;
    private Vector3 movementVelocity;
    private Vector3 lookRotation;
    private PlayerInput playerInput;


    // Gravity Variables
    private float verticalVelocity;
    public float gravity = -9.8f;

    // Animation
    private Animator animator;
    private float attackAnimationDuration;

    // Enemy Variables
    public bool isPlayer;
    private NavMeshAgent navMeshAgent;
    public Transform target;
    public float enemySpeed = 4f;

    //State Machine
    public enum characterState
    {
        normalState,
        attackState,
        deathState,
        beingHitState,
        slideState,
        spawnState
    }

    public characterState currentState;

    // Player Slide Variables
    private float attackStartTime;
    public float attackSlideDuration = 0.4f;
    public float attackSlideSpeed = 0.06f;

    // Health
    private Health health;

    // Damage Caster
    public DamageCaster damageCaster;

    // Material Variables
    MaterialPropertyBlock materialPropertyBlock;
    SkinnedMeshRenderer skinnedMeshRenderer;

    // ItemToDrop
    public GameObject itemToDrop;
    public int coin;

    // Impact
    private Vector3 impactOnCharacter;

    // Invincibility
    private bool Invincible;
    public float invincibleDuration = 2f;

    // Spawn State Variables
    private float spawnDuration = 2f;
    private float currentSpawnTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        damageCaster = GetComponentInChildren<DamageCaster>();

        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
        skinnedMeshRenderer.GetPropertyBlock(materialPropertyBlock);

        if(isPlayer )
        {
            playerInput = GetComponent<PlayerInput>();
        }
        else
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            target = GameObject.FindWithTag("Player").transform;
            navMeshAgent.speed = enemySpeed;
            SetCharacterState(characterState.spawnState);
        }
    }
    private void CalculateMovement()
    {
        if (playerInput.mouseButtonDown && controller.isGrounded)
        {
            SetCharacterState(characterState.attackState);
            return;
        }

        else if (playerInput.spaceKeyDown && controller.isGrounded)
        {
            SetCharacterState(characterState.slideState);
            return;
        }

        movementVelocity.Set(playerInput.horizontalInput, 0f, playerInput.verticalInput);
        movementVelocity.Normalize();
        movementVelocity = Quaternion.Euler(0, -45f, 0) * movementVelocity;
        movementVelocity *= moveSpeed * Time.deltaTime;

        animator.SetFloat("Speed", movementVelocity.magnitude);
        animator.SetBool("Airborne", !controller.isGrounded);

        if (movementVelocity != Vector3.zero)
            lookRotation = movementVelocity;
        transform.rotation = Quaternion.LookRotation(lookRotation);
    }
    private void CalculateEnemyMovement()
    {
        if(Vector3.Distance(target.position, transform.position) > navMeshAgent.stoppingDistance)
        {
            navMeshAgent.SetDestination(target.position);
            animator.SetFloat("Speed", 0.1f);
        }
        else
        {
            navMeshAgent.SetDestination(transform.position);
            animator.SetFloat("Speed", 0);

            SetCharacterState(characterState.attackState);
        }
    }
    private void FixedUpdate()
    {
        switch (currentState)
        {
            case characterState.normalState:
                if (isPlayer)
                {
                    CalculateMovement();
                }
                else
                {
                    CalculateEnemyMovement();
                }
                break;
            case characterState.attackState:
                if(isPlayer)
                {
                    if (Time.time < attackStartTime + attackSlideDuration)
                    {
                        float timePassed = Time.time - attackStartTime;
                        float lerpTime = timePassed / attackSlideDuration;
                        movementVelocity = Vector3.Lerp(transform.forward * attackSlideSpeed, Vector3.zero, lerpTime);
                    }

                    if(playerInput.mouseButtonDown && controller.isGrounded)
                    {
                        string currentClipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                        attackAnimationDuration = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                        if(currentClipName != "LittleAdventurerAndie_ATTACK_03" && attackAnimationDuration>0.5 && attackAnimationDuration<0.7)
                        {
                            playerInput.mouseButtonDown = false;
                            SetCharacterState(characterState.attackState);
                            CalculateMovement();
                        }
                    }
                }
                break;
            case characterState.deathState:
                return;
            case characterState.beingHitState:
                if (impactOnCharacter.magnitude > 0.2f)
                {
                    movementVelocity = impactOnCharacter * Time.deltaTime;
                }
                impactOnCharacter = Vector3.Lerp(impactOnCharacter, Vector3.zero, Time.deltaTime * 5);
                break;
            case characterState.slideState:
                movementVelocity = transform.forward * slideSpeed * Time.deltaTime;
                break;
            case characterState.spawnState:
                currentSpawnTime -= Time.deltaTime;
                if(currentSpawnTime <=0)
                {
                    SetCharacterState(characterState.normalState);
                }
                break;
        }
        if (isPlayer)
        {
            MoveCharacter();
            movementVelocity = Vector3.zero;
        }
    }
    private void MoveCharacter()
    {
        if (!controller.isGrounded)
        {
            verticalVelocity = gravity;
        }
        else
            verticalVelocity = gravity * 0.3f;

        movementVelocity += verticalVelocity * Vector3.up * Time.deltaTime;
        controller.Move(movementVelocity);
    }
    public void SetCharacterState(characterState newState)
    {
        if(isPlayer)
            playerInput.CleanCache();

        // Exiting state
        switch (currentState)
        {
            case characterState.normalState:
                break;
            case characterState.attackState:
                if(damageCaster != null)
                {
                    damageCaster.DisableDamageCaster();
                }
                if (isPlayer)
                {
                    GetComponent<PlayerVfxManager>().StopBlade();
                }
                break;
            case characterState.deathState:
                return;
            case characterState.beingHitState:
                break;
            case characterState.slideState:
                break;
            case characterState.spawnState:
                Invincible = false;
                break;
        }

        //Entering State
        switch (newState)
        {
            case characterState.normalState:
                break;
            case characterState.attackState:
                animator.SetTrigger("Attack");
                if(isPlayer)
                {
                    attackStartTime = Time.time;
                }
                if (!isPlayer)
                {
                    Quaternion newRotation = Quaternion.LookRotation(target.position - transform.position);
                    transform.rotation = newRotation;
                }
                break;
            case characterState.deathState:
                controller.enabled = false;
                //damageCaster.enabled = false;
                animator.SetTrigger("Death");
                StartCoroutine(DissolveAnimation());
                break;
            case characterState.beingHitState:
                animator.SetTrigger("BeingHit");
                if (isPlayer)
                {
                    Invincible = true;
                    StartCoroutine(CharacterInvincible());
                }
                break;
            case characterState.slideState:
                animator.SetTrigger("Slide");
                break;
            case characterState.spawnState:
                Invincible = true;
                currentSpawnTime = spawnDuration;
                StartCoroutine(AppearAnimation());
                break;
        }

        currentState = newState;

        Debug.Log("Switched to " + currentState);
    }
    public void AttackAnimationEnds()
    {
        SetCharacterState(characterState.normalState);
    }

    public void SlideAnimationEnds()
    {
        SetCharacterState(characterState.normalState);
    }
    public void BeingHitAnimationEnds()
    {
        if (currentState == characterState.deathState)
        {
            SetCharacterState(characterState.deathState);
        }
        else
            SetCharacterState(characterState.normalState);
    }
    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {
        if (Invincible)
        {
            return;
        }
        if (health != null)
        {
            health.ApplyDamage(damage);
        }
        if (!isPlayer)
        {
            EnemyVfxManager enemyVfxManager = GetComponent<EnemyVfxManager>();
            enemyVfxManager.BeingHit(attackerPos);
        }
        if (isPlayer)
        {
            SetCharacterState(characterState.beingHitState);
            HitImpact(attackerPos, 10f);
        }
        StartCoroutine(BlinkAnimation());
    }

    IEnumerator CharacterInvincible()
    {
        yield return new WaitForSeconds(invincibleDuration);

        Invincible = false;
    }
    private void HitImpact(Vector3 attackerPos, float impactPower)
    {
        Vector3 impactDir = transform.position - attackerPos;
        impactDir.Normalize();
        impactDir.y = 0;

        impactOnCharacter = impactDir * impactPower;
    }
    public void EnableDamageCaster()
    {
        damageCaster.EnableDamageCaster();
    }
    public void DisableDamageCaster()
    {
        damageCaster.DisableDamageCaster();
    }
    IEnumerator BlinkAnimation()
    {
        materialPropertyBlock.SetFloat("_blink", 0.4f);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);

        yield return new WaitForSeconds(0.2f);

        materialPropertyBlock.SetFloat("_blink", 0f);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
        
    }
    IEnumerator DissolveAnimation()
    {
        yield return new WaitForSeconds(2);

        float dissolveTimeDuration = 2f;
        float currentDisolveTime = 0;
        float dissolveHeightStart = 20f;
        float dissolveHeightTarget = -10f;
        float dissolveHeight;

        materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);

        while (currentDisolveTime < dissolveTimeDuration)
        {
            currentDisolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHeightStart, dissolveHeightTarget, currentDisolveTime / dissolveTimeDuration);
            materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
            yield return null;
        }

        DropItem();
    }

    IEnumerator AppearAnimation()
    {
        float appearTimeDuration = 2f;
        float currentAppearTime = 0;
        float dissolveHeightStart = -20f;
        float dissolveHeightTarget= 10f;
        float dissolveHeight;

        materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);

        while (currentAppearTime < appearTimeDuration)
        {
            currentAppearTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHeightStart, dissolveHeightTarget, currentAppearTime / appearTimeDuration);
            materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
            yield return null;
        }

        materialPropertyBlock.SetFloat("_enableDissolve", 0f);
        skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    public void DropItem()
    {
        if(itemToDrop!=null)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.identity);
        }
    }

    public void PickUpItem(PickUp item)
    {
        switch (item.item)
        {
            case PickUp.itemType.heal:
                PickUpHeal(item.value);
                break;
            case PickUp.itemType.coin:
                PickUpCoin(item.value);
                break;
        }
    }

    public void PickUpCoin(int addCoinAmount)
    {
        coin += addCoinAmount;
    }

    public void PickUpHeal(int addHealth)
    {
        GetComponent<PlayerVfxManager>().PlayPickUpHeal();
        health.AddHealth(addHealth);
    }

    public void RotateToTarget()
    {
        if(currentState != characterState.deathState)
        {
            transform.LookAt(target, Vector3.up);
        }

    }
}
