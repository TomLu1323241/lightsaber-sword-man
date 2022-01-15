using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PlayerController : MonoBehaviour
{
    [Header("Player Controls")]
    public float speed = 5;
    public float jumpVelocity = 5;

    [Header("Sword Controls")]
    public GameObject sword;
    public float swordDistance = 0.5f;
    [Header("Dash Controls")]
    public int startingDashes = 1;
    public float dashDistance = 5;
    public Slider[] dashTimers;
    public float dashRecoverySpeed = 2;
    [Header("Slow Down Time Controls")]
    public Slider slowTimer;
    public float slowAmount = 2;
    public float slowRecoverySpeed = 0.5f;
    public float slowDownRate = 0.5f;
    public GameObject postProcessGamebject;
    [Header("Upgrades")]
    public GameObject upgradeSystem;
    [Header("Menus")]
    public GameObject deadMenu;
    public GameObject endMenu;
    public GameObject pauseMenu;

    private Animator swordAnimator;
    private TriggerList swordTriggerList;
    private Rigidbody2D myRigidBody;
    private float horizontalV;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GroundDetection groudDetection;
    private bool dashing;
    private bool slowingDownTime;
    private ChromaticAberration postProcess;
    private bool parrying;
    private bool showUpgradeSystem;
    private bool reflectProjectiles;
    private bool freezeGame;
    private bool GODMODE;

    private int projectileLayer;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 1f * 0.02f;
        myRigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        groudDetection = GetComponentInChildren<GroundDetection>();
        swordAnimator = sword.GetComponent<Animator>();
        swordTriggerList = sword.GetComponent<TriggerList>();
        postProcess = postProcessGamebject.GetComponent<PostProcessVolume>().profile.GetSetting<ChromaticAberration>();

        LevelGlobals.player = this.gameObject;
        // Change dashTimers
        for (int i = startingDashes; i < dashTimers.Length; i++)
        {
            dashTimers[i].gameObject.SetActive(false);
        }
        dashing = false;
        slowingDownTime = false;
        parrying = false;
        showUpgradeSystem = false;
        reflectProjectiles = false;
        freezeGame = false;
        GODMODE = false;
        upgradeSystem.SetActive(showUpgradeSystem);

        projectileLayer = LayerMask.NameToLayer("projectile");
    }

    // Update is called once per frame
    void Update()
    {
        if (!dashing && !freezeGame && !pauseMenu.activeSelf)
        {
            movement();
            setAnimation();
            swordFunctions();
            dash();
            timeSlowDown();
            toggleUpgradeSystem();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }
    }

    public void UpdateUpgrades(Dictionary<UpgradeType, int> currentUpgrades)
    {
        foreach(KeyValuePair<UpgradeType, int> keyValue in currentUpgrades)
        {
            switch(keyValue.Key)
            {
                case UpgradeType.ExtraDash:
                    startingDashes = keyValue.Value + 1;
                    for (int i = 0; i < dashTimers.Length; i++)
                    {
                        if (i < startingDashes)
                        {
                            dashTimers[i].gameObject.SetActive(true);
                        } else
                        {
                            dashTimers[i].gameObject.SetActive(false);
                        }
                    }
                    break;
                case UpgradeType.IncreaseDashRecovery:
                    if (keyValue.Value == 0)
                    {
                        dashRecoverySpeed = 2;
                    } else
                    {
                        dashRecoverySpeed = 1;
                    }
                    break;
                case UpgradeType.IncreaseSlowDown:
                    if (keyValue.Value == 0)
                    {
                        slowAmount = 2;
                    }
                    else
                    {
                        slowAmount = 3;
                    }
                    break;
                case UpgradeType.ReflectProjectiles:
                    if (keyValue.Value == 0)
                    {
                        reflectProjectiles = false;
                    }
                    else
                    {
                        reflectProjectiles = true;
                    }
                    break;
                case UpgradeType.GODMODE:
                    if (keyValue.Value == 0)
                    {
                        GODMODE = false;
                    }
                    else
                    {
                        GODMODE = true;
                    }
                    break;
            }
        }
    }

    private void toggleUpgradeSystem()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            showUpgradeSystem = !showUpgradeSystem;
            upgradeSystem.SetActive(showUpgradeSystem);
        }
    }

    private void timeSlowDown()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            slowingDownTime = !slowingDownTime;
        }
        if (slowingDownTime)
        {
            Time.timeScale = slowDownRate;
            Time.fixedDeltaTime = slowDownRate * 0.02f;
            slowTimer.value = Mathf.Clamp(slowTimer.value - Time.deltaTime / slowAmount / slowDownRate, 0, 1);
            postProcess.intensity.value = Mathf.Clamp(postProcess.intensity.value + Time.deltaTime / slowAmount / slowDownRate, 0, 1);
            if (slowTimer.value == 0)
            {
                slowingDownTime = false;
            }
        } else
        {
            if (parrying)
            {
                return;
            }
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 1f * 0.02f;
            if (postProcess.intensity.value != 0)
            {
                StartCoroutine(RemoveTimeSlowEffect());
            }
            slowTimer.value = Mathf.Clamp(slowTimer.value + Time.deltaTime / slowAmount * slowRecoverySpeed, 0, 1);
        }
    }

    IEnumerator RemoveTimeSlowEffect()
    {
        while (postProcess.intensity.value != 0)
        {
            postProcess.intensity.value = Mathf.Clamp(postProcess.intensity.value - Time.deltaTime * 2, 0, 1);
            yield return new WaitForSeconds(1 / 60);
        }
    }

    private void dash()
    {
        // Dash recovery
        for (int i = 0; i < dashTimers.Length; i++)
        {
            if (!dashTimers[i].gameObject.activeSelf)
            {
                break;
            }
            if (dashTimers[i].value < 1f)
            {
                dashTimers[i].value = Mathf.Clamp(dashTimers[i].value + Time.deltaTime / dashRecoverySpeed, 0, 1);
                break;
            }
        }
        if (Input.GetMouseButtonDown(1) && hasDash())
        {
            dashing = true;
            removeDash();
            StartCoroutine(dashCoroutine());
        }
    }

    private void removeDash()
    {
        if (startingDashes == 1) // One dash
        {
            dashTimers[0].value = 0;
        } else
        {
            if (dashTimers[startingDashes - 1].value == 1) // mutiple dashes but last dash full
            {
                dashTimers[startingDashes - 1].value = 0;
            }
            else // mutiple dashes but still recharging
            {
                int lastFull = 0;
                for (int i = 0; i < dashTimers.Length; i++)
                {
                    if (!dashTimers[i].gameObject.activeSelf)
                    {
                        break;
                    }
                    if (dashTimers[i].value < 1f)
                    {
                        break;
                    }
                    lastFull = i;
                }
                dashTimers[lastFull].value = dashTimers[lastFull + 1].value;
                dashTimers[lastFull + 1].value = 0;
            }
        }
    }

    private bool hasDash()
    {
        return dashTimers[0].value == 1;
    }

    IEnumerator dashCoroutine()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 currentPos = transform.position;
        Vector2 dashPos = ((worldPosition - currentPos)).normalized * dashDistance + currentPos;
        float lerpValue = 0;
        while (lerpValue < 1)
        {
            myRigidBody.MovePosition(Vector3.Lerp(currentPos, dashPos, lerpValue));
            lerpValue += (1 - lerpValue) / 10 + 0.01f;
            yield return new WaitForSeconds(1 / 60);
        }
        myRigidBody.velocity = (currentPos - dashPos).normalized;
        myRigidBody.gravityScale = 1;
        dashing = false;
        yield return new WaitForSeconds(0.3f);
        myRigidBody.gravityScale = 5;
    }

    private void swordFunctions()
    {
        // Swing Sword
        if (Input.GetMouseButtonDown(0))
        {
            swordAnimator.Play("Saber Swing");
            List<Hitable> toProcess = new List<Hitable>();
            swordTriggerList.hitList.RemoveWhere(item => item == null);
            foreach (Collider2D hitCollider in swordTriggerList.hitList)
            {
                Hitable hit = hitCollider.gameObject.GetComponent<Hitable>();
                if (hit != null)
                {
                    toProcess.Add(hit);
                }
            }
            if (swordTriggerList.hitList.RemoveWhere(item => item.gameObject.GetComponent<Hitable>() != null) > 0)
            {
                StartCoroutine(perfectParrySlow());
            }
            foreach (Hitable hit in toProcess)
            {
                hit.onHit(reflectProjectiles);
            }
        }

        // Rotate Sword
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 worldPos2D = new Vector3(worldPosition.x, worldPosition.y, sword.transform.position.z);
        Vector3 direction = (worldPos2D - transform.position).normalized;
        sword.transform.position = transform.position + direction * swordDistance;
        sword.transform.right = worldPos2D - sword.transform.position;
    }

    IEnumerator perfectParrySlow()
    {
        parrying = true;
        Time.timeScale = 0.25f;
        Time.fixedDeltaTime = 0.25f * 0.02f;
        yield return new WaitForSecondsRealtime(.25f);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 1f * 0.02f;
        parrying = false;
    }

    private void movement()
    {
        // Left right movement
        horizontalV = Input.GetAxis("Horizontal");
        myRigidBody.velocity = new Vector2(horizontalV * speed, myRigidBody.velocity.y);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && groudDetection.onGround)
        {
            myRigidBody.velocity = new Vector2(myRigidBody.velocity.x, jumpVelocity);
        }
    }

    private void setAnimation()
    {
        // set running
        if (Mathf.Abs(horizontalV) > 0)
        {
            animator.SetBool("running", true);
        } else
        {
            animator.SetBool("running", false);
        }

        if (horizontalV == 0) // set flip when idle
        {

        } else // set flip when running
        {
            if (horizontalV < 0)
            {
                spriteRenderer.flipX = true;
            } else
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dashing)
        {
            if (collision.gameObject.layer == projectileLayer)
            {
                Hitable hitable = collision.gameObject.GetComponent<Hitable>();
                if (hitable != null)
                {
                    hitable.onHit(false);
                }
            }
        } else
        {
            if (GODMODE)
            {
                if (collision.gameObject.layer == projectileLayer)
                {
                    Destroy(collision.gameObject);
                }
            } else
            {
                if (collision.gameObject.layer == projectileLayer)
                {
                    freezeGame = true;
                    deadMenu.SetActive(true);
                }
            }
        }
    }

    public void EndTrigger()
    {
        endMenu.SetActive(true);
        freezeGame = true;
    }

}
