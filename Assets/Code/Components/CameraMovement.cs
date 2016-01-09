using UnityEngine;

namespace FinalFrontier
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
                //main cam
                m_movePos.x = (Input.GetAxis("Horizontal") * (m_movementSpeed + GetComponent<Camera>().orthographicSize));
                m_movePos.y = (Input.GetAxis("Vertical") * (m_movementSpeed + GetComponent<Camera>().orthographicSize));
                m_movePos.z = Input.GetAxis("Mouse ScrollWheel") * (m_zoomSpeed * 1f);
                //m_perspective.GetComponent<Camera>().fieldOfView += Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * m_zoomSpeed;

                m_movePos /= 100;

                transform.Translate(m_movePos);

                //perspective
                m_orthographic.GetComponent<Camera>().orthographicSize = -transform.position.z - (-transform.position.z * ORTHOGRAPHIC_VS_PERSPECTIVE_DIFFERENCE);
            }
        }
    }
}