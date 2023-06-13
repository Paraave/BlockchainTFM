using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ZXing;
using ZXing.QrCode;
using System.Linq;

public class InputFields : MonoBehaviour
{
    Blockchain bc = null;
    FirebaseCaller fb = null;
    Camera cam = null;

    [SerializeField] Color32 green = new Color32(46, 231, 0, 255);
    [SerializeField] Color32 red = new Color32(212, 47, 29, 255);
    [SerializeField] GameObject arrows = null;
    [SerializeField] GameObject buttonSync = null;
    [SerializeField] float speedMove = 10f;
    [SerializeField] bool moveLeft = false;
    [SerializeField] bool moveRight = false;

    [Header("QR")]
    [SerializeField] RawImage qrImage = null;
    Texture2D storeEncodedTexture = null;


    [Header("InputFields")]
    [SerializeField] TMP_InputField inputFieldCaducidad = null;
    [SerializeField] TMP_Dropdown dropdownLine = null;
    [SerializeField] TMP_InputField inputFieldPeso = null;
    [SerializeField] TMP_Dropdown dropdownProduct = null;

    private void Start()
    {
        bc=FindObjectOfType<Blockchain>();
        fb = FindObjectOfType<FirebaseCaller>();
        cam = Camera.main;


        storeEncodedTexture = new Texture2D(256, 256);

    }
    public void CreateBlock()
    {
        ValidarCaducidad();
        ValidarPeso();
        ValidarLinea();
        ValidarProducto();

        if (ValidarCaducidad() && ValidarPeso() && ValidarLinea() && ValidarProducto())
        {
            string caducidad = inputFieldCaducidad.text;
            string peso = inputFieldPeso.text;
            string line = dropdownLine.options[dropdownLine.value].text;
            string product = dropdownProduct.options[dropdownProduct.value].text;
            string IoT = SystemInfo.deviceName; //Devuelve el tipo de dispositivo IoT con el que se ha subido el bloque

            bc.CreateBlock("Razón Social: Pepin&CO" + "\n"+
                        "Producto: "+product + "\n"+
                        "Peso (Kg): "+ peso +"\n"+
                        "Fecha caducidad: "+caducidad + "\n"+
                        "Línea: "+ line + "\n" +
                        "IoT: " + IoT);
        }
    }

    public void ResetBC()
    {
        Debug.Log("Pressed ResetBC and Removed BC");

        bc.blockPosition = new Vector3(0, 0, 0);
        fb.ResetBC();
        bc.ResetBC();
    }

    public void SyncronizeButton()
    {
        Debug.Log("Intentando Conectar a Firebase");
        buttonSync.SetActive(false);
        arrows.SetActive(true);
    }

    private void Update()
    {
        RayCast();
        MovementCamera();


    }

    private void RayCast()
    {
        RaycastHit2D hit = Physics2D.Raycast(cam.transform.position, cam.transform.forward);
        if (hit.collider != null)
        {
            if(hit.collider.GetComponent<BlockAsset>())
            {
                var blockAsset = hit.collider.GetComponent<BlockAsset>();
                EncodeTextToQR(blockAsset);
            }

        }
    }


    #region QRCode
    private Color32[] Encode(string text, int width, int height)
    {
        BarcodeWriter writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(text);
    }

    private void EncodeTextToQR(BlockAsset blockAsset)
    {

        var dataCode = blockAsset.index.text + "\n" + blockAsset.hash.text + "\n" + blockAsset.data.text;

        string text = string.IsNullOrEmpty(dataCode) ? "Deberías escribir algo" : dataCode;
        Color32[] convertPixelToTexture = Encode(text, storeEncodedTexture.width, storeEncodedTexture.height);
        storeEncodedTexture.SetPixels32(convertPixelToTexture);
        storeEncodedTexture.Apply();

        qrImage.texture=storeEncodedTexture;
    }

    #endregion

    #region MoveArrows

    private void MovementCamera()
    {
        if (moveLeft)
        {
            if (cam.transform.position.x >= 0)
                cam.transform.position += transform.right * -speedMove * Time.deltaTime;
        }

        if (moveRight)
        {
            var lastBlock = bc.blockAssetsList.Last();

            if (lastBlock.gameObject.transform.position.x >= cam.transform.position.x)
                cam.transform.position += transform.right * speedMove * Time.deltaTime;
        }
    }
    public void PressLeft()
    {
        moveLeft = true;
    }

    public void PressRight()
    {
        moveRight = true;
    }

    public void UnpressLeft()
    {
        moveLeft = false;
    }

    public void UnpressRight()
    {
        moveRight = false;
    }
    #endregion

    #region InputFieldValidators

    private void GreenRedInputFields(GameObject inputField, bool b)
    {
        if (b)
        {
            inputField.GetComponent<Image>().color = green;
        }

        else
        {
            inputField.GetComponent<Image>().color = red;
        }
    }

    private bool ValidarCaducidad()
    {
        string formatoFecha = "dd/MM/yyyy";
        DateTime fecha;

        if (DateTime.TryParseExact(inputFieldCaducidad.text, formatoFecha, null, System.Globalization.DateTimeStyles.None, out fecha))
        {
            if (fecha > DateTime.Today)
            {
                GreenRedInputFields(inputFieldCaducidad.gameObject, true);
                return true;
            }
            else
            {
                GreenRedInputFields(inputFieldCaducidad.gameObject, false);
                return false;
            }
        }
        else
        {
            GreenRedInputFields(inputFieldCaducidad.gameObject, false);

            return false;
        }
    }

    private bool ValidarLinea()
    {
        if (dropdownLine.value != 0)
        {
            GreenRedInputFields(dropdownLine.gameObject, true);
            return true;
        }

        else
        {
            GreenRedInputFields(dropdownLine.gameObject, false);
            return false;
        }
    }


    private bool ValidarProducto()
    {
        if(dropdownProduct.value!=0)
        {
            GreenRedInputFields(dropdownProduct.gameObject, true);
            return true;
        }

        else
        {
            GreenRedInputFields(dropdownProduct.gameObject, false);
            return false;
        }
    }

    private bool ValidarPeso()
    {
        float kilograms = 0;

        if(float.TryParse(inputFieldPeso.text, out kilograms))
        {
            if (kilograms > 0)
            {
                GreenRedInputFields(inputFieldPeso.gameObject, true);

                return true;
            }

            else
            {
                GreenRedInputFields(inputFieldPeso.gameObject, false);

                return false;
            }
        }

        else 
        {
            GreenRedInputFields(inputFieldPeso.gameObject, false);

            return false; 
        }

    }
    #endregion
}
