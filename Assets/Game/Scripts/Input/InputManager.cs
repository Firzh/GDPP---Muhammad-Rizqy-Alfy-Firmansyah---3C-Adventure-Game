using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InputManager : MonoBehaviour
{

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
        float horizontalInput = Input.GetAxis("Horizontal"); // Nilai -1 (kiri/A) hingga 1 (kanan/D)
        float verticalInput = Input.GetAxis("Vertical");     // Nilai -1 (bawah/S) hingga 1 (atas/W)

        if (horizontalInput != 0 || verticalInput != 0)
        {
            // Vektor 3 ini bisa langsung digunakan untuk menggerakkan karakter
            Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
            
            // Contoh sederhana untuk melihat nilai input
            Debug.Log("Horizontal Axis: " + horizontalInput);
            Debug.Log("Vertical Axis: " + verticalInput);
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
