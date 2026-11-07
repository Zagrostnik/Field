namespace FieldMission
{
    public interface IPoolable
    {
        void OnSpawnedFromPool();
        void OnReturnedToPool();
    }
}
