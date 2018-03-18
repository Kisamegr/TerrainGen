using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise {

  public static float[,] PerlinNoise(int width, int height, float scale, float offsetX, float offsetY, int octaves, float persistence, float lacunarity) {
    float[,] map = new float[width,height];
    float min = float.MaxValue;
    float max = float.MinValue;
    

    // Generate the noise
    for (int x = 0; x < width; x++) {
      for (int y = 0; y < height; y++) {
        float sample = 0;
        float xCoord = (x / (float) width  - 0.5f) * scale + offsetX;
        float yCoord = (y / (float) height - 0.5f) * scale + offsetY;
        float frequency = 1;
        float amplitude = 1;

        for (int o = 0; o < octaves+1; o++) {
          float perlinValue = Mathf.PerlinNoise(xCoord * frequency, yCoord * frequency) * 2 - 1;
          sample += perlinValue * amplitude;

          frequency *= lacunarity;
          amplitude *= persistence;
        }


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
