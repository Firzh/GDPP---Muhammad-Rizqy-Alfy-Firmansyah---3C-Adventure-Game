using UnityEngine;
// using System.Collections.Generic;
// using System.Collections;
using System;

public class InputManager : MonoBehaviour
{
    public Action<Vector2> OnMoveInput;
    public Action<bool> OnSprintInput;
    public Action OnJumpInput;
    public Action OnClimbInput;
    public Action OnCancelClimb;
    public Action OnChangePoV;
    public Action OnCrouchInput;

    private void Update()
    {
        // Panggil metode pergerakan yang baru
        CheckMovementInput();

        // Panggil metode pengecekan input lainnya
        CheckJumpInput();
        CheckSprintInput();
        CheckCrouchInput();
        CheckChangePoVInput();
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
            if (OnJumpInput != null)
            {
                OnJumpInput();
                // Debug.Log("Jump");
            }
        }
    }

    private void CheckSprintInput()
    {
        bool isHoldSprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (isHoldSprintInput)
        {
            if (OnSprintInput != null)
            {
                OnSprintInput(true);
                // Debug.Log("Sprint On");
            }
        }
        else
        {
            if (OnSprintInput != null)
            {
                OnSprintInput(false);
                // Debug.Log("Sprint Off");
            }
        }
    }

    private void CheckCrouchInput()
    {
        bool isPressCrouchInput = Input.GetKey(KeyCode.LeftControl);
        if (isPressCrouchInput)
        {
            OnCrouchInput();
            Debug.Log("Jongkok");
        }
    }
    
    private void CheckChangePoVInput()
    {
        bool isPressChangePoVInput = Input.GetKeyDown(KeyCode.Q);
        if (isPressChangePoVInput)
        {
            OnChangePoV();
            Debug.Log("Mengubah perspektif kamera");
        }
    }

    private void CheckClimbInput()
    {
        bool isPressClimbInput = Input.GetKeyDown(KeyCode.E);
        if (isPressClimbInput)
        {
            OnClimbInput();
            // Debug.Log("Memanjat");
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
        bool isPressCancelInput = Input.GetKeyDown(KeyCode.C);
        if (isPressCancelInput)
        {
            OnCancelClimb();
            // Debug.Log("Membatalkan aksi");
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
