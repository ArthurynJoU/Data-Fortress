using UnityEngine;

public class EnemySpawner : MonoBehaviour, ITickable
{
    [Header("Dependencies")]
    [SerializeField]
    private GameContentFactory _contentFactory;

    private float _timer;
    private int _currentWaveIndex = 0;
    private int _currentStepIndex = 0;
    private int _spawnedInCurrentStep = 0;

    private enum SpawnerState
    {
        WaitingForWave,
        SpawningStep,
        Finished
    }

    private SpawnerState _state = SpawnerState.WaitingForWave;
    private LevelBase _myLevelData;

    public bool IsFinished()
    {
        if ( _state == SpawnerState.Finished )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Start()
    {
        _myLevelData = LevelManager.Instance.CurrentLevel;

        if ( _myLevelData != null && 
             _myLevelData.Waves != null && 
             _myLevelData.Waves.Count > 0 )
        {
            _timer = _myLevelData.Waves[0].DelayBeforeWave;
            _state = SpawnerState.WaitingForWave;
        }
        else
        {
            _state = SpawnerState.Finished;
            Debug.LogWarning("EnemySpawner: No data on waves!");
        }
    }

    public void Tick(float deltaTime)
    {
        if ( _state == SpawnerState.Finished )
        {
            return;
        }

        _timer -= deltaTime;

        if ( _timer <= 0f )
        {
            ProcessSpawning();
        }
    }

    private void ProcessSpawning()
    {
        WaveDefinition currentWave = _myLevelData.Waves[_currentWaveIndex];

        switch ( _state )
        {
            // Initialises the parameters for a new wave and transitions to the spawning state
            case SpawnerState.WaitingForWave:
                Debug.Log($"The wave has begun: {currentWave.Name}");
                _state = SpawnerState.SpawningStep;
                _currentStepIndex = 0;
                _spawnedInCurrentStep = 0;
                _timer = 0f;
                break;

                // Handles the sequential generation of enemies based on the current step's configuration
                case SpawnerState.SpawningStep:
                WaveStep currentStep = currentWave.Steps[_currentStepIndex];
                SpawnEnemy(currentStep.Config);
                _spawnedInCurrentStep++;

                // Check if all enemies for the current step have been deployed
                if ( _spawnedInCurrentStep >= currentStep.Count )
                {
                    _currentStepIndex++;
                    _spawnedInCurrentStep = 0;

                    // Check if all steps within the current wave have been completed
                    if ( _currentStepIndex >= currentWave.Steps.Count )
                    {
                        _currentWaveIndex++;

                        // Check if this was the final wave of the level
                        if ( _currentWaveIndex >= _myLevelData.Waves.Count )
                        {
                            _state = SpawnerState.Finished;
                            Debug.Log("All waves are complete!");
                        }
                        else
                        {
                            // Prepare for the next wave and apply the designated delay
                            _state = SpawnerState.WaitingForWave;
                            _timer = _myLevelData.Waves[_currentWaveIndex].DelayBeforeWave;
                        }
                    }
                    else
                    {
                        // Proceed to the next step within the same wave
                        _timer = currentWave.Steps[_currentStepIndex].Interval;
                    }
                }
                else
                {
                    // Continue spawning enemies for the current step
                    _timer = currentStep.Interval;
                }
                break;
        }
    }

    private void SpawnEnemy(EnemyLevelBase config)
    {
        EnemyBase enemy = _contentFactory.GetProduct(config.Prefab) as EnemyBase;

        if ( enemy != null )
        {
            Tile startTile = GameBoard.Instance.StartPoint;

            if ( startTile != null )
            {
                enemy.transform.position = startTile.WorldPosition;
            }
            else
            {
                Debug.LogError($"Unable to locate the starting cell using the coordinates {GameBoard.Instance.StartPoint}");
            }

            float randomScale = Random.Range(config.ScaleRange.Min, config.ScaleRange.Max);
            enemy.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            enemy.SetSpeed(Random.Range(config.SpeedRange.Min, config.SpeedRange.Max));
            enemy.SetDestination(GameBoard.Instance.EndPoint.Coordinates);
            enemy.BuildNewRoute();
        }
    }
}