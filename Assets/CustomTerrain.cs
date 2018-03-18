using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTerrain :MonoBehaviour {

  [Header("Mesh Properties")]
  public int width         = 10;
  public int length        = 10;
  [Range(0, 40)]
  public float heightScale = 10;
  public AnimationCurve heightCurve;

  [Header("Heightmap Properties")]
  public float offsetX     = 0;
  public float offsetY     = 0;
  [Range(0.01f, 15)]
  public float scale       = 1;
  [Range(0, 10)]
  public int octaves       = 4;
  [Range(0, 10)]
  public float persistense = 0.5f;
  [Range(0, 10)]
  public float lacunarity  = 2;


  private Mesh mesh;
  private Texture2D heightMap;
  private MeshFilter meshFilter;
  private MeshRenderer meshRenderer;

  private float[,] heightMapData;

  private void Start() {
    mesh = new Mesh();
    heightMap = new Texture2D(width, length);
    meshFilter =   GetComponent<MeshFilter>();
    meshRenderer = GetComponent<MeshRenderer>();

    offsetX = Random.Range(-1000, 1000);
    offsetY = Random.Range(-1000, 1000);
    //CreateHeightMap();
  }

  private void Update() {
    CreateHeightMap();
    CreateMesh();
  }

  void CreateMesh() {

    MeshData meshData = MeshGenerator.GenerateMeshData(width, length, heightMapData, heightScale, heightCurve);

    mesh.Clear(true);
    mesh.vertices  = meshData.vertices;
    mesh.uv        = meshData.uvs;
    mesh.triangles = meshData.triangles;

    meshFilter.mesh = mesh;
  }

  void CreateHeightMap() {
    heightMapData = Noise.PerlinNoise(width, length, scale, offsetX, offsetY, octaves, persistense, lacunarity);
    heightMap.Resize(width, length);

    TextureGenerator.GenerateTexture(heightMapData, ref heightMap);
    meshRenderer.sharedMaterial.SetFloat("_HeightScale", heightScale);
    meshRenderer.sharedMaterial.mainTexture = heightMap;
  }

}
