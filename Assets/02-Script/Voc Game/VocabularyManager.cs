using UnityEngine;

public class VocabularyManager : MonoBehaviour
{
    private WordManager wordManager;
    private TextMesh letterText;

    private void Start()
    {
        wordManager = FindFirstObjectByType<WordManager>();

        letterText = GetComponentInChildren<TextMesh>();

        if (letterText == null)
        {
            Debug.LogError("Kein TextMesh gefunden bei " + gameObject.name);
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("Geklickt auf: " + gameObject.name);

        if (wordManager != null && letterText != null)
        {
            wordManager.AddLetter(letterText.text);
        }
    }
}