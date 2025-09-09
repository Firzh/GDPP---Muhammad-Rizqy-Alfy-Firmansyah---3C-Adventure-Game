using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class InputManager : MonoBehaviour
{
    public Action<Vector2> OnMoveInput;

    private void Update()
    {
        // Panggil metode pergerakan yang baru
        CheckMovementInput();

        // Panggil metode pengecekan input lainnya
        CheckJumpInput();
        CheckSprintInput();
        CheckCrouchInput();
        CheckCameraChangeInput();
        CheckClimbInput();
        CheckGlideInput();
        CheckCancelInput();
        CheckPunchInput();
        CheckEscapeInput();
    }

    // Metode baru untuk pergerakan menggunakan GetAxis()
    private void CheckMovementInput()
    {
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");

        if (horizontalAxis != 0 || verticalAxis != 0)
        {
            // Buat Vector2 dari input dan panggil action
            Vector2 inputAxis = new Vector2(horizontalAxis, verticalAxis);
            OnMoveInput?.Invoke(inputAxis);

            // sama dengan 
            // if (OnMoveInput != null)
            // {
            //     OnMoveInput.Invoke(inputAxis);
            // }
        }
    }

    // --- Metode Aksi Lainnya (tetap sama) ---

    private void CheckJumpInput()
    {
        bool isPressJumpInput = Input.GetKeyDown(KeyCode.Space);
        if (isPressJumpInput)
        {
            Debug.Log("Jump");
        }
    }

    private void CheckSprintInput()
    {
        bool isPressSprint = Input.GetKey(KeyCode.LeftShift);
        if (isPressSprint)
        {
            Debug.Log("Lari");
        }
    }

    private void CheckCrouchInput()
    {
        bool isPressCrouch = Input.GetKey(KeyCode.LeftControl);
        if (isPressCrouch)
        {
            Debug.Log("Jongkok");
        }
    }
    
    private void CheckCameraChangeInput()
    {
        bool isPressCameraChange = Input.GetKeyDown(KeyCode.Q);
        if (isPressCameraChange)
        {
            Debug.Log("Mengubah perspektif kamera");
        }
    }

    private void CheckClimbInput()
    {
        bool isPressClimb = Input.GetKeyDown(KeyCode.E);
        if (isPressClimb)
        {
            Debug.Log("Memanjat");
        }
    }

    private void CheckGlideInput()
    {
        bool isPressGlide = Input.GetKeyDown(KeyCode.G);
        if (isPressGlide)
        {
            Debug.Log("Meluncur");
        }
    }

    private void CheckCancelInput()
    {
        bool isPressCancel = Input.GetKeyDown(KeyCode.C);
        if (isPressCancel)
        {
            Debug.Log("Membatalkan aksi");
        }
    }

    private void CheckPunchInput()
    {
        bool isPressPunch = Input.GetMouseButtonDown(0);
        if (isPressPunch)
        {
            Debug.Log("Memukul");
        }
    }

    private void CheckEscapeInput()
    {
        bool isPressEscape = Input.GetKeyDown(KeyCode.Escape);
        if (isPressEscape)
        {
            Debug.Log("Kembali ke Main Menu");
        }
    }
}
