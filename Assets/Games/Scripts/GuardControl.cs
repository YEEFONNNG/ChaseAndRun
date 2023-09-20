using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardControl : MonoBehaviour, IEnemyUtils
{
    [SerializeField]
    private Light _lighting = null;
    [SerializeField]
    private NavMeshAgent _agent = null;
    [SerializeField]
    private Animator _animator = null;
    [SerializeField]
    private float _initPatrol = 0f;
    [SerializeField]
    private float _visionRange = 0f;
    [SerializeField]
    private float _visionAngle = 0f;

    private const string AnimIDSpeedName = "Speed";
    private const string AnimIDMotionSpeedName = "MotionSpeed";
    private const string WallLayer = "Walls";
    private const string PlayerTag = "Player";
    private Transform _player = null;
    private Coroutine _movementCoroutine = null;
    private Coroutine _detectCoroutine = null;
    private Coroutine _delayCoroutine = null;
    private Coroutine _chaseCoroutine = null;
    private LayerMask _wallLayer;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private bool _isChecked = true;

    // Start is called before the first frame update
    void Start()
    {
        _wallLayer = LayerMask.GetMask(WallLayer);
        _player = GameObject.FindGameObjectWithTag(PlayerTag).transform;
        _agent.updateRotation = false;
        _animator.SetFloat(AnimIDMotionSpeedName, 1f);
        _delayCoroutine = StartCoroutine(DetectionTime(0f));
        Vector3 vectorToPlayer = _player.position - transform.position;
        _endPoint = transform.position + vectorToPlayer.normalized * _initPatrol;
    }

    public void AlarmAlert()
    {
        ChaseAlert();
    }

    IEnumerator EnemyMovement()
    {
        _startPoint = transform.position;
        bool isPatrol = false;
        bool isStart = false;
        float isRight = 4f;
        while (true)
        {
            if (isPatrol)
            {
                yield return new WaitUntil(() => !_agent.pathPending);
                if (_agent.remainingDistance > _agent.stoppingDistance)
                {
                    _animator.SetFloat(AnimIDSpeedName, 2f);
                }
                else
                {
                    _animator.SetFloat(AnimIDSpeedName, 0f);
                    isPatrol = false;
                    isStart = !isStart;
                    isRight = 4f;
                }
                transform.forward = _agent.desiredVelocity;
                yield return null;
            }
            else
            {
                Vector3 startDir = transform.forward;
                Vector3 endDir = transform.forward + (isRight-- > 2f ? transform.right : -transform.right);
                float animationTime = 0f;
                while (animationTime < 1f)
                {
                    transform.forward = Vector3.Lerp(startDir, endDir, animationTime);
                    animationTime += Time.deltaTime / 2f;
                    yield return null;
                }
                yield return new WaitForSecondsRealtime(Random.Range(1f, 2f));
                if (isRight == 0f)
                {
                    isPatrol = true;
                    _agent.SetDestination(isStart ? _startPoint : _endPoint);
                }
            }
        }
    }

    IEnumerator DetectPlayer()
    {
        while (true)
        {
            yield return null;
            Vector3 vectorToPlayer = _player.position - transform.position;
            Vector3 raycastPosition = transform.position + new Vector3(0f, 1.5f, 0f);
            if (Vector3.Distance(transform.position, _player.position) <= _visionRange)
            {
                if (Vector3.Angle(transform.forward, vectorToPlayer) <= _visionAngle)
                {
                    if (!Physics.Raycast(raycastPosition, vectorToPlayer, _visionRange, _wallLayer))
                    {
                        ChaseAlert();
                    }
                }
            }
            _isChecked = true;
        }
    }

    private void ChaseAlert()
    {
        DestroyCoroutine(ref _movementCoroutine);
        DestroyCoroutine(ref _detectCoroutine);
        DestroyCoroutine(ref _delayCoroutine);
        _lighting.color = Color.red;
        _endPoint = transform.position;
        _delayCoroutine = StartCoroutine(DetectionTime(Random.Range(1.5f, 2f)));
        if (_chaseCoroutine == null)
        {
            _chaseCoroutine = StartCoroutine(ChasePlayer());
        }
    }


    IEnumerator DetectionTime(float timer)
    {
        yield return new WaitForSecondsRealtime(timer);
        DestroyCoroutine(ref _chaseCoroutine);
        if (_detectCoroutine == null)
        {
            _detectCoroutine = StartCoroutine(DetectPlayer());
        }
        if (_movementCoroutine == null)
        {
            _movementCoroutine = StartCoroutine(EnemyMovement());
        }
        _isChecked = false;
        yield return new WaitUntil(() => _isChecked);
        _lighting.color = Color.white;
        _agent.speed = 2f;
        _agent.SetDestination(transform.position);
        _animator.SetFloat(AnimIDSpeedName, 0f);
        DestroyCoroutine(ref _delayCoroutine);
    }

    IEnumerator ChasePlayer()
    {
        _agent.speed = 4f;
        while (true)
        {
            yield return null;
            _agent.SetDestination(_player.position);
            yield return new WaitUntil(() => !_agent.pathPending);
            if (_agent.remainingDistance > _agent.stoppingDistance)
            {
                _animator.SetFloat(AnimIDSpeedName, 6f);
            }
            else
            {
                GameControl.Instance?.GameOver();
                _animator.SetFloat(AnimIDSpeedName, 0f);
            }
            transform.forward = _agent.desiredVelocity;
        }
    }

    public void DestroyCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
