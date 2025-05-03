using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoomTransparencyManager : MonoBehaviour
{
    public Transform player;
    public LayerMask buildingLayer; // Bina katmanı
    public string transparentLayerName = "TransparentWalls";

    [Range(0, 1)]
    public float transparencyAmount = 0.3f;

    public List<Room> rooms = new List<Room>();

    private Room currentRoom;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        foreach (Room room in rooms)
        {
            SetRoomWallsOpaque(room);
        }
    }

    void Update()
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
        foreach (Room room in rooms)
        {
            if (room.IsPlayerInside(player.position))
            {
                return room;
            }
        }

        return null;
    }

    private void SetRoomWallsOpaque(Room room)
    {
        foreach (Renderer wallRenderer in room.wallRenderers)
        {
            SetWallOpacity(wallRenderer, 1.0f, "Obstacle");
        }
    }

    private void SetRoomWallsTransparent(Room room)
    {
        foreach (Renderer wallRenderer in room.wallRenderers)
        {
            SetWallOpacity(wallRenderer, transparencyAmount, "TransparentWalls");
        }
    }

    private void SetWallOpacity(Renderer wallRenderer, float opacity, string transparentLayerName)
    {
        int layer = LayerMask.NameToLayer(transparentLayerName);

        Material[] materials = wallRenderer.materials;
        int defLayer = LayerMask.NameToLayer(transparentLayerName);
        if (wallRenderer.gameObject.layer == defLayer)
            wallRenderer.gameObject.layer = layer;
        else
            wallRenderer.gameObject.layer = defLayer;
        foreach (Material mat in materials)
        {
            if (opacity < 1.0f && mat.renderQueue < 3000)
            {
                mat.SetFloat("_Mode", 3); // Saydam modke
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
                // Opak modk
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
}

[System.Serializable]
public class Room
{
    public string roomName;

    public Collider roomCollider;

    public List<Renderer> wallRenderers = new List<Renderer>();

    public bool IsPlayerInside(Vector3 playerPosition)
    {
        if (roomCollider != null)
        {
            return roomCollider.bounds.Contains(playerPosition);
        }
        return false;
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
            FindRoomsAutomatically(script);
        }
    }

    // OSMAAAAAANNNNNNNNNNNNNNNNNNNN
    void FindRoomsAutomatically(RoomTransparencyManager script)
    {
        script.rooms.Clear();

        GameObject[] roomObjects = GameObject.FindGameObjectsWithTag("Room");

        foreach (GameObject roomObj in roomObjects)
        {
            Collider roomCollider = roomObj.GetComponent<Collider>();
            if (roomCollider != null)
            {
                Room room = new Room();
                room.roomName = roomObj.name;
                room.roomCollider = roomCollider;

                foreach (Transform child in roomObj.transform)
                {
                    if (child.CompareTag("Wall"))
                    {
                        Renderer wallRenderer = child.GetComponent<Renderer>();
                        if (wallRenderer != null)
                        {
                            room.wallRenderers.Add(wallRenderer);
                        }
                    }
                }

                script.rooms.Add(room);
            }
        }
    }
}
#endif
