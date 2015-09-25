using UnityEngine;

namespace FinalFrontier
{
    namespace Components
    {
        internal class CameraMovement : MonoBehaviour
        {

            public float m_movementSpeed;
            public float m_zoomSpeed;

            private Vector3 m_movePos = new Vector3(0, 0, 0);
            private void Update()
            {
                m_movePos.x = (Input.GetAxis("Horizontal") * Time.deltaTime * (m_movementSpeed + GetComponent<Camera>().orthographicSize));
                m_movePos.y = (Input.GetAxis("Vertical") * Time.deltaTime * (m_movementSpeed + GetComponent<Camera>().orthographicSize));
                GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * m_zoomSpeed;

                transform.Translate(m_movePos);
            }
        }
    }
}   
