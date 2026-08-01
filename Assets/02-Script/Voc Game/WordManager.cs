using TMPro;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    [SerializeField] private TMP_Text wordText;

    private string currentWord = "";

    public void AddLetter(string letter)
    {
        currentWord += letter;
        wordText.text = currentWord;
    }

    public void ResetWord()
    {
        currentWord = "";  
        wordText.text = ""; 
    }

public string GetCurrentWord()
{
    return currentWord;
}
}