using System.Collections;
using System.Collections.Generic;
using MPack;
using UnityEngine;

public class WebWeapon : MonoBehaviour
{
    static Quaternion DirectionToRotation(Vector3 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle - 135, Vector3.forward);
    }
    public float speed = 10;

    public Timer timer = new Timer(8);

    public Enemy enemy;

    private WebState state = WebState.TowardsEnemy;
    private Vector3 toWallDirection;

    private Transform relativeParent;
    private Vector3 relativePosition;

    private enum WebState {
        TowardsEnemy,
        TowardsWall,
        Waiting,
    }

    private LineRenderer line;

    private void Awake()
    {
        if (enemy)
        {
            state = WebState.Waiting;
            enemy.AddWeb(this);
        }
        else
        {
            line = GetComponent<LineRenderer>();
        }
    }

    public void Setup(RaycastHit2D hit)
    {
        enemy = hit.collider.GetComponent<Enemy>();
    }


    private void Update() {
        if (state == WebState.TowardsEnemy)
        {
            Vector3 oldPosition = line.GetPosition(0);
            Vector3 newPosition = Vector3.MoveTowards(oldPosition, enemy.transform.position, speed * Time.deltaTime);
            line.SetPosition(0, newPosition);
            line.SetPosition(1, enemy.transform.position);

            if ((newPosition - enemy.transform.position).sqrMagnitude <= 0.01f)
            {
                state = WebState.TowardsWall;
                transform.position = newPosition;
                line.enabled = false;

                SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = GameManager.ins.webSprite;
                spriteRenderer.sortingOrder = 5;

                toWallDirection = (enemy.transform.position - oldPosition).normalized;
                transform.rotation = DirectionToRotation(toWallDirection);
                transform.position = enemy.transform.position;

                enemy.onCollision += EnemyOnCollision;
                enemy.AddWeb(this);
            }
        }
        else if (state == WebState.TowardsWall)
        {
            transform.position += toWallDirection * speed * Time.deltaTime;
            enemy.transform.position = transform.position;
        }
        else if (!timer.UpdateEnd)
        {
            if (relativeParent)
                enemy.transform.position = transform.position = relativeParent.TransformPoint(relativePosition);
            else
                enemy.transform.position = transform.position;
        }
        else
        {
            enemy.RemoveWeb(this);
            Destroy(gameObject);
        }
    }

    private void EnemyOnCollision(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {}
        else
        {
            enemy.onCollision -= EnemyOnCollision;
            state = WebState.Waiting;

            if (collision.collider.GetComponent<CliffController>())
            {
                relativeParent = collision.transform;
                relativePosition = collision.transform.InverseTransformPoint(transform.position);
            }
        }
    }
}
