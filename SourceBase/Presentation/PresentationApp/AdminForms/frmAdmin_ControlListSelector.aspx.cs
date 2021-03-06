#region Namespace
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Interface.Administration;
using PresentationApp;
using Application.Common;
using Application.Presentation;
#endregion

public partial class AdminForms_frmAdmin_ControlListSelector : LogPage
{
    #region Member Variables
    int icount;
    DataSet dsList;
    #endregion 
    #region User Functions
    private Boolean ValidateData()
    {
        
        if (lstControlList.Items.Count == 0)
        {
            //MsgBuilder theBuilder = new MsgBuilder();
            //theBuilder.DataElements["Control"] = "Field Label";
            //IQCareMsgBox.Show("BlankListBox", theBuilder, this);
            return false;
        }
        return true;
    }
    #endregion
    #region Events
    protected void Page_Init(object sender, EventArgs e)
    {
        if (Session["AppLocation"] == null || Session.Count == 0 || Session["AppUserID"].ToString() == "")
        {
            IQCareMsgBox.Show("SessionExpired", this);
            Response.Redirect("~/frmlogin.aspx",true);
        }
        if (Request.QueryString["List"] != "")
        {
            Page.Title = Request.QueryString["List"].ToString();
        }

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //ICustomFields CustomFields;
        
            if (!IsPostBack)
            {
                if (Request.QueryString["Label"] != "")
                    lblField.Text = Request.QueryString["Label"];
                /*
                if (Convert.ToInt32(Request.QueryString["CFID"]) != 0)
                {
                    Int32 customfieldID = Convert.ToInt32(Request.QueryString["CFID"]);
                    CustomFields = (ICustomFields)ObjectFactory.CreateInstance("BusinessProcess.Administration.BCustomFields, BusinessProcess.Administration");
                    DataSet dsList = CustomFields.GetCustomList(customfieldID);
                    BindFunctions theBind = new BindFunctions();
                    theBind.BindList(lstControlList, dsList.Tables[0], "Name", "ID");
                }
                */
                try
                {
                    if ((Session["AddCustomList"] != null))
                    {
                        
                        DataTable theDT = new DataTable();
                        theDT = (DataTable)Session["AddCustomList"];
                        if (theDT.Rows.Count > 0)
                        {
                            for (int i = 0; i < theDT.Rows.Count; i++)
                            {
                                lstControlList.Items.Add(theDT.Rows[i][0].ToString());
                            }

                        }
   
                    }

                }
                catch (Exception ex)
                {
                   
                }

            }
            btnSubmit.Attributes.Add("onClick", "return Validate(lstControlList);");
            btnAdd.Attributes.Add("onClick", "CheckDuplicate('lstControlList');");
            txtList.Attributes.Add("onkeydown", "if(event.which || event.keyCode){if ((event.which == 13) || (event.keyCode == 13)) {document.getElementById('" + btnAdd.UniqueID + "').click();return false;}} else {return true}; ");
    }
    protected void btnAdd_Click(object sender, EventArgs e)
    {
        

        Boolean flagadd = false;
        if (lstControlList.Items.Count>0 )
        {
            for (int i = 0; i < lstControlList.Items.Count; i++)
            {
                if (lstControlList.Items[i].Text.Trim() == txtList.Text.Trim().ToString())
                {
                    flagadd = true;
                }
            }
        }
        if (flagadd == false)
        {
            if (txtList.Text.Trim() != "")
            {
                lstControlList.Items.Add(txtList.Text);
            }
        }
        txtList.Text = "";
        txtList.Focus();

    }
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (ValidateData() == false)
            return;
        DataRow theDR;
        DataTable theDT=new DataTable() ;
        theDT.Columns.Add("Name", System.Type.GetType("System.String"));
        
        foreach (ListItem item in lstControlList.Items)
        {
            theDR = theDT.NewRow();
            theDR[0] = item.Text.Trim();
            theDT.Rows.Add(theDR);
        }
         
        Session["AddCustomList"]=  theDT;
        string theScript;
        theScript = "<script language='javascript' id='DrgPopup'>\n";
        theScript += "window.close();\n";
        theScript += "</script>\n";
        RegisterStartupScript("DrgPopup", theScript);

    }
    protected void btnBack_Click(object sender, EventArgs e)
    {
        string theScript;
        theScript = "<script language='javascript' id='DrgPopup'>\n";
        theScript += "window.close();\n";
        theScript += "</script>\n";
        RegisterStartupScript("DrgPopup", theScript);
    }
    

        

#endregion 

    
    
    
}

