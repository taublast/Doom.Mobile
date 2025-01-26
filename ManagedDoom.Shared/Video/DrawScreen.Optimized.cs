//
// Copyright (C) 1993-1996 Id Software, Inc.
// Copyright (C) 2019-2020 Nobuaki Tanaka
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//



using System;
using System.Collections.Generic;

namespace ManagedDoom.Video
{
    public sealed partial class ThreeDRenderer
    {


    }

    public sealed partial class DrawScreen
    {

        public void DrawPatchParallelOptimized(Patch patch, int x, int y, int scale)
        {
            // Basic checks
            if (scale <= 0) return;
            if (patch == null) return;

            // "drawX/drawY" is where the scaled patch's top-left will be placed.
            int drawX = x - scale * patch.LeftOffset;
            int drawY = y - scale * patch.TopOffset;

            // The scaled width of this patch in screen pixels:
            int scaledWidth = scale * patch.Width;

            // 1) Horizontal clipping and initial fractional offset
            // We'll treat "fracU" as a 16.16 fixed. e.g. if scale=2, stepU = 0x8000 = (1 << 16)/2
            int stepU = (1 << 16) / scale; // the horizontal "advance" in 16.16 per column
            int fracU = stepU - 1;        // We'll start near 0, but minus 1 to emulate the old "Fixed.Epsilon" logic

            // We'll track how many columns to skip on the left, adjusting fracU accordingly
            int startCol = 0;
            if (drawX < 0)
            {
                int exceed = -drawX; // how many columns are off-screen to the left
                startCol += exceed;
                fracU += exceed * stepU;
            }
            int endCol = scaledWidth; // last column we might draw
            if (drawX + scaledWidth > width)
            {
                // clipped on the right
                endCol -= (drawX + scaledWidth) - width;
            }
            // If out of screen entirely, bail.
            if (endCol <= startCol) return;

            // 2) Run the horizontal loop in parallel
            Parallel.For(startCol, endCol, col =>
            {
                // localFrac = fracU + (col - startCol)*stepU
                // But to avoid repeated multiplication for each col, we do:
                int localFrac = fracU + (col - startCol) * stepU;

                // 16.16 integer => patch column index
                // patchCol = localFrac >> 16
                int patchCol = localFrac >> 16;
                // clamp if for some reason localFrac is slightly out of range
                if (patchCol < 0) patchCol = 0;
                if (patchCol >= patch.Width) patchCol = patch.Width - 1;

                // Where do we draw on the screen?
                int screenX = drawX + col;
                // Then draw the patch's column data.
                DrawColumnOptimized(patch.Columns[patchCol], screenX, drawY, scale);
            });
        }

        /// <summary>
        /// This does the vertical scaling of each "Column" array using integer-based stepping,
        /// and writes into screen 'data[]'.
        /// </summary>
        private void DrawColumnOptimized(Column[] segments, int dstX, int dstY, int scale)
        {
            // If it's off the screen horizontally, skip
            if (dstX < 0 || dstX >= width)
                return;

            // We'll do each "topDelta/length" run in the patch
            foreach (var segment in segments)
            {
                int scaledTop = segment.TopDelta * scale;  // how many vertical pixels above patch's "start"
                int scaledLen = segment.Length * scale;    // how many total scaled pixels

                int sourceIndex = segment.Offset;          // start index in segment.Data
                int drawY = dstY + scaledTop;             // top Y on screen

                // Now we do vertical clipping
                int startY = 0;
                if (drawY < 0)
                {
                    startY = -drawY;  // how many rows are off-screen above
                }
                int endY = scaledLen;
                if (drawY + scaledLen > height)
                {
                    endY -= (drawY + scaledLen - height);
                }
                if (endY <= startY) continue; // fully clipped

                // We'll do 16.16 stepping for the vertical scale
                // If scale=2, stepV=0x8000, etc.
                int stepV = (1 << 16) / scale;
                // We'll offset the fraction for the startY
                int fracV = stepV - 1 + startY * stepV;  // replicate the " - Fixed.Epsilon" offset

                // The "base" pointer in 'data' for pixel [dstX, drawY]
                // but we add 'startY' to skip the clipped portion
                int screenPos = (dstX * height) + (drawY + startY);

                // Now fill each row in [startY .. endY)
                for (int i = startY; i < endY; i++)
                {
                    // The "source row" in the patch column is (fracV >> 16)
                    int patchRow = fracV >> 16;
                    // clamp if needed
                    if (patchRow < 0) patchRow = 0;
                    if (patchRow >= segment.Length) patchRow = segment.Length - 1;

                    // Write the final 8-bit index into 'data[]'
                    data[screenPos] = segment.Data[sourceIndex + patchRow];

                    screenPos++;    // move down one pixel
                    fracV += stepV; // advance vertical fraction
                }
            }
        }

    }
}
