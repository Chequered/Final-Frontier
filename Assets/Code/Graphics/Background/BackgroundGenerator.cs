using UnityEngine;
using UnityEngine.UI;
namespace EndlessExpedition
{
    namespace Graphics
    {
        public class BackgroundGenerator : MonoBehaviour
        {
            //----------------------------------------------
            //CONFIG
            public int Planets;
            public int Stars;
            public int Suns;

            public float MaxMovementSpeedStar;
            public float MinMovementSpeedStar;
            //----------------------------------------------

            private RectTransform m_transform;

            private void Start()
            {
                Generate();
            }

            public void Generate()
            {
                m_transform = GetComponent<RectTransform>();

                float screenWidth = m_transform.sizeDelta.x * 2;
                float screenHeight = m_transform.sizeDelta.y * 2;

                //Stars
                for (int s = 0; s < Stars / BackgroundStarSheet.MAX_STARS; s++)
                {
                    GameObject starSheet = new GameObject("Star Sheet");
                    starSheet.transform.SetParent(transform);
                    starSheet.AddComponent<RawImage>();
                    starSheet.GetComponent<RectTransform>().sizeDelta = m_transform.sizeDelta;
                    starSheet.AddComponent<BackgroundStarSheet>().Generate(BackgroundStarSheet.MAX_STARS);

                    float scale = Random.Range(0.75f, 1.15f);
                    starSheet.transform.localScale = new Vector3(scale, scale, 1f);
                }

                //Planets
                for (int p = 0; p < Planets; p++)
                {
                    GameObject planet = new GameObject("Planet");
                    planet.transform.SetParent(transform);
                    planet.AddComponent<RawImage>();
                    planet.AddComponent<BackgroundPlanet>().Generate();
                    planet.transform.localPosition = new Vector3(Random.Range(-screenWidth / 2, screenWidth / 2), Random.Range(-screenHeight / 2, screenHeight / 2), Random.Range(30, 2550));

                    float scale = Random.Range(0.15f, 0.55f);
                    planet.transform.localScale = new Vector3(scale, scale, 1f);
                }

                //Suns
                for (int p = 0; p < Suns; p++)
                {
                    GameObject sun = new GameObject("Sun");
                    sun.transform.SetParent(transform);
                    sun.AddComponent<RawImage>();
                    sun.AddComponent<BackgroundSun>().Generate();
                    sun.transform.localPosition = new Vector3(Random.Range(-screenWidth / 2, screenWidth / 2), Random.Range(-screenHeight / 2, screenHeight / 2), Random.Range(30, 2550));

                    float scale = Random.Range(0.25f, 1.05f);
                    sun.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
        }
    }
}
