using UnityEngine;

public static class MyMath
{
    // ( -引数 ~ 引数 )の範囲でベクトルを生成する
    public static Vector3 RandomGenerateSize(Vector3 GenerateSize)
    {
        return new Vector3(
            Random.Range(-GenerateSize.x, GenerateSize.x),
            Random.Range(-GenerateSize.y, GenerateSize.y),
            Random.Range(-GenerateSize.z, GenerateSize.z));
    }

    // Vector3をxとzの情報を抜き取ってVector2に変換する
    public static Vector2 ConversionVector2(Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }

    // 線分が交差しているかどうか
    public static bool JudgeIentersected(Vector2 p1_s, Vector2 p1_e, Vector2 p2_s, Vector2 p2_e)
    {
        var ta = (p2_s.x - p2_e.x) * (p1_s.y - p2_s.y) + (p2_s.y - p2_e.y) * (p2_s.x - p1_s.x);
        var tb = (p2_s.x - p2_e.x) * (p1_e.y - p2_s.y) + (p2_s.y - p2_e.y) * (p2_s.x - p1_e.x);
        var tc = (p1_s.x - p1_e.x) * (p2_s.y - p1_s.y) + (p1_s.y - p1_e.y) * (p1_s.x - p2_s.x);
        var td = (p1_s.x - p1_e.x) * (p2_e.y - p1_s.y) + (p1_s.y - p1_e.y) * (p1_s.x - p2_e.x);

        return tc * td < 0 && ta * tb < 0;
    }

    // 重みづけランダム、返り値は0,1,2,3みたいな連番
    public static int GetRandomIndex(int[] array)
    {
        int totalWeight = 0;
        int value = 0;
        int returnValue = -1;
        for (int i = 0; i < array.Length; i++)
        {
            totalWeight += array[i];
        }
        value = Random.Range(1, totalWeight + 1);

        for (var i = 0; i < array.Length; i++)
        {
            if (array[i] >= value)
            {
                returnValue = i;
                break;
            }
            value -= array[i];
        }

        return returnValue;
    }
    
    /// 円と線分の交差判定
    public static bool IsLineIntersectedCircle(Vector2 a, Vector2 b, Vector2 p, float radius)
    {
        Vector2 ap = p - a;
        Vector2 ab = b - a;
        Vector2 bp = p - b;
        Vector2 normalAB = ab.normalized;

        float lenAX = Vector2.Dot(normalAB, ap);

        float shortestDistance = 0.0f;
        if (lenAX < 0)
        {
            shortestDistance = ap.magnitude;
        }
        else if (lenAX > ab.magnitude)
        {
            shortestDistance = bp.magnitude;
        }
        else
        {
            shortestDistance = Mathf.Abs(Vector3.Cross(normalAB, ap).magnitude);
        }

        Vector2 X = a + ab * lenAX;
        return shortestDistance < radius;
    }
}
