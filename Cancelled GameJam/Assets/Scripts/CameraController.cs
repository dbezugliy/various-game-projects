using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -10);
    
    [Header("Boundary Settings")]
    public float leftBoundary = -20f;
    public float rightBoundary = 20f;
    public float topBoundary = 5f;
    public float bottomBoundary = -5f;
    public bool constrainVertical = false;
    
    [Header("Camera Shake Settings")]
    public float maxShakeIntensity = 0.03f;
    public float shakeFrequency = 40f;
    
    [Header("Level Transition")]
    public LevelTransitionManager transitionManager;
    
    private Vector3 targetPosition;
    private bool leftZoneActive = true;
    private bool rightZoneActive = true;
    private bool leftTransitionTriggered = false;
    private bool rightTransitionTriggered = false;

    public AudioClip soundEffectOne;
    private AudioSource audioSource;

    public AudioClip rumbleSound;
    private AudioSource rumbleSource;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        rumbleSource = gameObject.AddComponent<AudioSource>();
        rumbleSource.clip = rumbleSound;
        rumbleSource.loop = true;
        rumbleSource.playOnAwake = false;
        rumbleSource.volume = 0f;
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        targetPosition = target.position + offset;
        
        //boundary constraints
        float clampedX = Mathf.Clamp(targetPosition.x, leftBoundary, rightBoundary);
        float clampedY = constrainVertical ? Mathf.Clamp(targetPosition.y, bottomBoundary, topBoundary) : targetPosition.y;
        
        targetPosition = new Vector3(clampedX, clampedY, targetPosition.z);
        
        // Check for boundary hits and transitions
        CheckBoundaryTransitions(clampedX);
        
        Vector3 shakeOffset = CalculateShakeOffset();
        transform.position = targetPosition + shakeOffset;
    }
    
    private void CheckBoundaryTransitions(float clampedX)
    {
        //if left boundary is hit
        if (Mathf.Approximately(clampedX, leftBoundary))
        {
            if (leftZoneActive && !leftTransitionTriggered) 
            { 
                Debug.Log("Hit left boundary");
                audioSource.PlayOneShot(soundEffectOne);
                TriggerLevelTransition();
                leftTransitionTriggered = true;
            }

            leftZoneActive = false;
            rightZoneActive = true;
        }
        //if right boundary is hit
        else if (Mathf.Approximately(clampedX, rightBoundary))
        {
            if (rightZoneActive && !rightTransitionTriggered) 
            { 
                Debug.Log("Hit right boundary");
                audioSource.PlayOneShot(soundEffectOne);
                TriggerLevelTransition();
                rightTransitionTriggered = true;
            }
            leftZoneActive = true;
            rightZoneActive = false;
        }
        else
        {
            //reset transition flags
            if (!Mathf.Approximately(clampedX, leftBoundary))
                leftTransitionTriggered = false;
            if (!Mathf.Approximately(clampedX, rightBoundary))
                rightTransitionTriggered = false;
        }
    }
    
    private void TriggerLevelTransition()
    {        
        Debug.Log("next level transition");
        transitionManager.StartNextTransition();
    }
    
    Vector3 CalculateShakeOffset()
    {
        if (target == null) return Vector3.zero;
        
        float playerX = target.position.x;
        float mapWidth = rightBoundary - leftBoundary;
        float normalizedX = (playerX - leftBoundary) / mapWidth;
        float shakeIntensity = 0f;
        
        //increases shake when approaching left or right boundaries
        if (leftZoneActive && normalizedX <= 0.25f)
        {
            shakeIntensity = Mathf.Lerp(0f, 1f, (0.25f - normalizedX) / 0.25f);
        }
        else if (rightZoneActive && normalizedX >= 0.75f)
        {
            shakeIntensity = Mathf.Lerp(0f, 1f, (normalizedX - 0.75f) / 0.25f);
        }
        
        //apply shake
        if (shakeIntensity > 0f)
        {
            float actualIntensity = shakeIntensity * maxShakeIntensity;
            float shakeX = Mathf.Sin(Time.time * shakeFrequency) * actualIntensity;
            float shakeY = Mathf.Cos(Time.time * shakeFrequency * 1.1f) * actualIntensity;

            if (!rumbleSource.isPlaying)
                rumbleSource.Play();

            rumbleSource.volume = Mathf.Clamp01(shakeIntensity);
            
            return new Vector3(shakeX, shakeY, 0);
        } else {
            if (rumbleSource.isPlaying)
                rumbleSource.Stop();
        }
        
        return Vector3.zero;
    }
    
    //manually trigger camera shake
    public void TriggerShake(float duration, float intensity)
    {
        StartCoroutine(ShakeCoroutine(duration, intensity));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float shakeX = Random.Range(-1f, 1f) * intensity;
            float shakeY = Random.Range(-1f, 1f) * intensity;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    //visualize boundaries
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        //vertical boundaries
        Gizmos.DrawLine(new Vector3(leftBoundary, bottomBoundary - 2, 0), 
                       new Vector3(leftBoundary, topBoundary + 2, 0));
        Gizmos.DrawLine(new Vector3(rightBoundary, bottomBoundary - 2, 0), 
                       new Vector3(rightBoundary, topBoundary + 2, 0));
        
        //horizontal boundaries
        if (constrainVertical)
        {
            Gizmos.DrawLine(new Vector3(leftBoundary - 2, bottomBoundary, 0), 
                           new Vector3(rightBoundary + 2, bottomBoundary, 0));
            Gizmos.DrawLine(new Vector3(leftBoundary - 2, topBoundary, 0), 
                           new Vector3(rightBoundary + 2, topBoundary, 0));
        }
        
        //draw shake zones
        float mapWidth = rightBoundary - leftBoundary;
        float leftShakeZone = leftBoundary + (mapWidth * 0.25f);
        float rightShakeZone = rightBoundary - (mapWidth * 0.25f);
        
        //active zones in bright yellow, inactive in dim yellow
        Gizmos.color = leftZoneActive ? Color.yellow : new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawLine(new Vector3(leftShakeZone, bottomBoundary - 1, 0), 
                       new Vector3(leftShakeZone, topBoundary + 1, 0));
        
        Gizmos.color = rightZoneActive ? Color.yellow : new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawLine(new Vector3(rightShakeZone, bottomBoundary - 1, 0), 
                       new Vector3(rightShakeZone, topBoundary + 1, 0));
    }
}