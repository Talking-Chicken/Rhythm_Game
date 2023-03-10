using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AK.Wwise.Event musicEvent;
    uint playingID;

    [SerializeField]
    float beatDuration;

    [SerializeField]
    float barDuration;
    bool durationSet = false;

    [SerializeField] NoteCircle noteCirclePrefab;
    [SerializeField] BeatLine beatLinePrefab;
    [SerializeField] Transform startPosition;

    private bool isMakingNote = false;
    public float BeatDuration {
        get { return beatDuration; }
    }
    public bool IsMakingNote {get=>isMakingNote;set=>isMakingNote=value;}

    #region Setup
    private void Awake() {
        playingID = musicEvent.Post(gameObject, (uint)(AkCallbackType.AK_MusicSyncAll | AkCallbackType.AK_EnableGetMusicPlayPosition | AkCallbackType.AK_MIDIEvent), CallbackFunction);
    }
    #endregion

    void Start()
    {
        
    }

    void CallbackFunction(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info) {
        AkMusicSyncCallbackInfo musicInfo;

        if (in_info is AkMusicSyncCallbackInfo) {
            musicInfo = (AkMusicSyncCallbackInfo)in_info;
            switch (in_type) {

                case AkCallbackType.AK_MusicSyncUserCue:
                    CreateBeatLine(musicInfo);
                    if (IsMakingNote) {
                        CreateCircleNote(musicInfo);
                        IsMakingNote = false;
                    }
                    break;

                case AkCallbackType.AK_MusicSyncBeat:
                    break;
                case AkCallbackType.AK_MusicSyncBar:
                    break;
            }

            if (in_type is AkCallbackType.AK_MusicSyncBar) {
                if (!durationSet) {
                    beatDuration = musicInfo.segmentInfo_fBeatDuration;
                    barDuration = musicInfo.segmentInfo_fBarDuration;
                    durationSet = true;
                }
            }

        }
    }

    public int GetMusicTimeInMS() {
        AkSegmentInfo segmentInfo = new AkSegmentInfo();
        AkSoundEngine.GetPlayingSegmentInfo(playingID, segmentInfo, true);
        return segmentInfo.iCurrentPosition;
    }

    #region Letter Spawning

    void CreateCircleNote(AkMusicSyncCallbackInfo info) {
        // Debug.Log($"Choosing letter {info}");

        NoteCircle pt = Instantiate(noteCirclePrefab, startPosition.position, Quaternion.identity);

        float multiplier = 1.0f;
        if (info.userCueName == "2") {
            multiplier = 2.0f;
        }
        //Debug.Log($"Multiplier: {multiplier}");

        AkSegmentInfo segmentInfo = new AkSegmentInfo();
        AkSoundEngine.GetPlayingSegmentInfo(playingID, segmentInfo, true);

        pt.InitializeCircle(multiplier, segmentInfo, this);

    }

    void CreateBeatLine(AkMusicSyncCallbackInfo info) {
        BeatLine pt = Instantiate(beatLinePrefab, new Vector2(-9,0), Quaternion.identity);

        float multiplier = 1.0f;
        if (info.userCueName == "2") {
            multiplier = 2.0f;
        }
        //Debug.Log($"Multiplier: {multiplier}");

        AkSegmentInfo segmentInfo = new AkSegmentInfo();
        AkSoundEngine.GetPlayingSegmentInfo(playingID, segmentInfo, true);

        pt.InitializeBeatline(multiplier, segmentInfo, this);
    }

    #endregion
}
