using UnityEngine;
using UnityEngine.UI;


namespace EndlessExpedition
{
    namespace Graphics
    {
        [RequireComponent(typeof(RawImage))]
        public class BackgroundSun: MonoBehaviour
        {
            public const int TEX_WIDTH = 32;
            public const int TEX_HEIGHT = 32;

            private RawImage m_image;

            private void Start()
            {
                Generate();
            }

            public void Generate()
            {
                m_image = GetComponent<RawImage>();
                float seed = Random.Range(-2500, 2500);

                Texture2D tex = new Texture2D(TEX_WIDTH, TEX_HEIGHT);
                tex.filterMode = FilterMode.Point;

                bool[,] circleShape = TerrainExtensions.DataCircle(TEX_WIDTH / 2, TEX_WIDTH / 2, TEX_WIDTH / 2);

                Color centre = Color.yellow * 0.85f;
                Color detail = Color.yellow * 1.55f;
                //Color edge = Color.yellow * 0.7f;

                for (int x = 0; x < TEX_WIDTH; x++)
                {
                    for (int y = 0; y < TEX_HEIGHT; y++)
                    {
                        if (!circleShape[x, y])
                        {
                            tex.SetPixel(x, y, Color.clear);
                            continue;
                        }
                        Color color = centre;

                        if (Random.Range(0f, 100f) <= 15f)
                            color = detail;

                        tex.SetPixel(x, y, color);
                    }
                }
                tex.Apply();
                m_image.texture = tex;
            }
        }
    }
}
