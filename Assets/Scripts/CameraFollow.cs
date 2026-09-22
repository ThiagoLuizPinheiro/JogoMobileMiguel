using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float altura = 10f;
    public float suavidade = 5f;

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 novaPosicao = new Vector3(
            player.position.x,
            player.position.y + altura,
            player.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            novaPosicao,
            suavidade * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}