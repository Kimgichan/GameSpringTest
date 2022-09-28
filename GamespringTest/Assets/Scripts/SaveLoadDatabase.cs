using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

using UnityEngine;

using Nodes;
using Enums;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "SaveLoadDatabase", menuName = "ScriptableObject/SaveLoadDatabase", order = int.MaxValue)]
public class SaveLoadDatabase : ScriptableObject
{
    [SerializeField] private string key;

    [SerializeField] private int rankingCount;
    [ReadOnly] [SerializeField] private RankingData rankingData;

    #region 함수
    public void Save(SaveLoadKind kind)
    {
        switch (kind)
        {
            case SaveLoadKind.Ranking:
                {
                    RankingSizeUpdate();
                    RenewalRanking(0);

                    File.WriteAllText($"{Application.persistentDataPath}/{Encrypt(SaveLoadKind.Ranking.ToString(), key)}.json",
                        Encrypt(JsonUtility.ToJson(rankingData), key));

                    break;
                }
        }
    }

    public void Load(SaveLoadKind kind)
    {
        switch (kind)
        {
            case SaveLoadKind.Ranking:
                {
                    var path = $"{Application.persistentDataPath}/{Encrypt(SaveLoadKind.Ranking.ToString(), key)}.json";

                    if (File.Exists(path))
                    {
                        rankingData = JsonUtility.FromJson<RankingData>(Decrypt(File.ReadAllText(path), key));

                        if(rankingData == null) rankingData = new RankingData(); ;
                    }
                    else
                    {
                        rankingData = new RankingData();
                    }

                    RankingSizeUpdate();
                    RenewalRanking(0);
                    break;
                }
        }
    }


    #region 랭킹
    public void RankingSizeUpdate()
    {
        // 랭킹 배열이 랭킹 카운트 보다 많을 경우
        for(int start = rankingData.points.Count - 1, end = rankingCount; start >= end; start--)
        {
            rankingData.points.RemoveAt(start);
        }

        // 랭킹 배열이 랭킹 카운트 보다 적을 경우
        for(int start = 0, end = rankingCount - rankingData.points.Count; start<end; start++)
        {
            rankingData.points.Add(0);
        }
    }
    public int RankingCount => rankingCount;


    /// <summary>
    /// 인덱스 값이 낮을수록 순위가 높음
    /// </summary>
    /// <param name="indx"></param>
    /// <returns></returns>
    public int GetRanking(int indx) => rankingData.points[indx];

    public void RenewalRanking(int newPoint)
    {
        // 혹 외부로 인해 배열의 순서(순위)가 변경(뒤죽박죽으로)됐을 경우를 고려.

        var points = rankingData.points;
        var minIndx = 0;

        for (int i = 1, icount = points.Count; i<icount; i++)
        {
            var point = points[i];

            if (point < points[minIndx])
            {
                minIndx = i;
            }
        }

        if(points[minIndx] < newPoint)
        {
            points[minIndx] = newPoint;
            rankingData.points = (from point in points orderby point descending select point).ToList();
            Save(SaveLoadKind.Ranking);

            return;
        }

        rankingData.points = (from point in points orderby point descending select point).ToList();
    }
    #endregion
    #endregion

    #region 복호화, 암호화 코드 https://intro0517.tistory.com/37
    // 복호화
    private string Decrypt(string textToDecrypt, string key)

    {
        RijndaelManaged rijndaelCipher = new RijndaelManaged();

        rijndaelCipher.Mode = CipherMode.CBC;

        rijndaelCipher.Padding = PaddingMode.PKCS7;



        rijndaelCipher.KeySize = 128;

        rijndaelCipher.BlockSize = 128;

        byte[] encryptedData = Convert.FromBase64String(textToDecrypt);

        byte[] pwdBytes = Encoding.UTF8.GetBytes(key);

        byte[] keyBytes = new byte[16];

        int len = pwdBytes.Length;

        if (len > keyBytes.Length)

        {

            len = keyBytes.Length;

        }

        Array.Copy(pwdBytes, keyBytes, len);

        rijndaelCipher.Key = keyBytes;

        rijndaelCipher.IV = keyBytes;

        byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        return Encoding.UTF8.GetString(plainText);

    }

    // 암호화
    private string Encrypt(string textToEncrypt, string key)

    {

        RijndaelManaged rijndaelCipher = new RijndaelManaged();

        rijndaelCipher.Mode = CipherMode.CBC;

        rijndaelCipher.Padding = PaddingMode.PKCS7;



        rijndaelCipher.KeySize = 128;

        rijndaelCipher.BlockSize = 128;

        byte[] pwdBytes = Encoding.UTF8.GetBytes(key);

        byte[] keyBytes = new byte[16];

        int len = pwdBytes.Length;

        if (len > keyBytes.Length)

        {

            len = keyBytes.Length;

        }

        Array.Copy(pwdBytes, keyBytes, len);

        rijndaelCipher.Key = keyBytes;

        rijndaelCipher.IV = keyBytes;

        ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

        byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);

        return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));

    }
    #endregion
}
