using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelTransitionData
{
    [Header("Level Information")]
    public GameObject fromLevel;
    public GameObject toLevel;
    public Transform spawnPoint;
    
    [Header("Transition Images")]
    public List<Sprite> transitionImages = new List<Sprite>();
    
    [Header("Camera Settings")]
    public float newLeftBoundary = -20f;
    public float newRightBoundary = 20f;
}

public class LevelTransitionManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas transitionCanvas;
    public Image transitionImageDisplay;
    public GameObject transitionUI;
    
    [Header("Player References")]
    public GameObject player;
    public CameraController cameraController;
    
    [Header("Level Transitions")]
    public List<LevelTransitionData> levelTransitions = new List<LevelTransitionData>();
        
    private int currentTransitionIndex = 0;
    private InputSystem_Actions controls;
    private int currentImageIndex = 0;
    private bool isTransitioning = false;
    private bool waitingForInput = false;
    private LevelTransitionData currentTransition;
    
    void Awake()
    {
        controls = new InputSystem_Actions();
        transitionUI.SetActive(false);
    }
    
    void OnEnable()
    {
        controls.Enable();
        controls.Player.Interact.performed += OnInteractPressed;
    }
    
    void OnDisable()
    {
        controls.Player.Interact.performed -= OnInteractPressed;
        controls.Disable();
    }
    
    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (isTransitioning && waitingForInput)
        {
            ShowNextImage();
        }
    }
    
    public void StartNextTransition()
    {
        if (isTransitioning) return;
        
        if (currentTransitionIndex >= levelTransitions.Count)
        {
            Debug.LogWarning("no more transitions");
            return;
        }
        
        Debug.Log($"starting transition {currentTransitionIndex + 1} of {levelTransitions.Count}");
        
        currentTransition = levelTransitions[currentTransitionIndex];
        currentTransitionIndex++;
        
        StartCoroutine(TransitionSequence());
    }
    
    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;
        currentImageIndex = 0;
        
        //show transition UI
        if (transitionUI != null)
        {
            transitionUI.SetActive(true);
        }
        
        List<Sprite> imagesToShow = GetTransitionImages();
        
        //first image
        if (imagesToShow.Count > 0)
        {
            transitionImageDisplay.sprite = imagesToShow[0];
            waitingForInput = true;
        }
        else
        {
            //directly to level change if no image
            yield return new WaitForSeconds(1f);
            CompleteTransition();
        }
        
        yield return null;
    }
    
    private List<Sprite> GetTransitionImages()
    {
        if (currentTransition.transitionImages.Count > 0)
        {
            return currentTransition.transitionImages;
        }
        
        return new List<Sprite>();
    }
    
    private void ShowNextImage()
    {
        waitingForInput = false;
        currentImageIndex++;
        
        List<Sprite> imagesToShow = GetTransitionImages();
        
        if (currentImageIndex < imagesToShow.Count)
        {
            transitionImageDisplay.sprite = imagesToShow[currentImageIndex];
            waitingForInput = true;
        }
        else
        {
            //all images shown
            CompleteTransition();
        }
    }
    
    private void CompleteTransition()
    {
        if (currentTransition == null) return;
        
        //switch levels
        if (currentTransition.fromLevel != null)
            currentTransition.fromLevel.SetActive(false);
            
        if (currentTransition.toLevel != null)
            currentTransition.toLevel.SetActive(true);
        
        //move player to spawn point
        if (currentTransition.spawnPoint != null && player != null)
        {
            player.transform.position = currentTransition.spawnPoint.position;
        }
        
        //update camera settings
        if (cameraController != null)
        {
            cameraController.leftBoundary = currentTransition.newLeftBoundary;
            cameraController.rightBoundary = currentTransition.newRightBoundary;
        }
        
        //hide transition UI
        if (transitionUI != null)
        {
            transitionUI.SetActive(false);
        }
                
        isTransitioning = false;
        waitingForInput = false;
        currentTransition = null;
    }
}