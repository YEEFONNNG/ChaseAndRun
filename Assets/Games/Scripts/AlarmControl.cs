using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmControl : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource = null;

    private const string PlayerTag = "Player";
    private const string EnemyTag = "Enemy";
    private List<Transform> _alertList = null;
    private List<Transform> _pendingList = null;

    // Start is called before the first frame update
    void Start()
    {
        _alertList = new List<Transform>();
        _pendingList = new List<Transform>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == PlayerTag)
        {
            if (_pendingList.Contains(other.transform))
            {
                _alertList.Add(other.transform);
                TriggerAlert();
            }
            else
            {
                _pendingList.Add(other.transform);
            }
        }
        if (other.tag == EnemyTag)
        {
            if (_alertList.Contains(other.transform))
            {
                _pendingList.Add(other.transform);
            }
            else
            {
                _alertList.Add(other.transform);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == PlayerTag)
        {
            if (_alertList.Contains(other.transform))
            {
                _alertList.Remove(other.transform);
            }
            else
            {
                _pendingList.Remove(other.transform);
            }
        }
        if (other.tag == EnemyTag)
        {
            if (_pendingList.Contains(other.transform))
            {
                _pendingList.Remove(other.transform);
            }
            else
            {
                _alertList.Remove(other.transform);
            }
        }
    }

    private void TriggerAlert()
    {
        _audioSource.PlayOneShot(_audioSource.clip);
        foreach (Transform child in _alertList)
        {
            IEnemyUtils comp;
            if (child.TryGetComponent<IEnemyUtils>(out comp))
            {
                comp.AlarmAlert();
            }
        }
    }
}
