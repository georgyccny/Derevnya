using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System;  // Add this line to include the System namespace
using System.Threading.Tasks;  // For async/await support
public class NPC : MonoBehaviour
{
    public float moveSpeed = 2f;
    public string npcName;
    public int maxCarry = 3;
    public string backstory = "I am a skilled lumberjack. I've been working in these woods for 20 years.";
    private Text nameText;
    private List<Vector2Int> currentPath;
    private int currentPathIndex;
    private bool isMoving = false;
    private Vector2Int currentTarget;
    private Vector2Int homePosition;
    // Resource management
    private int carriedResources = 0;
    private int depositedResources = 0;
    // UI elements
    private GameObject uiPanel;
    private Text infoText;
    private LineRenderer pathRenderer;
    // GPT-2 integration
    private TcpClient socketConnection;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];
    private string thoughts = "";
    void Start()
    {
        npcName = GenerateRandomName();
        CreateNameText();
        CreateUIPanel();
        SetupPathRenderer();
        FindHome();
        ConnectToGPT2Server();
        StartCoroutine(AIRoutine());
        StartCoroutine(GenerateThoughts());
    }
    void CreateNameText()
    {
        GameObject textObj = new GameObject("NameText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        nameText = textObj.AddComponent<Text>();
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 14;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;
        nameText.text = npcName;
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
        RectTransform rectTransform = nameText.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(2, 0.5f);
        rectTransform.localScale = new Vector3(0.01f, 0.01f, 1);
    }
    void CreateUIPanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        uiPanel = new GameObject("NPCInfoPanel");
        uiPanel.transform.SetParent(canvas.transform, false);
        Image panelImage = uiPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 500);
        GameObject textObj = new GameObject("InfoText");
        textObj.transform.SetParent(uiPanel.transform, false);
        infoText = textObj.AddComponent<Text>();
        infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        infoText.fontSize = 16;
        infoText.color = Color.white;
        infoText.alignment = TextAnchor.UpperLeft;
        // Updated Text RectTransform to avoid truncation
        RectTransform textRect = infoText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        textRect.sizeDelta = Vector2.zero; // Ensure the text box can expand
        infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
        infoText.verticalOverflow = VerticalWrapMode.Overflow; // Ensure text doesn't truncate vertically
        infoText.resizeTextForBestFit = true; // Optional: Dynamically resize text for best fit
        infoText.resizeTextMinSize = 12; // Minimum font size
        infoText.resizeTextMaxSize = 18; // Maximum font size
        uiPanel.SetActive(false);
    }
    void SetupPathRenderer()
    {
        pathRenderer = gameObject.AddComponent<LineRenderer>();
        pathRenderer.startWidth = 0.1f;
        pathRenderer.endWidth = 0.1f;
        pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathRenderer.startColor = Color.yellow;
        pathRenderer.endColor = Color.yellow;
        pathRenderer.enabled = false;
    }
    void FindHome()
    {
        int minDistance = int.MaxValue;
        Vector2Int currentPos = Vector2Int.RoundToInt(transform.position);
        for (int x = 0; x < GameManager.Instance.worldSize; x++)
        {
            for (int y = 0; y < GameManager.Instance.worldSize; y++)
            {
                Tile tile = GameManager.Instance.GetTileAt(x, y);
                if (tile != null && tile.type == Tile.TileType.House)
                {
                    int distance = Mathf.Abs(x - currentPos.x) + Mathf.Abs(y - currentPos.y);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        homePosition = new Vector2Int(x, y);
                    }
                }
            }
        }
    }
    void ConnectToGPT2Server()
    {
        try
        {
            socketConnection = new TcpClient("localhost", 5555);
            stream = socketConnection.GetStream();
            Debug.Log("Connected to GPT-2 server");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Socket error: " + e);
        }
    }
    IEnumerator AIRoutine()
    {
        while (true)
        {
            if (!isMoving)
            {
                DecideAction(thoughts);
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
        }
    }
    IEnumerator GenerateThoughts()
    {
        while (true)
        {
            // Build a dynamic prompt based on NPC context
            string context = $"Name: {npcName}\nBackstory: {backstory}\nLocation: {transform.position}\nProfession: Lumberjack\nTask: {(isMoving ? "Moving" : "Idle")}\n";
            context += $"Current resources: {carriedResources}/{maxCarry}\n\nWhat should I do next?";
            // Use async/await to avoid blocking the main thread
            Task<string> task = SendToGPT2Async(context);
            yield return new WaitUntil(() => task.IsCompleted);  // Wait for the task to complete
            thoughts = task.Result;
            if (uiPanel.activeSelf)
            {
                UpdateInfoText();
            }
            yield return new WaitForSeconds(5f);
        }
    }
    async Task<string> SendToGPT2Async(string input)
    {
        if (stream != null && stream.CanWrite)
        {
            try
            {
                // Add a newline character to signal the end of the message to the server
                byte[] sendBuffer = Encoding.ASCII.GetBytes(input + "\n");
                await stream.WriteAsync(sendBuffer, 0, sendBuffer.Length);  // Send data asynchronously
                StringBuilder response = new StringBuilder();
                byte[] buffer = new byte[1024];
                int bytesRead;
                // Read asynchronously until a newline character is found
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    response.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                    if (response.ToString().Contains("\n"))
                    {
                        break;
                    }
                }
                return response.ToString().Trim();  // Remove the trailing newline and any extra spaces
            }
            catch (SocketException e)
            {
                Debug.LogError($"Socket error: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Unexpected error: {e.Message}");
            }
        }
        return "No thoughts at the moment.";
    }
    void DecideAction(string thoughts)
    {
        if (thoughts.Contains("gather wood") || thoughts.Contains("collect resources"))
        {
            SetDestination(FindNearestForest());
        }
        else if (thoughts.Contains("go home") || thoughts.Contains("return home"))
        {
            SetDestination(homePosition);
        }
        else if (thoughts.Contains("explore") || thoughts.Contains("wander"))
        {
            SetDestination(GetRandomWalkableTile());
        }
        else
        {
            // Default behavior if no clear action is determined
            if (carriedResources >= maxCarry)
            {
                SetDestination(homePosition);
            }
            else
            {
                SetDestination(FindNearestForest());
            }
        }
    }
    void Update()
    {
        if (isMoving && currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector3 targetPosition = new Vector3(currentPath[currentPathIndex].x, currentPath[currentPathIndex].y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Count)
                {
                    isMoving = false;
                    if (Vector2Int.RoundToInt(transform.position) == homePosition)
                    {
                        DepositResources();
                    }
                    else
                    {
                        AttemptGatherResource();
                    }
                }
            }
            if (uiPanel.activeSelf)
            {
                UpdateInfoText();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }
    void SetDestination(Vector2Int target)
    {
        currentPath = Pathfinding.Instance.FindPath(Vector2Int.RoundToInt(transform.position), target);
        if (currentPath != null && currentPath.Count > 0)
        {
            currentPathIndex = 0;
            isMoving = true;
            currentTarget = target;
            UpdatePathVisualization();
        }
    }
    Vector2Int GetRandomWalkableTile()
    {
        Vector2Int position;
        do
        {
            position = new Vector2Int(UnityEngine.Random.Range(0, GameManager.Instance.worldSize), UnityEngine.Random.Range(0, GameManager.Instance.worldSize));
        } while (!Pathfinding.Instance.IsTileWalkable(position));
        return position;
    }
    Vector2Int FindNearestForest()
    {
        Vector2Int currentPos = Vector2Int.RoundToInt(transform.position);
        int searchRadius = 10;
        for (int r = 1; r <= searchRadius; r++)
        {
            for (int x = currentPos.x - r; x <= currentPos.x + r; x++)
            {
                for (int y = currentPos.y - r; y <= currentPos.y + r; y++)
                {
                    if (Mathf.Abs(x - currentPos.x) + Mathf.Abs(y - currentPos.y) == r)
                    {
                        Tile tile = GameManager.Instance.GetTileAt(x, y);
                        if (tile != null && tile.type == Tile.TileType.Forest)
                        {
                            return new Vector2Int(x, y);
                        }
                    }
                }
            }
        }
        return GetRandomWalkableTile(); // If no forest found, return a random tile
    }
    void AttemptGatherResource()
    {
        Vector2Int currentPos = Vector2Int.RoundToInt(transform.position);
        Tile currentTile = GameManager.Instance.GetTileAt(currentPos.x, currentPos.y);
        if (currentTile != null && currentTile.type == Tile.TileType.Forest)
        {
            if (currentTile.HarvestResource())
            {
                carriedResources++;
                Debug.Log($"{npcName} gathered a resource from the forest. Now carrying {carriedResources}.");
                if (carriedResources >= maxCarry)
                {
                    SetDestination(homePosition);
                }
                if (uiPanel.activeSelf)
                {
                    UpdateInfoText();
                }
            }
        }
    }
    void DepositResources()
    {
        depositedResources += carriedResources;
        Debug.Log($"{npcName} deposited {carriedResources} resources. Total deposited: {depositedResources}");
        carriedResources = 0;
        if (uiPanel.activeSelf)
        {
            UpdateInfoText();
        }
    }
    string GenerateRandomName()
    {
        string[] names = { "Alice", "Bob", "Charlie", "David", "Emma", "Frank", "Grace", "Henry", "Ivy", "Jack" };
        return names[UnityEngine.Random.Range(0, names.Length)];
    }
    void HandleClick()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            ToggleInfoPanel();
        }
        else
        {
            HideInfoPanel();
        }
    }
    void ToggleInfoPanel()
    {
        uiPanel.SetActive(!uiPanel.activeSelf);
        pathRenderer.enabled = uiPanel.activeSelf;
        if (uiPanel.activeSelf)
        {
            UpdateInfoText();
            UpdatePathVisualization();
        }
    }
    void HideInfoPanel()
    {
        uiPanel.SetActive(false);
        pathRenderer.enabled = false;
    }
    void UpdateInfoText()
    {
        string info = $"Name: {npcName}\n";
        info += $"Position: ({transform.position.x:F1}, {transform.position.y:F1})\n";
        info += $"State: {(isMoving ? "Moving" : "Idle")}\n";
        info += $"Carried Resources: {carriedResources}/{maxCarry}\n";
        info += $"Deposited Resources: {depositedResources}\n";
        info += $"Home Position: ({homePosition.x}, {homePosition.y})\n";
        if (isMoving)
        {
            info += $"Destination: ({currentTarget.x}, {currentTarget.y})\n";
            info += $"Path length: {currentPath.Count}\n";
            info += $"Current path index: {currentPathIndex}\n";
        }
        info += $"\nThoughts: {thoughts}\n";
        infoText.text = info;
    }
    void UpdatePathVisualization()
    {
        if (currentPath != null)
        {
            pathRenderer.positionCount = currentPath.Count;
            for (int i = 0; i < currentPath.Count; i++)
            {
                pathRenderer.SetPosition(i, new Vector3(currentPath[i].x, currentPath[i].y, transform.position.z));
            }
        }
        else
        {
            pathRenderer.positionCount = 0;
        }
    }
    void OnDestroy()
    {
        if (socketConnection != null)
        {
            socketConnection.Close();
        }
    }
}