using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPack;


public class Enemy : MonoBehaviour
{
    static Quaternion DirectionToRotation(Vector3 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }
    static Quaternion DirectionToRotation(Vector3 vector, float delta)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle - 90 + delta, Vector3.forward);
    }

    [Range(0f, 15f)]
    public float moveSpeed;
    public Timer switchTargetTimer;
    [Range(0f, 100f)]
    public float rotateSpeed;
    [Range(0, 20)]
    public int randomRotationDelta;
    private Quaternion targetRotation;

    private List<WebWeapon> webs;

    public bool Trapped => webs.Count > 0;

    // private Vector3 target;

    private void Awake()
    {
        if (webs == null) webs = new List<WebWeapon>();
    }

    private void Start() {
        // target = PlayerController.ins.transform.position;
    }

    private void Update()
    {
        if (webs.Count <= 0)
        {
            if (switchTargetTimer.UpdateEnd)
            {
                switchTargetTimer.Reset();
                Vector3 direction = (PlayerController.ins.transform.position - transform.position).normalized;
                targetRotation = DirectionToRotation(direction, Random.Range(-randomRotationDelta, randomRotationDelta));
            }

            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            // transform.position += direction * moveSpeed * Time.deltaTime;
            // transform.rotation = DirectionToRotation(direction);
        }
    }

    public System.Action<Collision2D> onCollision;

    public void AddWeb(WebWeapon web)
    {
        Awake();
        webs.Add(web);
    }

    public void RemoveWeb(WebWeapon web)
    {
        webs.Remove(web);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        onCollision?.Invoke(other);
    }

    private void OnCollisionStay2D(Collision2D other) {
        onCollision?.Invoke(other);
    }
}
