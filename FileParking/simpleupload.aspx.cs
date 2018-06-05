using System;


public partial class simpleupload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        

        if (!IsPostBack)
        {
            MultipleFileUpload.StorageFolder = "userdata"; 
        }

    }
}