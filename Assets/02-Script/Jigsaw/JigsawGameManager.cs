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

                // Unterstützung für Standard-Shader & URP
                if (material.HasProperty("_MainTex"))
                    material.SetTexture("_MainTex", texture);
                else if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", texture);
            }
        }
    }

    private void ScatterOnBar()
    {
        List<Transform> shuffled = new List<Transform>(pieces);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);
            Transform temp = shuffled[i];
            shuffled[i] = shuffled[random];
            shuffled[random] = temp;
        }

        int columns = 6;
        float spacingX = width + 0.03f;
        float spacingY = height + 0.03f;

        float startX = -((columns - 1) * spacingX) / 2f;

        for (int i = 0; i < shuffled.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            // WICHTIG: SetParent mit falscher/korrekter Welt-Positioning
            shuffled[i].SetParent(pieceHolder, false);

            shuffled[i].localPosition = new Vector3(
                startX + col * spacingX,
                -row * spacingY,
                -1f); // Vor dem Holder platzieren

            // Skalierung nach Parent-Wechsel absichern
            shuffled[i].localScale = new Vector3(width, height, 1);
        }
    }

    private void UpdateBorder()
    {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

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

            if (hit && hit.transform != null)
            {
                draggingPiece = hit.transform;

                // Bringt das Teil beim Ziehen optisch ganz nach vorne (-2f)
                Vector3 pos = draggingPiece.position;
                pos.z = -2f;
                draggingPiece.position = pos;

                offset = draggingPiece.position - Camera.main.ScreenToWorldPoint(mousePos);
                offset.z = 0;
            }
        }

        if (draggingPiece != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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

        // Ziel-Position lokal im gameHolder
        Vector3 localTarget = new Vector3(
            (-width * dimensions.x / 2f) + (width * col) + width / 2f,
            (-height * dimensions.y / 2f) + (height * row) + height / 2f,
            -1f);

        // Welt-Zielposition berechnen (unabhängig vom aktuellen Parent!)
        Vector3 worldTarget = gameHolder.TransformPoint(localTarget);

        // Abstand anhand der echten Welt-Koordinaten prüfen
        if (Vector2.Distance(draggingPiece.position, worldTarget) < width / 2f)
        {
            draggingPiece.SetParent(gameHolder);
            draggingPiece.localPosition = localTarget;
            draggingPiece.localScale = new Vector3(width, height, 1);

            // Deaktiviere den Collider, damit man es nicht mehr ziehen kann
            Collider2D collider = draggingPiece.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;

            piecesCorrect++;

            if (piecesCorrect >= pieces.Count)
            {
                if (playAgainButton != null) playAgainButton.SetActive(true);
            }
        }
        else
        {
            // Falls falsch abgelegt: wieder auf Z = -1 zurücksetzen
            Vector3 pos = draggingPiece.position;
            pos.z = -1f;
            draggingPiece.position = pos;
        }
    }
}