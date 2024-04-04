﻿using System.Threading.Tasks;
using UnityEngine;

namespace SimpleMatch
{
    public static class Animation
    {
        public static async Task AnimateSwapAsync(Transform first, Transform second)
        {
            const float duration = 0.25f;
            Vector3 fPos = first.position;
            Vector3 sPos = second.position;
            float startTime = Time.time;

            while (Time.time < startTime + duration)
            {
                float progress = (Time.time - startTime) / duration;
                first.position = Vector3.Lerp(fPos, sPos, progress);
                second.position = Vector3.Lerp(sPos, fPos, progress);
                await Task.Yield();
                if (first == null || second == null)
                {
                    return;
                }
                
            }

            first.position = sPos;
            second.position = fPos;
        }
    }
}