using UnityEngine;

public class Player : MonoBehaviour
{
    //public int lives;
    [SerializeField] float playerSpeed;
    [SerializeField] BulletController[] bullet = new BulletController[2];
    [SerializeField] GameObject SpawnPoint;
    public bool EscudoOn;
    [SerializeField] GameObject Escudo;
    CharacterController cc;


    void Start()
    {
        GameControllerScript.controller.Player = this; //player se coloca na variavel player do GameController
        //cc = GetComponent<CharacterController>();
        //cc.detectCollisions = false;

        Escudo.SetActive(false);
    }


    private void Update()
    {
        Mover();
        RotatePlayer();
        if (Input.GetMouseButtonDown(0)) Atirar(0, 30);
        if (Input.GetMouseButtonDown(1)) Atirar(1, 30);
    }

    void Mover()
    {
        float h = Input.GetAxis("Horizontal") * Time.deltaTime * playerSpeed; //esquerda e direita, x
        //float v = Input.GetAxis("Vertical") * Time.deltaTime * playerSpeed; 
        Vector3 xValue = new Vector3(h,0, 0);
        cc.Move(xValue);

    }

    void RotatePlayer()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 dir = worldMousePos - transform.position;
        dir.z = 0; //pra não rotacionar em z
        transform.rotation = Quaternion.LookRotation(dir, Vector3.forward);

    }

    public void PegarPowerUp(int powerUp)
    {
        switch (powerUp)
        {
            //case 1: // medkit
            //    GameControllerScript.controller.LifePoints(1);
            //    Debug.Log("PowerUp: +1 Vida!");
            //    break;

            //case 2: // spray
            //    GameControllerScript.controller.ChangePoints(*2); // multiplicar pontos por 2
            //    Debug.Log("PowerUp: +5 Pontos!");
            //    break;

            //case 3: // escudo
            //    StartCoroutine("ToggleEscudo");
            //    Debug.Log("PowerUp: Escudo ativado por 5 segundos!");
            //    break;
        }
    }

    void ToggleEscudo() //ativar imagem do escudo
    {
        if (EscudoOn)
        {
            EscudoOn = false;
            Escudo.SetActive(false);
        }
        else
        {
            EscudoOn = true;
            Escudo.SetActive(true);
        }
    }

    void TomarDano(int dano)
    {
        if (EscudoOn) return; // se o escudo estiver ativado, ignora o dano

        GameControllerScript.controller.pontos(-dano);

        GameControllerScript.controller.uiController.FlashDamage(0.7f); // mostra o efeito de dano
    }


    void Morrer()
    {
        if (GameControllerScript.controller.lives <= 0)
        {
            //o que acontece com o player, ex: animação
            GameControllerScript.controller.uiController.GameOver(); //já pede pra mudar de cena no GameController
        }

    }

    void Atirar(int qualBala, float balaSpeed)
    {
        Instantiate(bala[qualBala], SpawnPoint.transform.position, SpawnPoint.transform.rotation);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            Debug.Log("Pegou PowerUp");

            PowerUp powerUp = other.GetComponent<PowerUp>();

            if (powerUp != null)
            {
                PegarPowerUp(powerUp.id); // aplica o efeito correto
            }

            Destroy(other.gameObject); // remove o PowerUp da cena
        }

        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();

            if (bullet != null)
            {
                TomarDano(1);
            }
        }
    }
}