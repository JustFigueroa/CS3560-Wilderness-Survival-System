using System.Collections;
using UnityEngine;

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

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

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

        // Wait until BoardManager has spawned the player
        yield return new WaitUntil(() =>
            boardManager != null &&
            boardManager.PlayerTransform != null
        );

        SetOverviewView();

        // Keep the full-board view visible
        yield return new WaitForSeconds(overviewDuration);

        yield return ZoomToPlayer();

        followingPlayer = true;
    }

    private void SetOverviewView()
    {
        float boardWidth =
            boardManager.BoardWidth * boardManager.tileWidth;

        float boardHeight =
            boardManager.BoardHeight * boardManager.tileHeight;

        float boardCenterX =
            (boardManager.BoardWidth - 1) *
            boardManager.tileWidth / 2f;

        float boardCenterY =
            (boardManager.BoardHeight - 1) *
            boardManager.tileHeight / 2f;

        transform.position = new Vector3(
            boardCenterX,
            boardCenterY,
            transform.position.z
        );

        float screenAspect =
            (float)Screen.width / Screen.height;

        float requiredCameraHeight =
            boardWidth / screenAspect / 2f;

        float requiredVerticalHeight =
            boardHeight / 2f;

        cam.orthographicSize =
            Mathf.Max(
                requiredCameraHeight,
                requiredVerticalHeight
            ) + boardPadding;
    }

    private IEnumerator ZoomToPlayer()
    {
        Vector3 startingPosition = transform.position;
        float startingZoom = cam.orthographicSize;

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / zoomDuration);

            progress = Mathf.SmoothStep(0f, 1f, progress);

            Transform player =
                boardManager.PlayerTransform;

            if (player != null)
            {
                Vector3 targetPosition = new Vector3(
                    player.position.x,
                    player.position.y,
                    transform.position.z
                );

                transform.position = Vector3.Lerp(
                    startingPosition,
                    targetPosition,
                    progress
                );
            }

            cam.orthographicSize = Mathf.Lerp(
                startingZoom,
                playerZoom,
                progress
            );

            yield return null;
        }
    }

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

        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }
}