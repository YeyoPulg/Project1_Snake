using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Serpiente : MonoBehaviour
{
    public GameObject Block; //OBJETO QUE SE USA PARA CREAR EL MUNDO
    public GameObject Comida; //OBJETO DE LA COMIDA
    public GameObject Map; //HACE REFERENCIA A AL MAPA DEL MUNDO
    public int al, an; //VARIABLES PARA EL TAMAÑO DEL MUNDO

    private Queue<GameObject> ColaSnake = new Queue<GameObject>(); //COLA DE LA SERPIENTE, SALE EL PRIMERO Y ENTRA AL ULTIMO
    private GameObject CabezaSnake; // CABEZA DE LA SERPIENTE
    private GameObject comida; //ESCENA
    private Vector3 direc = Vector3.right;//DIRECCIÓN DE LA SERVIENTE DEFAULT

    private enum TipoCasilla
    {
        Vacio,Obstaculo,Comida
    }

    private TipoCasilla[,] mapa;

    private void Awake()
    {
        mapa = new TipoCasilla[an, al];
        CreateMap(); //METODO CREAR MUNDO CrearMuros();
        int posx = an / 2;
        int posy = al / 2;
        
        CabezaSnake = CreateSnake(posx, posy);//METODO PARA CREAR LA SERPIENTE NuevoBloque(posx, posy);
        InsComidaPosRam();//METODO INSTANCIAR COMIDA InstanciarItemEnPosicionAleatoria();
        StartCoroutine(Movimiento());
        
    }

    private void MoverItemPosicionAleatoria()
    {
        Vector2Int posicion = ObPosVacia(); //METODO PARA OBTENER UNA POSICION VACIA
        comida.transform.position = new Vector3(posicion.x, posicion.y);
        EstablecerMapa(comida.transform.position, TipoCasilla.Comida);
    }

    private void InsComidaPosRam() //INISTANCIAR LA COMIDA
    {
        Vector2Int pos = ObPosVacia(); //METODO PARA OBTENER UNA POSICION VACIA
        comida = CreateComida(pos.x, pos.y);//METODO CREAR COMIDA
    }

    private Vector2Int ObPosVacia() //OBTENEMOS UNA POSICION VACIA
    {
        List<Vector2Int> posicionesVacias = new List<Vector2Int>(); //POSICIONES VACIAS EN LISTA

        //CALCULA POSICIONES VACIAS EN LO ALTO Y ANCHO
        for(int x=0; x < an; x++)
        {
            for(int y = 0; y < al; y++)
            {
                if(mapa[x,y] == TipoCasilla.Vacio)
                {
                    posicionesVacias.Add(new Vector2Int(x, y));
                }
            }
        }
        return posicionesVacias[Random.Range(0,posicionesVacias.Count)]; //RETORNA LA POSICION VACIA EN UN RANGO RANDOM
    }

    private TipoCasilla ObtenerMapa (Vector3 posicion)
    {
        return mapa[Mathf.RoundToInt(posicion.x), Mathf.RoundToInt(posicion.y)];

    }

    private void EstablecerMapa(Vector3 posicion, TipoCasilla valor)
    {
        mapa[Mathf.RoundToInt(posicion.x), Mathf.RoundToInt(posicion.y)] = valor;
    }


    private IEnumerator Movimiento() //MOVIMIENTO DE LA SERPIENTE
    {
        WaitForSeconds espera = new WaitForSeconds(0.15f); // TIEMPO DE ESPERA ENTRE PASO DE LA SERPIENTE
       
        while (true) //SIEMPRE MOVERSE
        {
            //CALCULAR LA POSICIÓN A LA QUE DEBE MOVERSE EL OBJETO EN LA COLA
            Vector3 nuevaPosicion = CabezaSnake.transform.position + direc;
            TipoCasilla casillaAOcupar = ObtenerMapa(nuevaPosicion);

            if(casillaAOcupar == TipoCasilla.Obstaculo)
            {
                Debug.Log("Muerto!");
                yield return new WaitForSeconds(2);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                yield break;
            }
            else
            {
                GameObject lastColaSnake;
                if(casillaAOcupar == TipoCasilla.Comida)
                {
                    lastColaSnake = CreateSnake(nuevaPosicion.x, nuevaPosicion.y);
                    MoverItemPosicionAleatoria();
                }

                //SACAR EL OBJETO QUE MAS TIEMPO LLEVA EN LA COLA
                lastColaSnake = ColaSnake.Dequeue();
                EstablecerMapa(lastColaSnake.transform.position, TipoCasilla.Vacio);

                //APLICAR POSICIÓN NUEVA
                lastColaSnake.transform.position = nuevaPosicion;
                EstablecerMapa(nuevaPosicion, TipoCasilla.Obstaculo);
                ColaSnake.Enqueue(lastColaSnake);

                //LA CABEZA SERA LA PRIMERA PARTE DE LA COLA
                CabezaSnake = lastColaSnake;
                //HACEMOS ESPERA LA COROUTINE
                yield return espera;

            }


        }
    }

    private GameObject CreateSnake(float x, float y) //CREAR LA SERPIENTE O JUGADOR
    {
        //INSTANCIAR MISMO BLOQUE USADO PARA EL MAPA, PONER EL OBJETO COMO HIJO EN EL SNAKE.
        GameObject nuevo = Instantiate(Block, new Vector3(x, y), Quaternion.identity, this.transform);
        //INSTANCIAR OBJETO COLA DE LA SERPIENTE, AGREGAR A LA COLA
        ColaSnake.Enqueue(nuevo);
        EstablecerMapa(nuevo.transform.position, TipoCasilla.Obstaculo);
        return nuevo; //INICIALIZA COMO LA CABEZA
    }

    private GameObject CreateComida(int x, int y) //PARA CREAR COMIDAD
    {
        //INTANCIAR OBJETO COMIDA
        GameObject nuevo = Instantiate(Comida, new Vector3(x, y), Quaternion.identity, Map.transform);
        EstablecerMapa(nuevo.transform.position, TipoCasilla.Comida);
        return nuevo;
    }


    private void CreateMap() //DOS CICLOS ANIDADOS PARA RECORRER LAS POCISIONES
    {
        for(int x = 0; x < an; x++) //CICLO RECORRE EL ANCHO
        {
            for(int y = 0; y < al; y++) //CICLO RECORRE EL ALTO
            {
                //CREAER BORDES
                if (x== 0|| x==an-1 || y==0 ||y==al -1)
                {
                    //CREAR MUNDO
                    Vector3 posicion = new Vector3(x, y);//INTERACCION CON EL BUCLE

                    //INSTANCIAR EL OBJETO EN LA POS. CON ROTACIÓN ESTANDAR, PONER LOS OBJETOS COMO HIJOS EN EL MAP.
                    Instantiate(Block, posicion, Quaternion.identity, Map.transform);
                    EstablecerMapa(posicion, TipoCasilla.Obstaculo);
                }

            }
        }
    }
    //Update es llamada una vez cada frame
    private void Update() //PARA ACCEDER A LAS TECLAS Y MOVIMIENTO
    {
        float hori = Input.GetAxisRaw("Horizontal"); //RECIBO VALORES EN HORIZONTAL EJE (X)
        float vert = Input.GetAxisRaw("Vertical"); //RECIBO VALORES EN VERTICAL EJE (Y)

        Vector3 direccionSeleccionada = new Vector3(hori, vert);//DEFINO VARIABLE PARA TOMAR LA POSICIÓN 

        if (direccionSeleccionada != Vector3.zero)//CONDICIONAL PARA TOMAR VALOR Y SU MOVIMIENTO
        {
            direc = direccionSeleccionada; //TOMA LA ULTIMA TECLA PULSADA, SOLO SI ES DIFERENTE DE 0
        }
    }

}
