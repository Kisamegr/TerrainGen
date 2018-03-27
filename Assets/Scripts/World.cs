using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World :MonoBehaviour {

  [Header("Assets")]
  public MeshFilter   terrainMeshFilter;
  public MeshRenderer terrainMeshRenderer;
  public MeshFilter   waterMeshFilter;
  public MeshRenderer waterMeshRenderer;


  [Header("World Properties")]
  public int width         = 10;
  public int length        = 10;
  public int seed;


  [Header("Terrain Properties")]
  [Range(1, 40)]
  public float heightScale = 10;
  public AnimationCurve heightCurve;
  public float offsetX     = 0;
  public float offsetY     = 0;
  [Range(0.01f, 15)]
  public float scale       = 1;
  [Range(0, 10)]
  public int octaves       = 4;
  [Range(0, 2)]
  public float persistense = 0.5f;
  [Range(0, 10)]
  public float lacunarity  = 2;

  [Header("Water Properties")]
  [Range(0f, 1f)]
  public float waterLevel = 0.15f;
  public int reflectionResolution = 256;


  private Mesh terrainMesh;
  private Mesh waterMesh;
  private Texture2D heightMap;
  private float[,] heightMapData;

  private Camera mainCamera;
  private Camera reflectionCamera;
  private Camera refractionCamera;

  private RenderTexture reflectionTexture;
  private RenderTexture refractionTexture;

  Vector3 clipPlanePos = Vector3.zero;
  Vector3 clipPlaneNormal = Vector3.up;

  private void Start() {
    //offsetX = Random.Range(-1000, 1000);
    //offsetY = Random.Range(-1000, 1000);

    mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

    CreateWaterCameras();
    UpdateWorld();
  }

  public void OnWillRenderObject() {
    UpdateCameras();
  }

  public void UpdateWorld() {
    UpdateTerrain();
    UpdateWater();
    terrainMeshRenderer.transform.position = new Vector3(0, -heightScale * waterLevel, 0);
  }

  public void UpdateTerrain() {
    CreateHeightMap();
    CreateMesh(ref terrainMesh, true);
    terrainMeshFilter.mesh = terrainMesh;
  }

  public void UpdateWater() {
    CreateMesh(ref waterMesh, false);
    waterMeshFilter.mesh = waterMesh;
  }

  public void OnValidate() {
    if (Application.isPlaying && heightMap)
      UpdateWorld();
  }

  void CreateHeightMap() {
    heightMapData = Noise.PerlinNoise(width, length, scale, seed, offsetX, offsetY, octaves, persistense, lacunarity);

    if (!heightMap) {
      heightMap = new Texture2D(width, length);
      heightMap.filterMode = FilterMode.Point;
    }
    else
      heightMap.Resize(width, length);

    TextureGenerator.GenerateTexture(heightMapData, ref heightMap);
    terrainMeshRenderer.sharedMaterial.SetFloat("_HeightScale", heightScale);
    terrainMeshRenderer.sharedMaterial.mainTexture = heightMap;
  }

  void CreateMesh(ref Mesh mesh, bool includeHeight) {
    MeshData meshData;

    if (includeHeight)
      meshData = MeshGenerator.GenerateMeshData(width, length, heightMapData, heightScale, heightCurve);
    else
      meshData = MeshGenerator.GenerateMeshData(width, length);

    if (!mesh)
      mesh = new Mesh();
    else
      mesh.Clear(true);

    mesh.vertices  = meshData.vertices;
    mesh.uv        = meshData.uvs;
    mesh.triangles = meshData.triangles;
    mesh.RecalculateNormals();
  }

  void UpdateCameras() {
    Vector3 cameraPos = mainCamera.transform.position;
    Quaternion cameraRot = mainCamera.transform.rotation;

    float cameraDistance = 2 * Mathf.Abs(cameraPos.y - waterLevel);
    reflectionCamera.transform.position = new Vector3(cameraPos.x, cameraPos.y - cameraDistance, cameraPos.z);

    Vector3 cameraEulerAngles = mainCamera.transform.rotation.eulerAngles;
    cameraEulerAngles.x *= -1;
    Quaternion rotation = new Quaternion();
    rotation.eulerAngles = cameraEulerAngles;
    reflectionCamera.transform.rotation = rotation;

    refractionCamera.transform.position = cameraPos;
    refractionCamera.transform.rotation = cameraRot;

    Vector4 reflectionClipPlane = CameraSpacePlane(reflectionCamera, clipPlanePos, clipPlaneNormal, 1.0f);
    reflectionCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(reflectionClipPlane);

    Vector4 refractionClipPlane = CameraSpacePlane(refractionCamera, clipPlanePos, clipPlaneNormal, -1.0f);
    refractionCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(refractionClipPlane);

    reflectionCamera.Render();
    refractionCamera.Render();

    waterMeshRenderer.sharedMaterial.SetTexture("_ReflectionTexture", reflectionTexture);
    waterMeshRenderer.sharedMaterial.SetTexture("_RefractionTexture", refractionTexture);
  }

  void CreateWaterCameras() {
    reflectionTexture = new RenderTexture(reflectionResolution, Mathf.FloorToInt(reflectionResolution / mainCamera.aspect), 0);
    refractionTexture = new RenderTexture(reflectionResolution, Mathf.FloorToInt(reflectionResolution / mainCamera.aspect), 0);

    // Reflection camera
    GameObject reflectionCameraObj = new GameObject("Reflection Camera");
    reflectionCamera = reflectionCameraObj.AddComponent<Camera>();
    reflectionCamera.CopyFrom(mainCamera);
    reflectionCamera.enabled = false;
    reflectionCamera.cullingMask = reflectionCamera.cullingMask & ~(1 << LayerMask.NameToLayer("Water"));
    reflectionCamera.targetTexture = reflectionTexture;

    // Refraction Camera
    GameObject refractionCameraObj = new GameObject("Refraction Camera");
    refractionCamera = refractionCameraObj.AddComponent<Camera>();
    refractionCamera.CopyFrom(mainCamera);
    refractionCamera.enabled = false;
    refractionCamera.cullingMask = reflectionCamera.cullingMask & ~(1 << LayerMask.NameToLayer("Water"));
    refractionCamera.targetTexture = refractionTexture;

  }

  // Given position/normal of the plane, calculates plane in camera space.
  Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
    Vector3 offsetPos = pos + normal * 0.07f;
    Matrix4x4 m = cam.worldToCameraMatrix;
    Vector3 cpos = m.MultiplyPoint(offsetPos);
    Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
    return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
  }

  // Calculates reflection matrix around the given plane
  static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane) {
    reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
    reflectionMat.m01 = (-2F * plane[0] * plane[1]);
    reflectionMat.m02 = (-2F * plane[0] * plane[2]);
    reflectionMat.m03 = (-2F * plane[3] * plane[0]);

    reflectionMat.m10 = (-2F * plane[1] * plane[0]);
    reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
    reflectionMat.m12 = (-2F * plane[1] * plane[2]);
    reflectionMat.m13 = (-2F * plane[3] * plane[1]);

    reflectionMat.m20 = (-2F * plane[2] * plane[0]);
    reflectionMat.m21 = (-2F * plane[2] * plane[1]);
    reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
    reflectionMat.m23 = (-2F * plane[3] * plane[2]);

    reflectionMat.m30 = 0F;
    reflectionMat.m31 = 0F;
    reflectionMat.m32 = 0F;
    reflectionMat.m33 = 1F;
  }

}
