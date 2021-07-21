using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinedLine : MonoBehaviour
{
    public float strenth = 1;

    public float time = 20;

    public Collider hitPoint;

    private LineRenderer line;

    private CliffController point1Cliff;
    private Vector3 point1RelativeToCliff;

    private CliffController point2Cliff;
    private Vector3 point2RelativeToCliff;

    private bool fullyConnected;

    private void Awake() {
        line = GetComponent<LineRenderer>();
    }

    public void Point1Hit(LayerMask layerMask)
    {
        Vector2 point1 = line.GetPosition(0);
        Vector2 point2 = line.GetPosition(1);

        RaycastHit2D hit = Physics2D.Raycast(point1, point1 - point2, 2, layerMask);

        if (hit.collider != null)
        {
            // Debug.DrawLine(point1, point2, Color.blue, 2f);
            // Debug.DrawLine(point1, hit.point, Color.red, 2f);
            // Debug.DrawLine(point2, hit.point, Color.yellow, 2f);
            // Debug.Break();

            line.SetPosition(0, hit.point);
            point1Cliff = hit.collider.GetComponent<CliffController>();
            if (point1Cliff != null)
            {
                point1Cliff.AddJoinedLine(this);
                point1RelativeToCliff = point1Cliff.transform.InverseTransformPoint(hit.point);
            }
        }
    }

    public void Point2Hit(RaycastHit2D hit)
    {
        // Vector2 point1 = line.GetPosition(0);
        // Vector2 point2 = line.GetPosition(1);

        // RaycastHit2D hit = Physics2D.Raycast(point2, point2 - point1, 2, layerMask);
        
        // if (hit.collider != null)
        // {
            line.SetPosition(1, hit.point);
            point2Cliff = hit.collider.GetComponent<CliffController>();
            if (point2Cliff != null)
            {
                point2Cliff.AddJoinedLine(this);
                point2RelativeToCliff = point2Cliff.transform.InverseTransformPoint(hit.point);
            }
        // }
    }

    public void FullyConnected()
    {
        fullyConnected = true;
    }

    private void Update() {
        if (point1Cliff != null)
        {
            line.SetPosition(0, point1Cliff.transform.TransformPoint(point1RelativeToCliff));
            if (fullyConnected) time -= Time.deltaTime * 0.23f;
        }
        if (point2Cliff != null)
        {
            line.SetPosition(1, point2Cliff.transform.TransformPoint(point2RelativeToCliff));
            if (fullyConnected) time -= Time.deltaTime * 0.23f;
        }

        if (fullyConnected)
        {
            time -= Time.deltaTime;

            if (time <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public Vector3 GetPosition(int index)
    {
        return line.GetPosition(index);
    }

    public void SetPosition(int index, Vector3 point)
    {
        line.SetPosition(index, point);
    }

    public void SetPositions(Vector3[] points)
    {
        line.SetPositions(points);
    }

    private void OnDestroy()
    {
        point1Cliff?.RemoveJoinedLine(this);
        point2Cliff?.RemoveJoinedLine(this);
    }
}
