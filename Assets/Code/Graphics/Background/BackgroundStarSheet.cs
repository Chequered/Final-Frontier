using UnityEngine;
using UnityEngine.UI;


namespace EndlessExpedition
{
    namespace Graphics
    {
        [RequireComponent(typeof(RawImage))]
        public class BackgroundStarSheet : MonoBehaviour
        {
            public const int MAX_STARS = 50;
            public const int TEX_WIDTH = 512;
            public const int TEX_HEIGHT = 512;

            private RawImage m_image;
            
            public void Generate(int amount)
            {
                m_image = GetComponent<RawImage>();

                Texture2D tex = new Texture2D(TEX_WIDTH, TEX_HEIGHT);
                tex.filterMode = FilterMode.Point;

                Color[] pix = tex.GetPixels();
                for (int i = 0; i < pix.Length; i++)
                {
                    pix[i] = Color.clear;
                }

                tex.SetPixels(pix);

                for (int i = 0; i < amount; i++)
                {
                    tex.SetPixel(Random.Range(0, TEX_WIDTH), Random.Range(0, TEX_HEIGHT), Color.white);
                }
                tex.Apply();
                m_image.texture = tex;
            }
        }
    }
}
