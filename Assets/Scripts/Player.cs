using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public CharacterController controller;
    public Transform orientation;


    [Header("Health")]
    public int maxHp = 150;

    private int hp;

    [Header("UI")]
    public GameObject UI;


    [Header("Movement")]
    public float speed = 12f;
    public float gravity = -9.81f * 3f;
    public float jumpHeight = 2f;
    public float trampolineJumpHeight = 7;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundLayerMask;
    public float smoothTime = 0.3f;

    Vector3 velocity;
    bool isGrounded;
    int activeJumps = 0;


    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashTime = 0.1f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private bool isDashCooldown = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection;
    private float doubleTapTime = 0.2f;
    private float lastTapTimeW = -1f;
    private float lastTapTimeA = -1f;
    private float lastTapTimeS = -1f;
    private float lastTapTimeD = -1f;


    [Header("Climbing")]
    public LayerMask wallLayerMask;
    public float climbSpeed = 10f;
    public float maxClimbTime = 0.75f;
    public float detectionLength = 0.7f;
    public float spehereCastRadius = 0.25f;
    public float maxWallLookAngle = 30f;

    private float climbTimer;
    private bool climbing;
    private float wallLookAngle;
    private RaycastHit frontWallHit;
    private bool wallFront;


    [Header("Grappling Hook")]
    public GameObject hand;
    public LayerMask interactionMask;
    public float maxDist = 20f;
    public float maxAttrForce = 15f;
    public float grapplingHookCooldown = 2f;
    public float grapplingEndDist = 0.75f;

    private bool grapplingHookActive = false;
    private bool isGrapplingHookCooldown = false;
    private Vector3 grapplingPoint = Vector3.zero;
    private LineRenderer grapplingHookRenderer;


    // Start is called before the first frame update
    void Start()
    {
        grapplingHookRenderer = gameObject.GetComponent<LineRenderer>();
        grapplingHookRenderer.startWidth = 0.1f;
        grapplingHookRenderer.endWidth = 0.1f;
        grapplingHookRenderer.positionCount = 2;
        grapplingHookRenderer.enabled = false;

        hp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayerMask);

        if (isGrounded && velocity.y < 0 && !grapplingHookActive)
        {
            velocity.y = -2f;
            activeJumps = 0;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Time.time - lastTapTimeW <= doubleTapTime)
            {
                TriggerDash(transform.forward);
            }
            lastTapTimeW = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time - lastTapTimeA <= doubleTapTime)
            {
                TriggerDash(-transform.right);
            }
            lastTapTimeA = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (Time.time - lastTapTimeS <= doubleTapTime)
            {
                TriggerDash(-transform.forward);
            }
            lastTapTimeS = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - lastTapTimeD <= doubleTapTime)
            {
                TriggerDash(transform.right);
            }
            lastTapTimeD = Time.time;
        }

        if (isDashing && !isDashCooldown)
        {
            // ------ Dashing ------
            transform.position += dashDirection * dashDistance / dashTime * Time.deltaTime;
            dashTimer += Time.deltaTime;

            if (dashTimer >= dashTime)
            {
                isDashing = false;
                isDashCooldown = true;
                StartCoroutine(DashCooldown());
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Mouse1) && !isGrapplingHookCooldown)
            {
                // Grappling Hook ís available
                Vector3 handPos = hand.gameObject.transform.position;
                Ray ray = new Ray(handPos, orientation.transform.forward);
                RaycastHit hitObj;

                if (!grapplingHookActive && grapplingPoint == Vector3.zero)
                {
                    if (Physics.Raycast(ray, out hitObj, maxDist, interactionMask))
                    {
                        // Grappling Hook intersects with an object
                        grapplingPoint = hitObj.point;
                    }
                }

                if (grapplingPoint != Vector3.zero)
                {
                    if (Vector3.Distance(handPos, grapplingPoint) < 0/*grapplingEndDist*/)
                    {
                        // Grappling Hook has reached object and should stop
                        StopGrapplingHook();
                    }
                    else
                    {
                        // Grappling Hook is active
                        grapplingHookActive = true;
                        grapplingHookRenderer.SetPositions(new Vector3[] { hand.gameObject.transform.position, grapplingPoint });
                        grapplingHookRenderer.enabled = true;
                        Vector3 dir = (grapplingPoint - handPos).normalized;
                        if (velocity.magnitude < maxAttrForce) velocity += dir;
                    }
                }
                else
                {
                    // Grappling Hook does not intersect with an object
                    StopGrapplingHook();
                }
            }
            else
            {
                if (grapplingHookActive)
                {
                    StopGrapplingHook();
                }

                if (isGrounded)
                {
                    velocity.x = 0f;
                    velocity.z = 0f;
                }
                else
                {
                    velocity = Vector3.SmoothDamp(velocity, Vector3.zero, ref velocity, smoothTime);
                }

                float x = Input.GetAxis("Horizontal");
                float z = Input.GetAxis("Vertical");

                Vector3 move = transform.right * x + transform.forward * z;
                controller.Move(move * speed * Time.deltaTime);

                // ------ Jumping ------
                if (Input.GetButtonDown("Jump") && activeJumps < 2)
                {
                    if (climbing) StopClimbing();
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    activeJumps++;
                }

                // ------ Climbing ------
                WallCheck();
                if (climbing)
                {
                    float climbVelocity = Input.GetKey(KeyCode.LeftShift) ? 0f : climbSpeed;
                    velocity = new Vector3(velocity.x, climbVelocity, velocity.z);
                }
                else
                {
                    velocity.y += gravity * Time.deltaTime;
                }
            }

            controller.Move(velocity * Time.deltaTime);
        }
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, spehereCastRadius, orientation.forward, out frontWallHit, detectionLength, wallLayerMask);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (isGrounded)
        {
            climbTimer = maxClimbTime;
        }

        if ((wallFront && Input.GetKey(KeyCode.W)) || Input.GetKey(KeyCode.LeftShift)) /*&& wallLookAngle < maxWallLookAngle*/
        {
            if (!climbing && climbTimer > 0) StartClimbing();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }
        else
        {
            if (climbing) StopClimbing();
        }
    }

    private void StartClimbing()
    {
        climbing = true;
    }

    private void StopClimbing()
    {
        climbing = false;
    }

    private void TriggerDash(Vector3 direction)
    {
        isDashing = true;
        dashTimer = 0f;
        dashDirection = direction;
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        isDashCooldown = false;
    }

    private IEnumerator GrapplingHookCooldown()
    {
        yield return new WaitForSeconds(grapplingHookCooldown);
        isGrapplingHookCooldown = false;
    }

    private void StopGrapplingHook()
    {
        //velocity = Vector3.zero;
        grapplingPoint = Vector3.zero;
        grapplingHookActive = false;
        isGrapplingHookCooldown = true;
        grapplingHookRenderer.enabled = false;
        StartCoroutine(GrapplingHookCooldown());
    }

    public void Hit(int damage)
    {
        hp -= damage;
        UI.GetComponent<UI>().setHp(hp);
    }

    public void TrampolineJump()
    {
        velocity.y = Mathf.Sqrt(trampolineJumpHeight * -2f * gravity);
    }
}
