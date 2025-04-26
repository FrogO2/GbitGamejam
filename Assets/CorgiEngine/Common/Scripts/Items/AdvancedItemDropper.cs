using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ItemLauncher : MonoBehaviour
{
    public enum TriggerMode { AutoStart, KeyPress, CollisionEnter }

    [Header("Base Settings")]
    public GameObject ItemPrefab;
    [Min(0)] public float InitialDelay = 1f;  // 默认改为1秒方便测试

    [Header("Launch Settings")]
    public Vector3 LaunchForce = new Vector3(0f, 15f, 0f);
    public Vector3 ForceVariation = new Vector3(0f, 5f, 0f);

    [Header("Trigger Mode")]
    public TriggerMode triggerMode = TriggerMode.AutoStart;
    [SerializeField] private KeyCode triggerKey = KeyCode.Space;

    [Header("Effects")]
    public AudioClip launchSound;
    public ParticleSystem launchParticles;

    private AudioSource audioSource;
    private bool isActivated;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Debug.Log($"Launcher initialized in {triggerMode} mode");
        
        if (triggerMode == TriggerMode.AutoStart)
        {
            StartCoroutine(LaunchRoutine());
        }
    }

    private void Update()
    {
        if (triggerMode == TriggerMode.KeyPress && Input.GetKeyDown(triggerKey))
        {
            StartCoroutine(LaunchRoutine());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (triggerMode == TriggerMode.CollisionEnter && !isActivated)
        {
            StartCoroutine(LaunchRoutine());
        }
    }

    private IEnumerator LaunchRoutine()
    {
        if (isActivated) yield break;
        isActivated = true;

        Debug.Log($"Launching in {InitialDelay}s");
        yield return new WaitForSeconds(InitialDelay);

        if (ItemPrefab == null)
        {
            Debug.LogError("Missing item prefab!");
            yield break;
        }

        Vector3 spawnPos = transform.position + Vector3.up;
        GameObject newItem = Instantiate(ItemPrefab, spawnPos, Quaternion.identity);
        
        if (newItem.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector3 force = new Vector3(
                LaunchForce.x + Random.Range(-ForceVariation.x, ForceVariation.x),
                LaunchForce.y + Random.Range(-ForceVariation.y, ForceVariation.y),
                LaunchForce.z + Random.Range(-ForceVariation.z, ForceVariation.z)
            );
            rb.AddForce(force, ForceMode2D.Impulse);
            Debug.Log($"Applied force: {force}");
        }
        else
        {
            Debug.LogError("Item missing Rigidbody component");
        }

        PlayEffects(spawnPos);
        isActivated = false;
    }

    private void PlayEffects(Vector3 position)
    {
        if (launchSound) AudioSource.PlayClipAtPoint(launchSound, position);
        if (launchParticles) 
        {
            var particles = Instantiate(launchParticles, position, Quaternion.identity);
            Destroy(particles.gameObject, particles.main.duration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + LaunchForce.normalized * 2f);
    }
}