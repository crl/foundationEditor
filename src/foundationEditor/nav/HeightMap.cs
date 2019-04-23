using foundation;
using UnityEngine;

namespace foundationEditor
{
    public class HeightMap
    {
        private int w;
        private int h;
        private Texture2D texture2D;
        private static Color[] colors;
        public HeightMap(int w, int h, TextureFormat format, bool mipmap=false)
        {
            this.w = w;
            this.h = h;
            texture2D = new Texture2D(w, h, format, mipmap);
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            colors = new Color[w * h];
        }

        public Texture2D EndDraw()
        {
            texture2D.SetPixels(colors);
            texture2D.Apply();
            return texture2D;
        }

        public void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (p1.y > p2.y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.y > p3.y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.y > p2.y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            // computing lines' directions
            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            // Computing slopes
            if (p2.y - p1.y > 0)
                dP1P2 = (p2.x - p1.x) / (p2.y - p1.y);
            else
                dP1P2 = 0;

            if (p3.y - p1.y > 0)
                dP1P3 = (p3.x - p1.x) / (p3.y - p1.y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.y; y <= (int)p3.y; y++)
                {
                    if (y < p2.y)
                    {
                        ProcessScanLine(y, p1, p3, p1, p2, color);
                    }
                    else
                    {
                        ProcessScanLine(y, p1, p3, p2, p3, color);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 - - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.y; y <= (int)p3.y; y++)
                {
                    if (y < p2.y)
                    {
                        ProcessScanLine(y, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        ProcessScanLine(y, p2, p3, p1, p3, color);
                    }
                }
            }
        }
        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        void ProcessScanLine(int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, Color color)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.y == pb.y or pc.y == pd.y, gradient is forced to 1
            var gradient1 = pa.y != pb.y ? (y - pa.y) / (pb.y - pa.y) : 1;
            var gradient2 = pc.y != pd.y ? (y - pc.y) / (pd.y - pc.y) : 1;

            int sx = (int)Interpolate(pa.x, pb.x, gradient1);
            int ex = (int)Interpolate(pc.x, pd.x, gradient2);
            if (sx > ex)
            {
                int t = sx;
                sx = ex;
                ex = t;
            }

            // starting Z & ending Z
            float z1 = Interpolate(pa.z, pb.z, gradient1);
            float z2 = Interpolate(pc.z, pd.z, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                DrawPoint(new Vector3(x, y, z), color);
            }
        }

        private void DrawPoint(Vector3 v, Color color)
        {
            if (v.x < 0 || v.x >= w)
            {
                return;
            }
            if (v.y < 0 || v.y >= h)
            {
                return;
            }
            uint clr = (uint)((v.z + 100) * 100);
            int index = (int)(v.y * w + v.x);
            colors[index] = ColorUtils.RGBToColor(clr);
        }

        // Clamping values to keep them between 0 and 1

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Mathf.Clamp(gradient, 0, 1);
        }

    }
}