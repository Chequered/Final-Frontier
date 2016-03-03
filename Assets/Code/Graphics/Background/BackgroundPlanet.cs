using UnityEngine;
using UnityEngine.UI;


namespace EndlessExpedition
{
    namespace Graphics
    {
        [RequireComponent(typeof(RawImage))]
        public class BackgroundPlanet : MonoBehaviour
        {
            public const int TEX_WIDTH = 32;
            public const int TEX_HEIGHT = 32;

            private RawImage m_image;
            private float m_rotationSpeed;
            private Vector3 m_startPos;

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

                float[,] perlin = TerrainExtensions.GeneratePerlinMap(TEX_WIDTH, TEX_HEIGHT, seed,
                    new float[] { 5.5f, 5.5f, 6.5f}, //scale
                    new float[] { 0f, 250f, 125f }, //seed addition
                    new float[] { 1f, 0.8f, 0.75f}); //strengths

                bool[,] circleShape = TerrainExtensions.DataCircle(TEX_WIDTH / 2, TEX_WIDTH / 2, TEX_WIDTH / 2);

                Color a = new Color(Random.Range(0f, 0.6f), Random.Range(0f, 0.6f), Random.Range(0f, 0.6f));
                Color b = new Color(Random.Range(0f, 0.6f), Random.Range(0f, 0.6f), Random.Range(0f, 0.6f));
                Color c = new Color(Random.Range(0f, 0.6f), Random.Range(0f, 0.6f), Random.Range(0f, 0.6f));

                for (int x = 0; x < TEX_WIDTH; x++)
                {
                    for (int y = 0; y < TEX_HEIGHT; y++)
                    {
                        if (!circleShape[x, y])
                        {
                            tex.SetPixel(x, y, Color.clear);
                            continue;
                        }

                        float p = perlin[x, y];
                        Color color = Color.green;
                        if (p <= 0.425f)
                            color = a;
                        else if (p <= 0.54f)
                            color = b;
                        else if (p <= 0.58f)
                            color = c;
                        
                        tex.SetPixel(x, y, color);
                    }
                }
                tex.Apply();
                m_image.texture = tex;
                m_startPos = transform.localPosition;
            }

            private void Update()
            {
                transform.Rotate(new Vector3(0f, 0f, m_rotationSpeed));
                Vector3 cam2D = Camera.main.transform.position;
                cam2D.z /= 10;

                transform.localPosition = m_startPos - cam2D;
            }

            public void StartRotation(float speed)
            {
                m_rotationSpeed = speed;
            }

            public void StopRotation()
            {
                m_rotationSpeed = 0f;
            }
        }
    }
}
