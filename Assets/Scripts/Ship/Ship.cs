using UnityEngine;
using UnityEngine.Events;

using System.IO;
using System.Collections.Generic;

[System.Serializable]
public struct Engine
{
    public float Thrust;
    public float Accel;
    public float Torque;
}

[System.Serializable]
public enum LaserColor
{
    Green,
    Red
}

[System.Serializable]
public struct Weapons
{
    public int Damage;
    public int MuzzleVelocity;
    public float FireDelay;
    public LaserColor Color;
}

[System.Serializable]
public struct Physical
{
    public float Mass;
    public float Drag;
    public float AngularDrag;
}

[System.Serializable]
public struct ShipSpecs
{
    public string TypeName;
    public int HullStrength;
    public bool IsCapital;

    public Physical Physics;
    public Engine Engine;
    public Weapons Weapons;
}

public class Ship : MonoBehaviour, IDamageable
{
    public ShipSpecs Specs;
    public Pilot Pilot;

    public GameObject[] SeparateOnDestroy = System.Array.Empty<GameObject>();
    public ParticleSystem DestroyFXPrefab = null;

    [Tooltip("Do this if the ship already exists in the world on startup.")]
    public bool LoadSpecOnStartup = false;

    [SerializeField] private Laser greenLaserPrefab = null;
    [SerializeField] private Laser redLaserPrefab = null;
    [SerializeField] private AudioSource turretLaserAudio = null;

    private ShipPhysics physics = null;

    private Queue<Cannon> laserBarrels = new Queue<Cannon>();
    private List<Turret> turrets = new List<Turret>();

    private float finalDestructionCountdown = 0f;
    private bool isDestroyed = false;
    private int hull = 100;

    public UnityEvent OnFireLaser;
    public UnityEvent OnDestroyed;

    private void Awake()
    {
        physics = new ShipPhysics(GetComponent<Rigidbody>(), ref Specs);
        Pilot.ownShip = this;
        Pilot.ownTarget = GetComponent<Target>();
        Pilot.Init();

        finalDestructionCountdown = Random.Range(2f, 3f);
    }

    private void Start()
    {
        if (LoadSpecOnStartup)
            LoadSpecs(ShipLibrary.GetSpecForShip(name));
    }

    private void Update()
    {
        if (isDestroyed)
            return;

        if (Input.GetKeyDown(KeyCode.K) && Pilot.IsPlayer)
            ApplyDamage(new DamageEventParams() { Damage = Specs.HullStrength, DamagedBy = null });

        if (Pilot.IsPlayer)
            Pilot.CapturePlayerInput();
        else
            Pilot.RunAI();

        if (turrets.Count > 0)
        {
            foreach (var turret in turrets)
                turret.Update();
        }
    }

    private void FixedUpdate()
    {
        if (isDestroyed && !Specs.IsCapital)
        {
            finalDestructionCountdown -= Time.fixedDeltaTime;
            if (finalDestructionCountdown <= 0)
            {
                Instantiate(DestroyFXPrefab, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
        else
        {
            physics.Update(Pilot, ref Specs);

            if (Pilot.IsFiring && laserBarrels.Count > 0)
            {
                var nextCannon = laserBarrels.Peek();
                if (nextCannon.AttemptFire())
                {
                    laserBarrels.Dequeue();
                    laserBarrels.Enqueue(nextCannon);
                    OnFireLaser.Invoke();
                }
            }

        }
    }

    //[Sirenix.OdinInspector.Button]
    public void DestroyShip()
    {
        ApplyDamage(new DamageEventParams { Damage = Specs.HullStrength * 2 });
    }

    public void ApplyDamage(DamageEventParams damageEvent)
    {
        if (isDestroyed)
            return;

        hull -= damageEvent.Damage;

        if (hull <= 0)
        {
            isDestroyed = true;

            // Unparent the camera so it doesn't get destroyed and make a lot of
            // assumptions about how it has the demo simple camera controller on it.
            var camera = GetComponentInChildren<SimpleCameraController>();
            if (camera != null)
            {
                camera.transform.SetParent(null);
                camera.transform.rotation = Quaternion.LookRotation(camera.transform.forward);
                camera.enabled = true;
            }

            // Put the ship in an uncontrollable spin. Capitals handle destruction
            // differently.
            if (!Specs.IsCapital)
                physics.NotifyShipDestroyed();

            // Pick a random part to separate and physicalize.
            if (SeparateOnDestroy.Length > 0)
            {
                var toSeparate = SeparateOnDestroy[Random.Range(0, SeparateOnDestroy.Length)];
                toSeparate.transform.SetParent(null);
                var debris = toSeparate.AddComponent<Debris>();
                debris.InitializePhysics(physics.Velocity);
                debris.TimeToLive = Random.Range(1f, 2f);
                debris.DestroyFxPrefab = DestroyFXPrefab;
            }

            // Play the destruction FX.
            Instantiate(DestroyFXPrefab, transform.position, transform.rotation);

            OnDestroyed.Invoke();
        }
    }

    //[Sirenix.OdinInspector.Button]
    public void SaveShipToFile()
    {
        var shipFilePath = $"{GameFiles.ShipsFolder}/{name}.json";
        var serializedSpecs = Newtonsoft.Json.JsonConvert.SerializeObject(Specs);
        File.WriteAllText(shipFilePath, serializedSpecs);
    }

    public void LoadSpecs(ShipSpecs specs)
    {
        Specs = specs;

        name = Specs.TypeName;
        hull = Specs.HullStrength;

        var laserPrefab = greenLaserPrefab;
        if (Specs.Weapons.Color == LaserColor.Red)
            laserPrefab = redLaserPrefab;

        var ownTarget = GetComponent<Target>();
        ownTarget.IsCapital = specs.IsCapital;

        laserBarrels.Clear();
        turrets.Clear();
        foreach (var child in GetComponentsInChildren<Transform>())
        {
            if (child.name.StartsWith("HpLaser"))
            {
                laserBarrels.Enqueue(new Cannon(this, child, laserPrefab, Specs.Weapons));
                continue;
            }

            if (child.name.StartsWith("Turret"))
            {
                var turret = new Turret(
                    this,
                    ownTarget,
                    turretLaserAudio,
                    child,
                    laserPrefab,
                    Pilot.FireChance,
                    Specs.Weapons);

                turrets.Add(turret);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position, $"{Pilot.Throttle * 100:0}%");
        if (Pilot.AttackTarget != null)
        {
            //Shapes.Draw.LineThicknessSpace = Shapes.ThicknessSpace.Pixels;
            //Shapes.Draw.Line(transform.position, Pilot.AttackTarget.Position, color: Color.red, thickness: 1f);
        }
    }
#endif
}
