using System.Collections.Generic;
using UnityEngine;



    public class PlayerController : MonoBehaviour
    {
        public float mouseRotateSpeed = 0.3f;

        private Animator animator;
        private Rigidbody rigidbody;
        private Vector3 movement;
        
        private float vertexInput;


        public float MouseSensitivity;
        private float m_HorizontalAngle;
        private float m_VerticalAngle;
        public float cameraRange;
        
        
        public float speed;
        public Transform head;
        public Transform lookAt;
        public Actions actions;
        public GameObject aimingCamera;
        public GameObject idleCamera;
        public Camera camera;
        private int currentAnimation;
        public List<string> animations;

        public Ray aimingRay;

        public GameObject trial;
        public Transform weapon;
        public static PlayerController Instance { get; private set; }
        
        
        //持枪
        public Transform rightGunBone;
        public Transform leftGunBone;
        public Arsenal[] arsenal;

        
        void Awake() {
            if (Instance != null) {
                Destroy(this.gameObject);
            }
            Instance = this;
            
            //初始化为持枪或非持枪
            animator = GetComponent<Animator> ();
            if (arsenal.Length > 0)
                SetArsenal (arsenal[1].name);

            //Cursor.lockState = CursorLockMode.Locked;
           // Cursor.visible = false;
        }

        void Start() {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            animations = new List<string>()
            {
                "Hikick"
            };
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            
            m_HorizontalAngle = transform.localEulerAngles.y;
        }

        void Update() {
            
            if (actions.isAiming)
            {
                if (!aimingCamera.activeSelf)
                {
                    aimingCamera.SetActive(true);
                    idleCamera.SetActive(false);
                    camera = aimingCamera.GetComponent<Camera>();
                }
            }
            else
            {
                if (!idleCamera.activeSelf)
                {
                    aimingCamera.SetActive(false);
                    idleCamera.SetActive(true);
                    camera = idleCamera.GetComponent<Camera>();
                }
            }


            //turn player
            float turnPlayer =  Input.GetAxis("Mouse X") * MouseSensitivity;
            m_HorizontalAngle = m_HorizontalAngle + turnPlayer;
            if (m_HorizontalAngle > 360) m_HorizontalAngle -= 360.0f;
            if (m_HorizontalAngle < 0) m_HorizontalAngle += 360.0f;
            Vector3 currentAngles = transform.localEulerAngles;
            currentAngles.y = m_HorizontalAngle;
            transform.localEulerAngles = currentAngles;
            
            
            
            //camera lookup & down
            var turnCam = -Input.GetAxis("Mouse Y");
            turnCam = turnCam * MouseSensitivity;
            m_VerticalAngle = Mathf.Clamp(turnCam + m_VerticalAngle, -cameraRange, cameraRange);
            currentAngles = head.localEulerAngles;
            currentAngles.x = m_VerticalAngle;
            head.localEulerAngles = currentAngles;

            //sitting
            if (Input.GetKeyDown(KeyCode.N))
            {
                actions.SendMessage("Sitting", vertexInput);
            }
            //jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //if(vertexInput < 0.01f )
                    actions.SendMessage("Jump");
            }
            
            //Aiming
            if (Input.GetMouseButtonDown(1))
            {
                if (!actions.isAiming)
                {
                    if (vertexInput< 0.01f)
                    {
                        actions.SendMessage("Aiming");  
                    }
                }
                else
                {
                    actions.SendMessage("Stay");
                }
            }
            
            //Fire
            if (Input.GetMouseButtonDown(0))
            {
                if (actions.isAiming)
                {
                    Fire();        
                    actions.SendMessage("Attack");
                }
            }


 
 
        }


        private void FixedUpdate()
        {
            vertexInput = Mathf.Clamp(Input.GetAxis("Vertical"), 0, 1);
            movement = GetForward() * speed  * Time.deltaTime *  vertexInput;
            //Debug.Log(speed  * Time.fixedDeltaTime *  vertexInput);
            if (vertexInput > 0.01)
            {
                actions.SendMessage("Run", vertexInput);
                rigidbody.MovePosition(rigidbody.transform.position + movement);    
            }

        }

        public void Fire()
        {
            aimingRay = camera.ScreenPointToRay( new Vector2(Screen.width/2, Screen.height/2));
            RaycastHit hit = new RaycastHit();
            //准星射线能碰撞则重新计算
            if (Physics.Raycast(aimingRay, out hit,30,LayerMask.GetMask("Enemy","Default","Environment")))
            {
                Debug.Log(hit.collider.gameObject);
                GameObject newTrial = Instantiate(trial, aimingRay.origin, trial.transform.rotation);
                Trail trailScript = newTrial.GetComponent<Trail>();
                trailScript.SetActive(weapon.position, hit.point);
            }

        }
        
        public Vector3 GetForward()
        {
            Vector3 forward = lookAt.position - head.position;
            forward.y = 0;
            return forward.normalized;
        }
        
        public void SetArsenal(string name) {
            foreach (Arsenal hand in arsenal) {
                if (hand.name == name) {
                    if (rightGunBone.childCount > 0)
                        Destroy(rightGunBone.GetChild(0).gameObject);
                    if (leftGunBone.childCount > 0)
                        Destroy(leftGunBone.GetChild(0).gameObject);
                    if (hand.rightGun != null) {
                        GameObject newRightGun = (GameObject) Instantiate(hand.rightGun);
                        newRightGun.transform.parent = rightGunBone;
                        newRightGun.transform.localPosition = Vector3.zero;
                        newRightGun.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    }
                    if (hand.leftGun != null) {
                        GameObject newLeftGun = (GameObject) Instantiate(hand.leftGun);
                        newLeftGun.transform.parent = leftGunBone;
                        newLeftGun.transform.localPosition = Vector3.zero;
                        newLeftGun.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    }
                    animator.runtimeAnimatorController = hand.controller;
                    return;
                }
            }
        }
        
        [System.Serializable]
        public struct Arsenal {
            public string name;
            public GameObject rightGun;
            public GameObject leftGun;
            public RuntimeAnimatorController controller;
        }
    }
