using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CharacterController))]
public class FPS_PlayerController : MonoBehaviour
{
    #region Variables Fisicas
    [Header("---- FISICAS ----")]
    [Tooltip("Fuerza de la gravedad, por defecto 9.8")]
    public float gravity = 98f;

    [Tooltip("Velocidad de caida del personaje, se calcula ingame.")]
    public float fallVelocity;

    [Tooltip("Fuerza recibida por el personaje, en cualquier direccion.")]
    [SerializeField] Vector3 impact;
    #endregion

    #region Variables Personaje
    [Header("----  Personaje  ----")]
    [Tooltip("Fuerza de salto del personaje.")]
    public float jumpForce;

    private float horizontalMove;
    private float verticalMove;
    private Vector3 playerInput;
    CharacterController cc_player;
    [Tooltip("Velocidad de movimiento del personaje en suelo.")]
    public float playerSpeed;
    private Vector3 playerDirection;

    bool isOnSlope = false; //Está en rampa
    [Tooltip("Valor restado al 'slope limit' del CharacterController para que se den pequeños deslizamientos en rampas que el personaje puede subir.")]
    public float slopeOfset = 15; //Un valor que restamos al slopeLimit para mostrar caidas en rampas que puede subir
    [Tooltip("Velocidad con la que se desliza el personaje")]
    public float slideSpeed = 6;
    [Tooltip("Indicador de gravedad en el momento de deslizamiento.")]
    public float slideGravity = 10;
    private Vector3 hitNormal; //Vector normal del objeto con el colisionamos
    #endregion

    #region Variables Camara
    //----------CAM--------//
    [Header("----  Camara  ----")]
    [Tooltip("Cámara principal del juego, esta seguirá al personaje.")]
    public Camera mainCamera;
    float horizontalCam;
    float verticalCam;
    private Vector3 camForward;
    private Vector3 camRight;
    #endregion

    void Start()
    {
        Assert.AreNotEqual(null, mainCamera);
        cc_player = GetComponent<CharacterController>();     
    }

    void Update()
    {
        #region Inputs
        //Recogemos los valores de los input
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");
        horizontalCam = Input.GetAxis("Mouse X");
        verticalCam = Input.GetAxis("Mouse Y");
        //Debug.Log(new Vector2(horizontalCam, verticalCam));
        #endregion
        //Aplicamos los valores de los imput
        transform.Rotate(0,horizontalCam,0);
        mainCamera.transform.Rotate(-verticalCam,0,0);

        playerInput = new Vector3(horizontalMove, 0, verticalMove);
        //Normalizamos el vector de movimiento.
        playerInput = Vector3.ClampMagnitude(playerInput, 1);

        camDirection();
        playerDirection = playerInput.x * camRight + playerInput.z * camForward; 

        playerDirection = playerDirection * playerSpeed;

        //Aplicamos la gravedad
        setGravity();

        //Comprobamos si hay que realizar movimiento nuevo.
        playerActions();

        //Activamos los efectos sobre el jugador.
        playerEfects();

        //Aplicamos el movimiento del personaje
        cc_player.Move(playerDirection * Time.deltaTime);

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
        if (cc_player.isGrounded && Input.GetButtonDown("Jump"))
        {
            //Salta
            fallVelocity = jumpForce;
            playerDirection.y = fallVelocity;
        }
    }

    /// <summary>
    /// Realiza calculos de efectos recibidos por el jugador. p.e.
    /// </summary>
    void playerEfects(){
        #region Fuerzas
        if(impact.magnitude > 0.2){
            playerDirection = playerDirection + transform.forward * impact.x + transform.right * impact.z;
        }
        if(impact.magnitude > 0){
            //Consumimos el impacto a lo largo del tiempo
            impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        }
        #endregion
    }

    /// <summary>
    /// Calcula los efectos de gravedad en el personaje
    /// </summary>
    void setGravity(){
        if (cc_player.isGrounded)
        {
            fallVelocity = -gravity * Time.deltaTime;
        }else{
            fallVelocity -= gravity * Time.deltaTime;
        }
        playerDirection.y = fallVelocity;
        calculateSlide();
    }

    /// <summary>
    /// Calcula el deslizamiento en trampas, si superan un ángulo determinado.
    /// </summary>
    void calculateSlide(){
        //Comprobamos si esta en rampa.
        float angle = Vector3.Angle(Vector3.up, hitNormal);
        isOnSlope = angle >= (cc_player.slopeLimit - slopeOfset);
        if (isOnSlope)
        {
            playerDirection.x += (1f - hitNormal.y) * hitNormal.x * slideSpeed; //1f - hitnormal.y aumenta la caida cuanto mas pronunciada sea la cuesta.
            playerDirection.z += (1f - hitNormal.y) * hitNormal.z * slideSpeed;
            playerDirection.y -= slideGravity;
            }
        }

    /// <summary>
    /// Añade una fuerza determinada por su dirección y potenciador.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="force"></param>
    public void addForce(Vector3 direction, float force){
        direction.Normalize();
        if(direction.y < 0){
            direction.y = -direction.y;
        }
        impact += direction.normalized * force; // / *mass; si tuvieramos masa

    }
}
