using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPack;
using MPack.Aseprite;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    static Quaternion DirectionToRotation(Vector3 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle + 90, Vector3.forward);
    }

    static public PlayerController ins;

    private LineRenderer linePrefab;
    private JoinedLine line;
    public float lineSpeed;
    private bool lineShooted;
    // private Vector3 lineTargetPosition;
    private Vector2 lineDirection;

    private bool wallHit;
    private bool lineHit;

    private new Camera camera;

    public LayerMask groundLayer;

    public Vector2 moveSpeed;
    [Range(1f, 10f)]
    public float backFireSpeed;

    public float hurtForce;
    public Timer hurtRecoverTimer;
    public Timer invincibleTimer;
    public Timer invincibleBlinkTimer;

    public SpriteRenderer spriteRenderer;
    private new Rigidbody2D rigidbody2D;
    private Color origin;
    private Color transparent;

    private AseAnimator animator;
    private AudioSource audioSource;
    public AudioClip shootWebSound;
    public AudioClip hurt;

    private bool failed;

    private void Awake()
    {
        ins = this;

        // spriteRenderer = GetComponent<SpriteRenderer>();
        transparent = origin = spriteRenderer.color;
        transparent.a = 0;

        linePrefab = GetComponentInChildren<LineRenderer>();
        linePrefab.gameObject.SetActive(false);

        rigidbody2D = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<AseAnimator>();

        camera = Camera.main;
        hurtRecoverTimer.Running = false;
        invincibleTimer.Running = false;

    }

    private void Update()
    {
        if (invincibleTimer.Running)
        {
            if (invincibleBlinkTimer.UpdateEnd)
            {
                invincibleBlinkTimer.ReverseMode = !invincibleBlinkTimer.ReverseMode;
            }

            spriteRenderer.color = Color.Lerp(origin, transparent, invincibleBlinkTimer.Progress);

            if (invincibleTimer.UpdateEnd)
            {
                invincibleTimer.Running = false;
                spriteRenderer.color = origin;
            }
        }
        if (hurtRecoverTimer.Running)
        {
            if (hurtRecoverTimer.UpdateEnd)
            {
                hurtRecoverTimer.Running = false;
                rigidbody2D.velocity = Vector2.zero;
                rigidbody2D.drag = 0f;
            }
        }
        else
        {
            HandleShootLine();
            if (!lineShooted && !failed) HandleMovment();
        }
    }

    void HandleMovment()
    {
        Vector2Int moveDelta = Vector2Int.zero;

        if (Input.GetKey(KeyCode.A)) moveDelta.x = -1;
        if (Input.GetKey(KeyCode.D)) moveDelta.x += 1;
        if (Input.GetKey(KeyCode.S)) moveDelta.y = -1;
        if (Input.GetKey(KeyCode.W)) moveDelta.y += 1;

        if (moveDelta == Vector2Int.zero) animator.Play(0);
        else animator.Play(1);
        transform.position += new Vector3(moveDelta.x * moveSpeed.x * Time.deltaTime, moveDelta.y * moveSpeed.y * Time.deltaTime);
    }

    void HandleShootLine()
    {
        if (lineShooted)
        {
            if (!wallHit) line.SetPosition(0, transform.position);

            if (!lineHit)
            {
                Vector3 oldPosition = line.GetPosition(1);
                Vector3 newPosition = oldPosition + (Vector3)(lineDirection * lineSpeed * Time.deltaTime);
                line.SetPosition(1, newPosition);

                RaycastHit2D hit = Physics2D.Linecast(oldPosition, newPosition, groundLayer);

                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Enemy")) HitEnemyWithWeb(hit);
                    else
                    {
                        lineHit = true;

                        line.Point2Hit(hit);
                        if (wallHit) ConnectLine();
                    }
                }
            }
        }
        else
        {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                lineDirection = (mousePosition - (Vector2)transform.position).normalized;

                GameObject newLineGameObject = Instantiate(linePrefab).gameObject;
                newLineGameObject.SetActive(true);

                line = newLineGameObject.AddComponent<JoinedLine>();
                line.SetPositions(new Vector3[] {
                    transform.position,
                    transform.position + (Vector3)(lineDirection * lineSpeed * Time.deltaTime),
                });

                lineShooted = true;
                lineHit = false;
                wallHit = false;

                rigidbody2D.velocity = -lineDirection * backFireSpeed;
                rigidbody2D.drag = 0f;

                audioSource.PlayOneShot(shootWebSound, 2f);
                audioSource.Play();

                animator.Play(2);
            }
            else
            {
                transform.rotation = DirectionToRotation((Vector3)mousePosition - transform.position);
            }
        }
    }

    private void ConnectLine()
    {
        lineShooted = false;
        line.FullyConnected();
        audioSource.Stop();
    }

    private void HandleBackWallHit(Collision2D collision)
    {
        rigidbody2D.velocity = Vector2.zero;
        wallHit = true;

        line.SetPosition(0, collision.contacts[0].point);
        line.Point1Hit(groundLayer);
        if (lineHit) ConnectLine();
    }

    private void HandleBackHurt(Collision2D collision)
    {
        if (invincibleTimer.Running) return;

        if (collision.collider.GetComponent<Enemy>().Trapped) return;

        if (line != null)
        {
            Destroy(line.gameObject);
            lineShooted = false;
            audioSource.Stop();
        }

        rigidbody2D.velocity = (transform.position - collision.collider.transform.position).normalized * hurtForce;
        hurtRecoverTimer.Reset();
        invincibleTimer.Reset();
        rigidbody2D.drag = 0.5f;
    }

    private void HitEnemyWithWeb(RaycastHit2D enemyHit)
    {
        Enemy enemy = enemyHit.collider.GetComponent<Enemy>();

        WebWeapon web = line.gameObject.AddComponent<WebWeapon>();
        web.transform.localScale = new Vector3(1.3f, 1.3f, 1);
        web.Setup(enemyHit);

        Destroy(line);
        lineShooted = false;
        rigidbody2D.velocity = Vector2.zero;
        audioSource.Stop();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Enemy")) HandleBackHurt(other);
        else if (!wallHit && lineShooted) HandleBackWallHit(other);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy")) HandleBackHurt(other);
        else if (!wallHit && lineShooted) HandleBackWallHit(other);
    }

    public void Failed()
    {
        failed = true;
    }
}
