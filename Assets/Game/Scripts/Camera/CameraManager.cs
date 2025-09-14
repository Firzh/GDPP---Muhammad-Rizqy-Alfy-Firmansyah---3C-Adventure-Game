using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    public CameraState CameraState;
    [SerializeField]
    private CinemachineCamera _fpsCamera;

    [SerializeField]
    private CinemachineCamera _tpsCamera;

    [SerializeField]
    private InputManager _inputManager;

    public void Start()
    {
        _inputManager.OnChangePoV += SwitchCamera;
    }

    public void SetTPSFieldOfView(float fieldOfView)
    {
        _tpsCamera.Lens.FieldOfView = fieldOfView;
    }

    public void SetFPSClampedCamera(bool isClamped, Vector3 playerRotation)
    {
        // In Cinemachine 3, use the new Camera component
        CinemachinePanTilt pov = _fpsCamera.GetComponent<CinemachinePanTilt>();

        if (isClamped)
        {
            pov.PanAxis.Wrap = false;
            // Membuat Vector3 baru dengan mengurangi komponen x dari playerRotation dengan 45
            float minAngle = -0.05f;
            float maxAngle = 0.05f;
            Vector3 RotasiAdjustMin = new Vector3(playerRotation.x, playerRotation.y - minAngle, playerRotation.z);
            pov.PanAxis.Range.y = RotasiAdjustMin.y;

            Vector3 RotasiAdjustMax = new Vector3(playerRotation.x, playerRotation.y + maxAngle, playerRotation.z);
            pov.PanAxis.Range.y = RotasiAdjustMax.y;
        }
        else
        {
            Vector3 RotasiMin = new Vector3(playerRotation.x, playerRotation.y, playerRotation.z);
            pov.PanAxis.Range.y = -180;
            pov.PanAxis.Range.y = 180;
            pov.PanAxis.Wrap = true;
        }
    }

    private void SwitchCamera()
    {
        if (CameraState == CameraState.ThirdPerson)
        {
            CameraState = CameraState.FirstPerson;
            _tpsCamera.gameObject.SetActive(false);
            _fpsCamera.gameObject.SetActive(true);
        }
        else
        {
            CameraState = CameraState.ThirdPerson;
            _tpsCamera.gameObject.SetActive(true);
            _fpsCamera.gameObject.SetActive(false);
        }
    }
}
