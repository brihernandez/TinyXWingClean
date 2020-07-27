using UnityEngine;

using System.Collections.Generic;
using System.IO;

public static class ShipLibrary
{
    private static Dictionary<string, ShipSpecs> loadedSpecs = new Dictionary<string, ShipSpecs>();

    static ShipLibrary()
    {
        var allShipFiles = Directory.GetFiles(GameFiles.ShipsFolder);
        foreach (var shipFilePath in allShipFiles)
        {
            if (File.Exists(shipFilePath))
            {
                var extension = Path.GetExtension(shipFilePath);
                if (extension != ".json")
                    continue;

                var shipName = Path.GetFileNameWithoutExtension(shipFilePath);
                Debug.Log($"Loading {shipName} from {shipFilePath}");

                var fileContents = File.ReadAllText(shipFilePath);
                var specs = Newtonsoft.Json.JsonConvert.DeserializeObject<ShipSpecs>(fileContents);
                loadedSpecs.Add(shipName, specs);
            }
            else
            {
                Debug.LogError($"Could not find ship {shipFilePath} in path {shipFilePath}");
            }
        }
    }

    public static bool HasSpecForShip(string shipName) => loadedSpecs.ContainsKey(shipName);
    public static ShipSpecs GetSpecForShip(string shipName) => loadedSpecs[shipName];
}

public class ShipSpawner : MonoBehaviour
{
    Dictionary<string, Ship> cachedShipPrefabs = new Dictionary<string, Ship>();

    public bool SpawnShipOnStart = false;
    public string ShipName = "TIE Fighter";
    public int Count = 4;
    public KeyCode SpawnKey = KeyCode.H;

    private void Start()
    {
        if (SpawnShipOnStart)
            SpawnSquadron();
    }

    private void Update()
    {
        if (Input.GetKeyDown(SpawnKey))
            SpawnSquadron();
    }

    //[Sirenix.OdinInspector.Button]
    public void SpawnSquadron()
    {
        SpawnSquadron(ShipName, Count);
    }

    public Ship SpawnShip(string shipName)
    {
        Ship spawnedShip = null;

        try
        {
            Ship shipPrefab = null;
            if (cachedShipPrefabs.ContainsKey(shipName))
                shipPrefab = cachedShipPrefabs[shipName];
            else
            {
                shipPrefab = Resources.Load<Ship>($"Ships/{shipName}");
                cachedShipPrefabs.Add(shipName, shipPrefab);
            }

            spawnedShip = Instantiate(shipPrefab, transform.position, transform.rotation);
            if (ShipLibrary.HasSpecForShip(shipPrefab.name))
                spawnedShip.LoadSpecs(ShipLibrary.GetSpecForShip(shipPrefab.name));
            else
                Debug.LogError($"Did not find specs for {shipPrefab.name}");
        }
        catch
        {
            Debug.LogError($"Could not find ship prefab \"{shipName}\"");
        }

        return spawnedShip;
    }

    public void SpawnSquadron(string shipName, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            var ship = SpawnShip(shipName);

            var formationPoint = GetFormationPoint(i);
            ship.transform.position = formationPoint;
            ship.transform.rotation = transform.rotation;
        }
    }

    private Vector3 GetFormationPoint(int index)
    {
        var formationPoint = Vector3.zero;
        formationPoint.x = 30f * ((index + 1) / 2) * (index % 2 == 0 ? -1f : 1f);
        formationPoint.z = -30f * ((index + 1) / 2);

        return transform.TransformPoint(formationPoint);
    }

#if UNITY_EDITOR
    //private void OnDrawGizmos()
    //{
    //    Shapes.Draw.SphereRadius = 10f;
    //    Shapes.Draw.SphereRadiusSpace = Shapes.ThicknessSpace.Pixels;
    //    Shapes.Draw.LineThickness = 4f;
    //    Shapes.Draw.LineThicknessSpace = Shapes.ThicknessSpace.Pixels;

    //    for (int i = 0; i < Count; ++i)
    //    {
    //        var formationPoint = GetFormationPoint(i);
    //        UnityEditor.Handles.Label(formationPoint, i.ToString());

    //        var distanceToCamera = Vector3.Distance(
    //        UnityEditor.SceneView.lastActiveSceneView.camera.transform.position,
    //        formationPoint);
    //        distanceToCamera /= 20f;

    //        Shapes.Draw.Color = Color.blue;
    //        Shapes.Draw.Line(formationPoint, formationPoint + transform.forward * distanceToCamera);
    //        Shapes.Draw.Color = Color.red;
    //        Shapes.Draw.Line(formationPoint, formationPoint + transform.right * distanceToCamera);
    //        Shapes.Draw.Color = Color.green;
    //        Shapes.Draw.Line(formationPoint, formationPoint + transform.up * distanceToCamera);

    //        Shapes.Draw.Color = Color.white;
    //        Shapes.Draw.Sphere(formationPoint);
    //    }
    //}
#endif

}
