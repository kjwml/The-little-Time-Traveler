using System.Collections.Generic;
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

    private List<Transform> pieces;

    private Vector2Int dimensions;

    private float width;
    private float height;

    private Transform draggingPiece;
    private Vector3 offset;

    private int piecesCorrect;

    void Start()
    {
        pieces = new List<Transform>();

     
        if (PuzzleData.SelectedTexture != null)
            puzzleTexture = PuzzleData.SelectedTexture;

        dimensions = GetDimensions(puzzleTexture, difficulty);

        CreateJigsawPieces(puzzleTexture);

        ScatterOnBar();

        UpdateBorder();

        piecesCorrect = 0;
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
        width = 1f / dimensions.x;
        height = 1f / dimensions.y;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameHolder);

                piece.localPosition = new Vector3(
                    (-width * dimensions.x / 2f) + (width * col) + width / 2f,
                    (-height * dimensions.y / 2f) + (height * row) + height / 2f,
                    -1f);

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
                material.SetTexture("_MainTex", texture);
            }
        }
    }

    private void ScatterOnBar()
    {
        // Puzzleteile mischen
        List<Transform> shuffled = new List<Transform>(pieces);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            Transform temp = shuffled[i];
            shuffled[i] = shuffled[random];
            shuffled[random] = temp;
        }

        // Anzahl der Teile pro Reihe
        int columns = 6;

        float spacingX = width + 0.03f;
        float spacingY = height + 0.03f;

        float startX = -((columns - 1) * spacingX) / 2f;

        for (int i = 0; i < shuffled.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            shuffled[i].SetParent(pieceHolder);

            shuffled[i].localPosition = new Vector3(
                startX + col * spacingX,
                -row * spacingY,
                0f);
        }
    }

    private void UpdateBorder()
    {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();

        float halfWidth = (width * dimensions.x) / 2f;
        float halfHeight = (height * dimensions.y) / 2f;

        lineRenderer.positionCount = 5;

        lineRenderer.SetPosition(0, new Vector3(-halfWidth, halfHeight, 0));
        lineRenderer.SetPosition(1, new Vector3(halfWidth, halfHeight, 0));
        lineRenderer.SetPosition(2, new Vector3(halfWidth, -halfHeight, 0));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, 0));
        lineRenderer.SetPosition(4, new Vector3(-halfWidth, halfHeight, 0));

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = true;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            RaycastHit2D hit = Physics2D.Raycast(
                Camera.main.ScreenToWorldPoint(mousePos),
                Vector2.zero);

            if (hit)
            {
                draggingPiece = hit.transform;

              
                draggingPiece.position += Vector3.forward;

                offset = draggingPiece.position -
                         Camera.main.ScreenToWorldPoint(mousePos);

                offset.z = 0;
            }
        }

        if (draggingPiece != null)
        {
            Vector3 mouseWorld =
                Camera.main.ScreenToWorldPoint(
                    Mouse.current.position.ReadValue());

            mouseWorld.z = draggingPiece.position.z;

            draggingPiece.position = mouseWorld + offset;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                SnapAndDisableIfCorrect();

                draggingPiece = null;
            }
        }
    }

    private void SnapAndDisableIfCorrect()
    {
        int index = pieces.IndexOf(draggingPiece);

        int col = index % dimensions.x;
        int row = index / dimensions.x;

        Vector3 target = new Vector3(
            (-width * dimensions.x / 2f) + (width * col) + width / 2f,
            (-height * dimensions.y / 2f) + (height * row) + height / 2f,
            -1f);

        if (Vector2.Distance(draggingPiece.localPosition, target) < width / 2f)
        {
            draggingPiece.SetParent(gameHolder);

            draggingPiece.localPosition = target;

            draggingPiece.GetComponent<BoxCollider2D>().enabled = false;

            piecesCorrect++;

            if (piecesCorrect >= pieces.Count)
            {
                playAgainButton.SetActive(true);
            }
        }
    }
}