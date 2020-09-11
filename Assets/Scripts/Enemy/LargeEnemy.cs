using UnityEngine;
using UnityEngine.UI;

public class LargeEnemy : Enemy
{
    public Mask mask;
    private float originalSize;
    private float wakeTime = 20f;
    private float wakeDuration;
    private float precentage;
    private State state;

    private GameObject target;
    
    enum State
    {
        
        sleep,
        active
    }
    private void Start()
    {
        originalSize = mask.rectTransform.rect.width;
        animator = GetComponent<Animator>();
        state = State.sleep;
        target = null;
    }
    public void SetValue(float value)
    {				      
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize * value);
    }

    public void SetWakeDuration(float time)
    {
        wakeDuration = time;
        precentage = Mathf.Clamp(wakeDuration / wakeTime, 0, 1f);
        SetValue(precentage);
        if (precentage == 1)
        {
            SetActive();
        }
    }
    
    public float GetWakeDuration()
    {
        return wakeDuration;
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }    
    public void SetActive()
    {
        //激活怪物
        transform.LookAt(target.transform);
        animator.SetTrigger("Active");
    }

    public void KillCharacter()
    {
        Debug.Log("kill");
        target.GetComponent<MovementInput>().ChangeHealth(-5);
    }
}
