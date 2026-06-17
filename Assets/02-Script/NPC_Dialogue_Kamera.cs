using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NPC_Dialogue_Kamera: MonoBehaviour
{
 
    public GameObject dialoguePanel;
    public Text dialogueText;
    public GameObject contButton;

  
    public string[] dialogue;
    public float wordSpeed = 0.05f;

    private int index;
    private bool dialogueStarted;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        contButton.SetActive(false);
    }

    private void Update()
    {
        if (dialogueStarted && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueText.text == dialogue[index])
            {
                NextLine();
            }
        }

        if (dialogueStarted &&
            dialogueText.text == dialogue[index])
        {
            contButton.SetActive(true);
        }
    }

    private void OnMouseDown()
    {
        OpenDialogue();
    }

 
    private void OnBecameVisible()
    {
        OpenDialogue();
    }

    private void OpenDialogue()
    {
       
        if (dialogueStarted)
            return;

        dialogueStarted = true;
        index = 0;

        dialoguePanel.SetActive(true);
        dialogueText.text = "";
        contButton.SetActive(false);

        StartCoroutine(Typing());
    }

    IEnumerator Typing()
    {
        foreach (char letter in dialogue[index].ToCharArray())
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

        dialogueText.text = "";
        index = 0;
        dialogueStarted = false;

        dialoguePanel.SetActive(false);
        contButton.SetActive(false);
    }
}