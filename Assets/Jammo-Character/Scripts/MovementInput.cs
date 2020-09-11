using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//This script requires you to have setup your animator with 3 parameters, "InputMagnitude", "InputX", "InputZ"
//With a blend tree to control the inputmagnitude and allow blending between animations.
[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour {

	public static MovementInput instance
	{
		 get;
		private set;
	}
	
	
    public float Velocity;
    [Space]

	public float InputX;
	public float InputZ;
	public Vector3 desiredMoveDirection;
	public bool blockRotationPlayer;
	public float desiredRotationSpeed = 0.1f;
	public Animator anim;
	public float Speed;
	public float allowPlayerRotation = 0.1f;
	public Camera cam;
	public CharacterController controller;
	public bool isGrounded;
	
	
	//人物状态
	public State state;
	public State preState;
    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float HorizontalAnimSmoothTime = 0.2f;
    [Range(0, 1f)]
    public float VerticalAnimTime = 0.2f;
    [Range(0,1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;

    public float verticalVel;
    private Vector3 moveVector;
    
    //身体的材质，用来把身体设置成透明状态
    public Material bodyMaterial;
    public float transparetTime;
	
    //Jump
    public float jumpTime = 0.5f;
    private float durarion = 0;
    private bool isJump = false;
    private int iTime = 0;
    private float jumpSpeed = 3.5f;
    private float currentSpeed = 3.5f;
    
    // if transparent
    public bool isTransparent = false;
    private int  transparentCD = 30;
    private float cdTimer = 0;
    private bool isInCd;

    public Text cdText;
	    
    //血量
    private int maxHealth = 5;
    private int currentHealth;
    
    private float normalHeight = 1.9f;
    private float sitHeight =0.5f;
    private float normalCenterY = 1.2f ;
    private float sitCenterY = 0.4f;
    //附近是否有水晶
    private bool  isAroundCrystal = false;
    private GameObject crystal = null;
    private float gatherDuration;
    private float gatherTime = 5f;
    private int needCrystals = 5;
    private int ownCrystal = 0;
    
    //是否被照射到
    private bool easyFound = false;
    private float easyFoundDuration = 0f;
    private float easyFoundTime = 10f;
    public GameObject attentionIcon;
    
    public enum State
    {
	    idle,
	    jumping,
	    getDown,
	    injured,
	    attack,//攻击状态，用来区分玩家是否处于攻击动作，过滤重复攻击
	    gather,//收集水晶
    }
    
	// Use this for initialization
	void Start () {
		anim = this.GetComponent<Animator> ();
		cam = Camera.main;
		controller = this.GetComponent<CharacterController> ();
		state = State.idle;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		currentHealth = maxHealth;
		ownCrystal = 0;
		instance = this;
		easyFound = false;
		attentionIcon.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		TransparentCD();
		EasyFounding();
		InputMagnitude ();
		GetInput();
		Jump();
    }
	/// <summary>
	/// 玩家跳跃
	/// </summary>
	void Jump()
	{
		moveVector = Vector3.zero;
		isGrounded = controller.isGrounded;
		if (isGrounded || controller.collisionFlags == CollisionFlags.Below)
		{
			verticalVel = 0;
		}
		else
		{
			verticalVel -= 1;
		}
		if (isJump)
		{
			durarion = durarion + Time.deltaTime;
			iTime++;
			moveVector += new Vector3(0, currentSpeed * Time.deltaTime , 0);
			currentSpeed *= 0.99f;
			controller.Move(moveVector);
			if (durarion >= jumpTime)
			{
				isJump = false;
				state = preState;
			}
		}
		else
		{
			if (!isGrounded)
			{
				moveVector += new Vector3(0, verticalVel * .05f * Time.deltaTime, 0);
				controller.Move(moveVector);
			}
		}
	}
	
	
    void PlayerMoveAndRotation() {
		InputX = Input.GetAxis ("Horizontal");
		InputZ = Input.GetAxis ("Vertical");

		var camera = Camera.main;
		var forward = cam.transform.forward;
		var right = cam.transform.right;

		forward.y = 0f;
		right.y = 0f;

		forward.Normalize ();
		right.Normalize ();

		desiredMoveDirection = forward * InputZ + right * InputX + Vector3.up * -0.01f;
		desiredMoveDirection = desiredMoveDirection.normalized;
		if (blockRotationPlayer == false) {
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (desiredMoveDirection), desiredRotationSpeed);
			if (state == State.getDown)
			{
				controller.Move(desiredMoveDirection * Time.deltaTime * Velocity / 5f);
			}
			else
			{
				if (anim.GetBool("Hurt"))
				{
					controller.Move(desiredMoveDirection * Time.deltaTime * Velocity/2f);

				}
				else
				{
					controller.Move(desiredMoveDirection * Time.deltaTime * Velocity);
				}
				
			}
			
		}
	}

    public void LookAt(Vector3 pos)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), desiredRotationSpeed);
    }

    public void RotateToCamera(Transform t)
    {

        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        desiredMoveDirection = forward;

        t.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
    }

	void InputMagnitude() {

		//Get space Input to jump; Big jump
		if (Input.GetKeyDown(KeyCode.Q)&& state == State.idle )
		{
			ResetState();
			anim.SetTrigger("Jump");
			state = State.jumping;
		}
		// Normal Jump
		if (controller.collisionFlags == CollisionFlags.Below || isGrounded)
		{
			if (Input.GetButtonDown("Jump") && state == State.idle)
			{
				ResetState();
				isJump = true;
				currentSpeed = jumpSpeed;
				state = State.jumping;
				anim.SetTrigger("LittleJump");
			}
		}
		
		//Calculate Input Vectors
		InputX = Input.GetAxis ("Horizontal");
		InputZ = Input.GetAxis ("Vertical");

		//anim.SetFloat ("InputZ", InputZ, VerticalAnimTime, Time.deltaTime * 2f);
		//anim.SetFloat ("InputX", InputX, HorizontalAnimSmoothTime, Time.deltaTime * 2f);

		//Calculate the Input Magnitude
		Speed = new Vector2(InputX, InputZ).sqrMagnitude;

        //Physically move player

		if (Speed > allowPlayerRotation) {
			anim.SetFloat ("Blend", Speed, StartAnimTime, Time.deltaTime);
			//在采集状态的时候打断采集
			if(state == State.gather)
				ResetState();
			PlayerMoveAndRotation ();
		} else if (Speed < allowPlayerRotation) {
			anim.SetFloat ("Blend", Speed, StopAnimTime, Time.deltaTime);
			if(state == State.gather)
				GatherCrystal();
		}
	}

	
	/// <summary>
	/// 切换状态，隐身/趴下/攻击
	/// </summary>
	void GetInput()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			//隐身
			if (!isInCd)
			{
				Transparent();
			}
		}
		if (state == State.idle)
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				Attack();
			}
		}
		//趴下
		if (state == State.idle || state == State.getDown)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				GetDown();
			}
		}
		
		//在水晶附近可以进行收集水晶
		if (state == State.idle && isAroundCrystal)
		{
			Debug.Log("collection");
			if (Input.GetKeyDown(KeyCode.F))
			{
				BeginGatherCrystal();
			}
		}
	}
	
	/// <summary>
	/// 攻击
	/// </summary>
	void Attack()
	{
		ResetState();
		state = State.attack;
		anim.SetTrigger("Attack");
	}
	
	/// <summary>
	/// 趴下
	/// </summary>
	void GetDown()
	{
		if (anim.GetBool("Get Down"))
		{
			controller.height = normalHeight;
			state = State.idle;
			Vector3 center = controller.center;
			center.y = normalCenterY;
			controller.center = center;
		}
		else
		{
			controller.height = sitHeight;
			Vector3 center = controller.center;
			center.y = sitCenterY;
			controller.center = center;
			state = State.getDown;
		}

		anim.SetBool("Get Down",!anim.GetBool("Get Down"));
	}
	
	/// <summary>
	/// 身体隐身
	/// </summary>
	void Transparent()
	{
		isInCd = true;
		cdTimer = 0;
		Color color = bodyMaterial.color;
		color.a = 0.3f;
		bodyMaterial.color = color;
		isTransparent = true;
		TransparentUI.instance.InCD();

		StartCoroutine(TransparentTimer());
	}

	void TransparentCD()
	{
		if (isInCd)
		{
			cdTimer += Time.deltaTime;
			int showTime = (int) cdTimer;
			TransparentUI.instance.SetValue(transparentCD - showTime);
			if (cdTimer >= transparentCD)
			{
				isInCd = false;
				TransparentUI.instance.OverCD();
			}
		}
	}
	
	void BeginGatherCrystal()
	{
		gatherDuration = 0;
		state = State.gather;
		transform.LookAt(gameObject.transform);
		anim.SetBool("Gather",true);
		ProgressBar.instance.SetValue(0);
		ProgressBar.instance.ShowProgressBar();
	}

	void GatherCrystal()
	{
		gatherDuration += Time.deltaTime;
		
		ProgressBar.instance.SetValue(Mathf.Clamp(gatherDuration/gatherTime,0f,1.0f));
		Debug.Log(gatherDuration);
		if (gatherDuration >= gatherTime)
		{
			Destroy(crystal);
			ownCrystal += 1;
			CrystalsUI.instance.GatherCrystal(1);
			isAroundCrystal = false;
			crystal = null;
			ProgressBar.instance.HideProgressBar();
			ResetState();
		}
	}
	/// <summary>
	/// 开启协程计时隐身
	/// </summary>
	/// <returns></returns>
	IEnumerator TransparentTimer()
	{
		yield return new WaitForSeconds(transparetTime);
		
		Color color = bodyMaterial.color;
		color.a = 1;
		bodyMaterial.color = color;
		isTransparent = false;
	}


	
	/// <summary>
	/// 外部调用，人物受伤函数，播放受击动作
	/// </summary>
	public void Injured()
	{
		ResetState();
		ChangeHealth(-1);

	}

	public void ChangeHealth(int  value)
	{
		currentHealth = Mathf.Clamp(currentHealth + value, 0, maxHealth);
		UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
		anim.SetTrigger("Injure");

		if (currentHealth / (float) maxHealth < 0.6)
		{
			anim.SetBool("Hurt",true);
		}
		else
		{
			anim.SetBool("Hurt",false);

		}

		if (currentHealth == 0)
		{
			GameManager.instance.GameLose();
		}
	}
	
	public void SetState(State newState)
	{
		state = newState;
	}
	
	/// <summary>
	/// 进入水晶可采集范围
	/// </summary>
	/// <param name="crystal">可采集的水晶，用于设置玩家朝向</param>
	public void AroundCrystal(GameObject crystal)
	{
		isAroundCrystal = true;
		this.crystal = crystal;
		durarion = 0;
		Debug.Log("AROUND");
	}

	public void AwayFromCrystal(GameObject crystal)
	{
		isAroundCrystal = false;
		this.crystal = null;
		durarion = 0;
	}
	
	/// <summary>
	/// 转换成正常状态，重置所有参数
	/// </summary>
	public void ResetState()
	{
		state = State.idle;
		controller.height = normalHeight;
		Vector3 center = controller.center;
		center.y = normalCenterY;
		controller.center = center;
		isJump = false;
		anim.SetBool("Gather",false);
		anim.SetBool("Get Down",false);
		ProgressBar.instance.HideProgressBar();
		durarion = 0;
		gatherDuration = 0;
		iTime = 0;
		
	}

	/// <summary>
	/// 判断状态
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
	public bool IsState(State state)
	{
		return this.state == state;
	}

	public bool IsFinishTask()
	{
		return ownCrystal == needCrystals;
	}


	public void Scan()
	{
		Debug.Log("isScan");
		attentionIcon.SetActive(true);
		easyFound = true;
		easyFoundDuration = 0;
	}

	public bool IsEasyFound()
	{
		return easyFound;
	}
	public void EasyFounding()
	{
		if (easyFound)
		{
			easyFoundDuration += Time.deltaTime;
			if (easyFoundDuration >= easyFoundTime)
			{
				easyFound = false;
				attentionIcon.SetActive(false);
				easyFoundDuration = 0;
			}
		}
	}

	public bool IfTransparent()
	{
		return isTransparent;
	}
}
