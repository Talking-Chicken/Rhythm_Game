using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*this class generate note when pressing the corresponding key*/
public class BeatLine : MonoBehaviour
{
    AudioManager audioManager;
    public enum HitOrMiss { Perfect, Good, Okay, Miss, Late }
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
    private bool isMakingNote = false;
    private SpriteRenderer noteSprite;

    public bool IsCheckingInput {get=>isCheckingInput;set=>isCheckingInput=value;}
    public bool IsMakingNote {get=>isMakingNote; set=>isMakingNote=value;}
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
            Vector3 deltaPosition = Vector3.Lerp(startPosition, destination, t / movementTime) - transform.position;
            if (!hasSetMovementSpeed && t != 0.0f) {
                hasSetMovementSpeed = true;
                movementSpeed = deltaPosition;
            }
            transform.position += deltaPosition;
            t += Time.deltaTime;
            yield return null;
        }

        if (IsMakingNote) {
            audioManager.CreateCircleNote(audioManager.MusicInfo);
        }

        if (IsCheckingInput) {
            IsCheckingInput = false;
            FadeOut(HitOrMiss.Miss);
        }
    }

    void CheckInput() {

        // foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(KeyCode.A)) {
                HitOrMiss hm = HitOrMiss.Miss;

                int currentTime = audioManager.GetMusicTimeInMS();
                int offBy = targetTime - currentTime;

                if (offBy >= 0) {
                    if (offBy <= perfectWindow) {
                        IsMakingNote = true;
                        hm = HitOrMiss.Perfect;
                        GameManager.Instance.Score += 5;
                    } else if (offBy <= goodWindow) {
                        IsMakingNote = true;
                        hm = HitOrMiss.Good;
                        GameManager.Instance.Score += 3;
                    } else {
                        hm = HitOrMiss.Okay;
                        GameManager.Instance.Score += 1;
                    }
                }
                else {
                    if (offBy > -perfectWindow) {
                        hm = HitOrMiss.Late;
                    }
                }
                // Debug.Log($"Target Time: {targetTime} || Input time: {currentTime} ||  OffBy: {offBy} || Hit or Miss: {hm}", gameObject);

                IsCheckingInput = false;
                FadeOut(hm);
            }
        // }
    }

    #region Animations
    public void FadeOut(HitOrMiss hm) {
        StartCoroutine(FadeOutRoutine(hm));
    }
    IEnumerator FadeOutRoutine(HitOrMiss hm) {
        noteSprite.color = Color.blue;
        float t = 0.0f;
        while (t < 0.5f) {
            t += Time.deltaTime;
            transform.position += movementSpeed;
            noteSprite.color = Color.Lerp(Color.blue, Color.clear, t);
            yield return null;
        }
        Destroy(gameObject);
    }
    #endregion
}
