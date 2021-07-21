using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CliffController : MonoBehaviour
{
    public float sperateSpeed;

    public float loseThredhold;

    public float joinedLinePower;
    private List<JoinedLine> joinedLines;

    private void Awake() {
        joinedLines = new List<JoinedLine>();
    }

    private void Update()
    {
        float force = Mathf.MoveTowards(sperateSpeed, 0, joinedLines.Count * joinedLinePower);

        transform.position += new Vector3(0, force * Time.deltaTime, 0);

        if (sperateSpeed < 0 && transform.position.y <= loseThredhold)
        {
            GameManager.ins.Lose();
        }
        else if (sperateSpeed > 0 && transform.position.y >= loseThredhold)
        {
            GameManager.ins.Lose();
        }
    }

    public void AddJoinedLine(JoinedLine joinedLine)
    {
        joinedLines.Add(joinedLine);
    }

    public void RemoveJoinedLine(JoinedLine joinedLine)
    {
        joinedLines.Remove(joinedLine);
    }
}
