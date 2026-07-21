using UnityEngine;

public class ResetButton : MonoBehaviour
{
        [SerializeField] private WordManager wordManager;

        private void OnMouseDown()
        {
            if (wordManager != null)
            {
                wordManager.ResetWord();
            }
        }
   }
