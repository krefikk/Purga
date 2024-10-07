using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraBehavior : MonoBehaviour
{
    private CinemachineVirtualCamera cinemachineCamera;
    private CinemachineFramingTransposer transposer;
    public PlayerController player;

    // Camera Tracking Offset
    private bool facingValue = true;
    private float transitionDuration = 1f;

    // Falling Offset
    private bool isFalling = false;
    private bool fallValue = false;
    private float fallTimer = 0f;
    private float fallThreshold = 1f; // Falling duration threshold in seconds

    // Camera Movement
    private Coroutine fallCoroutine;
    private Coroutine directionCoroutine;

    // State Management
    private Vector2 currentTargetOffset;
    private Queue<Vector2> offsetQueue = new Queue<Vector2>();

    private void Start()
    {
        cinemachineCamera = GetComponent<CinemachineVirtualCamera>();
        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        facingValue = player.IsFacingRight();
        fallValue = player.IsFalling();
        currentTargetOffset = transposer.m_TrackedObjectOffset;
    }

    private void Update()
    {
        UpdateBias();
        UpdateFallBias();
    }

    private void UpdateBias()
    {
        if (facingValue != player.IsFacingRight())
        {
            facingValue = player.IsFacingRight();
            if (directionCoroutine != null)
            {
                StopCoroutine(directionCoroutine);
            }
            if (player.IsFacingRight())
            {
                directionCoroutine = StartCoroutine(MoveCamera(new Vector2(1, transposer.m_TrackedObjectOffset.y)));
            }
            else
            {
                directionCoroutine = StartCoroutine(MoveCamera(new Vector2(-1, transposer.m_TrackedObjectOffset.y)));
            }
        }
    }

    private void UpdateFallBias()
    {
        if (player.IsFalling())
        {
            fallTimer += Time.deltaTime;

            if (!isFalling && fallTimer >= fallThreshold)
            {
                isFalling = true;
                if (fallCoroutine != null)
                {
                    StopCoroutine(fallCoroutine);
                }
                fallCoroutine = StartCoroutine(MoveCamera(new Vector2(transposer.m_TrackedObjectOffset.x, -7)));
            }
        }
        else
        {
            fallTimer = 0f;

            if (isFalling)
            {
                isFalling = false;
                if (fallCoroutine != null)
                {
                    StopCoroutine(fallCoroutine);
                }
                fallCoroutine = StartCoroutine(MoveCamera(new Vector2(transposer.m_TrackedObjectOffset.x, 1)));
            }
        }
    }

    /*
    void UpdateFallBias()
    {
        if (player.IsFalling())
        {
            fallTimer += Time.deltaTime;
            if (!isFalling && fallTimer >= fallThreshold)
            {
                isFalling = true;
                transposer.m_BiasY = -0.5f;
                transposer.m_ScreenY = 0.4f;
            }
        }
        else
        {
            fallTimer = 0f;
            if (isFalling)
            {
                isFalling = false;
                transposer.m_BiasY = 0;
                transposer.m_ScreenY = 0.5f;
            }
        }
    }
    */

    private IEnumerator MoveCamera(Vector2 targetOffset)
    {
        float duration = transitionDuration; // Duration of the transition in seconds
        Vector2 startOffset = transposer.m_TrackedObjectOffset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transposer.m_TrackedObjectOffset = Vector2.Lerp(startOffset, targetOffset, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transposer.m_TrackedObjectOffset = targetOffset;
    }

}
