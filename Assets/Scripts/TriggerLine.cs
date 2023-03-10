using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLine : MonoBehaviour
{
    [SerializeField] private bool isCheckingNote = false;
    void OnTriggerEnter2D(Collider2D collider) {
        if (isCheckingNote) {
            if (collider.GetComponent<NoteCircle>() != null) {
                collider.GetComponent<NoteCircle>().IsCheckingInput = true;
            }
        } else {
            if (collider.GetComponent<BeatLine>() != null) {
                collider.GetComponent<BeatLine>().IsCheckingInput = true;
            }
        }
    }
}
