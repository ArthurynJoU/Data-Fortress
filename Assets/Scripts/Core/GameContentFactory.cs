using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameContentFactory", menuName = "Setup/Factories/Game Content Factory")]
/// <summary>
/// A game content factory with object pooling support.
/// </summary>
public class GameContentFactory: GameObjectFactory
{
    // Key is class type (e.g., typeof(Trojan))
    // Value is queue of ready objects
    private Dictionary<Type, Queue<IProduct>> _poolDictionary = new Dictionary<Type, Queue<IProduct>>();

    public T GetProduct<T>(T prefab) where T : MonoBehaviour, IProduct
    {
        Type type = prefab.GetType();

        if ( !_poolDictionary.ContainsKey(type) )
        {
            _poolDictionary[type] = new Queue<IProduct>();
        }

        while ( _poolDictionary[type].Count > 0 )
        {
            T instance = (T)_poolDictionary[type].Dequeue();

            if ( instance != null )
            {
                instance.gameObject.SetActive(true);
                instance.Initialize();
                return instance;
            }
        }

        T newInstance = CreateInstance(prefab);
        if ( newInstance is GameEntity entity )
        {
            entity.OriginFactory = this;
        }
        newInstance.gameObject.SetActive(true);
        newInstance.Initialize();
        return newInstance;
    }

    // to remove an entity that is no longer needed on board
    public void Reclaim(IProduct entity)
    {
        Type type = entity.GetType();

        if ( !_poolDictionary.ContainsKey(type) )
        {
            _poolDictionary[type] = new Queue<IProduct>();
        }

        entity.gameObject.SetActive(false);
        _poolDictionary[type].Enqueue(entity);
    }

    public void ClearPool()
    {
        _poolDictionary.Clear();
    }

    // scriptableObject in Unity saves its data when exiting Play Mode
    // if you don't clear the dictionary, it will retain references to deleted objects, which will cause errors...
    private void OnDisable()
    {
        ClearPool();
    }
}