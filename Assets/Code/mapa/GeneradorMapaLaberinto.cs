using UnityEngine;
using System.Collections.Generic;

public class GeneradorMapaLaberinto : MonoBehaviour
{
    [Header("Tamaño del mapa")]
    public int filas = 10;
    public int columnas = 10;

    [Header("Modo aleatorio")]
    public bool modoAleatorio = true;

    [Header("Cantidad de zonas especiales")]
    public int cantidadZonasSeguras = 3;
    public int cantidadMetas = 1;

    [Header("Prefabs")]
    public GameObject prefabPared;
    public GameObject prefabCamino;
    public GameObject prefabZonaSegura;
    public GameObject prefabMeta;

    [Header("Tamaño deseado de cada celda en Unity units")]
    public float anchoDeseado = 1f;
    public float altoDeseado = 1f;

    [Header("Agentes (hasta 4)")]
    public GameObject prefabAgente1;
    public GameObject prefabAgente2;
    public GameObject prefabAgente3;
    public GameObject prefabAgente4;

    [Header("Mapa manual (si modoAleatorio = false)")]
    public int[,] mapaManual = new int[5, 5]
    {
        {1,1,1,1,1},
        {1,0,0,3,1},
        {1,2,1,0,1},
        {1,0,0,0,1},
        {1,1,1,1,1}
    };

    private int[,] mapa;

    void Start()
{
    // Contar agentes válidos
    int cantidadAgentes = 0;
    GameObject[] prefabs = { prefabAgente1, prefabAgente2, prefabAgente3, prefabAgente4 };
    foreach (var agente in prefabs)
    {
        if (agente != null) cantidadAgentes++;
    }

    if (modoAleatorio)
    {
        mapa = GenerarLaberintoDFS(filas, columnas);

        // Reforzar bordes con paredes
        for (int x = 0; x < columnas; x++)
        {
            mapa[0, x] = 1;
            mapa[filas - 1, x] = 1;
        }
        for (int y = 0; y < filas; y++)
        {
            mapa[y, 0] = 1;
            mapa[y, columnas - 1] = 1;
        }

        ColocarZonasEspeciales(2, cantidadZonasSeguras);     // Zonas seguras
        ColocarZonasEspeciales(3, cantidadAgentes);           // Una meta por agente
    }
    else
    {
        if (mapaManual.GetLength(0) != filas || mapaManual.GetLength(1) != columnas)
        {
            Debug.LogError("Dimensiones del mapa manual no coinciden con filas y columnas especificadas.");
            return;
        }
        mapa = mapaManual;
    }

    InstanciarMapa();
    AjustarCamara();
    InstanciarAgentes();
}


    int[,] GenerarLaberintoDFS(int f, int c)
    {
        int[,] grid = new int[f, c];
        for (int y = 0; y < f; y++)
            for (int x = 0; x < c; x++)
                grid[y, x] = 1;

        System.Random rand = new System.Random();
        void Carve(int x, int y)
        {
            int[] dx = { 0, 0, 2, -2 };
            int[] dy = { 2, -2, 0, 0 };
            int[] orden = { 0, 1, 2, 3 };
            for (int i = 0; i < orden.Length; i++)
            {
                int r = rand.Next(i, orden.Length);
                int tmp = orden[i]; orden[i] = orden[r]; orden[r] = tmp;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[orden[i]];
                int ny = y + dy[orden[i]];
                if (nx >= 0 && ny >= 0 && nx < c && ny < f && grid[ny, nx] == 1)
                {
                    grid[ny, nx] = 0;
                    grid[y + dy[orden[i]] / 2, x + dx[orden[i]] / 2] = 0;
                    Carve(nx, ny);
                }
            }
        }

        int sx = rand.Next(c / 2) * 2 + 1;
        int sy = rand.Next(f / 2) * 2 + 1;
        grid[sy, sx] = 0;
        Carve(sx, sy);

        return grid;
    }

    void ColocarZonasEspeciales(int tipo, int cantidad)
    {
        System.Random rand = new System.Random();
        int intentos = 0;
        int colocados = 0;
        while (colocados < cantidad && intentos < 1000)
        {
            int y = rand.Next(filas);
            int x = rand.Next(columnas);
            if (mapa[y, x] == 0)
            {
                mapa[y, x] = tipo;
                colocados++;
            }
            intentos++;
        }
    }

    void InstanciarMapa()
    {
        for (int y = 0; y < filas; y++)
        {
            for (int x = 0; x < columnas; x++)
            {
                GameObject prefab = null;
                switch (mapa[y, x])
                {
                    case 0: prefab = prefabCamino; break;
                    case 1: prefab = prefabPared; break;
                    case 2: prefab = prefabZonaSegura; break;
                    case 3: prefab = prefabMeta; break;
                }
                if (prefab != null)
                {
                    Vector2 posicion = new Vector2(x * anchoDeseado, -y * altoDeseado);
                    GameObject celda = Instantiate(prefab, posicion, Quaternion.identity, transform);

                    SpriteRenderer sr = celda.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Vector2 size = sr.bounds.size;
                        float escalaX = anchoDeseado / size.x;
                        float escalaY = altoDeseado / size.y;
                        celda.transform.localScale = new Vector3(escalaX, escalaY, 1f);
                    }
                }
            }
        }
    }

    void AjustarCamara()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float ancho = columnas * anchoDeseado;
            float alto = filas * altoDeseado;
            cam.orthographicSize = Mathf.Max(ancho / cam.aspect, alto) * 0.5f;
            cam.transform.position = new Vector3(ancho / 2f - anchoDeseado / 2f, -alto / 2f + altoDeseado / 2f, -10f);
        }
    }

    void InstanciarAgentes()
{
    GameObject[] agentes = { prefabAgente1, prefabAgente2, prefabAgente3, prefabAgente4 };
    List<Vector2Int> posicionesLibres = new List<Vector2Int>();

    // Recolectar posiciones transitables
    for (int y = 0; y < filas; y++)
    {
        for (int x = 0; x < columnas; x++)
        {
            if (mapa[y, x] == 0)
            {
                posicionesLibres.Add(new Vector2Int(x, y));
            }
        }
    }

    Debug.Log($"[Instanciación] Celdas libres disponibles: {posicionesLibres.Count}");

    System.Random rand = new System.Random();
    int contador = 1;

    foreach (GameObject agentePrefab in agentes)
    {
        if (agentePrefab == null)
        {
            Debug.LogWarning("[Instanciación] Prefab de agente es nulo.");
            continue;
        }

        if (posicionesLibres.Count == 0)
        {
            Debug.LogWarning("[Instanciación] No hay más celdas libres para colocar agentes.");
            break;
        }

        int idx = rand.Next(posicionesLibres.Count);
        Vector2Int pos = posicionesLibres[idx];
        posicionesLibres.RemoveAt(idx);

        Vector2 posicionMundo = new Vector2(pos.x * anchoDeseado, -pos.y * altoDeseado);
        GameObject instancia = Instantiate(agentePrefab, posicionMundo, Quaternion.identity);

        // Renombrar para depuración
        instancia.name = $"Agente_{contador++}";

        // Forzar asignación del script si no lo tiene
        AgenteExplorador explorador = instancia.GetComponent<AgenteExplorador>();
        if (explorador == null)
        {
            explorador = instancia.AddComponent<AgenteExplorador>();
        }

        // Asignar el generador para que no quede nulo
        explorador.generadorMapa = this;
        explorador.anchoCelda = anchoDeseado;
        explorador.altoCelda = altoDeseado;

        // Escalado (si tiene sprite)
        SpriteRenderer sr = instancia.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Vector2 size = sr.bounds.size;
            if (size.x > 0 && size.y > 0)
            {
                float escalaX = anchoDeseado / size.x;
                float escalaY = altoDeseado / size.y;
                instancia.transform.localScale = new Vector3(escalaX, escalaY, 1f);
            }
        }

        Debug.Log($"[Instanciación] Agente instanciado: {instancia.name}");
    }
}




    public int[,] GetMapa()
    {
        return mapa;
    }
}
