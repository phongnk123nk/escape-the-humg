using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quan ly mini-game giao hang trong phong tin hoc.
/// Package va Location duoc spawn ngau nhien tren cac object Road.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Delivery Order Mini Game Manager")]
public class DeliveryOrderMiniGameManager : MonoBehaviour
{
    [Header("References")]
    public Transform car;
    public Transform contentRoot;
    public List<GameObject> packageObjects = new List<GameObject>();
    public List<GameObject> locationObjects = new List<GameObject>();
    public List<Transform> roadPoints = new List<Transform>();
    public TextMesh messageText;
    public ComputerRoomMiniGameIcon foodIcon;
    public SpriteRenderer blackoutRenderer;
    public SpriteRenderer jumpscareRenderer;

    [Header("Settings")]
    public int ordersToWin = 3;
    public float minimumPackageLocationDistance = 1.2f;
    public float minimumDistanceFromPreviousSpawn = 1.6f;
    public bool autoRefreshRoadPoints = true;
    public string resultNumber = "2";
    public float blackScreenDelay = 0.45f;
    public float jumpscareTime = 0.9f;
    public bool hideMiniGameAfterComplete = true;

    [Header("State")]
    public int deliveredOrders;
    public bool hasOrder;
    public bool completed;

    private GameObject activePackage;
    private GameObject activeLocation;
    private Coroutine completionRoutine;
    private readonly List<Transform> usedPackageRoads = new List<Transform>();
    private readonly List<Transform> usedLocationRoads = new List<Transform>();
    private readonly List<Vector3> previousSpawnPositions = new List<Vector3>();

    private void Awake()
    {
        AutoCollectReferences();
        PrepareObjects();
        HideCompletionOverlay();
    }

    private void OnEnable()
    {
        if (!completed)
        {
            ResetGame();
        }
    }

    public void ResetGame()
    {
        deliveredOrders = 0;
        hasOrder = false;
        completed = false;
        completionRoutine = null;
        HideCompletionOverlay();
        usedPackageRoads.Clear();
        usedLocationRoads.Clear();
        previousSpawnPositions.Clear();
        HideAllPackagesAndLocations();
        SpawnNextOrder();
    }

    public void HandleCarTrigger(Collider2D other)
    {
        if (completed || other == null)
        {
            return;
        }

        if (activePackage != null && other.gameObject == activePackage && !hasOrder)
        {
            PickPackage();
            return;
        }

        if (activeLocation != null && other.gameObject == activeLocation && hasOrder)
        {
            DeliverPackage();
        }
    }

    private void PickPackage()
    {
        hasOrder = true;
        activePackage.SetActive(false);
        activeLocation.SetActive(true);
        ShowMessage("Bạn có 1 đơn hàng");
    }

    private void DeliverPackage()
    {
        deliveredOrders++;
        hasOrder = false;
        activeLocation.SetActive(false);

        if (deliveredOrders >= ordersToWin)
        {
            completed = true;
            HideAllPackagesAndLocations();
            ShowMessage("Đã giao đủ 3 đơn hàng!");

            if (completionRoutine == null)
            {
                completionRoutine = StartCoroutine(PlayCompletionSequence());
            }

            return;
        }

        ShowMessage("Đã giao " + deliveredOrders + "/" + ordersToWin + " đơn hàng");
        SpawnNextOrder();
    }

    private IEnumerator PlayCompletionSequence()
    {
        AutoCollectReferences();

        if (blackoutRenderer != null)
        {
            blackoutRenderer.gameObject.SetActive(true);
        }

        if (jumpscareRenderer != null)
        {
            jumpscareRenderer.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(blackScreenDelay);

        if (jumpscareRenderer != null)
        {
            jumpscareRenderer.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(jumpscareTime);

        HideCompletionOverlay();

        if (foodIcon != null)
        {
            foodIcon.ShowResultNumber(resultNumber);
        }

        if (hideMiniGameAfterComplete)
        {
            gameObject.SetActive(false);
        }

        completionRoutine = null;
    }

    private void HideCompletionOverlay()
    {
        if (blackoutRenderer != null)
        {
            blackoutRenderer.gameObject.SetActive(false);
        }

        if (jumpscareRenderer != null)
        {
            jumpscareRenderer.gameObject.SetActive(false);
        }
    }

    private void SpawnNextOrder()
    {
        AutoCollectReferences();

        if (packageObjects.Count == 0 || locationObjects.Count == 0 || roadPoints.Count == 0)
        {
            ShowMessage("Thiếu Package, Location hoặc Road");
            return;
        }

        HideAllPackagesAndLocations();

        activePackage = packageObjects[deliveredOrders % packageObjects.Count];
        activeLocation = locationObjects[deliveredOrders % locationObjects.Count];

        Transform packageRoad = PickRoad(usedPackageRoads, null);
        Transform locationRoad = PickRoad(usedLocationRoads, packageRoad);

        activePackage.transform.position = packageRoad.position;
        activeLocation.transform.position = locationRoad.position;
        previousSpawnPositions.Add(packageRoad.position);
        previousSpawnPositions.Add(locationRoad.position);
        activePackage.SetActive(true);
        activeLocation.SetActive(false);

        ShowMessage("Tìm đơn hàng " + (deliveredOrders + 1) + "/" + ordersToWin);
    }

    private Transform PickRoad(List<Transform> usedRoads, Transform avoidRoad)
    {
        List<Transform> candidates = new List<Transform>();
        for (int i = 0; i < roadPoints.Count; i++)
        {
            Transform road = roadPoints[i];
            if (road == null || usedRoads.Contains(road))
            {
                continue;
            }

            if (avoidRoad != null && Vector2.Distance(road.position, avoidRoad.position) < minimumPackageLocationDistance)
            {
                continue;
            }

            if (IsTooCloseToPreviousSpawn(road.position))
            {
                continue;
            }

            candidates.Add(road);
        }

        if (candidates.Count == 0)
        {
            Transform farthest = PickFarthestRoad(avoidRoad);
            usedRoads.Add(farthest);
            return farthest;
        }

        Transform picked = candidates[Random.Range(0, candidates.Count)];
        usedRoads.Add(picked);
        return picked;
    }

    private bool IsTooCloseToPreviousSpawn(Vector3 position)
    {
        for (int i = 0; i < previousSpawnPositions.Count; i++)
        {
            if (Vector2.Distance(position, previousSpawnPositions[i]) < minimumDistanceFromPreviousSpawn)
            {
                return true;
            }
        }

        return false;
    }

    private Transform PickFarthestRoad(Transform avoidRoad)
    {
        Transform bestRoad = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < roadPoints.Count; i++)
        {
            Transform road = roadPoints[i];
            if (road == null || road == avoidRoad)
            {
                continue;
            }

            float nearestDistance = float.MaxValue;
            for (int j = 0; j < previousSpawnPositions.Count; j++)
            {
                nearestDistance = Mathf.Min(nearestDistance, Vector2.Distance(road.position, previousSpawnPositions[j]));
            }

            if (previousSpawnPositions.Count == 0)
            {
                nearestDistance = Random.value;
            }

            if (avoidRoad != null)
            {
                nearestDistance += Vector2.Distance(road.position, avoidRoad.position);
            }

            if (nearestDistance > bestScore)
            {
                bestScore = nearestDistance;
                bestRoad = road;
            }
        }

        if (bestRoad != null)
        {
            return bestRoad;
        }

        return roadPoints[Random.Range(0, roadPoints.Count)];
    }

    private void PrepareObjects()
    {
        for (int i = 0; i < packageObjects.Count; i++)
        {
            if (packageObjects[i] == null) continue;
            packageObjects[i].tag = "Package";
            EnsureTrigger(packageObjects[i]);
        }

        for (int i = 0; i < locationObjects.Count; i++)
        {
            if (locationObjects[i] == null) continue;
            locationObjects[i].tag = "Location";
            EnsureTrigger(locationObjects[i]);
        }

        if (car != null)
        {
            DeliveryOrderCarTrigger trigger = car.GetComponent<DeliveryOrderCarTrigger>();
            if (trigger == null)
            {
                trigger = car.gameObject.AddComponent<DeliveryOrderCarTrigger>();
            }

            trigger.manager = this;
        }
    }

    private void EnsureTrigger(GameObject target)
    {
        Collider2D collider = target.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = target.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
    }

    private void HideAllPackagesAndLocations()
    {
        for (int i = 0; i < packageObjects.Count; i++)
        {
            if (packageObjects[i] != null)
            {
                packageObjects[i].SetActive(false);
            }
        }

        for (int i = 0; i < locationObjects.Count; i++)
        {
            if (locationObjects[i] != null)
            {
                locationObjects[i].SetActive(false);
            }
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageText.gameObject.SetActive(true);
    }

    private void AutoCollectReferences()
    {
        if (contentRoot == null)
        {
            Transform importedContent = transform.Find("ImportedDeliverySceneContent");
            contentRoot = importedContent != null ? importedContent : transform;
        }

        if (car == null)
        {
            car = FindDeepChild(contentRoot, "Car");
        }

        if (blackoutRenderer == null)
        {
            Transform blackout = transform.Find("DeliveryEndBlackout");
            blackoutRenderer = blackout != null ? blackout.GetComponent<SpriteRenderer>() : null;
        }

        if (jumpscareRenderer == null)
        {
            Transform jumpscare = transform.Find("DeliveryEndJumpscare");
            jumpscareRenderer = jumpscare != null ? jumpscare.GetComponent<SpriteRenderer>() : null;
        }

        if (packageObjects.Count == 0)
        {
            CollectByNameOrTag(contentRoot, "Package", "Package", packageObjects);
        }

        if (locationObjects.Count == 0)
        {
            CollectByNameOrTag(contentRoot, "Location", "Location", locationObjects);
        }

        if (autoRefreshRoadPoints || roadPoints.Count == 0)
        {
            roadPoints.Clear();
            CollectRoadPoints(contentRoot);
        }
    }

    private void CollectByNameOrTag(Transform root, string namePrefix, string tagName, List<GameObject> results)
    {
        if (root == null)
        {
            return;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child == root)
            {
                continue;
            }

            if (child.name.StartsWith(namePrefix) || child.CompareTag(tagName))
            {
                results.Add(child.gameObject);
            }
        }
    }

    private void CollectRoadPoints(Transform root)
    {
        if (root == null)
        {
            return;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (IsRoadSpawnPoint(child.name))
            {
                roadPoints.Add(child);
            }
        }
    }

    private bool IsRoadSpawnPoint(string objectName)
    {
        return objectName.StartsWith("Road")
            || objectName.StartsWith("Corner")
            || objectName.StartsWith("Curve")
            || objectName.StartsWith("Intersection")
            || objectName.StartsWith("Bridge");
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }
}
