using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    private MeshRenderer mesh;

    public float maxHealth;
    public float health;
    public float displayHealth;
    public AnimationCurve healthLerpCurve;
    public bool alive;

    [SerializeField]
    private float damageAnimDuration;
    public AnimationCurve healthAnimCurve;

    [SerializeField]
    private Material deadCharMat;
    Material defMat;
    
    public UnityEvent onDeath;

    void Awake()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
    }

    void Start()
    {
        health = maxHealth;
        alive = true;
        defMat = mesh.material;
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
        Debug.Log("Damage: " + damage);
        health -= damage;
        mesh.material = defMat;
        StopCoroutine(DamageAnim(0));
        StartCoroutine(DamageAnim(damage));
    }

    IEnumerator DamageAnim(float damage)
    {
        Material damageMat = new Material(defMat.shader);
        damageMat.color = Color.red;
        mesh.material = damageMat;

        float startTime = Time.time;
        while (Time.time - startTime < damageAnimDuration)
        {
            float prog = (Time.time - startTime) / damageAnimDuration;
            damageMat.color = Color.Lerp(Color.red, defMat.color, healthAnimCurve.Evaluate(prog));
            yield return new WaitForEndOfFrame();
        }

        mesh.material = defMat;
    }

    void Kill()
    {
        mesh.material = deadCharMat;
        onDeath.Invoke();
    }
}
