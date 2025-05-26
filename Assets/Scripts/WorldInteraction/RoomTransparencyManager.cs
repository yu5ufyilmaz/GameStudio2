using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomTransparencyManager : MonoBehaviour
{
    public static RoomTransparencyManager Instance { get; private set; }
    public Transform player;
    public LayerMask buildingLayer;
    public string transparentLayerName = "TransparentWalls";

    [Range(0, 1)]
    public float transparencyAmount = 0.3f;

    public List<Room> rooms = new List<Room>();
    private Room currentRoom;
    
    // Chunk sistemi için yeni değişkenler
    private float lastUpdateTime = 0f;
    private float updateInterval = 0.5f; // 0.5 saniyede bir kontrol et
    private bool needsRoomRefresh = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        InitializeSystem();
    }

    private void InitializeSystem()
    {
        // Player referansını bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Streaming Manager ile entegrasyon
        if (StreamingManager.Instance != null)
        {
            // Streaming events'i dinle (eğer StreamingManager'da public eventler varsa)
            InvokeRepeating(nameof(CheckForNewRooms), 1f, 2f); // 2 saniyede bir yeni room'ları kontrol et
        }
        
        FindRoomsAutomatically();
    }

    private void CheckForNewRooms()
    {
        // Aktif olmayan room'ları temizle
        rooms.RemoveAll(room => room.roomCollider == null || !room.roomCollider.gameObject.activeInHierarchy);
        
        // Yeni room'ları ekle
        FindRoomsAutomatically();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SinsCity_P_T")
        {
            needsRoomRefresh = true;
            Invoke(nameof(RefreshAfterSceneLoad), 1f); // 1 saniye bekle
        }
    }

    private void RefreshAfterSceneLoad()
    {
        InitializeSystem();
        needsRoomRefresh = false;
    }

    void Update()
    {
        // Player null kontrolü
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            return;
        }

        // Performans için sürekli güncellemek yerine aralıklarla güncelle
        if (Time.time - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = Time.time;
            
            // Room listesi boşsa veya yenileme gerekiyorsa
            if (rooms.Count == 0 || needsRoomRefresh)
            {
                FindRoomsAutomatically();
                needsRoomRefresh = false;
            }
            
            CheckRoomTransition();
        }
    }

    private void CheckRoomTransition()
    {
        Room newRoom = GetRoomContainingPlayer();

        if (newRoom != currentRoom)
        {
            if (currentRoom != null)
            {
                SetRoomWallsOpaque(currentRoom);
            }

            if (newRoom != null)
            {
                SetRoomWallsTransparent(newRoom);
            }

            currentRoom = newRoom;
        }
    }

    private Room GetRoomContainingPlayer()
    {
        if (player == null) return null;
        
        foreach (Room room in rooms)
        {
            // Null kontrolü ekle
            if (room != null && room.roomCollider != null && 
                room.roomCollider.gameObject.activeInHierarchy)
            {
                if (room.IsPlayerInside(player.position))
                {
                    return room;
                }
            }
        }

        return null;
    }

    public void FindRoomsAutomatically()
    {
        // Sadece aktif room'ları bul
        GameObject[] roomObjects = GameObject.FindGameObjectsWithTag("Room");
        
        // Mevcut aktif room'ları kontrol et
        List<Room> existingActiveRooms = new List<Room>();
        foreach (Room room in rooms)
        {
            if (room != null && room.roomCollider != null && 
                room.roomCollider.gameObject.activeInHierarchy)
            {
                existingActiveRooms.Add(room);
            }
        }

        foreach (GameObject roomObj in roomObjects)
        {
            // Sadece aktif olan room objelerini işle
            if (!roomObj.activeInHierarchy) continue;
            
            // Bu room zaten listede var mı kontrol et
            bool alreadyExists = false;
            foreach (Room existingRoom in existingActiveRooms)
            {
                if (existingRoom.roomCollider != null && 
                    existingRoom.roomCollider.gameObject == roomObj)
                {
                    alreadyExists = true;
                    break;
                }
            }
            
            if (alreadyExists) continue;

            Collider roomCollider = roomObj.GetComponent<Collider>();
            if (roomCollider != null)
            {
                Room room = new Room();
                room.roomName = roomObj.name;
                room.roomCollider = roomCollider;
                FindWallsInChildren(roomObj.transform, room);
                
                // Sadece duvarları olan room'ları ekle
                if (room.allWalls.Count > 0)
                {
                    existingActiveRooms.Add(room);
                }
            }
        }
        
        rooms = existingActiveRooms;
    }

    private void FindWallsInChildren(Transform parent, Room room)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Wall"))
            {
                Renderer wallRenderer = child.GetComponent<Renderer>();
                if (wallRenderer != null && wallRenderer.gameObject.activeInHierarchy)
                {
                    room.allWalls.Add(wallRenderer);
                }
            }
            
            if (child.CompareTag("Room"))
            {
                FindWallsInChildren(child, room);
            }
            else
            {
                // "Room" tag'i olmasa bile çocukları kontrol et
                FindWallsInChildren(child, room);
            }
        }
    }

    private void SetRoomWallsOpaque(Room room)
    {
        if (room?.allWalls == null) return;
        
        foreach (Renderer wallRenderer in room.allWalls)
        {
            if (wallRenderer != null && wallRenderer.gameObject.activeInHierarchy)
            {
                SetWallOpacity(wallRenderer, 1.0f, "Obstacle");
            }
        }
    }

    private void SetRoomWallsTransparent(Room room)
    {
        if (room?.allWalls == null) return;
        
        foreach (Renderer wallRenderer in room.allWalls)
        {
            if (wallRenderer != null && wallRenderer.gameObject.activeInHierarchy)
            {
                SetWallOpacity(wallRenderer, transparencyAmount, "TransparentWalls");
            }
        }
    }

    private void SetWallOpacity(Renderer wallRenderer, float opacity, string targetLayerName)
    {
        if (wallRenderer == null) return;
        
        int layer = LayerMask.NameToLayer(targetLayerName);
        wallRenderer.gameObject.layer = layer;
        
        Material[] materials = wallRenderer.materials;
        
        foreach (Material mat in materials)
        {
            if (mat == null) continue;
            
            if (opacity < 1.0f && mat.renderQueue < 3000)
            {
                // Transparent moda geç
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            else if (opacity >= 1.0f && mat.renderQueue >= 3000)
            {
                // Opaque moda geç
                mat.SetFloat("_Mode", 0);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }

            Color color = mat.color;
            color.a = opacity;
            mat.color = color;
        }
    }

    // Manuel refresh metodu - test için
    [ContextMenu("Refresh Rooms")]
    public void ManualRefresh()
    {
        rooms.Clear();
        FindRoomsAutomatically();
        Debug.Log($"Found {rooms.Count} rooms");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        CancelInvoke();
    }
}

[System.Serializable]
public class Room
{
    public string roomName;
    public Collider roomCollider;
    public List<Renderer> wallRenderers = new List<Renderer>();
    public List<Renderer> allWalls = new List<Renderer>();

    public bool IsPlayerInside(Vector3 playerPosition)
    {
        if (roomCollider != null && roomCollider.gameObject.activeInHierarchy)
        {
            return roomCollider.bounds.Contains(playerPosition);
        }
        return false;
    }

    public void SetWallsVisible(bool isVisible)
    {
        foreach (Renderer wallRenderer in allWalls)
        {
            if (wallRenderer != null)
            {
                wallRenderer.enabled = isVisible;
            }
        }
    }
}

[System.Serializable]
public class Door
{
    public Room room1;
    public Room room2;
    public bool isOpen = false;
    public Renderer doorRenderer;
    public Animator doorAnimator;

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsOpen", isOpen);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RoomTransparencyManager))]
public class RoomBasedWallTransparencyEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomTransparencyManager script = (RoomTransparencyManager)target;

        if (GUILayout.Button("Find Colliders Automatically for Rooms"))
        {
            script.FindRoomsAutomatically();
        }
        
        if (GUILayout.Button("Manual Refresh"))
        {
            script.ManualRefresh();
        }
    }
}
#endif