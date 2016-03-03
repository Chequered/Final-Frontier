using EndlessExpedition.Managers;
using EndlessExpedition.Terrain;
using UnityEngine;

namespace EndlessExpedition
{
    namespace Components
    {
        internal class CameraMovement : MonoBehaviour
        {
            private const float ORTHOGRAPHIC_VS_PERSPECTIVE_DIFFERENCE = 0.165f;

            public float m_movementSpeed = 500;
            public float m_zoomSpeed = 1500;

            private GameObject m_orthographic;

            private void Start()
            {
                m_orthographic = transform.FindChild("Orthographic").gameObject;
                m_orthographic.GetComponent<Camera>().orthographicSize += 0.01f;
                            }

            private Vector3 m_movePos = new Vector3(0, 0, 0);
            private void Update()
            {
                if (InputManager.isTypingInInputField)
                    return;

                //main cam
                m_movePos.x = (Input.GetAxis("Horizontal") * (m_movementSpeed * GetComponent<Camera>().orthographicSize));
                m_movePos.y = (Input.GetAxis("Vertical") * (m_movementSpeed  * GetComponent<Camera>().orthographicSize));
                m_movePos.z = Input.GetAxis("Mouse ScrollWheel") * (m_zoomSpeed * 1f);
                //m_perspective.GetComponent<Camera>().fieldOfView += Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * m_zoomSpeed;

                m_movePos /= 100;

                transform.Translate(m_movePos);

                //Clamp
                Vector3 clampPos = transform.position;
                if (clampPos.x < 0f)
                    clampPos.x = 0f;
                if (clampPos.x > TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE)
                    clampPos.x = TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE;
                if (clampPos.y < 0f)
                    clampPos.y = 0f;
                if (clampPos.y > TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE)
                    clampPos.y = TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE;
                if (clampPos.z > -6f)
                    clampPos.z = -6f;
                if (clampPos.z < -(TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE))
                    clampPos.z = -(TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE);
                
                transform.position = clampPos;

                //perspective
                m_orthographic.GetComponent<Camera>().orthographicSize = -transform.position.z - (-transform.position.z * ORTHOGRAPHIC_VS_PERSPECTIVE_DIFFERENCE);
            }
        }
    }
}