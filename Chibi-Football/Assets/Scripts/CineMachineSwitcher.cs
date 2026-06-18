using UnityEngine;
using UnityEngine.InputSystem;

public class CMSwitcher : MonoBehaviour
{
    [SerializeField]
    private InputAction action;
    private Animator animator;
    private int cameraFixed = 1;
    private int cameraSide = 0;
    private int cameraEnd = 0; 
    private int cameraThirdPerson = 0; 
    private int cameraTop = 0;

    private void Awake() {
        animator = GetComponent<Animator>();

    }
    
    private void OnEnable() {
        action.Enable();
    }

    private void OnDisable() {
        action.Disable();
    }

    void Start()
    {
        action.performed += _ => SwitchState();
    }

    private void SwitchState(){
        if (cameraFixed == 1){
            animator.Play("ViewFixed");
            cameraFixed = 0;
            cameraSide = 1;
        } else if (cameraSide == 1){
            animator.Play("ViewSide");
            cameraSide = 0;
            cameraEnd = 1;
        } else if (cameraEnd == 1){
            animator.Play("ViewEnd");
            cameraEnd = 0;
            cameraTop = 1;
        } else if (cameraTop == 1){
            animator.Play("ViewTop");
            cameraTop = 0;
            cameraThirdPerson = 1;
        } else if (cameraThirdPerson == 1){
            animator.Play("ViewThirdPerson");
            cameraThirdPerson = 0;
            cameraFixed = 1;
        }
    }   
}
