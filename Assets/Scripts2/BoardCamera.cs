using System.Collections;
using UnityEngine;

// Shows the complete board at the beginning of the game, then zooms to and
// follows the player.
public class BoardCamera : MonoBehaviour
{
    [Header("References")]
    public BoardManager boardManager;

    [Header("Overview Settings")]
    public float overviewDuration = 3f;
    public float boardPadding = 2f;

    [Header("Player Camera Settings")]
    public float playerZoom = 6f;
    public float zoomDuration = 2f;
    public float followSpeed = 5f;

    private Camera cam;
    private bool followingPlayer;

    // Gets the Camera component used by this script.
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Waits for the board and player to exist before starting the camera
    // overview and player zoom sequence.
    private IEnumerator Start()
    {
        if (cam == null)
        {
            Debug.LogError("BoardCamera requires a Camera component.");
            yield break;
        }

        if (!cam.orthographic)
        {
            Debug.LogError("BoardCamera requires an Orthographic camera.");
            yield break;
        }

        // Wait until BoardManager has spawned the player.
        yield return new WaitUntil(() => boardManager != null && boardManager.PlayerTransform != null);

        SetOverviewView();

        // Keep the full-board view visible for the configured duration.
        yield return new WaitForSeconds(overviewDuration);

        yield return ZoomToPlayer();

        followingPlayer = true;
    }

    // Positions and zooms the camera so the entire board is visible.
    private void SetOverviewView()
    {
        float boardWidth = boardManager.BoardWidth * boardManager.tileWidth;

        float boardHeight = boardManager.BoardHeight * boardManager.tileHeight;

        float boardCenterX = (boardManager.BoardWidth - 1) * boardManager.tileWidth / 2f;

        float boardCenterY = (boardManager.BoardHeight - 1) * boardManager.tileHeight / 2f;

        transform.position = new Vector3(boardCenterX, boardCenterY, transform.position.z);

        float screenAspect = (float)Screen.width / Screen.height;

        float requiredCameraHeight = boardWidth / screenAspect / 2f;

        float requiredVerticalHeight = boardHeight / 2f;

        cam.orthographicSize = Mathf.Max(requiredCameraHeight, requiredVerticalHeight) + boardPadding;
    }

    // Smoothly moves the camera from the board overview to the player.
    private IEnumerator ZoomToPlayer()
    {
        Vector3 startingPosition = transform.position;
        float startingZoom = cam.orthographicSize;

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / zoomDuration);

            // Smooth the transition so the camera does not move linearly.
            progress = Mathf.SmoothStep(0f, 1f, progress);

            Transform player =
                boardManager.PlayerTransform;

            if (player != null)
            {
                Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);

                transform.position = Vector3.Lerp(startingPosition, targetPosition, progress);
            }

            cam.orthographicSize = Mathf.Lerp(startingZoom, playerZoom, progress);

            yield return null;
        }
    }

    // After the initial zoom, keep the camera centered on the player.
    private void LateUpdate()
    {
        if (!followingPlayer)
        {
            return;
        }

        Transform player = boardManager.PlayerTransform;

        if (player == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
