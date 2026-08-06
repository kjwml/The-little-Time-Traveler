using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPC_Dialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Text dialogueText;
    public string[] dialogue;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip [] dialogueAudioClips;

    public GameObject contButton;
    public float wordSpeed = 0.05f;

    private int index;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        contButton.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!dialoguePanel.activeInHierarchy)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = "";
            index = 0;

            StopAllCoroutines();
            StartCoroutine(Typing());
        }
    }

    IEnumerator Typing()
    {
        if (audioSource != null && dialogueAudioClips.Length > index && dialogueAudioClips [index] != null)
        {
            audioSource.Stop();
            audioSource.clip = dialogueAudioClips[index];
            audioSource.Play();
        }
        
        foreach (char letter in dialogue[index])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }

        contButton.SetActive(true);
    }

    public void NextLine()
    {
        contButton.SetActive(false);

        if (index < dialogue.Length - 1)
        {
            index++;
            dialogueText.text = "";

            StopAllCoroutines();
            StartCoroutine(Typing());
        }
        else
        {
            CloseDialogue();
        }
    }

    private void CloseDialogue()
    {
        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        dialogueText.text = "";
        index = 0;

        dialoguePanel.SetActive(false);
        contButton.SetActive(false);
    }
}