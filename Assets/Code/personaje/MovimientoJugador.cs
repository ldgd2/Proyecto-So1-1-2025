using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float velocidad = 3f;
    public LayerMask capaObstaculos;
    public float radioChequeo = 0.2f;
    public Transform spriteTransform;

    private Rigidbody2D rb;
    private Vector2 input;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Rotación en 4 direcciones
        if (input != Vector2.zero && spriteTransform != null)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                // Movimiento horizontal
                if (input.x > 0)
                    spriteTransform.rotation = Quaternion.Euler(0, 0, 0);       // Derecha
                else
                    spriteTransform.rotation = Quaternion.Euler(0, 0, 180);     // Izquierda
            }
            else
            {
                // Movimiento vertical
                if (input.y > 0)
                    spriteTransform.rotation = Quaternion.Euler(0, 0, 90);      // Arriba
                else
                    spriteTransform.rotation = Quaternion.Euler(0, 0, -90);     // Abajo
            }
        }
    }

    void FixedUpdate()
    {
        if (input != Vector2.zero)
        {
            Vector2 destino = rb.position + input * velocidad * Time.fixedDeltaTime;

            Collider2D colision = Physics2D.OverlapCircle(destino, radioChequeo, capaObstaculos);
            if (colision == null)
            {
                rb.MovePosition(destino);
            }
            else
            {
                Debug.Log("Colisión detectada con: " + colision.name);
            }
        }
    }
}
