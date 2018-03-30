using UnityEngine;

public class World :MonoBehaviour {

  [Header("World Properties")]
  public int width         = 10;
  public int length        = 10;
  public int seed;
  public float waterLevel = 0.15f;

  private TerrainGenerator terrainGenerator;
  private WaterGenerator waterGenerator;


  static World _instance;
  private void Awake() {
    _instance = this;
  }

  public static World GetInstance() {
    return _instance;
  }

  private void Start() {
    // Find the gameobjects
    GameObject terrainObj = GameObject.Find("Terrain");
    GameObject waterObj   = GameObject.Find("Water");

    // Set the generator scripts
    if (terrainObj)
      terrainGenerator = terrainObj.GetComponent<TerrainGenerator>();
    if (waterObj)
      waterGenerator   = waterObj.GetComponent<WaterGenerator>();

    // Create the world
    GenerateWorld();
  }

  public void GenerateWorld() {
    // Regenerate the terrain and water meshes
    if (terrainGenerator)
      terrainGenerator.Regenerate();
    if (waterGenerator)
      waterGenerator.Regenerate();
  }


}
