using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Elements")]
    [SerializeField] private int difficulty = 4;
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;

    [Header("UI Elements")]
    [SerializeField] private List<Texture2D> imageTextures;
    [SerializeField] private Transform levelSelectPanel;
    [SerializeField] private Image levelSelectPrefab;
    [SerializeField] private GameObject playAgainButton;

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    private Transform draggingPieces = null;
    private Vector3 offset;

    private int piecesCorrect;

    void Start()
    {
        foreach (Texture2D texture in imageTextures)
        {
            Image image = Instantiate(levelSelectPrefab, levelSelectPanel);

            image.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2()
            );

            image.GetComponent<Button>().onClick.AddListener(() => StartGame(texture));
        }
    }

    public void StartGame(Texture2D jigsawTexture)
    {
        levelSelectPanel.gameObject.SetActive(false);

        pieces = new List<Transform>();

        dimensions = GetDimensions(jigsawTexture, difficulty);

        CreateJigsawPieces(jigsawTexture);

        Scatter();

        UpdateBorder();

        piecesCorrect = 0;
    }


    Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty)
    {
        Vector2Int dimensions = Vector2Int.zero;

        if (jigsawTexture.width < jigsawTexture.height)
        {
            dimensions.x = difficulty;
            dimensions.y = (difficulty * jigsawTexture.height) / jigsawTexture.width;
        }
        else
        {
            dimensions.x = (difficulty * jigsawTexture.width) / jigsawTexture.height;
            dimensions.y = difficulty;
        }

        return dimensions;
    }

    void CreateJigsawPieces(Texture2D jigsawTexture)
    {
        width = 1f / dimensions.x;
        height = 1f / dimensions.y;

        float aspect = (float)jigsawTexture.width / jigsawTexture.height;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameHolder);

                piece.transform.localPosition = new Vector3(
                    (-width * dimensions.x / 2f) + (width * col) + (width / 2f),
                    (-height * dimensions.y / 2f) + (height * row) + (height / 2f),
                    -1f
                );

                piece.transform.localScale = new Vector3(width, height, 1f);

                piece.name = $"Piece {(row * dimensions.x) + col}";

                pieces.Add(piece.transform);

                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;

                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(width1 * col, height1 * row);
                uv[1] = new Vector2(width1 * (col+1), height1 * row);
                uv[0] = new Vector2(width1 * col, height1 * (row+1));
                uv[0] = new Vector2(width1 * (col+1), height1 * (row+1));

                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;

                piece.GetComponent<MeshRenderer>().material.SetTexture("_MainText", jigsawTexture);
            }
        }
    }

    private void Scatter() {
        float orthoHeight = Camera.main.orthographicSize;
        float screenAspect = (float)Screen.width / Screen.height;
        float orthoWidth = (screenAspect * orthoHeight);

        float pieceWidth = gameHolder.localScale.x;
        float pieceHeight = gameHolder.localScale.y;

        orthoHeight -= pieceHeight;
        orthoWidth = pieceWidth;

        foreach (Transform piece in pieces){
            float x = Random.Range(-orthoWidth, orthoWidth);
            float y = Random.Range(-orthoHeight, orthoHeight);
            piece.position = new Vector3(x, y, -1);

        }
    } 

    private void UpdateBorder() {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();

        float halfWidth = (width * dimensions.x) / 2f;
        float halfHeight = (height * dimensions.y) / 2f;

        float borderZ = 0f;

        lineRenderer.SetPosition(0, new Vector3(-halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(1, new Vector3(halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(2, new Vector3(halfWidth, -halfHeight, borderZ));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, borderZ));

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.enabled = true;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            RaycastHit2D hit = Physics2D.Raycast(
                Camera.main.ScreenToWorldPoint(mousePosition),
                Vector2.zero);

            if (hit)
            {
                draggingPieces = hit.transform;
                offset = draggingPieces.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                offset += Vector3.back;

            }
        }

        if (draggingPieces != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            draggingPieces.position += Vector3.forward;
            SnapAndDisableIfCorrect();
            draggingPieces = null;
        }

        if (draggingPieces != null)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue());

            newPosition.z = draggingPieces.position.z;
            newPosition += offset;
            draggingPieces.position = newPosition;
        }
    }

    private void SnapAndDisableIfCorrect() {
        int pieceIndex = pieces.IndexOf(draggingPieces);

        int col = pieceIndex % dimensions.x;
        int row = pieceIndex / dimensions.x;

        Vector2 targetPosition = new((-width * dimensions.x / 2) + (width * col) + (width / 2),
                                       (-height * dimensions.y / 2) + (height * row) + (height / 2));

        if (Vector2.Distance(draggingPieces.localPosition, targetPosition) < (width / 2))
        {
            draggingPieces.localPosition = targetPosition;
            draggingPieces.GetComponent<BoxCollider2D>().enabled = false;

            piecesCorrect++;
            if (piecesCorrect == pieces.Count){
                playAgainButton.SetActive(true);

            }
        }
    }
}
  