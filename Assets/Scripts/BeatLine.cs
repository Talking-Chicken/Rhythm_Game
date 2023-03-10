using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatLine : MonoBehaviour
{
    AudioManager audioManager;
    public enum HitOrMiss { Perfect, Good, Okay, Miss, Late }
    [SerializeField] int perfectWindow = 100;
    [SerializeField] int goodWindow = 250;
    [SerializeField] int okayWindow = 500;
    float movementTime = 1.0f; //这定义根本没用
    int targetTime;
    [SerializeField] Vector2 destination;
    [SerializeField] private Vector2 startPosition;
    private bool isCheckingInput = false;
    private SpriteRenderer noteSprite;

    public bool IsCheckingInput {get=>isCheckingInput;set=>isCheckingInput=value;}
    void Start()
    {
        startPosition = transform.position;
        noteSprite = GetComponent<SpriteRenderer>();
    }

    
    void Update()
    {
        if (IsCheckingInput) {
            CheckInput();
        }
    }

    public void InitializeBeatline(float multiplier, AkSegmentInfo segInfo, AudioManager ls) {
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
            transform.position = Vector2.Lerp(startPosition, destination, t / movementTime);
            t += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        if (IsCheckingInput) {
            IsCheckingInput = false;
            FadeOut(HitOrMiss.Miss);
        }
    }

    void CheckInput() {

        foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(KeyCode.A)) {
                HitOrMiss hm = HitOrMiss.Miss;

                int currentTime = audioManager.GetMusicTimeInMS();
                int offBy = targetTime - currentTime;

                if (offBy >= 0) {
                    audioManager.IsMakingNote = true;
                    if (offBy <= perfectWindow)
                        hm = HitOrMiss.Perfect;
                    else if (offBy <= goodWindow)
                        hm = HitOrMiss.Good;
                    else
                        hm = HitOrMiss.Okay;
                }
                else {
                    if (offBy > -perfectWindow) {
                        hm = HitOrMiss.Late;
                        Debug.Log("missed");
                    }
                }
                // Debug.Log($"Target Time: {targetTime} || Input time: {currentTime} ||  OffBy: {offBy} || Hit or Miss: {hm}", gameObject);

                IsCheckingInput = false;
                FadeOut(hm);
            }
        }
    }

    #region Animations
    public void FadeOut(HitOrMiss hm) {
        StartCoroutine(FadeOutRoutine(hm));
    }
    IEnumerator FadeOutRoutine(HitOrMiss hm) {
        noteSprite.color = Color.blue;
        float t = 0.0f;
        while (t < 1.0f) {
            t += Time.deltaTime;
            noteSprite.color = Color.Lerp(Color.blue, Color.clear, t);
            yield return null;
        }
        Destroy(gameObject);
    }
    #endregion
}
