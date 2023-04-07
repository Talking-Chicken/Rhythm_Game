using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class NoteCircle : MonoBehaviour
{
    AudioManager audioManager;
    [SerializeField] private AK.Wwise.Event drumEvent, endDrumEvent;
    public enum HitOrMiss { Perfect, Good, Okay, Miss, Late }
    [SerializeField] Color[] successColors = new Color[5];
    [SerializeField] int perfectWindow = 100;
    [SerializeField] int goodWindow = 250;
    [SerializeField] int okayWindow = 500;
    float movementTime = 1.0f; //这定义根本没用
    Vector3 movementSpeed = Vector3.zero;
    bool hasSetMovementSpeed = false;
    int targetTime;
    [SerializeField] Vector2 destination;
    [SerializeField] private Vector2 startPosition;
    private bool isCheckingInput = false;
    private SpriteRenderer noteSprite;
    private bool isKeyPressed = false;

    public bool IsCheckingInput {get=>isCheckingInput;set=>isCheckingInput=value;}
    void Start()
    {
        startPosition = transform.position;
        noteSprite = GetComponent<SpriteRenderer>();
        drumEvent.Post(gameObject);
    }

    
    void Update()
    {
        if (IsCheckingInput) {
            CheckInput();
        }
    }

    public void InitializeCircle(float multiplier, AkSegmentInfo segInfo, AudioManager ls) {
        audioManager = ls;
        int currentTime = audioManager.GetMusicTimeInMS();
        targetTime = Mathf.FloorToInt(currentTime + (ls.BeatDuration * 1000 * multiplier));
        movementTime = (targetTime - currentTime) * 0.001f;
        // Debug.Log($"Current time: {currentTime} || Target time: {targetTime}", gameObject);
        StartCoroutine(MoveNoteRoutine());
    }

    IEnumerator MoveNoteRoutine() {
        float t = 0.0f;
        while (t < movementTime) {
            Vector3 deltaPosition = Vector3.Lerp(startPosition, destination, t / movementTime) - transform.position;
            if (!hasSetMovementSpeed && t != 0.0f) {
                hasSetMovementSpeed = true;
                movementSpeed = deltaPosition;
            }
            transform.position += deltaPosition;
            t += Time.deltaTime;
            yield return null;
        }
        if (IsCheckingInput) {
            IsCheckingInput = false;
            FadeOut(HitOrMiss.Miss);
        }
    }

    void CheckInput() {
        if (Input.GetKeyDown(KeyCode.L) && !isKeyPressed) {
            HitOrMiss hm = HitOrMiss.Miss;
            isKeyPressed = true;
            endDrumEvent.Post(gameObject);

            int currentTime = audioManager.GetMusicTimeInMS();
            int offBy = targetTime - currentTime;

            if (offBy >= 0) {
                if (offBy <= perfectWindow)
                    hm = HitOrMiss.Perfect;
                else if (offBy <= goodWindow)
                    hm = HitOrMiss.Good;
                else
                    hm = HitOrMiss.Okay;
            }
            else {
                if (offBy > -perfectWindow)
                    hm = HitOrMiss.Late;
            }
            // Debug.Log($"Target Time: {targetTime} || Input time: {currentTime} ||  OffBy: {offBy} || Hit or Miss: {hm}", gameObject);

            IsCheckingInput = false;
            FadeOut(hm);
        }
    }

    #region Animations
    public void FadeOut(HitOrMiss hm) {
        StartCoroutine(FadeOutRoutine(hm));
    }
    IEnumerator FadeOutRoutine(HitOrMiss hm) {
        noteSprite.color = successColors[(int)hm];
        float t = 0.0f;
        while (t < 0.5f) {
            t += Time.deltaTime;
            transform.position += movementSpeed;
            noteSprite.color = Color.Lerp(successColors[(int)hm], Color.clear, t);
            yield return null;
        }
        Destroy(gameObject);
    }
    #endregion
}
