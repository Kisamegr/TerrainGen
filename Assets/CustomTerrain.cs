using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTerrain :MonoBehaviour {

  public int width  = 10;
  public int length = 10;
  public float scale = 1;
  public float heightScale = 10;

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

    //CreateHeightMap();
  }

  private void Update() {
    CreateHeightMap();
    CreateMesh();
  }

  void CreateMesh() {

    MeshData meshData = MeshGenerator.GenerateMeshData(width, length, heightScale, heightMapData);

    mesh.Clear(true);
    mesh.vertices  = meshData.vertices;
    mesh.uv        = meshData.uvs;
    mesh.triangles = meshData.triangles;

    meshFilter.mesh = mesh;
  }

  void CreateHeightMap() {
    heightMapData = Noise.PerlinNoise(width, length, scale);
    heightMap.Resize(width, length);

    TextureGenerator.GenerateTexture(heightMapData, ref heightMap);
    meshRenderer.sharedMaterial.mainTexture = heightMap;
  }

}
