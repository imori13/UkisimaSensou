using UnityEngine;

public static class MyMath
{
    public static Vector3 RandomGenerateSize(Vector3 GenerateSize)
    {
        return new Vector3(
            Random.Range(-GenerateSize.x, GenerateSize.x),
            Random.Range(-GenerateSize.y, GenerateSize.y),
            Random.Range(-GenerateSize.z, GenerateSize.z));
    }

    public static Vector3 DiscardTheValueOfY(Vector3 value)
    {
        return new Vector3(value.x, 0, value.z);
    }
}
