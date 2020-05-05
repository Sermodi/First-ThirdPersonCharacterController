using UnityEngine;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    #region Variables Fisicas
    [Header("---- FISICAS ----")]
    [Tooltip("Fuerza de la gravedad, por defecto 9.8")]
    public float gravity = 9.8f;

    [Tooltip("Velocidad de caida del personaje, se calcula ingame.")]
    public float fallVelocity;
    #endregion
    #region Variables Personaje
    [Header("----  Personaje  ----")]
    [Tooltip("Fuerza de salto del personaje.")]
    public float jumpForce;

    private float horizontalMove;
    private float verticalMove;
    private Vector3 playerInput;
    public CharacterController player;
    [Tooltip("Velocidad de movimiento del personaje en suelo.")]
    public float playerSpeed;
    private Vector3 playerDirection;

    //Está en rampa
    public bool isOnSlope = false;
    [Tooltip("Valor restado al 'slope limit' del CharacterController para que se den pequeños deslizamientos en rampas que el personaje puede subir.")]
    public float slopeOfset = 15; //Un valor que restamos al slopeLimit para mostrar caidas en rampas que puede subir
    [Tooltip("Velocidad con la que se desliza el personaje")]
    public float slideSpeed = 6;
    [Tooltip("Indicador de gravedad en el momento de deslizamiento.")]
    public float slideGravity = 10;
    private Vector3 hitNormal;
    #endregion
    #region Variables Camara
    //----------CAM--------//
    public bool useRelativeCamMovement;
    public float rotateSpeed;
    [Header("----  Camara  ----")]
    [Tooltip("Cámara principal del juego, esta seguirá al personaje.")]
    public Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Assert.AreNotEqual(null, mainCamera);
        player = GetComponent<CharacterController>();     
    }

    // Update is called once per frame
    void Update()
    {
        #region Inputs
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");
        #endregion
        playerInput = new Vector3(horizontalMove, 0, verticalMove);
        //Normalizamos el vector de movimiento.
        playerInput = Vector3.ClampMagnitude(playerInput, 1);

        if (useRelativeCamMovement)
        {
            //Comprobamos la direccion de la camara para orientar la direccion del player acorde.
            camDirection();
            playerDirection = playerInput.x * camRight + playerInput.z * camForward;
        }
        else{
            playerDirection = playerInput.x * transform.forward.normalized * rotateSpeed/100 + playerInput.z * transform.right.normalized * rotateSpeed/100;
        }

        playerDirection = playerDirection * playerSpeed;

        player.transform.LookAt(player.transform.position + playerDirection);

        //Aplicamos la gravedad
        setGravity();

        //Comprobamos si hay que realizar movimiento nuevo.
        playerActions();

        //Aplicamos el movimiento del personaje
        player.Move(playerDirection * Time.deltaTime);

        Debug.Log(player.velocity);
    }

    /// <summary>
    /// OnControllerColliderHit is called when the controller hits a
    /// collider while performing a Move.
    /// </summary>
    /// <param name="hit">The ControllerColliderHit data associated with this collision.</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    /// <summary>
    /// Calcula la dirección de la camara, tanto frontal como lateral. 
    /// </summary>
    void camDirection(){
        camForward = mainCamera.transform.forward.normalized;
        camRight = mainCamera.transform.right.normalized;

        camForward.y = 0;
        camRight.y = 0;
    }

    /// <summary>
    /// Realiza acciones del jugador que puedan alterar su posición
    /// </summary>
    void playerActions(){
        if (player.isGrounded && Input.GetButtonDown("Jump"))
        {
            //Salta
            fallVelocity = jumpForce;
            playerDirection.y = fallVelocity;
        }
    }

    /// <summary>
    /// Calcula los efectos de gravedad en el personaje
    /// </summary>
    void setGravity(){
        if (player.isGrounded)
        {
            fallVelocity = -gravity * Time.deltaTime;
        }else{
            fallVelocity -= gravity * Time.deltaTime;
        }
        playerDirection.y = fallVelocity;
        calculateSlide();
    }

    /// <summary>
    /// Calcula los efectos de gravedad en el personaje
    /// </summary>
    void calculateSlide(){
        //Comprobamos si esta en rampa.
        float angle = Vector3.Angle(Vector3.up, hitNormal);
        isOnSlope = angle >= (player.slopeLimit - slopeOfset);
        if (isOnSlope)
        {
            playerDirection.x += (1f - hitNormal.y) * hitNormal.x * slideSpeed; //1f - hitnormal.y aumenta la caida cuanto mas pronunciada sea la cuesta.
            playerDirection.z += (1f - hitNormal.y) * hitNormal.z * slideSpeed;
            playerDirection.y -= slideGravity;
            }
        }
}
