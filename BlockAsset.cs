using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using TMPro;

public class BlockAsset : MonoBehaviour
{

    public TextMeshPro index = null;
    public TextMeshPro timeStamp = null;
    public TextMeshPro data = null;
    public TextMeshPro hash = null;
    public TextMeshPro previousHash = null;

    public void CreateBlockAsset(string index, string timeStamp, string data, string previousHash, string hash)
    {
        this.index.text = index;
        this.timeStamp.text = timeStamp;
        this.data.text = data;
        this.previousHash.text = previousHash;
        this.hash.text = hash;

        FindObjectOfType<Blockchain>().blockAssetsList.Add(this.gameObject);
    }

}
