using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckButtonLogic : MonoBehaviour
{
    [Header("Referenzen")]
    [SerializeField] private WordManager wordManager; 
    [SerializeField] private GameObject winPanel;    

    [Header("Einstellungen")]
    [SerializeField] private List<string> allowedWords = new List<string> { "IONIC" };

    private void OnMouseDown()
    {
        CheckWord();
    }

    public void CheckWord()
    {
        if (wordManager == null)
        {
            Debug.LogError("Fehler: Du musst den WordManager im Inspector zuweisen!");
            return;
        }

    
        string solveString = wordManager.GetCurrentWord().Trim().ToUpper();

        Debug.Log("Zusammengesetztes Wort ist: '" + solveString + "'");

       
        bool isCorrect = false;
        foreach (string word in allowedWords)
        {
            if (word.Trim().ToUpper() == solveString)
            {
                isCorrect = true;
                break;
            }
        }

   
        if (isCorrect)
        {
            Debug.Log("RICHTIG! Win Panel wird geöffnet.");
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
        }
        else
        {
            Debug.Log("FALSCH! Spiel wird neu geladen.");
   
            wordManager.ResetWord();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}