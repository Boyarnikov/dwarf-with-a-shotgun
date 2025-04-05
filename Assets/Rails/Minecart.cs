using UnityEngine;
using UnityEngine.UIElements;

public class Minecart : MonoBehaviour
{
    [Header("References")]
    public Node startingNode;
    public LineRenderer directionIndicator;

    [Header("Movement Settings")]
    public float friction = 0.1f;
    public float impulseStrength = 5f;
    public float minSpeed = 0.1f;
    public float speedLossFactor = 0.33f;
    public Vector2 desiredDirection;

    private Node _currentNode;
    private Node _targetNode;
    private float _currentSpeed;
    private float _progress;
    private Vector3 _moveDirection;
    private bool _useDesiredDirectionForNextNode;

    void Start()
    {
        Initialize(startingNode);
        desiredDirection = Random.insideUnitCircle.normalized;
        UpdateDirectionIndicator();
        _useDesiredDirectionForNextNode = false; 
    }

    void Initialize(Node startNode)
    {
        _currentNode = startNode;
        transform.position = _currentNode.transform.position;
        _targetNode = null;
        _currentSpeed = 0;
    }

    void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0))
        {
            ApplyImpulse();
        }
        */
        UpdateDirectionIndicator();
    }

    void FixedUpdate()
    {
        if (_currentSpeed > minSpeed && _targetNode != null)
        {
            MoveAlongPath();
            ApplyFriction();
        }
        else if (_targetNode == null)
        {
            ChooseNewPath();
        }
    }

    void ApplyImpulse()
    {
        _useDesiredDirectionForNextNode = true;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        desiredDirection = -(mousePos - transform.position).normalized;

        // Reverse direction if needed
        if (_targetNode != null)
        {
            Vector3 currentEdgeDir = (_targetNode.transform.position - _currentNode.transform.position).normalized;
            float angle = Vector3.Angle(desiredDirection, currentEdgeDir);

            if (angle > 90f)
            {
                // Reverse path direction
                Node temp = _currentNode;
                _currentNode = _targetNode;
                _targetNode = temp;
                _progress = 1 - _progress;
                _moveDirection = (_targetNode.transform.position - _currentNode.transform.position).normalized;
            }
        }

        _currentSpeed += impulseStrength;
    }

    public void ApplyShootingImpulse(Vector2 direction, float strength)
    {
        impulseStrength = strength;
        desiredDirection = direction;

        // Reverse direction if needed
        if (_targetNode != null)
        {
            Vector3 currentEdgeDir = (_targetNode.transform.position - _currentNode.transform.position).normalized;
            float angle = Vector3.Angle(desiredDirection, currentEdgeDir);

            if (angle > 90f)
            {
                // Reverse path direction
                Node temp = _currentNode;
                _currentNode = _targetNode;
                _targetNode = temp;
                _progress = 1 - _progress;
                _moveDirection = (_targetNode.transform.position - _currentNode.transform.position).normalized;
            }
        }

        _currentSpeed += impulseStrength;
    }

    void ApplyFriction()
    {
        _currentSpeed = Mathf.Max(_currentSpeed * (1 - friction), minSpeed);
    }

    void MoveAlongPath()
    {
        _progress += _currentSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.Lerp(
            _currentNode.transform.position,
            _targetNode.transform.position,
            Mathf.Clamp01(_progress)
        );

        if (_progress >= 1f)
        {
            CompleteMovement();
        }
    }

    void CompleteMovement()
    {
        _currentNode = _targetNode;
        _progress = 0f;
        ChooseNewPath();
    }

    void ChooseNewPath()
    {
        Node bestNode = null;
        float bestScore = float.MaxValue;

        if (_currentNode.connectedNodes.Count < 3) {
            _useDesiredDirectionForNextNode = false;
        }

        foreach (Node candidate in _currentNode.connectedNodes)
        {
            if (candidate == null) continue;

            Vector3 candidateDir = (candidate.transform.position - _currentNode.transform.position).normalized;
            float score;

            if (_useDesiredDirectionForNextNode)
            {
                // First crossroad after impulse: prioritize desired direction
                score = Vector3.Angle(desiredDirection, candidateDir);
            }
            else
            {
                // Normal operation: combine current direction and desired direction
                float pathAngle = Vector3.Angle(_moveDirection, candidateDir);
                float desiredAngle = Vector3.Angle(desiredDirection, candidateDir);
                score = pathAngle + desiredAngle * 0.5f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestNode = candidate;
            }
        }

        if (bestNode != null)
        {
            SetNewTarget(bestNode, bestScore);
            _useDesiredDirectionForNextNode = false; // Reset flag after first use
        }
    }

    void SetNewTarget(Node target, float angleScore)
    {
        _targetNode = target;
        _moveDirection = (_targetNode.transform.position - _currentNode.transform.position).normalized;
        desiredDirection = _moveDirection;
        
        // Apply speed loss based on angle change
        float angleFactor = Mathf.Clamp01(angleScore / 180f);
        _currentSpeed *= 1 - (angleFactor * speedLossFactor);
        _progress = 0f;
    }

    void UpdateDirectionIndicator()
    {
        if (directionIndicator != null)
        {
            directionIndicator.SetPosition(0, transform.position);
            directionIndicator.SetPosition(1, transform.position + (Vector3)desiredDirection * 2);
        }
    }

    void OnValidate()
    {
        if (startingNode != null && !Application.isPlaying)
        {
            transform.position = startingNode.transform.position;
        }
    }
}