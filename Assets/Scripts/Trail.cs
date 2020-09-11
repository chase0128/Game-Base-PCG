using System;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public LineRenderer renderer;
    [HideInInspector]
    public Vector3 origin;
    [HideInInspector]
    public Vector3 target;
    [HideInInspector]
    public Vector3 direction;

    private float maginute;
    
    private float speed = 40;
    private float length = 1f;
    private Vector3 currentPosition;

    public GameObject impactEffect;
    public ParticleSystem effect;
    public enum State
    {
        idle,
        active,
        explosion
    }
    
    public State state
    {
        get;
        set;
    }

    public void SetActive(Vector3 origin,Vector3 target)
    {
        state = State.active;
        this.origin = origin;
        this.target = target;
        direction = (target - origin).normalized;
        currentPosition = origin + direction * length;
        maginute = (target - origin).magnitude;
        renderer.enabled = true;
        renderer.SetPosition(0, origin);
        renderer.SetPosition(1, currentPosition);

    }
    
    public void Renderer()
    {
        Vector3 pos = gameObject.transform.position;
        renderer.SetPosition(0, currentPosition - direction * length);
        renderer.SetPosition(1, currentPosition);
    }
    private void Update()
    {
        if (state == State.active)
        {
            currentPosition = currentPosition + direction * Time.deltaTime * speed;
            Renderer();
            if ((currentPosition - origin).magnitude > (target - origin).magnitude)
            {
                renderer.enabled = false;
                state = State.explosion;
                impactEffect = Instantiate(impactEffect, target, impactEffect.transform.rotation);
                impactEffect .transform.LookAt(origin);
                effect = impactEffect .GetComponent<ParticleSystem>();
                effect.Play();
            }
        }

        if (state == State.explosion)
        {
            if (effect.isPaused || effect.isStopped)
            {
                state = State.idle;
                Destroy(impactEffect);
                Destroy(gameObject);
            }
        }
    }
}
