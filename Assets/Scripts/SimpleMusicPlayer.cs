using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMusicPlayer : MonoBehaviour
{
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
