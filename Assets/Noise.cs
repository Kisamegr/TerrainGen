using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise {

  public static float[,] PerlinNoise(int width, int height, float scale) {
    float[,] map = new float[width,height];
    float min = float.MaxValue;
    float max = float.MinValue;

    // Generate the noise
    for (int x = 0; x < width; x++) {
      for (int y = 0; y < height; y++) {
        float xCoord = x / (float) width  * scale;
        float yCoord = y / (float) height * scale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);

        if (sample < min) min = sample;
        if (sample > max) max = sample;

        map[x, y] = sample;
      }
    }

    // Normalise the data
    for (int x = 0; x < width; x++) {
      for (int y = 0; y < height; y++) {
        map[x, y] = Mathf.InverseLerp(min, max, map[x, y]);
      }
    }

    return map;
  }

}
