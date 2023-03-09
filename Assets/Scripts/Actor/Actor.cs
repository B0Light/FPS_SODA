using UnityEngine;


// This class contains general information describing an actor (player or enemies).
// It is mostly used for AI detection logic and determining if an actor is friend or foe
public class Actor : MonoBehaviour
{
    public int Affiliation; //team
    public Transform AimPoint;

    ActorsManager m_ActorsManager;

    void Start()
    {
        m_ActorsManager = GameObject.FindObjectOfType<ActorsManager>();
 
        // Register as an actor
        if (!m_ActorsManager.Actors.Contains(this))
        {
            m_ActorsManager.Actors.Add(this);
        }
    }

    void OnDestroy()
    {
        // Unregister as an actor
        if (m_ActorsManager)
        {
            m_ActorsManager.Actors.Remove(this);
        }
    }
}
