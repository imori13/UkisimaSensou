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
        // return tc * td <= 0 && ta * tb <= 0; // 端点を含む場合
    }
}
