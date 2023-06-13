using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System;
using Firebase;

public class FirebaseCaller : MonoBehaviour
{
    private DatabaseReference dbref;
    private Blockchain bc = null;
    public bool syncronizeComplete = false;

    public void SincronizarFireBase()
    {
        bc = FindObjectOfType<Blockchain>();
        dbref = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseDatabase.DefaultInstance
            .GetReference(".info/connected")
            .ValueChanged += (object sender, ValueChangedEventArgs e) => {
                if (e.DatabaseError != null)
                {
                    Debug.LogError(e.DatabaseError.Message);
                    return;
                }

                bool isConnected = (bool)e.Snapshot.Value;
                if (isConnected)
                {
                    Debug.Log("Conectado a Firebase");
                    dbref.Child("blocks").OrderByChild("index").ValueChanged += FirstSyncronize;


                        dbref.ChildChanged += HandleChildChanged;
                        dbref.Child("blocks").ValueChanged += HandleValueChanged;
                    
                }

                else
                {
                    Debug.Log("Desconectado de Firebase");
                }
            };
    }

    public void LoadBlockFB(Blockchain.Block block)
    {
        string blockJson = JsonUtility.ToJson(block);
        dbref.Child("blocks").Push().SetRawJsonValueAsync(blockJson);
    }

    void FirstSyncronize(object sender, ValueChangedEventArgs args)
    {
        if (!syncronizeComplete)
        {
            Debug.Log("sincronizando bloques fb");
            DataSnapshot snapshot = args.Snapshot;
            bc.blockPosition += new Vector3(0f, 0, 0);


            foreach (DataSnapshot blockSnapshot in snapshot.Children)
            {
                Blockchain.Block block = JsonUtility.FromJson<Blockchain.Block>(blockSnapshot.GetRawJsonValue());
                bc.chain.Add(block);

                GameObject newBlockAsset = Instantiate(bc.blockAsset, bc.blockPosition, Quaternion.identity);
                newBlockAsset.GetComponent<BlockAsset>().CreateBlockAsset(block.index.ToString(), block.timestamp, block.data, block.previousHash, block.hash);

                bc.blockPosition += new Vector3(4f, 0, 0);

            }

            syncronizeComplete = true;
        }

    }

    public void ResetBC()
    {
        dbref.RemoveValueAsync();
    }

    void HandleChildChanged(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("CAMBIOS EN FB RESINCRONIZANDO...");

        bc.ResetBC();
        bc.blockPosition = new Vector3(0, 0, 0);

        Debug.Log("ELIMINANDO BC ASSETS..");

        DataSnapshot snapshot = args.Snapshot;
        foreach (DataSnapshot blockSnapshot in snapshot.Children)
        {
            Blockchain.Block block = JsonUtility.FromJson<Blockchain.Block>(blockSnapshot.GetRawJsonValue());
            bc.chain.Add(block);

            GameObject newBlockAsset = Instantiate(bc.blockAsset, bc.blockPosition, Quaternion.identity);
            newBlockAsset.GetComponent<BlockAsset>().CreateBlockAsset(block.index.ToString(), block.timestamp, block.data, block.previousHash, block.hash);


            bc.blockPosition += new Vector3(4f, 0, 0);
            Debug.Log("RECONSTRUYENDO BC ASSETS..");
        }
    }

    void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        Debug.Log("CADENA RESETEADA");

        bc.ResetBC();
        bc.blockPosition = new Vector3(0, 0, 0);

        DataSnapshot snapshot = args.Snapshot;
        foreach (DataSnapshot blockSnapshot in snapshot.Children)
        {
            Blockchain.Block block = JsonUtility.FromJson<Blockchain.Block>(blockSnapshot.GetRawJsonValue());
            bc.chain.Add(block);

            GameObject newBlockAsset = Instantiate(bc.blockAsset, bc.blockPosition, Quaternion.identity);
            newBlockAsset.GetComponent<BlockAsset>().CreateBlockAsset(block.index.ToString(), block.timestamp, block.data, block.previousHash, block.hash);

            bc.blockPosition += new Vector3(4f, 0, 0);
        }
    }


}
