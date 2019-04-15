﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    private MeshRenderer mesh;

    [Header("Basic Variables")]
    public float maxHealth;
    public float health;
    public float displayHealth;
    public AnimationCurve healthLerpCurve;
    public bool alive;
    
    [Header("Damage Animation")]
    [SerializeField]
    private int damageAnimState;
    [SerializeField]
    private float damageAnimDuration;
    [SerializeField]
    private AnimationCurve healthAnimCurve;
    
    public Material deadCharMat;
    Material defaultMat;
    
    public UnityEvent onDeath;

    void Awake()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
    }

    void Start()
    {
        health = maxHealth;
        alive = true;
        defaultMat = mesh.material;
        onDeath.AddListener(((FlagBaseController)FindObjectOfType(typeof(FlagBaseController))).OnCarryerDie);
    }

    void Update()
    {
        if (health <= 0)
        {
            alive = false;
            Kill();
        }
        displayHealth = Mathf.Lerp(displayHealth, health, .2f);
    }

    public void Damage(float damage)
    {
        health -= damage;
        mesh.material = defaultMat;
        StopCoroutine(DamageAnim(0));
        StartCoroutine(DamageAnim(damage));
    }

    // TODO: do this without a coroutine
    IEnumerator DamageAnim(float damage)
    {
        Material damageMat = new Material(defaultMat.shader);
        damageMat.color = Color.red;
        mesh.material = damageMat;

        float startTime = Time.time;
        while (Time.time - startTime < damageAnimDuration)
        {
            float prog = (Time.time - startTime) / damageAnimDuration;
            damageMat.color = Color.Lerp(Color.red, defaultMat.color, healthAnimCurve.Evaluate(prog));
            yield return new WaitForEndOfFrame();
        }

        mesh.material = defaultMat;
    }

    void Kill()
    {
        mesh.material = deadCharMat;
        GetComponent<CharacterMotion>().disableMovement = true;
        onDeath.Invoke();
    }

    //This onTriggerEnter function allows the grenades to damage anything that has this script
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<Grenade>() != null)
        {
            Grenade grenade = other.gameObject.GetComponent<Grenade>();
            float grenadeDamage = grenade.damage;
            Debug.Log("DAMAGE");
            Damage(grenadeDamage);
        }
    }
}
