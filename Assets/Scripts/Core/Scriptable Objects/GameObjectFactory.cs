using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "NewGameObjectFactory", menuName = "Setup/Factories/Object Factory")]
/// <summary>
/// Factory based on ScriptableObject.
/// Allows to spawn objects without having a MonoBehaviour on the scene.
/// </summary>
public class GameObjectFactory: ScriptableObject
{
    /// <summary>
    /// Creates an instance of the prefab in the desired scene.
    /// </summary>
    /// <typeparam name="T">The type of component we expect to receive</typeparam>
    /// <param name="prefab">Prefab object for cloning</param>
    /// <returns>Initialised object.</returns>
    public T CreateInstance<T>(T prefab) where T : MonoBehaviour
    {
        // Take the prefab and create a live copy of it on the stage.
        T instance = Instantiate(prefab);

        SceneManager.MoveGameObjectToScene(instance.gameObject, SceneManager.GetActiveScene());
    
        return instance;
    }
}