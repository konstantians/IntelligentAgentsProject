namespace IntelligentAgents.GatewayAPI.HelperMethods;

public class UtilityMethods : IUtilityMethods
{
    public List<string> ReturnKIdWithSmallestCosineDistances(Dictionary<string, float[]> idEmbeddingPairs, float[] target, int numberOfRecordsThatShouldBeReturned)
    {
        return idEmbeddingPairs
        .Select(pair => new
        {
            Id = pair.Key,
            Distance = CalculateCosineDistance(pair.Value, target)
        })
        .OrderBy(x => x.Distance)
        .Take(numberOfRecordsThatShouldBeReturned)
        .Select(x => x.Id)
        .ToList();
    }

    public float CalculateCosineDistance(float[] firstVector, float[] secondVector)
    {
        if (firstVector.Length != secondVector.Length)
            throw new ArgumentException("The vectors do not have the same size");

        float sumABVector = 0;
        float sumASquaredVector = 0;
        float sumBSquaredVector = 0;
        for (int i = 0; i < firstVector.Length; i++)
        {
            sumABVector += firstVector[i] * secondVector[i];
            sumASquaredVector += firstVector[i] * firstVector[i];
            sumBSquaredVector += secondVector[i] * secondVector[i];
        }

        float cosineSimilarity = sumABVector / (float)(Math.Sqrt(sumASquaredVector) * Math.Sqrt(sumBSquaredVector));
        float cosineDistance = 1 - cosineSimilarity;
        return cosineDistance;
    }
}