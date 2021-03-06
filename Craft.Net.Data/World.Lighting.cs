﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data
{
    public partial class World
    {
        internal static byte[] LightReductionIndex;
        public static void RecreateLightIndex()
        {
            LightReductionIndex = new byte[0];
            foreach (var item in Item.Items)
            {
                if (item is Block)
                {
                    if (item.Id >= LightReductionIndex.Length)
                        Array.Resize(ref LightReductionIndex, item.Id + 1);
                    LightReductionIndex[item.Id] = (item as Block).LightReduction;
                }
            }
        }

        public void LightChunk(Chunk chunk)
        {
            chunk.ClearLight();
            chunk.CalculateInitialSkylight();
            for (int y = 0; y < Chunk.Height; y++)
            {
                for (int x = 0; x < Chunk.Width; x++)
                {
                    for (int z = 0; z < Chunk.Depth; z++)
                    {
                        CalculateSkyLight(chunk, x, y, z);
                    }
                }
            }
            //for (int y = 0; y < Chunk.Height; y++)
            //{
            //    for (int x = Chunk.Width - 1; x >= Chunk.Width; x--)
            //    {
            //        for (int z = Chunk.Depth - 1; z >= 0; z--)
            //        {
            //            CalculateSkyLight(chunk, x, y, z);
            //        }
            //    }
            //}
        }

        private void CalculateSkyLight(Chunk chunk, int x, int y, int z)
        {
            byte self = chunk.GetSkyLight(x, y, z);
            // We use sbytes here so we can use -1 as the light value of an ungenerated block
            sbyte left = GetSkyLight(chunk, x - 1, y, z);
            sbyte right = GetSkyLight(chunk, x + 1, y, z);
            sbyte forwards = GetSkyLight(chunk, x, y, z + 1);
            sbyte backwards = GetSkyLight(chunk, z, y, z - 1);
            if (left > self)
                self = (byte)(left - 1);
            if (right > self)
                self = (byte)(right - 1);
            if (forwards > self)
                self = (byte)(forwards - 1);
            if (backwards > self)
                self = (byte)(backwards - 1);
            chunk.SetSkyLight(x, y, z, self);
        }

        private sbyte GetSkyLight(Chunk chunk, int x, int y, int z)
        {
            if (x < 0)
            {
                x = Chunk.Width + x;
                chunk = GetChunkWithoutGeneration(chunk.AbsolutePosition + Vector3.Left);
                if (chunk == null) return -1;
            }
            if (x >= Chunk.Width)
            {
                x -= Chunk.Width;
                chunk = GetChunkWithoutGeneration(chunk.AbsolutePosition + Vector3.Right);
                if (chunk == null) return -1;
            }
            if (z < 0)
            {
                z = Chunk.Depth + z;
                chunk = GetChunkWithoutGeneration(chunk.AbsolutePosition + Vector3.Backwards);
                if (chunk == null) return -1;
            }
            if (z >= Chunk.Depth)
            {
                z -= Chunk.Depth;
                chunk = GetChunkWithoutGeneration(chunk.AbsolutePosition + Vector3.Forwards);
                if (chunk == null) return -1;
            }
            return (sbyte)chunk.GetSkyLight(x, y, z);
        }
    }
}
