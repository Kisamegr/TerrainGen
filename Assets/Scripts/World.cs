using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World :MonoBehaviour {

  [Header("Assets")]
  public MeshFilter   terrainMeshFilter;
  public MeshRenderer terrainMeshRenderer;
  public MeshFilter   waterMeshFilter;
  public MeshRenderer waterMeshRenderer;
  

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


  private Mesh terrainMesh;
  private Texture2D heightMap;
  private float[,] heightMapData;

  private void Start() {
    offsetX = Random.Range(-1000, 1000);
    offsetY = Random.Range(-1000, 1000);

    UpdateTerrain();
  }

  public void UpdateTerrain() {
    CreateHeightMap();
    CreateTerrain();
  }

  public void OnValidate() {
    if(Application.isPlaying && heightMap)
      UpdateTerrain();
  }

  void CreateTerrain() {
    MeshData meshData = MeshGenerator.GenerateMeshData(width, length, heightMapData, heightScale, heightCurve);

    if(!terrainMesh)
      terrainMesh = new Mesh();
    else
      terrainMesh.Clear(true);

    terrainMesh.vertices  = meshData.vertices;
    terrainMesh.uv        = meshData.uvs;
    terrainMesh.triangles = meshData.triangles;

    terrainMeshFilter.mesh = terrainMesh;
  }

  void CreateHeightMap() {
    heightMapData = Noise.PerlinNoise(width, length, scale, offsetX, offsetY, octaves, persistense, lacunarity);

    if(!heightMap)
      heightMap = new Texture2D(width, length);
    else
      heightMap.Resize(width, length);

    TextureGenerator.GenerateTexture(heightMapData, ref heightMap);
    terrainMeshRenderer.sharedMaterial.SetFloat("_HeightScale", heightScale);
    terrainMeshRenderer.sharedMaterial.mainTexture = heightMap;
  }

}
