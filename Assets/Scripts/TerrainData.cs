using UnityEngine;

[CreateAssetMenu(fileName = "New TerrainData", menuName = "Terrain Data")]
public class TerrainData : ScriptableObject {

  [Header("Terrain Properties")]
  [Range(1, 300)]
  public int size = 200;
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
}
