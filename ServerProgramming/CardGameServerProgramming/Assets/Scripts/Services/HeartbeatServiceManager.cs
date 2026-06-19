using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartbeatServiceManager : MonoBehaviour
{
    public Action SendHeartBeatPulse;

    [SerializeField] float HeartbeatGapInSeconds = 2.5f;

    float currentTimer = 0f;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        currentTimer += Time.deltaTime;

        if (currentTimer > HeartbeatGapInSeconds)
        {
            SendHeartBeatPulse?.Invoke();
            currentTimer = 0f;
        }
    }
}
