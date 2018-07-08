using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityStandardAssets.Characters.FirstPerson;
public class UIManager : MonoBehaviour {

  [Header("Prefabs")]
  public GameObject lodLayoutPrefab;

  [Header("Modes")]
  public MouseOrbitImproved editCamera;
  public RigidbodyFirstPersonController player;

  [Header("Panels")]
  public GameObject terrainEditPanel;
  public GameObject firstPersonPanel;
  public GameObject linearPropertiesPanel;
  public GameObject voxelPropertiesPanel;
  public Transform lodObjectsPanel;


  [Header("Linear Terrain Properties")]
  public Dropdown terrainTypeChange;
  public Dropdown linearChunkSize;
  public Slider linearHeightScale;
  public Text linearHeightScaleValue;

  [Header("Voxel Terrain Properties")]
  public Dropdown voxelChunkSize;
  public Dropdown voxelHeightLayers;
  public Slider voxelSize;
  public Text voxelSizeValue;

  [Header("Noise Properties")]
  public Slider noiseScale;
  public Text noiseScaleValue;
  public InputField seed;
  public InputField offsetX;
  public InputField offsetY;
  public Slider octaves;
  public Text octavesValue;
  public Slider persistense;
  public Text persistenseValue;
  public Slider lacunarity;
  public Text lacunarityValue;



  private bool terrainEdit;
  private World world;
  private bool initialValuesSet = false;

  private void Start() {
    world = World.GetInstance();
    ModeChange(true);
    TerrainTypeChange(world.terrainData.useVoxels ? 1 : 0);
    UpdateUI();
    ResetLodPanel();
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.Tab)) {
      ModeChange(!terrainEdit);
    }
  }

  public void UpdateTerrainData() {
    if (!initialValuesSet)
      return;

    if (!world.terrainData.useVoxels) {
      world.terrainData.size = int.Parse(linearChunkSize.captionText.text);
      world.terrainData.heightScale = linearHeightScale.value;
    }
    else {
      world.terrainData.size = int.Parse(voxelChunkSize.captionText.text);
      world.terrainData.HeightLayersNumber = int.Parse(voxelHeightLayers.captionText.text);
      world.terrainData.cellSize = (int) voxelSize.value;
    }
    world.terrainData.scale = noiseScale.value;
    world.terrainData.seed = int.Parse(seed.text);
    world.terrainData.offsetX = int.Parse(offsetX.text);
    world.terrainData.offsetY = int.Parse(offsetY.text);
    world.terrainData.octaves = (int) octaves.value;
    world.terrainData.persistense = persistense.value;
    world.terrainData.lacunarity = lacunarity.value;

    UpdateUIValues();
    world.terrainData.NotifyUpdate();
  }

  public void UpdateUI() {
    if (!world.terrainData.useVoxels) {
      linearHeightScale.value = world.terrainData.heightScale;
      for (int i = 0; i<linearChunkSize.options.Count; i++) {
        if (linearChunkSize.options[i].text == world.terrainData.size.ToString())
          linearChunkSize.value = i;
      }
    }
    else {
      voxelSize.value = world.terrainData.cellSize;
      for (int i = 0; i<voxelChunkSize.options.Count; i++) {
        if (voxelChunkSize.options[i].text == world.terrainData.size.ToString())
          voxelChunkSize.value = i;
      }
      for (int i = 0; i<voxelHeightLayers.options.Count; i++) {
        if (voxelHeightLayers.options[i].text == world.terrainData.HeightLayersNumber.ToString())
          voxelHeightLayers.value = i;
      }
    }

    noiseScale.value = world.terrainData.scale;
    seed.text =  world.terrainData.seed.ToString();
    offsetX.text = world.terrainData.offsetX.ToString();
    offsetY.text = world.terrainData.offsetY.ToString();
    octaves.value = world.terrainData.octaves;
    persistense.value = world.terrainData.persistense;
    lacunarity.value = world.terrainData.lacunarity;

    UpdateUIValues();
    initialValuesSet = true;
  }

  public void UpdateUIValues() {
    if (!world.terrainData.useVoxels) {
      linearHeightScaleValue.text = linearHeightScale.value.ToString();
    }
    else {
      voxelSizeValue.text = voxelSize.value.ToString();
    }

    noiseScaleValue.text = noiseScale.value.ToString();
    octavesValue.text = octaves.value.ToString();
    persistenseValue.text = persistense.value.ToString();
    lacunarityValue.text = lacunarity.value.ToString();
  }

  public void ModeChange(bool terrainEdit) {
    this.terrainEdit = terrainEdit;
    terrainEditPanel.SetActive(terrainEdit);
    firstPersonPanel.SetActive(!terrainEdit);
    player.gameObject.SetActive(!terrainEdit);
    player.ResetPosition();
    editCamera.gameObject.SetActive(terrainEdit);
    RenderSettings.fog = !terrainEdit;

    if (terrainEdit) { 
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }
  }

  public void TerrainTypeChange(int value) {
    bool useVoxels = value == 1;

    world.terrainData.useVoxels = useVoxels;
    linearPropertiesPanel.SetActive(!useVoxels);
    voxelPropertiesPanel.SetActive(useVoxels);
    terrainTypeChange.value = value;

    if (initialValuesSet)
      ResetWorld();
  }

  public void ResetWorld() {
    world.terrainData.size = world.terrainData.useVoxels
      ? int.Parse(voxelChunkSize.captionText.text)
      : int.Parse(linearChunkSize.captionText.text);

    world.Reset();
  }

  public void ResetMaterial() {
    world.terrainData.ApplyToMaterial(world.terrainMaterial);
  }

  public void ResetLodPanel() {
    foreach(Transform child in lodObjectsPanel) {
      Destroy(child.gameObject);
    }

    int count = 0;
    foreach(LODInfo lodInfo in world.lodInfo) {
      CreateLodObject(lodInfo, count++);
    }
  }

  public LodObject CreateLodObject(LODInfo lodInfo, int index) {
    GameObject lodGameObject = Instantiate(lodLayoutPrefab, lodObjectsPanel);
    LodObject lodObject = lodGameObject.GetComponent<LodObject>();

    Debug.Log(index);
    lodObject.lodValue.value = lodInfo.Lod;
    lodObject.lodText.text = lodInfo.Lod.ToString();
    lodObject.distance.text = lodInfo.Distance.ToString();
    lodObject.colliderToggle.isOn = lodInfo.UseForCollider;

    lodObject.lodValue.onValueChanged.AddListener(delegate { SetLodValues(lodObject, index); });
    lodObject.distance.onEndEdit.AddListener(delegate { SetLodValues(lodObject, index); });
    lodObject.colliderToggle.onValueChanged.AddListener(delegate { SetLodValues(lodObject, index); });
    lodObject.removeButton.onClick.AddListener(delegate { RemoveLod(lodInfo, index); });

    return lodObject;
  }

  public void SetLodValues(LodObject lodObject, int index) {
    Debug.Log("SET VALUES  " + index);
    world.lodInfo[index].SetValues(
      (int) lodObject.lodValue.value,
      int.Parse(lodObject.distance.text),
      lodObject.colliderToggle.isOn);

    world.Reset();
  }

  public void RemoveLod(LODInfo lodInfo, int index) {
    List<LODInfo> newLodInfos = new List<LODInfo>();
    for (int i = 0; i<world.lodInfo.Length; i++)
      if (i != index)
        newLodInfos.Add(world.lodInfo[i]);

    world.lodInfo = newLodInfos.ToArray();

    ResetLodPanel();
    world.Reset();
  }

  public void AddLod() {
    LODInfo newLodInfo = new LODInfo(world.lodInfo[world.lodInfo.Length-1]);
    CreateLodObject(newLodInfo, world.lodInfo.Length);

    List<LODInfo> newLodInfos = new List<LODInfo>();
    newLodInfos.AddRange(world.lodInfo);
    newLodInfos.Add(newLodInfo);

    world.lodInfo = newLodInfos.ToArray();

    ResetLodPanel();
    world.Reset();
  }
}
