using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public class Blockchain : MonoBehaviour
{
    public List<Block> chain= new List<Block>();
    public List<GameObject> blockAssetsList = new List<GameObject>();
    public GameObject blockAsset = null;
    public Vector3 blockPosition = new Vector3(-10, 0, 0);
    FirebaseCaller fbCaller = null;


    public void Start() 
    {
        fbCaller = FindObjectOfType<FirebaseCaller>();
    }


    public void CreateGenesisBlock()
    {
        if (chain.Count <= 0)
        {
            Block genesis = new Block(0, DateTime.Now.ToString(), "", "");
            chain.Add(genesis);

            blockPosition = new Vector3(0, 0, 0);

            GameObject newBlockAsset = Instantiate(blockAsset, blockPosition, Quaternion.identity);
            newBlockAsset.GetComponent<BlockAsset>().CreateBlockAsset(genesis.index.ToString(), genesis.timestamp.ToString(), genesis.data, genesis.previousHash, genesis.hash);


            fbCaller.LoadBlockFB(genesis);
        }
    }

    public void CreateBlock(string data)
    {
        if (chain.Count >= 1)
        {
            Block previousBlock = chain[chain.Count - 1];

            Block block = new Block(previousBlock.index + 1, DateTime.Now.ToString(), data, previousBlock.hash);
            chain.Add(block);

            GameObject newBlockAsset = Instantiate(blockAsset, blockPosition, Quaternion.identity);
            newBlockAsset.GetComponent<BlockAsset>().CreateBlockAsset(block.index.ToString(), block.timestamp.ToString(), block.data, block.previousHash, block.hash);
            blockPosition += new Vector3(4f, 0, 0);

            fbCaller.LoadBlockFB(block);
        }

        else { Debug.Log("No hay bloques previos, inicialice la cadena generando un bloque génesis"); }

    }

    static string CalculateHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public void ResetBC()
    {
        foreach (var item in blockAssetsList)
        {
            Destroy(item.gameObject);
        }
        blockAssetsList.Clear();
        chain.Clear();
    }


    [System.Serializable]
    public class Block
    {
        public int index;
        public string timestamp;
        public string data;
        public string previousHash;
        public string hash;

        public Block(int index, string timestamp, string data, string previousHash)
        {
            this.index = index;
            this.previousHash = previousHash;
            this.data = data;
            this.timestamp = timestamp;
            hash = CalculateHash(index.ToString() + timestamp.ToString() + data + previousHash.ToString());
        }
    }


}