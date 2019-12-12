using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float rotate_speed = 0.05f;
    private float move_speed = 0.1f;
    private float pan_speed = 0.1f;

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetAxis("Fire2") == 1f) {
             transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * rotate_speed, Input.GetAxis("Mouse X") * rotate_speed, 0));
             float x = transform.rotation.eulerAngles.x;
             if (85 <= x && x <= 95) {
                 x = ((x-90) > 0 ? 95 : 85);
             }
             if (265 <= x && x <= 275) {
                 x = ((x-270) > 0 ? 275 : 265);
             }
             float y = transform.rotation.eulerAngles.y;
             transform.rotation = Quaternion.Euler(x, y, 0);
        }

        if (Input.GetAxis("Fire3") == 1f) {
            Vector3 mouse_move_vec = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0);
            transform.position += transform.rotation * mouse_move_vec * mouse_move_speed;
        }

        float mouse_scroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouse_scroll != 0f) {
            transform.position += transform.forward * mouse_scroll * pan_speed;
        }
        */

        Touch[] touchs = Input.touches;
        if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Moved) {
            float touchX = touchs[0].deltaPosition.x;
            float touchY = touchs[0].deltaPosition.y;
             transform.Rotate(new Vector3(-touchY * rotate_speed, touchX * rotate_speed, 0));
             float x = transform.rotation.eulerAngles.x;
             if (85 <= x && x <= 95) {
                 x = ((x-90) > 0 ? 95 : 85);
             }
             if (265 <= x && x <= 275) {
                 x = ((x-270) > 0 ? 275 : 265);
             }
             float y = transform.rotation.eulerAngles.y;
             transform.rotation = Quaternion.Euler(x, y, 0);
        } else if (Input.touchCount == 2) {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrePos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrePos = touchOne.position - touchOne.deltaPosition;

            float preMagnitude = (touchOnePrePos-touchZeroPrePos).magnitude;
            float currentMagnitude = (touchOne.position-touchZero.position).magnitude;

            float defference = currentMagnitude - preMagnitude;

            transform.position += transform.forward * defference * pan_speed;
        }

    }
}
