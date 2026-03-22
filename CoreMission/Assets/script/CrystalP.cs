using UnityEngine;

public class CrystalP : MonoBehaviour
{
    public GameObject pickupVFX;
    public GameObject crystalVisual;
    public GameObject idleVFX;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            
            collected = true;

            if (pickupVFX != null)
            {
                GameObject vfx = Instantiate(pickupVFX, transform.position + Vector3.up * 1.0f, Quaternion.identity);

                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                }
            }

            if (crystalVisual != null)
            {
                crystalVisual.SetActive(false);
            }

            if (idleVFX != null)
            {
                idleVFX.SetActive(false);
            }

            CrystalManager.Instance.AddCrystal();

            Destroy(gameObject, 0.15f);
        }
    }
}