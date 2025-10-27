using UnityEngine;

public class Barrel : MonoBehaviour
{
    public ParticleSystem Explosion;
    public GameObject Bar;
    public void OnTriggerEnter(Collider other)
    {
        Explosion.Play();
        Destroy(Bar);
    }
}
