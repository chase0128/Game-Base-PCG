using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Animator))]
public class Actions : MonoBehaviour {

	private Animator animator;
	public  bool isAiming;
	const int countOfDamageAnimations = 3;
	int lastDamageAnimation = -1;
	public bool isSitting;
	void Awake () {
		animator = GetComponent<Animator> ();
	}

	public void Stay () {
		animator.SetBool("Aiming", false);
		animator.SetFloat ("Speed", 0f);
		isAiming = false;
	}

	public void Walk () {
		animator.SetBool("Aiming", false);
		animator.SetFloat ("Speed", 0.5f);
		isAiming = false;
	}

	public void Run (float speed) {
		animator.SetBool("Aiming", false);
		animator.SetFloat ("Speed", speed);
		isAiming = false;
	}

	public void Attack () {
		//Aiming ();
		animator.SetTrigger ("Attack");
	}

	public void Death () {
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Death"))
			animator.Play("Idle", 0);
		else
			animator.SetTrigger ("Death");
		isSitting = false;
		isAiming = false;
	}

	public void Damage () {
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Death")) return;
		int id = Random.Range(0, countOfDamageAnimations);
		if (countOfDamageAnimations > 1)
			while (id == lastDamageAnimation)
				id = Random.Range(0, countOfDamageAnimations);
		lastDamageAnimation = id;
		animator.SetInteger ("DamageID", id);
		animator.SetTrigger ("Damage");
	}

	public void Jump () {
		animator.SetBool ("Squat", false);
		animator.SetFloat ("Speed", 0f);
		animator.SetBool("Aiming", false);
		animator.SetTrigger ("Jump");
		isAiming = false;
		isSitting = false;
	}

	public void Aiming () {
		//animator.SetBool ("Squat", false);
		animator.SetBool("Squat", false);
		animator.SetFloat ("Speed", 0f);
		animator.SetBool("Aiming", !animator.GetBool("Aiming"));
		isAiming = true;
		isSitting = false;
	}

	public void Sitting () {
		animator.SetBool ("Squat", !animator.GetBool("Squat"));
		animator.SetBool("Aiming", false);
		isAiming = false;
		isSitting = true;
	}
}
