using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Elements")]

    [Range(2, 6)]
    [SerializeField] private int difficulty = 4;

    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private Material jigsawMaterial;

    private List<Transform> pieces;
    private Vector2Int dimensions;

    private float width;
    private float height;

    public void StartGame(Texture2D jigsawTexture)
    {
        pieces = new List<Transform>();

        dimensions = GetDimensions(jigsawTexture, difficulty);

        CreateJigsawPieces(jigsawTexture);
    }

    private Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty)
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

    private void CreateJigsawPieces(Texture2D jigsawTexture)
    {
        height = 1f / dimensions.y;

        float aspect = (float)jigsawTexture.width / jigsawTexture.height;

        width = aspect / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameHolder);

                piece.localScale = new Vector3(width, height, 1f);

                piece.name = $"Piece {(row * dimensions.x + col)}";

                pieces.Add(piece);

                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;

                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(width1 * col, height1 * row);
                uv[1] = new Vector2(width1 * (col + 1), height1 * row);
                uv[2] = new Vector2(width1 * col, height1 * (row + 1));
                uv[3] = new Vector2(width1 * (col + 1), height1 * (row + 1));

                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;

                piece.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jigsawTexture);

                Texture2D tex = jigsawMaterial.GetTexture("_MainTex") as Texture2D;
                StartGame(tex);

                piece.localPosition = new Vector3(col * width, row * height, 0);
            }
        }
    }

}