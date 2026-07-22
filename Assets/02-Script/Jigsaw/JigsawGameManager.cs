using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JigsawGameManager : MonoBehaviour
{
    [Header("Game Elements")]
    [SerializeField] private int difficulty = 4;
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform pieceHolder;
    [SerializeField] private Transform piecePrefab;

    [Header("Puzzle")]
    [SerializeField] private Texture2D puzzleTexture;

    [Header("UI")]
    [SerializeField] private GameObject playAgainButton;

    [Header("Puzzle Dimensions")]
    [SerializeField] private Vector2 totalPuzzleSize = new Vector2(10f, 10f);

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    private Transform draggingPiece = null;
    private Vector3 offset;
    private int piecesCorrect;

    void Start()
    {
        pieces = new List<Transform>();

        if (PuzzleData.SelectedTexture != null)
            puzzleTexture = PuzzleData.SelectedTexture;

        dimensions = GetDimensions(puzzleTexture, difficulty);

        CreateJigsawPieces(puzzleTexture);
        Scatter();
        UpdateBorder();

        piecesCorrect = 0;

        if (playAgainButton != null) playAgainButton.SetActive(false);
    }

    public static class PuzzleData
    {
        public static Texture2D SelectedTexture;
    }

    Vector2Int GetDimensions(Texture2D texture, int difficulty)
    {
        Vector2Int dim = Vector2Int.zero;

        if (texture.width < texture.height)
        {
            dim.x = difficulty;
            dim.y = (difficulty * texture.height) / texture.width;
        }
        else
        {
            dim.x = (difficulty * texture.width) / texture.height;
            dim.y = difficulty;
        }

        return dim;
    }

    void CreateJigsawPieces(Texture2D texture)
    {
        width = totalPuzzleSize.x / dimensions.x;
        height = totalPuzzleSize.y / dimensions.y;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
               
                Transform piece = Instantiate(piecePrefab, gameHolder);

                piece.localPosition = new Vector3(
                    (-totalPuzzleSize.x / 2f) + (width * col) + (width / 2f),
                    (-totalPuzzleSize.y / 2f) + (height * row) + (height / 2f),
                    -0.1f);

                piece.localScale = new Vector3(width, height, 1);
                piece.name = $"Piece {(row * dimensions.x) + col}";

                pieces.Add(piece);

                float u = 1f / dimensions.x;
                float v = 1f / dimensions.y;

                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(u * col, v * row);
                uv[1] = new Vector2(u * (col + 1), v * row);
                uv[2] = new Vector2(u * col, v * (row + 1));
                uv[3] = new Vector2(u * (col + 1), v * (row + 1));

                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;

                Material material = piece.GetComponent<MeshRenderer>().material;

                if (material.HasProperty("_MainTex"))
                    material.SetTexture("_MainTex", texture);
                else if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", texture);
            }
        }
    }

    private void Scatter()
    {
        float orthoHeight = Camera.main.orthographicSize;
        float screenAspect = (float)Screen.width / Screen.height;
        float orthoWidth = screenAspect * orthoHeight;

        float pieceWidth = width * gameHolder.localScale.x;
        float pieceHeight = height * gameHolder.localScale.y;

        orthoHeight -= pieceHeight;
        orthoWidth -= pieceWidth;

        foreach (Transform piece in pieces)
        {
            float x = Random.Range(-orthoWidth, orthoWidth);
            float y = Random.Range(-orthoHeight, orthoHeight);

            piece.position = new Vector3(x, y, gameHolder.position.z - 0.1f);
        }
    }

    private void UpdateBorder()
    {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        float halfWidth = totalPuzzleSize.x / 2f;
        float halfHeight = totalPuzzleSize.y / 2f;

        float borderZ = 0f;

        lineRenderer.positionCount = 5;

        lineRenderer.SetPosition(0, new Vector3(-halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(1, new Vector3(halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(2, new Vector3(halfWidth, -halfHeight, borderZ));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, borderZ));
        lineRenderer.SetPosition(4, new Vector3(-halfWidth, halfHeight, borderZ));

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.enabled = true;
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; 

        
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit && pieces.Contains(hit.transform))
            {
                draggingPiece = hit.transform;
                offset = draggingPiece.position - mouseWorldPos;

                draggingPiece.position = new Vector3(draggingPiece.position.x, draggingPiece.position.y, gameHolder.position.z - 0.5f);
            }
        }

        if (draggingPiece != null)
        {
            Vector3 newPosition = mouseWorldPos + offset;
            newPosition.z = gameHolder.position.z - 0.5f;
            draggingPiece.position = newPosition;
        }

        if (draggingPiece != null && Input.GetMouseButtonUp(0))
        {
            SnapAndDisableIfCorrect();
            draggingPiece = null;
        }
    }

    private void SnapAndDisableIfCorrect()
    {
        int pieceIndex = pieces.IndexOf(draggingPiece);

        int col = pieceIndex % dimensions.x;
        int row = pieceIndex / dimensions.x;

        Vector3 targetLocalPosition = new Vector3(
            (-totalPuzzleSize.x / 2f) + (width * col) + (width / 2f),
            (-totalPuzzleSize.y / 2f) + (height * row) + (height / 2f),
            -0.1f);

        float snapTolerance = width * 0.4f;

        if (Vector3.Distance((Vector2)draggingPiece.localPosition, (Vector2)targetLocalPosition) < snapTolerance)
        {
            draggingPiece.localPosition = targetLocalPosition;

            if (draggingPiece.TryGetComponent<BoxCollider2D>(out var collider))
            {
                collider.enabled = false;
            }

            piecesCorrect++;

            if (piecesCorrect == pieces.Count)
            {
                if (playAgainButton != null)
                    playAgainButton.SetActive(true);
            }
        }
        else
        {
            Vector3 localPos = draggingPiece.localPosition;
            localPos.z = -0.1f;
            draggingPiece.localPosition = localPos;
        }
    }

    public void RestartGame()
    {
        foreach (Transform piece in pieces)
        {
            if (piece != null)
                Destroy(piece.gameObject);
        }

        pieces.Clear();

        if (gameHolder.TryGetComponent<LineRenderer>(out var lineRenderer))
        {
            lineRenderer.enabled = false;
        }

        if (playAgainButton != null)
            playAgainButton.SetActive(false);
    }
}