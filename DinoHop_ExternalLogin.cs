/* 
	Script: DinoHop_ExternalLogin.cs
	Author: Sergio Schiavo
	Project: DinoHop
	
 	Description: 
	This class is responsible for handling authentication/User Creation in a external server. It works posting/requesting information through an API.
 
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DinoHop_ExternalLogin.cs : MonoBehaviour
{

    string File_Name, baseURL;

    GameObject login_title, btnLogout;
    Text Username, error;
    InputField user_box, password_box;


    void Start()
    {
        btnLogout.SetActive(false);
        File_Name = Application.persistentDataPath + "/User.cjr";
        baseURL = "http://www.electronicmotiongames.com/dinohop/";
        if (!System.IO.File.Exists(File_Name))
        {
            WriteFile("");
            btnLogout.SetActive(false);
        }
        else
        {

            var _user = ReadFile();

            if (_user != "")
            {
                btnLogout.SetActive(true);
                Username.text = _user;

                login_title.SetActive(false);
                this.gameObject.GetComponent<RecordsLoader>().ResetInterface();
            }

        }
    }

    public void Logout()
    {
        System.IO.File.WriteAllText(File_Name, "");
        Username.text = "";
        login_title.SetActive(true);
        btnLogout.SetActive(false);
        this.gameObject.GetComponent<RecordsLoader>().ResetInterface();


    }

    void Update()
    {

    }

    public void WriteFile(string user_name)
    {
        System.IO.File.WriteAllText(File_Name, user_name);
    }

    public string ReadFile()
    {
        return (System.IO.File.ReadAllText(File_Name));
    }

    public void tryLogin()
    {
        if (user_box.text.Length > 3 && user_box.text.Length < 16)
        {
            if (password_box.text.Length > 4 && password_box.text.Length < 12)
            {
                ReceiveDataFromServer();
            }
            else
            {
                error.enabled = true;
                error.text = "invalid password";
            }
        }
        else
        {
            error.enabled = true;
            error.text = "invalid user name";
        }
    }


    public void addUser()
    {
        if (user_box.text.Length > 3 && user_box.text.Length < 16)
        {
            if (password_box.text.Length > 4 && password_box.text.Length < 12)
            {
                addDataToServer();
            }
            else
            {
                error.enabled = true;
                error.text = "invalid password";
            }
        }
        else
        {
            error.enabled = true;
            error.text = "invalid user name";
        }
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

    }

    private void ReceiveDataFromServer()
    {
        var form = new WWWForm();
        form.AddField("userName", user_box.text);
        form.AddField("password", md5(password_box.text));

        WWW w = new WWW(baseURL + "getData_DinoHopAuthenticateUser.php", form);

        StartCoroutine(WaitForRequest(w));

        var recs = System.Text.Encoding.UTF7.GetString(w.bytes);

        if (recs == "SUCCESS")
        {
            WriteFile(user_box.text);
            login_title.SetActive(false);
            gameObject.GetComponent<RecordsLoader>().ResetInterface();


            btnLogout.SetActive(true);
            Username.text = user_box.text;
        }
        else
        {
            error.enabled = true;
            error.text = "invalid user or password";
        }
    }

    private void addDataToServer()
    {
        var form = new WWWForm();
        form.AddField("userName", user_box.text);

        WWW w = new WWW(baseURL + "getData_DinoHopUserCheck.php", form);

        StartCoroutine(WaitForRequest(w));

        var recs = System.Text.Encoding.UTF7.GetString(w.bytes);

        if (recs == "SUCCESS")
        {
            error.enabled = true;
            error.text = "This user already exists";
        }
        else
        {
            var form2 = new WWWForm();
            form2.AddField("userName", user_box.text);
            form2.AddField("password", md5(password_box.text));

            WWW w2 = new WWW(baseURL + "addData_DinoHopCreateNewUser.php", form);

            StartCoroutine(WaitForRequest(w2));

            var recs2 = System.Text.Encoding.UTF7.GetString(w2.bytes);
            if (recs2 == "SUCCESS")
            {
                WriteFile(user_box.text);
                login_title.SetActive(false);
                gameObject.GetComponent<RecordsLoader>().ResetInterface();

            }
        }
    }


    public static string md5(string str)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        byte[] bytes = encoding.GetBytes(str);
        var sha = new System.Security.Cryptography.MD5CryptoServiceProvider();
        return System.BitConverter.ToString(sha.ComputeHash(bytes));
    }

}
