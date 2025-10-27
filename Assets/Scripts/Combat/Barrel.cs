using UnityEngine;

public class Barrel : MonoBehaviour
{
    public ParticleSystem Explosion;
    public GameObject Bar;

    public void DestroyBarrel()
    {
        Explosion.Play();
        Destroy(Bar);
    }
}
