using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMovement : MonoBehaviour
{
    public bool enableWaterSystem = false;
    public float movementSpeed = 5f;
    public float mudSpeedMultiplier = 0.5f;
    public float iceAcceleration = 0.1f;
    public float maxIceSpeed = 10f;
    public float iceFriction = 0.98f;
    public float verticalFriction = 0.9f;
    public MovementKey moveUpKey = MovementKey.W;
    public MovementKey moveDownKey = MovementKey.S;
    public MovementKey moveLeftKey = MovementKey.A;
    public MovementKey moveRightKey = MovementKey.D;
    public Transform cameraTransform;
    public bool enableDeathClones = false;
    public Transform respawnPoint;
    public float rotationSpeed = 10f;
    public TMP_Text deathText;
    public Button respawnButton;
    public Transform turnPivot;
    public bool disableRRespawn;
    public float waterHeight = 5f;
    public float waterDensity = 1027f;
    public float waterDrag = 8f;
    public float swimForce = 9f;
    public Color waterColor = new Color(0.08f, 0.38f, 0.49f, 0.92f);
    public float surfaceSnapForce = 12f;
    public float buoyancyOffset = 0.4f;
    public Texture2D rippleNoise;
    public int textureSize = 1024;
    public float waveFrequency = 15f;
    public float waveFalloff = 5f;
    public string savePath = "Assets/GeneratedRippleTexture.png";
    public float rippleSpeed = 3f;
    public float rippleScale = 0.7f;
    public float rippleAmplitude = 0.4f;
    public float rippleDecay = 0.85f;
    public float surfaceWaveHeight = 0.3f;
    public float respawnJumpHeight = 5f;
    public float respawnDuration = 1f;
    public LevelTimer levelTimer;
    private Rigidbody rb;
    private Collider col;
    private GameObject waterPlane;
    private Material waterMaterial;
    private ParticleSystem splashParticles;
    private PostProcessVolume postProcess;
    private Vector3 moveDirection;
    private bool isOnMud;
    private bool isOnIce;
    private Vector3 velocity;
    private int groundContactCount;
    private int deathCount;
    private float targetYRotation;
    private bool manualRotation;
    private bool isRespawning;
    private bool isInWater;
    private float originalDrag;
    private float originalMass;
    private Vector4[] rippleCenters = new Vector4[10];
    private float[] rippleStrengths = new float[10];
    private int currentRipple;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.freezeRotation = true;
        targetYRotation = transform.eulerAngles.y;
        originalDrag = rb.linearDamping;
        originalMass = rb.mass;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (deathText) deathText.text = ": " + deathCount;
        if (respawnButton) respawnButton.onClick.AddListener(TriggerRespawn);
        if (enableWaterSystem) InitializeWaterSystem();
    }
    void InitializeWaterSystem()
    {
#if UNITY_EDITOR
        if (rippleNoise == null)
        {
            GenerateRippleTexture();
            rippleNoise = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
        }
#endif
        waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterPlane.transform.position = new Vector3(0, waterHeight, 0);
        waterPlane.transform.localScale = new Vector3(200, 1, 200);
        Destroy(waterPlane.GetComponent<Collider>());
        BoxCollider bc = waterPlane.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(200, 2, 200);
        bc.center = new Vector3(0, 1, 0);
        waterMaterial = new Material(Shader.Find("Custom/WaterRipple"));
        waterMaterial.SetColor("_Color", waterColor);
        waterMaterial.SetTexture("_RippleNoise", rippleNoise);
        waterMaterial.SetFloat("_RippleSpeed", rippleSpeed);
        waterMaterial.SetFloat("_RippleScale", rippleScale);
        waterMaterial.SetFloat("_RippleAmplitude", rippleAmplitude);
        waterMaterial.SetFloat("_SurfaceWaveHeight", surfaceWaveHeight);
        waterPlane.GetComponent<Renderer>().material = waterMaterial;
        GameObject splash = new GameObject("SplashEffects");
        splash.transform.SetParent(transform);
        splashParticles = splash.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = splashParticles.main;
        main.startSpeed = 2f;
        main.startLifetime = 1f;
        main.startSize = 0.2f;
        main.maxParticles = 30;
        ParticleSystem.EmissionModule em = splashParticles.emission;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
        ParticleSystemRenderer pr = splash.GetComponent<ParticleSystemRenderer>();
        pr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        postProcess = gameObject.AddComponent<PostProcessVolume>();
        postProcess.isGlobal = true;
        PostProcessProfile profile = ScriptableObject.CreateInstance<PostProcessProfile>();
        DepthOfField dof = profile.AddSettings<DepthOfField>();
        dof.focusDistance.overrideState = true;
        ColorGrading cg = profile.AddSettings<ColorGrading>();
        cg.colorFilter.overrideState = true;
        postProcess.profile = profile;
        rb.mass = Mathf.Min(originalMass, 1.6f);
    }
#if UNITY_EDITOR
    [ContextMenu("Generate Ripple Texture")]
    public void GenerateRippleTexture()
    {
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[tex.width * tex.height];
        float center = textureSize * 0.5f;
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float wave = Mathf.Sin(distance * waveFrequency) * Mathf.Exp(-distance / textureSize * waveFalloff);
                wave += Mathf.Sin(distance * waveFrequency * 0.7f) * Mathf.Exp(-distance / textureSize * waveFalloff * 0.8f) * 0.5f;
                wave = wave * 0.5f + 0.5f;
                pixels[y * tex.width + x] = new Color(wave, wave, wave, 1);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(savePath, tex.EncodeToPNG());
        AssetDatabase.Refresh();
    }
#endif
    void Update()
    {
        float moveX = 0f, moveZ = 0f;
        if (Input.GetKey(GetKeyCode(moveUpKey))) moveZ = 1f;
        if (Input.GetKey(GetKeyCode(moveDownKey))) moveZ = -1f;
        if (Input.GetKey(GetKeyCode(moveLeftKey))) moveX = -1f;
        if (Input.GetKey(GetKeyCode(moveRightKey))) moveX = 1f;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        if (moveDirection != Vector3.zero && !manualRotation)
            targetYRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        if (Input.GetKeyDown(KeyCode.R) && !disableRRespawn) TriggerRespawn();
        if (enableWaterSystem)
        {
            if (Input.GetKey(KeyCode.F) && isInWater) rb.AddForce(Vector3.down * swimForce, ForceMode.Acceleration);
            if (Input.GetKey(KeyCode.R) && isInWater) rb.AddForce(Vector3.up * swimForce, ForceMode.Acceleration);
            UpdateUnderwaterEffects();
            UpdateWaterShader();
        }
        if (!isInWater && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -Mathf.Infinity, 0), rb.linearVelocity.z);
    }
    void UpdateUnderwaterEffects()
    {
        if (postProcess == null) return;
        float underwaterAmount = Mathf.Clamp01((waterHeight - transform.position.y + buoyancyOffset) / 3f);
        postProcess.weight = underwaterAmount;
        DepthOfField dof = postProcess.profile.GetSetting<DepthOfField>();
        dof.focusDistance.value = Mathf.Lerp(15f, 0.3f, underwaterAmount);
        ColorGrading cg = postProcess.profile.GetSetting<ColorGrading>();
        cg.colorFilter.value = Color.Lerp(Color.white, new Color(0.7f, 0.8f, 1f, 1f), underwaterAmount);
    }
    void UpdateWaterShader()
    {
        waterMaterial.SetVectorArray("_RippleCenters", rippleCenters);
        waterMaterial.SetFloatArray("_RippleStrengths", rippleStrengths);
        waterMaterial.SetInt("_RippleCount", currentRipple + 1);
    }
    void CreateRipple(Vector3 position)
    {
        currentRipple = (currentRipple + 1) % 10;
        rippleCenters[currentRipple] = new Vector4(position.x, position.z, 0, 0);
        rippleStrengths[currentRipple] = 1f;
    }
    void FixedUpdate()
    {
        if (isRespawning) return;
        bool grounded = groundContactCount > 0;
        if (isOnMud) velocity = moveDirection * (movementSpeed * mudSpeedMultiplier);
        else if (isOnIce)
        {
            if (moveDirection != Vector3.zero)
            {
                velocity += moveDirection * iceAcceleration;
                if (velocity.magnitude > maxIceSpeed) velocity = velocity.normalized * maxIceSpeed;
            }
            else velocity *= iceFriction;
        }
        else velocity = moveDirection * movementSpeed;
        if (enableWaterSystem && isInWater)
        {
            float submergedHeight = waterHeight - (transform.position.y - buoyancyOffset);
            float submergedVolume = Mathf.Clamp01(submergedHeight / col.bounds.size.y);
            float volume = col.bounds.size.x * col.bounds.size.y * col.bounds.size.z;
            float buoyancyStrength = Mathf.Max(0, waterDensity - (rb.mass / volume)) * Physics.gravity.magnitude;
            rb.AddForce(Vector3.up * buoyancyStrength * submergedVolume * 0.7f, ForceMode.Force);
            if (submergedHeight < 0.4f)
            {
                rb.AddForce(Vector3.down * surfaceSnapForce * 1.5f, ForceMode.Force);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -Mathf.Infinity, 0.1f), rb.linearVelocity.z);
            }
            rb.linearDamping = Mathf.Lerp(waterDrag * 0.3f, waterDrag, submergedVolume);
            rb.angularDamping = waterDrag * 3;
            if (rb.linearVelocity.magnitude > 0.3f) CreateRipple(transform.position + Vector3.down * 0.7f);
        }
        else rb.linearDamping = originalDrag;
        for (int i = 0; i < 10; i++) rippleStrengths[i] *= rippleDecay;
        velocity.y = grounded ? 0 : rb.linearVelocity.y * verticalFriction;
        rb.linearVelocity = velocity;
        float currentAngle = transform.eulerAngles.y;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetYRotation, rotationSpeed * Time.fixedDeltaTime);
        Quaternion newRot = Quaternion.Euler(0, newAngle, 0);
        if (turnPivot != null)
        {
            float angleDelta = newAngle - currentAngle;
            Vector3 pivotOffset = transform.position - turnPivot.position;
            Vector3 rotatedOffset = Quaternion.Euler(0, angleDelta, 0) * pivotOffset;
            rb.MoveRotation(newRot);
            rb.MovePosition(turnPivot.position + rotatedOffset);
        }
        else rb.MoveRotation(newRot);
        if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetYRotation)) < 0.1f) manualRotation = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (enableWaterSystem && other.gameObject == waterPlane)
        {
            isInWater = true;
            rb.useGravity = false;
            splashParticles.Play();
        }
        if (other.CompareTag("bablockno") || other.CompareTag("death")) TriggerRespawn();
    }
    void OnTriggerExit(Collider other)
    {
        if (enableWaterSystem && other.gameObject == waterPlane)
        {
            isInWater = false;
            rb.useGravity = true;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("bablockyes")) collision.gameObject.GetComponent<Renderer>().material.color = Color.green;
        else if (collision.gameObject.CompareTag("bablockno"))
        {
            collision.gameObject.GetComponent<Renderer>().material.color = Color.red;
            TriggerRespawn();
            return;
        }
        if (collision.collider.CompareTag("ground") || collision.collider.CompareTag("Mud") || collision.collider.CompareTag("Ice")) groundContactCount++;
        if (collision.collider.CompareTag("Mud")) isOnMud = true;
        if (collision.collider.CompareTag("Ice")) isOnIce = true;
        if (collision.collider.CompareTag("Checkpoint"))
        {
            Checkpoint cp = collision.collider.GetComponent<Checkpoint>();
            respawnPoint = (cp != null && cp.useCustomPoint && cp.customRespawnPoint != null) ? cp.customRespawnPoint : collision.transform;
        }
        if (collision.collider.CompareTag("Finish"))
        {
            if (levelTimer != null) levelTimer.FinishLevel();
            LoadNextScene();
        }
        if (collision.collider.CompareTag("death")) HandleDeath(collision);
    }
    public void TriggerRespawn()
    {
        if (isRespawning) return;
        TeleportToRespawn();
    }
    public void TeleportToRespawn()
    {
        if (respawnPoint != null)
        {
            StartCoroutine(RespawnAnimation());
        }
    }
    IEnumerator RespawnAnimation()
    {
        isRespawning = true;
        rb.isKinematic = true;
        Vector3 start = transform.position;
        Vector3 end = respawnPoint.position;
        Vector3 randomAxis = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        float elapsed = 0f;
        while (elapsed < respawnDuration)
        {
            float t = elapsed / respawnDuration;
            float height = 4 * respawnJumpHeight * t * (1 - t);
            transform.position = Vector3.Lerp(start, end, t) + Vector3.up * height;
            transform.Rotate(randomAxis, rotationSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
        rb.isKinematic = false;
        deathCount++;
        if (deathText) deathText.text = ": " + deathCount;
        isRespawning = false;
    }
    void HandleDeath(Collision collision)
    {
        TeleportToRespawn();
        if (collision != null && collision.collider.CompareTag("death") && enableDeathClones)
        {
            Vector3 pos = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            GameObject clone = Instantiate(gameObject, pos, Quaternion.identity);
            Destroy(clone.GetComponent<PlayerMovement>());
            foreach (Collider c in clone.GetComponents<Collider>()) Destroy(c);
            BoxCollider cloneCollider = clone.AddComponent<BoxCollider>();
            Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
            cloneRb.mass = rb.mass;
            Physics.IgnoreCollision(cloneCollider, GetComponent<Collider>());
            foreach (GameObject deathObj in GameObject.FindGameObjectsWithTag("death"))
            {
                Collider deathCollider = deathObj.GetComponent<Collider>();
                if (deathCollider != null) Physics.IgnoreCollision(cloneCollider, deathCollider);
            }
        }
    }
    void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(nextSceneIndex);
    }
    KeyCode GetKeyCode(MovementKey key)
    {
        switch (key)
        {
            case MovementKey.W: return KeyCode.W;
            case MovementKey.A: return KeyCode.A;
            case MovementKey.S: return KeyCode.S;
            case MovementKey.D: return KeyCode.D;
            default: return KeyCode.None;
        }
    }
}
public class Checkpoint : MonoBehaviour
{
    public bool useCustomPoint;
    public Transform customRespawnPoint;
}
public enum MovementKey { W, A, S, D }
