﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Application.Common;
using Interface.FormBuilder;
using Application.Presentation;
using System.Text.RegularExpressions;



namespace IQCare.FormBuilder
{
    public partial class frmFormBuilder : Form
    {
        //for setting css for dynamic controls, create global object for css class
        clsCssStyle theStyle = new clsCssStyle();
        Form theForm;
        //fixed dimenstions of the panel
        const int iPanelLeft = 12;
        const int iPanelHeight = 440; //470;
        const int iPanelWidth = 935;

        //fixed dimenstions of the grid
        const int iDataGridLeft = 13;
        const int iDataGridHeight = 345; //370;
        const int iDataGridWidth = 880;
        const int iDataGridTop = 44;

        int iPanelCount = 0;
        int iControlCount = 0;
        //position of panel from the top, keeps on changing as new panel adds to the form.
        //By default first panel value is stored.
        int iPanelTop = 14;
        int iSelectedFieldRowId = 0;
        bool IsHandleAdded;

        DataGridViewRow dgwRemoveSelectedRow = new DataGridViewRow();

        DataSet dsFieldDetails = new DataSet();

        DataSet dsCollectDataToSave = new DataSet();
        DataSet dsUpdateMode = new DataSet();
     /////Datatables for Changes in Form Version//////////////
        DataTable dtmstformversion;
        DataTable dtTabchanges;
        DataTable dtSectionchanges;
        DataTable dtFieldChanges;
        DataTable dtConFieldchanges;
        DataRow[] theDRButton;
        DataSet DSFormVerTables;
        /////////////////////////////////////////////////////
      
        //DataTable dtDeleteFields;

        private static DataTable _dtDeleteFields = new DataTable();
        public static DataTable dtDeleteFields
        {
            get
            {
                return _dtDeleteFields;
            }
            set
            {
                _dtDeleteFields = value;
            }
        }

        private static DataTable _dtDeleteSections = new DataTable();
        public static DataTable dtDeleteSections
        {
            get
            {
                return _dtDeleteSections;
            }
            set
            {
                _dtDeleteSections = value;
            }
        }

        private static DataTable _dtDeleteTabs = new DataTable();
        public static DataTable dtDeleteTabs
        {
            get
            {
                return _dtDeleteTabs;
            }
            set
            {
                _dtDeleteTabs = value;
            }
        }
        static int tabID = 0;

        //DataTable dtDeleteSections;
        public static DataTable dtManageSectionPos;
        public static DataTable dtManageTabPos;

        DataTable dtMstFeatureForManageField;
        DataTable dtMstSectionForManageField;
        DataTable dtLnkFormsForManageField; //for fields

        static string strGetPath = GblIQCare.GetPath();
        Image imgList = Image.FromFile(strGetPath + "\\List.bmp");
        Image imgDisabledList = Image.FromFile(strGetPath + "\\listdesible.bmp");
        Image imgBusinessRule = Image.FromFile(strGetPath + "\\brule.bmp");
        Image imgInactiveBR = Image.FromFile(strGetPath + "\\nonactive.bmp");
        Image imgButtonUp = Image.FromFile(strGetPath + "\\btnUp.Image.gif");
        Image imgButtonDown = Image.FromFile(strGetPath + "\\btnDown.Image.gif");

        public frmFormBuilder()
        {
            InitializeComponent();
            tabID = 0;
        }

        public void frmFormBuilder_Load(object sender, EventArgs e)
        {
            try
            {
                dtformrel.Format = DateTimePickerFormat.Custom;
                dtformrel.CustomFormat = "       ";
                #region "Technical Area Combo"

                //fill drop down list for technical area
                if (cmbTechArea.DataSource == null)
                {
                    IManageForms objManageForms;
                    BindFunctions objBindControls = new BindFunctions();
                    DataSet dsModule;
                    objManageForms = (IManageForms)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BManageForms,BusinessProcess.FormBuilder");
                    dsModule = objManageForms.GetPublishedModuleList();
                    objBindControls.Win_BindCombo(cmbTechArea, dsModule.Tables[0], "ModuleName", "ModuleId");
                }
                ClearBusinessRules();
                //cmbTechArea.SelectedIndex = 0;
                #endregion

                if (GblIQCare.RefillDdlFields == 1)
                {
                    IFieldDetail objFieldDetail;
                    objFieldDetail = (IFieldDetail)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFieldDetails,BusinessProcess.FormBuilder");
                    //pass fieldName if want to retrieve specific field from GetCustomFields
                    dsFieldDetails = objFieldDetail.GetCustomFields("", 0, GblIQCare.iManageCareEnded);
                    GblIQCare.RefillDdlFields = 0;
                    //RestoreFormState();  - sanjay
                }
                else
                {
                   
                    //set css begin
                    theStyle.setStyle(this);
                    //set css end 
                    IFieldDetail objFieldDetail;
                    objFieldDetail = (IFieldDetail)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFieldDetails,BusinessProcess.FormBuilder");
                    //pass fieldName if want to retrieve specific field from GetCustomFields
                    dsFieldDetails = objFieldDetail.GetCustomFields("", 0, GblIQCare.iManageCareEnded);

                    dsUpdateMode.Tables.Clear();

                    if (dtDeleteFields != null)
                        dtDeleteFields.Clear();

                    dtDeleteFields = new DataTable();
                    //  dtDeleteFields = clsCommon.RemoveFieldLnk_Forms();
                    dtDeleteFields = clsCommon.RemoveFieldLnkGridView_Forms();
                    dtDeleteSections = clsCommon.ManageSectionPos();

                    if(dtDeleteTabs !=null)
                        dtDeleteFields.Clear();

                    dtDeleteTabs = clsCommon.ManageDeleteTab();



                    if (GblIQCare.iFormBuilderId > 0) //update mode
                    {
                        IFormBuilder objFormBuilder;
                        objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                        dsUpdateMode = objFormBuilder.GetFormDetail(GblIQCare.iFormBuilderId);
                        theDRButton = dsFieldDetails.Tables[1].Select("Value='" + GblIQCare.iFormBuilderId + "' and BusRuleId='29'"); 
                        FillData();
                    }
                    else
                    {
                        txtvernumber.Enabled = false;
                        txtlastupdatedate.Enabled = false;
                        txtlastupdatedby.Enabled = false;
                    }

                }
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindow("#C1", theBuilder, this);
                return;
            }
        }
        public void ClearBusinessRules()
        {
            GblIQCare.blnSingleVisit = false;
            GblIQCare.blnMultivisit = false;
            GblIQCare.blnSignatureOneachpage = false;
            GblIQCare.blncontrolunabledesable = true;
            GblIQCare.dtformBusinessValues.Clear();
            GblIQCare.dtformBusinessValues.Columns.Clear();
            GblIQCare.dtformBusinessValues.Rows.Clear();
        }
        private void FillData()
        {

            //mst feature
            if (dsUpdateMode.Tables[0].Rows.Count > 0)
            {
               
               // TextBox txtFormName = (propParentForm.Controls["txtFormName"] as TextBox);
                txtFormName.Text = dsUpdateMode.Tables[0].Rows[0]["FeatureName"].ToString();
                //feature id is already in GblIQCare.iFormBuilderId in update mode

               // ComboBox cmbTechArea = (propParentForm.Controls["cmbTechArea"] as ComboBox);
                cmbTechArea.SelectedValue = dsUpdateMode.Tables[0].Rows[0]["ModuleId"].ToString();
                cmbTechArea.Enabled = false;

                //// get fields according to modules////   ---- Sanjay - 30-11-2010
                IFieldDetail objFieldDetail = (IFieldDetail)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFieldDetails,BusinessProcess.FormBuilder");
                //pass fieldName if want to retrieve specific field from GetCustomFields
                dsFieldDetails = objFieldDetail.GetCustomFields("", 0, GblIQCare.iManageCareEnded);
                ///////////////////////////////////////

                //RadioButton rdoSingleVisit = (propParentForm.Controls["rdoSingleVisit"] as RadioButton);
                //RadioButton rdoMultipleVisit = (propParentForm.Controls["rdoMultipleVisit"] as RadioButton);

                if (dsUpdateMode.Tables[0].Rows[0]["MultiVisit"].ToString() == "0")
                    GblIQCare.blnSingleVisit = true;
                else if (dsUpdateMode.Tables[0].Rows[0]["MultiVisit"].ToString() == "1")
                    GblIQCare.blnMultivisit = true;
            }
            if (dsUpdateMode.Tables[6].Rows.Count > 0)
            {
                btnformbusinessrules.BackColor = System.Drawing.Color.Green;
                GblIQCare.dtformBusinessValues = dsUpdateMode.Tables[6];
            }
            DataSet clone_dsUpdateMode = new DataSet();
            clone_dsUpdateMode = dsUpdateMode.Copy();
            //mst section
            if (dsUpdateMode.Tables[1].Rows.Count > 0)
            {
                for (int j = 0; j < dsUpdateMode.Tables[3].Rows.Count; j++) 
                {

                    if (Convert.ToInt32(dsUpdateMode.Tables[3].Rows[j]["Signature"]) == 1)
                    {
                        GblIQCare.blnSignatureOneachpage = true;
                    }
                        UserCtrlFormBuilder ctl = CreateTab(dsUpdateMode.Tables[3].Rows[j]["TabName"].ToString());
                        
                       // DataRow[] filterrow     =    dsUpdateMode.Tables[1].Select("SectionID="+dsUpdateMode.Tables[3].Rows[j]["SectionID"].ToString());

                        DataTable sectiontable1 = GetSectionTabFeatureBYTabID(dsUpdateMode, Convert.ToInt32(dsUpdateMode.Tables[3].Rows[j]["TabID"].ToString()), 1);
                        dsUpdateMode.Tables[1].Clear();
                        dsUpdateMode.Tables[1].Merge(sectiontable1);


                        DataTable Fieldtable2 = GetSectionTabFeatureBYTabID(dsUpdateMode, Convert.ToInt32(dsUpdateMode.Tables[3].Rows[j]["TabID"].ToString()), 2);
                        dsUpdateMode.Tables[2].Clear();
                        dsUpdateMode.Tables[2].Merge(Fieldtable2);
                      
                        ctl.dsUpdateMode = dsUpdateMode;
                        for (int k = 0; k < dsUpdateMode.Tables[1].Rows.Count; k++)
                        {
                           ctl.CreatePanel(PMTCTConstants.strUpdate);
                           
                        } 
                         ctl.CreateField();
                        dsUpdateMode = clone_dsUpdateMode.Copy();
                        sectiontable1.Dispose();
                        sectiontable1 = null;

                        Fieldtable2.Dispose();
                        Fieldtable2 = null;
                }
            }
            if (dsUpdateMode.Tables[5].Rows[0]["Signature"].ToString() == "1")

                GblIQCare.blncontrolunabledesable = false;
            else
                GblIQCare.blncontrolunabledesable = true;
            if (dsUpdateMode.Tables[7].Rows.Count > 0)
            {
                if (dsUpdateMode.Tables[7].Rows[0]["Id"] != DBNull.Value)
                    GblIQCare.FormVersionId = Convert.ToInt32(dsUpdateMode.Tables[7].Rows[0]["Id"]);
                else
                    GblIQCare.FormVersionId = 0;
                GblIQCare.FormVersionNo = Convert.ToDecimal(dsUpdateMode.Tables[7].Rows[0]["VersionName"]);
                GblIQCare.FormVersionDate = Convert.ToDateTime(dsUpdateMode.Tables[7].Rows[0]["VersionDate"]);
                txtvernumber.Text = dsUpdateMode.Tables[7].Rows[0]["VersionName"].ToString();
                if(dsUpdateMode.Tables[7].Rows[0]["Updatedate"] != DBNull.Value)
                    txtlastupdatedate.Text = Convert.ToDateTime(dsUpdateMode.Tables[7].Rows[0]["Updatedate"]).ToString("dd-MMM-yyyy");
                txtlastupdatedby.Text = dsUpdateMode.Tables[7].Rows[0]["UserName"].ToString();
                if (dsUpdateMode.Tables[7].Rows[0]["PaperVersion"] != DBNull.Value)
                    txtpaperversion.Text = dsUpdateMode.Tables[7].Rows[0]["PaperVersion"].ToString();
                if (dsUpdateMode.Tables[7].Rows[0]["VersionDate"] != DBNull.Value)
                    dtformrel.Text = Convert.ToDateTime(dsUpdateMode.Tables[7].Rows[0]["VersionDate"]).ToString("dd-MMM-yyyy");
                txtvernumber.Enabled = false;
                txtlastupdatedate.Enabled = false;
                txtlastupdatedby.Enabled = false;
            }

            clone_dsUpdateMode.Dispose();
            clone_dsUpdateMode = null;



#region Add Field
            ///////////////////*************************************
            //////////////////////lnk_froms
            ////////////////////if (dsUpdateMode.Tables[2].Rows.Count > 0)
            ////////////////////{
            ////////////////////    int iDsRow = 0;
            ////////////////////    foreach (Control ctrlControls in pnlMainPanel.Controls)
            ////////////////////    {
            ////////////////////        if (ctrlControls is Panel && ctrlControls.Visible != false)
            ////////////////////        {
            ////////////////////            //for loop again to fetch datagrid details
            ////////////////////            foreach (Control ctrl in ctrlControls.Controls)
            ////////////////////            {
            ////////////////////                if (ctrl is DataGridView)
            ////////////////////                {
            ////////////////////                    DataGridView dgwGridData = ctrl as DataGridView;
            ////////////////////                    // clsCommon.CreateGridColumnsForFormBuilder(dgwGridData, dsFieldDetails);
            ////////////////////                    //dgwGridData.DataSource = dsUpdateMode.Tables[2];
            ////////////////////                    DataTable dtLnk_Form = new DataTable();
            ////////////////////                    dtLnk_Form = clsCommon.CreateTableLnk_Forms();
            ////////////////////                    if (dsUpdateMode.Tables[2].Rows.Count > 0)
            ////////////////////                    {

            ////////////////////                        foreach (DataRow drFields in dsUpdateMode.Tables[2].Rows)
            ////////////////////                        {
            ////////////////////                            if (dsUpdateMode.Tables[1].Rows[iDsRow]["SectionId"].ToString() == drFields["SectionId"].ToString())
            ////////////////////                            {
            ////////////////////                                DataRow row;
            ////////////////////                                row = dtLnk_Form.NewRow();
            ////////////////////                                row["id"] = Convert.ToInt32(drFields["ID"].ToString());
            ////////////////////                                row["FeatureId"] = drFields["featureId"].ToString();
            ////////////////////                                row["SectionId"] = drFields["SectionId"].ToString();
            ////////////////////                                //row["ControlId"] = clsCommon.FetchSearchValue("Id", drFields["FieldId"].ToString(), dsFieldDetails.Tables[0]).Rows[0]["ControlId"];
            ////////////////////                                row["FieldId"] = Convert.ToInt32(drFields["FieldId"].ToString());
            ////////////////////                                row["FieldLabel"] = drFields["FieldLabel"].ToString();
            ////////////////////                                row["UserId"] = drFields["UserId"].ToString();
            ////////////////////                                row["Predefined"] = drFields["Predefined"].ToString();
            ////////////////////                                row["InsertUpdateStatus"] = PMTCTConstants.strUpdate;
            ////////////////////                                dtLnk_Form.Rows.Add(row);
            ////////////////////                            }

            ////////////////////                        }
            ////////////////////                        iDsRow += 1;
            ////////////////////                        //enter data in grid

            ////////////////////                        foreach (DataRow drFields in dtLnk_Form.Rows)
            ////////////////////                        {
            ////////////////////                            DataSet dsSetFieldDetail = dsFieldDetails.Copy();
            ////////////////////                            DataView dvFilteredRow = new DataView();
            ////////////////////                            dvFilteredRow = dsSetFieldDetail.Tables[0].DefaultView;
            ////////////////////                            DataTable dtRow = new DataTable();

            ////////////////////                            if (drFields["FieldId"].ToString().Contains("7100") == true)
            ////////////////////                                dvFilteredRow.RowFilter = "ID='71' and predefine=" + drFields["Predefined"].ToString();
            ////////////////////                            else
            ////////////////////                                dvFilteredRow.RowFilter = "ID='" + drFields["FieldId"].ToString() + "' and predefine=" + drFields["Predefined"].ToString();

            ////////////////////                            dtRow = dvFilteredRow.ToTable();
            ////////////////////                            if (dtRow.Rows.Count > 0)
            ////////////////////                            {
            ////////////////////                                int iBRValExist, iLstValExist;
            ////////////////////                                String[] strSearchCol = new string[2];
            ////////////////////                                String[] strSearchColVal = new string[2];

            ////////////////////                                strSearchCol[0] = "FieldId";
            ////////////////////                                strSearchCol[1] = "Predefined";
            ////////////////////                                strSearchColVal[0] = drFields["FieldId"].ToString();
            ////////////////////                                strSearchColVal[1] = drFields["Predefined"].ToString();
            ////////////////////                                // check for business rule
            ////////////////////                                if (clsCommon.FetchSearchValue(strSearchCol, strSearchColVal, dsSetFieldDetail.Tables[4]))
            ////////////////////                                {
            ////////////////////                                    iBRValExist = 1;
            ////////////////////                                }
            ////////////////////                                else
            ////////////////////                                {
            ////////////////////                                    iBRValExist = 0;
            ////////////////////                                }

            ////////////////////                                // check for List
            ////////////////////                                if (clsCommon.FetchSearchValue(strSearchCol, strSearchColVal, dsSetFieldDetail.Tables[2]))
            ////////////////////                                {
            ////////////////////                                    iLstValExist = 1;
            ////////////////////                                }
            ////////////////////                                else
            ////////////////////                                {
            ////////////////////                                    iLstValExist = 0;
            ////////////////////                                }


            ////////////////////                                dgwGridData.Rows.Add(dtRow.Rows[0]["FieldName"].ToString(), drFields["FieldLabel"].ToString(), dtRow.Rows[0]["Name"].ToString(), iLstValExist, iBRValExist, drFields["Id"].ToString(), "", drFields["Predefined"].ToString(), drFields["FieldId"].ToString());
            ////////////////////                            }
            ////////////////////                        }
            ////////////////////                    }
            ////////////////////                }//endif of datagridview check
            ////////////////////            }
            ////////////////////        }//endif of panel control check

            ////////////////////    }//end of foreach panel
            ////////////////////  /////////////////////////////////////  pnlMainPanel.AutoScrollPosition = new Point(0, 0);
            ////////////////////}
#endregion
            ///////////////////**************************************
        }

        public DataTable GetSectionTabFeatureBYTabID(DataSet ds,int tabid ,int tabbleNo)
        {
            // var context = new ConnectDataContext(Properties.Settings.Default.ConnectConnectionString);
            var myQuery = (from sec in ds.Tables[tabbleNo].AsEnumerable()
                           join lnk  in ds.Tables[4].AsEnumerable()
                               on sec.Field<int>("SectionID") equals
                               lnk.Field<int>("SectionID")
                           join tab in ds.Tables[3].AsEnumerable()
                               on lnk.Field<int>("TabID") equals
                               tab.Field<int>("TabID") where  tab.Field<int>("TabID") == tabid
   
                           select sec);

            DataTable dataTable = myQuery.CopyToDataTable();

            return dataTable;
        } 

        private UserCtrlFormBuilder CreateTab(string tabName)
        {
           
            TabPage page = new TabPage(tabName);
            if (dsUpdateMode.Tables[3].Rows.Count >= (tabID + 1))
                {
                    page.Name = "Tab_" + tabID + "_" + dsUpdateMode.Tables[3].Rows[tabID]["TabID"].ToString();
                }

            //page.Dock = DockStyle.Fill;
            tabFormBuilder.TabPages.Add(page);
            UserCtrlFormBuilder ctrl = new UserCtrlFormBuilder();
            ctrl.Name = "UserCtrl_" + tabID;
            ctrl.propParentForm = this;
            ctrl.propFormName = txtFormName.Text;
            ctrl.propTechAreaID = Convert.ToInt32(cmbTechArea.SelectedValue);
            ctrl.propSingleVisit = GblIQCare.blnSingleVisit;
            ctrl.propMultipleVisit = GblIQCare.blnMultivisit;
            ctrl.dsFieldDetails = dsFieldDetails;
            ctrl.Dock = DockStyle.Fill;
            ctrl.Tag = "pnlSubPanel";
            theStyle.setStyle(ctrl);
            tabFormBuilder.TabPages[tabID].Controls.Add(ctrl);
            tabID += 1;
            return ctrl;
        }

        private void txtFormName_KeyPress(object sender, KeyPressEventArgs e)
        {
            String strVal = e.KeyChar.ToString();
            string strSearch = "-=\\/!@#$%^*()+|.,<>?`~\";:'[]{}";
            if (strSearch.IndexOf(strVal) >= 0)
            {
                e.Handled = true;
            }

        }

        private void cmbTechArea_SelectionChangeCommitted(object sender, EventArgs e)
        {
            GblIQCare.RefillDdlFields = 1;
            frmFormBuilder_Load(sender, e);
        }
        
        
        private void btnAddTab_Click(object sender, EventArgs e)
        {
            if (ValidationCheck())
            {
                return;
            }
            TabPage page = new TabPage("Tab" + tabID);
            page.Name = "Tab_" + tabID + "_0";
            tabFormBuilder.TabPages.Add(page);
            UserCtrlFormBuilder ctrl = new UserCtrlFormBuilder();
            ctrl.propParentForm = this;
            ctrl.propFormName = txtFormName.Text;
            ctrl.propTechAreaID = Convert.ToInt32(cmbTechArea.SelectedValue);
            ctrl.propSingleVisit = GblIQCare.blnSingleVisit;
            ctrl.propMultipleVisit = GblIQCare.blnMultivisit;
            ctrl.dsFieldDetails = dsFieldDetails;
            ctrl.Tag = "pnlSubPanel";
            theStyle.setStyle(ctrl);
            tabFormBuilder.TabPages[tabFormBuilder.TabPages.Count-1].Controls.Add(ctrl);
            tabID += 1;

        }

        public bool ValidationCheck()
        {
            bool ret =false;
             if (txtFormName.Text.ToString().Trim() == "")
            {
                IQCareWindowMsgBox.ShowWindow("PMTCTFormNameMandatory", this);
                txtFormName.Focus();
                ret = true;
            }
            if (cmbTechArea.SelectedIndex == 0)
            {
                IQCareWindowMsgBox.ShowWindow("SelectTechnicalArea", this);
                cmbTechArea.Focus();
                ret = true;
                
            }
            return ret;
        }
        public  DataTable CreateTabTable_Forms()
        {

            DataTable dtTab = new DataTable();
            DataColumn dtDataColumn;

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "TabID"; //FieldId
            dtTab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.String");
            dtDataColumn.ColumnName = "TabName";
            dtTab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "FeatureID";
            dtTab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "UserId";
            dtTab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "DeleteFlag";
            dtTab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "seq";
            dtTab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.String");
            dtDataColumn.ColumnName = "InsertUpdateStatus";
            dtTab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "Signature";
            dtTab.Columns.Add(dtDataColumn);
            return dtTab;
        }

        public DataTable CreateLnk_FormTabSection()
        {

            DataTable dtLnk_Tab = new DataTable();
            DataColumn dtDataColumn;

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "ID"; //FieldId
            dtLnk_Tab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "TabID"; //FieldId
            dtLnk_Tab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "SectionID";
            dtLnk_Tab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "FeatureID";
            dtLnk_Tab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "UserId";
            dtLnk_Tab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.String");
            dtDataColumn.ColumnName = "InsertUpdateStatus";
            dtLnk_Tab.Columns.Add(dtDataColumn);

            dtDataColumn = new DataColumn();
            dtDataColumn.DataType = Type.GetType("System.Int32");
            dtDataColumn.ColumnName = "DeleteFlag";
            dtLnk_Tab.Columns.Add(dtDataColumn);

            return dtLnk_Tab;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //btnSave.Enabled = false;
            bool blnSaveData = true;
            bool blnDupForm = false;
            dsCollectDataToSave.Tables.Clear();
            if (dtformrel.Text.ToString().Trim() == "")
            {
                IQCareWindowMsgBox.ShowWindow("Please Select the Form Release Date", "!", "", this);
                dtformrel.Focus();
                blnSaveData = false;
            }


            //set mst_feature value for form
            if (txtFormName.Text.ToString().Trim() == "")
            {
                IQCareWindowMsgBox.ShowWindow("PMTCTFormNameMandatory", this);
                txtFormName.Focus();
                blnSaveData = false;
            }
            else if (cmbTechArea.SelectedIndex == 0)
            {
                IQCareWindowMsgBox.ShowWindow("SelectTechnicalArea", this);
                cmbTechArea.Focus();
                blnSaveData = false;
            }
            
            else
            {
                int IModuleId = Convert.ToInt32(cmbTechArea.SelectedValue);
                IFormBuilder objFormBuilder;
                objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                if (GblIQCare.iFormBuilderId <= 0)
                {
                    blnDupForm = objFormBuilder.CheckDuplicate(PMTCTConstants.strMstFeature, PMTCTConstants.strColMstFeatureName, txtFormName.Text.ToString().Trim(), "1", IModuleId);
                }
                else
                {
                    blnDupForm = objFormBuilder.CheckDuplicate(PMTCTConstants.strMstFeature, PMTCTConstants.strColMstFeatureName, txtFormName.Text.ToString().Trim(), PMTCTConstants.strColMstFeatureId, GblIQCare.iFormBuilderId.ToString(), "1", IModuleId);
                }
                if (blnDupForm == true && blnSaveData != false)
                {
                    //duplicate feature name error msg
                    IQCareWindowMsgBox.ShowWindow("PMTCTDupFormName", this);
                    txtFormName.Focus();
                    blnSaveData = false;
                }
            }
            DataTable dtMstFeature = new DataTable();
            dtMstFeature = clsCommon.CreateTableMstFeature();
            DataRow row;
            int iFormId;
            int iSectionId = 0;

            row = dtMstFeature.NewRow();
            if (GblIQCare.iFormBuilderId != 0)
            {
                iFormId = GblIQCare.iFormBuilderId;
                row["FeatureId"] = iFormId;
                row["InsertUpdateStatus"] = PMTCTConstants.strUpdate;
            }
            else
            {
                //fetch max form id
                IFormBuilder objFormBuilder;
                objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                iFormId = objFormBuilder.RetrieveMaxId(PMTCTConstants.strMstFeature);
                iFormId += 1;
                row["InsertUpdateStatus"] = PMTCTConstants.strInsert;

            }
            row["FeatureId"] = iFormId;
            row["FeatureName"] = txtFormName.Text.ToString().Trim();
            row["ReportFlag"] = 0;
            row["DeleteFlag"] = 0;
            row["AdminFlag"] = 0;
            row["UserId"] = GblIQCare.AppUserId;
            row["CountryId"] = GblIQCare.AppCountryId;
            row["SystemId"] = 1;
            row["ModuleId"] = cmbTechArea.SelectedValue; //PMTCTConstants.iPMTCT_MODULE_ID;
            //if (GblIQCare.dtformBusinessValues.Rows.Count > 0)
            //{
                row["MultiVisit"] = (GblIQCare.blnSingleVisit) ? 0 : 1;
            //}
            //else
            //{
            //    row["MultiVisit"] = 0;
            //}

            dtMstFeature.Rows.Add(row);

            //set lnk_form & mst_section values
            DataTable dtLnk_Form = new DataTable();
            DataTable dtMstSection = new DataTable();

            DataTable dtTab = new DataTable();
            DataTable dtLnk_Sectiontab = new DataTable();
            dtTab = CreateTabTable_Forms();
            dtLnk_Sectiontab = CreateLnk_FormTabSection();
            DataRow rowTab;
            DataRow Lnk_Sectiontab;

            dtLnk_Form = clsCommon.CreateTableLnk_Forms();
            dtMstSection = clsCommon.CreateTableMstSection();

            DataRow rowLnk_Form;
            DataRow rowMstSection;
            int iSectionChkId = 0;

            int iSectionRowCounter = 0;
          

            int iTabSequence = 1;
            int iTabID = 0;
            foreach (Control tabctrl  in tabFormBuilder.Controls)
            {
                if (tabctrl.GetType().ToString() == "System.Windows.Forms.TabPage")
                {
                    foreach (UserControl userctrl in tabctrl.Controls)
                    {
                        foreach (Control ctrlpnlsection in userctrl.Controls)
                        {
                            if ((ctrlpnlsection.GetType().ToString() == "System.Windows.Forms.Panel") && ctrlpnlsection.Name == "pnlSectionBuilder")
                            {

                                foreach (Control ctrlpnlmainsection in ctrlpnlsection.Controls)
                                {
                                    if ((ctrlpnlmainsection.GetType().ToString() == "System.Windows.Forms.Panel") && ctrlpnlmainsection.Name.Contains("pnlMainPanel"))
                                    {
                                        int iTabSequenceReset = 0;
                                       // string[] strTabNameSplit;
                                        //strTabNameSplit = tabctrl.Name.Split('_');
                                        //if (strTabNameSplit.Length > 2)
                                        //    iTabSequenceReset = System.Convert.ToInt32(strTabNameSplit[2]);
                                        int iSecSequence = 1;
                                        int iTabIdInUpdateMode = 0;
                                        rowTab = dtTab.NewRow();
                                        //sectionid in update mode
                                        if (GblIQCare.iFormBuilderId != 0)
                                        {
                                           iTabIdInUpdateMode = System.Convert.ToInt32(tabctrl.Name.Split('_')[2]);

                                            if (iTabIdInUpdateMode > 0)
                                            {
                                                rowTab["TabId"] = iTabIdInUpdateMode;
                                            }
                                            else if (iTabID > 0)
                                            {
                                                iTabID += 1;
                                            }
                                            else
                                            {
                                                IFormBuilder objFormBuilder;
                                                objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                                                iTabID = objFormBuilder.RetrieveMaxId(PMTCTConstants.strMstTab);
                                                iTabID += 1;
                                            }

                                            rowTab["InsertUpdateStatus"] = PMTCTConstants.strUpdate;

                                        }
                                        else //Tab id in insert mode
                                        {
                                            if (iTabID > 0) //if max section already set, then increment section one by one in insert mode
                                                iTabID += 1;
                                            else
                                            {
                                                IFormBuilder objFormBuilder;
                                                objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                                                iTabID = objFormBuilder.RetrieveMaxId(PMTCTConstants.strMstTab);
                                                iTabID += 1;
                                                //set sectionid in insert mode
                                                rowTab["InsertUpdateStatus"] = PMTCTConstants.strInsert;
                                            }
                                        }
                                        if (iTabIdInUpdateMode > 0)
                                            rowTab["TabID"] = iTabIdInUpdateMode;
                                        else
                                            rowTab["TabID"] = iTabID;

                                        rowTab["TabName"] = tabctrl.Text.ToString().Trim();   /// ctrl.Text.ToString().Trim();
                                        if (iTabSequenceReset > 0)
                                            rowTab["seq"] = iTabSequenceReset;
                                        else
                                            rowTab["seq"] = iTabSequence;

                                        rowTab["DeleteFlag"] = 0;
                                        rowTab["UserId"] = GblIQCare.AppUserId;
                                        rowTab["FeatureId"] = iFormId;
                                        if (GblIQCare.blnSignatureOneachpage)
                                            rowTab["Signature"] = 1;
                                        else rowTab["Signature"] = 0;
                                        dtTab.Rows.Add(rowTab);
                                        iTabSequence += 1;


                                        //////////////////////////////Tab Name
                                      

                                        #region sectionpanel save

                                        foreach (Control ctrlControls in ctrlpnlmainsection.Controls)
                                        {
                                          //  if (ctrlControls is Panel && ctrlControls.Visible != false)
                                            if (ctrlControls is Panel && ctrlControls.Name.Contains("pnlDynamicPanel"))
                                            {

                                                if (ctrlControls.Tag.ToString() != "deleted")
                                                {

                                                    //for each panel execute code
                                                    foreach (Control ctrl in ctrlControls.Controls)
                                                    {
                                                        //section name
                                                        if (ctrl is TextBox)
                                                        {
                                                            if (ctrl.Text.ToString().Trim() != "")
                                                            {
                                                                int iSecSequenceReset = 0;
                                                                string[] strNameSplit;
                                                                strNameSplit = ctrlControls.Name.Split('_');
                                                                if (strNameSplit.Length > 2)
                                                                    iSecSequenceReset = System.Convert.ToInt32(strNameSplit[2]);

                                                                int iSecIdInUpdateMode = 0;
                                                                rowMstSection = dtMstSection.NewRow();
                                                                //sectionid in update mode
                                                                if (GblIQCare.iFormBuilderId != 0)
                                                                {
                                                                    iSecIdInUpdateMode = System.Convert.ToInt32(ctrl.Name.Split('_')[2]);

                                                                    if (iSecIdInUpdateMode > 0)
                                                                    {
                                                                        rowMstSection["SectionId"] = iSecIdInUpdateMode;
                                                                    }
                                                                    else if (iSectionId > 0)
                                                                    {
                                                                        iSectionId += 1;
                                                                    }
                                                                    else
                                                                    {
                                                                        IFormBuilder objFormBuilder;
                                                                        objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                                                                        iSectionId = objFormBuilder.RetrieveMaxId(PMTCTConstants.strMstSection);
                                                                        iSectionId += 1;
                                                                    }

                                                                    rowMstSection["InsertUpdateStatus"] = PMTCTConstants.strUpdate;

                                                                }
                                                                else //section id in insert mode
                                                                {
                                                                    if (iSectionId > 0) //if max section already set, then increment section one by one in insert mode
                                                                        iSectionId += 1;
                                                                    else
                                                                    {
                                                                        IFormBuilder objFormBuilder;
                                                                        objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                                                                        iSectionId = objFormBuilder.RetrieveMaxId(PMTCTConstants.strMstSection);
                                                                        iSectionId += 1;
                                                                        //set sectionid in insert mode
                                                                        rowMstSection["InsertUpdateStatus"] = PMTCTConstants.strInsert;
                                                                    }
                                                                }
                                                                if (iSecIdInUpdateMode > 0)
                                                                    rowMstSection["SectionId"] = iSecIdInUpdateMode;
                                                                else
                                                                    rowMstSection["SectionId"] = iSectionId;

                                                                rowMstSection["SectionName"] = ctrl.Text.ToString().Trim();
                                                                if (iSecSequenceReset > 0)
                                                                    rowMstSection["Sequence"] = iSecSequenceReset;
                                                                else
                                                                    rowMstSection["Sequence"] = iSecSequence;

                                                                rowMstSection["CustomFlag"] = 0;
                                                                rowMstSection["DeleteFlag"] = (ctrlControls.Tag.ToString() == "deleted") ? "1" : "0";
                                                                rowMstSection["UserId"] = GblIQCare.AppUserId;
                                                                rowMstSection["FeatureId"] = iFormId;
                                                                dtMstSection.Rows.Add(rowMstSection);

                                                                iSecSequence += 1;


                                                                rowLnk_Form = dtLnk_Sectiontab.NewRow();

                                                                //  
                                                                if (iTabIdInUpdateMode > 0)
                                                                    rowLnk_Form["TabID"] = iTabIdInUpdateMode;
                                                                else
                                                                    rowLnk_Form["TabID"] = iTabID;


                                                                if (iSecIdInUpdateMode > 0)
                                                                    rowLnk_Form["SectionID"] = iSecIdInUpdateMode;
                                                                else
                                                                    rowLnk_Form["SectionID"] = iSectionId;

                                                                rowLnk_Form["UserId"] = GblIQCare.AppUserId;
                                                                rowLnk_Form["FeatureId"] = iFormId;
                                                                rowLnk_Form["DeleteFlag"] = 0;
                                                                if (GblIQCare.iFormBuilderId != 0)
                                                                {
                                                                    rowLnk_Form["InsertUpdateStatus"] = PMTCTConstants.strUpdate;
                                                                    // rowLnk_Form["ID"]

                                                                }
                                                                else
                                                                {
                                                                    rowLnk_Form["InsertUpdateStatus"] = PMTCTConstants.strInsert;
                                                                    rowLnk_Form["ID"] = 0;
                                                                }
                                                                dtLnk_Sectiontab.Rows.Add(rowLnk_Form);




                                                            }
                                                            else if (blnSaveData != false) //if section empty,show error,but error status shud not be already false
                                                            {
                                                                IQCareWindowMsgBox.ShowWindow("PMTCTSectionNameMandatory", this);
                                                                ctrl.Focus();
                                                                blnSaveData = false;
                                                            }
                                                        } //End of textbox control

                                                    }//end of foreach inside panel control


                                                    foreach (Control ctrl in ctrlControls.Controls)
                                                    {

                                                        if (ctrl is CheckBox)
                                                        {

                                                            int ichkID = 0;
                                                            int iSecIdInUpdateModeChk = 0;
                                                            //sectionid in update mode
                                                            if (GblIQCare.iFormBuilderId != 0)
                                                            {
                                                                iSecIdInUpdateModeChk = System.Convert.ToInt32(ctrl.Name.Split('_')[2]);
                                                                //iSecIdInUpdateModeChk = System.Convert.ToInt32(ctrl.Name.Split('_')[1]);
                                                                if (iSecIdInUpdateModeChk > 0)
                                                                {
                                                                    ichkID = iSecIdInUpdateModeChk;
                                                                }
                                                                else if (iSectionChkId > 0)
                                                                {
                                                                    iSectionChkId += 1;
                                                                }
                                                                else
                                                                {
                                                                    IFormBuilder objFormBuilder;
                                                                    objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                                                                    iSectionChkId = objFormBuilder.RetrieveMaxId(PMTCTConstants.strMstSection);
                                                                    iSectionChkId += 1;
                                                                }
                                                            }
                                                            else //section id in insert mode
                                                            {
                                                                if (iSectionChkId > 0) //if max section already set, then increment section one by one in insert mode
                                                                    iSectionChkId += 1;
                                                                else
                                                                {
                                                                    IFormBuilder objFormBuilder;
                                                                    objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                                                                    iSectionChkId = objFormBuilder.RetrieveMaxId(PMTCTConstants.strMstSection);
                                                                    iSectionChkId += 1;
                                                                }
                                                            }
                                                            if (iSecIdInUpdateModeChk > 0)
                                                                ichkID = iSecIdInUpdateModeChk;
                                                            else
                                                                ichkID = iSectionChkId;

                                                            DataRow[] result = dtMstSection.Select("SectionId=" + ichkID.ToString());
                                                            foreach (DataRow rowresult in result)
                                                            {
                                                                rowresult["IsgridView"] = ((CheckBox)ctrl).Checked == true ? "1" : "0";
                                                            }
                                                            dtMstSection.AcceptChanges();
                                                            //dtMstSection.Rows[0]["IsgridView"] = ((CheckBox)ctrl).Checked == true ? "1" : "0";
                                                            //dtMstSection.AcceptChanges();


                                                        }

                                                    }



                                                    int iSeq = 1;
                                                    //for loop again to fetch datagrid details
                                                    foreach (Control ctrl in ctrlControls.Controls)
                                                    {

                                                        if (ctrl is DataGridView)
                                                        {
                                                            DataGridView dgwGridData = ctrl as DataGridView;
                                                            if (dgwGridData.Rows.Count > 0 && dgwGridData.Rows[0].Cells[0].Value != null)
                                                            {
                                                                //foreach (DataGridView dgwGridData in ctrl)
                                                                for (int iGridRow = 0; iGridRow < dgwGridData.Rows.Count; iGridRow++)
                                                                {

                                                                    rowLnk_Form = dtLnk_Form.NewRow();
                                                                    if (dgwGridData.Rows[iGridRow].Cells[0].Value != null && blnSaveData != false)
                                                                    {
                                                                        if (dgwGridData.Rows[iGridRow].Cells["Field Label"].Value != null && dgwGridData.Rows[iGridRow].Cells["Field Label"].Value.ToString().Trim() != "")
                                                                        {
                                                                            //update mode of field
                                                                            if (GblIQCare.iFormBuilderId != 0 && dgwGridData.Rows[iGridRow].Cells["ID"].Value != null && dgwGridData.Rows[iGridRow].Cells["ID"].Value.ToString() != "")
                                                                            {
                                                                                rowLnk_Form["InsertUpdateStatus"] = PMTCTConstants.strUpdate;
                                                                                // rowLnk_Form["Id"] = dgwGridData.Rows[iGridRow].Cells["ID"].Value;
                                                                            }
                                                                            else //insert mode
                                                                            {
                                                                                rowLnk_Form["InsertUpdateStatus"] = PMTCTConstants.strInsert;
                                                                            }

                                                                            //update mode
                                                                            if (GblIQCare.iFormBuilderId > 0)
                                                                            {
                                                                                ////DataSet dsSetFieldDetail = dsFieldDetails.Copy();
                                                                                ////DataView dvFilteredRow = new DataView();
                                                                                ////dvFilteredRow = dsSetFieldDetail.Tables[0].DefaultView;
                                                                                ////DataTable dtRow = new DataTable();

                                                                                ////dvFilteredRow.RowFilter = "FieldName='" + dgwGridData.Rows[iGridRow].Cells["Table Field Name"].Value + "'";
                                                                                ////dtRow = dvFilteredRow.ToTable();
                                                                                if (dgwGridData.Rows[iGridRow].Cells["Id"].Value == null)
                                                                                    rowLnk_Form["Id"] = "0";
                                                                                else
                                                                                    rowLnk_Form["Id"] = dgwGridData.Rows[iGridRow].Cells["Id"].Value;
                                                                                //rowLnk_Form["FieldId"] = dgwGridData.Rows[iGridRow].Cells["FieldId"].Value;
                                                                                if (dgwGridData.Rows[iGridRow].Cells["FieldId"].Value.ToString().StartsWith("999971") == true)
                                                                                {
                                                                                    rowLnk_Form["FieldId"] = dgwGridData.Rows[iGridRow].Cells["FieldId"].Value.ToString().Replace("9999", "");
                                                                                }
                                                                                else
                                                                                    rowLnk_Form["FieldId"] = dgwGridData.Rows[iGridRow].Cells["FieldId"].Value;

                                                                            }
                                                                            else //insert mode
                                                                            {
                                                                                rowLnk_Form["FieldId"] = dgwGridData.Rows[iGridRow].Cells["FieldId"].Value.ToString().Trim();
                                                                            }
                                                                            rowLnk_Form["FeatureId"] = iFormId;
                                                                            rowLnk_Form["SectionId"] = dtMstSection.Rows[iSectionRowCounter]["SectionId"];
                                                                            rowLnk_Form["FieldLabel"] = dgwGridData.Rows[iGridRow].Cells["Field Label"].Value.ToString().Trim();
                                                                            rowLnk_Form["AdditionalInformation"] = dgwGridData.Rows[iGridRow].Cells["Additional Information"].Value == null ? "" : dgwGridData.Rows[iGridRow].Cells["Additional Information"].Value.ToString().Trim();
                                                                            rowLnk_Form["Sequence"] = iSeq;
                                                                            rowLnk_Form["Predefined"] = dgwGridData.Rows[iGridRow].Cells["Predefined"].Value;
                                                                            rowLnk_Form["UserId"] = GblIQCare.AppUserId;
                                                                            dtLnk_Form.Rows.Add(rowLnk_Form);


                                                                            iSeq += 1;
                                                                        }
                                                                        else if (blnSaveData != false) // error status shud not be false
                                                                        {
                                                                            //error message for field name not entered.
                                                                            IQCareWindowMsgBox.ShowWindow("PMTCTFieldLabelMandatory", this);
                                                                            //set focus on field label
                                                                            blnSaveData = false;
                                                                        }
                                                                    }

                                                                }//end of foreach of datagrid 
                                                                //first grid data goes in first section, and so on.
                                                            }//if field exist in section i.e. dgwGridData.rows.count>0
                                                            else if (blnSaveData != false) //else if field doesnot exist in section then show error message
                                                            {
                                                                IQCareWindowMsgBox.ShowWindow("PMTCTNoFieldCreated", this);
                                                                blnSaveData = false;
                                                            }
                                                            iSectionRowCounter += 1;
                                                        }//endif of datagridview check
                                                    }
                                                }
                                            }//endif of panel control check

                                        }//end of foreach panel

                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (dtTab.Rows.Count > 0)
            {

                var duplicates = dtTab.AsEnumerable().GroupBy(r => new { TabName= r[1] }).Where(gr => gr.Count() > 1).ToList();
                if (duplicates.Count > 0)
                {
                    String dup = "";
                    for (int i = 0; i < duplicates.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(dup))
                        {
                            dup += " , ";
                        }
                        dup += duplicates[i].Key.TabName;
                    }
                    MsgBuilder theBuilder = new MsgBuilder();
                    theBuilder.DataElements["Control"] = dup;
                    IQCareWindowMsgBox.ShowWindow("PMTCTDupTab", theBuilder, this);
                    blnSaveData = false;
                }
                if ((dtMstSection.Rows.Count > 0) && blnSaveData && (dtLnk_Sectiontab.Rows.Count>0))
                {
                   
                    for(int i= 0 ; i<dtTab.Rows.Count;i++)
                    {
                     int tabSectionlnkcount = 0;
                     var myQuery = (from tab in dtLnk_Sectiontab.AsEnumerable()
                                     join sec in  dtMstSection.AsEnumerable()
                                      on tab.Field<int>("SectionID") equals sec.Field<int>("SectionID")
                                    where tab.Field<int>("TabId") == Convert.ToInt32(dtTab.Rows[i]["TabId"]) & sec.Field<int>("deleteflag")==0
                                    select tab);


                   tabSectionlnkcount = myQuery.Count();
                   if (tabSectionlnkcount == 0)
                     {
                      SelectTab(dtTab.Rows[i]["TabName"].ToString());
                      blnSaveData = false;
                      break;                     
                     }

                     //if (blnSaveData)
                     //{
                     //    if (dtDeleteSections != null)
                     //    {
                     //        if (dtDeleteSections.Rows.Count > 0)
                     //        {
                     //            int deleteSectioninTabcount = 0;
                     //            var theDeletesection = (from tab in dtLnk_Sectiontab.AsEnumerable()
                     //                                    join sec in dtMstSection.AsEnumerable()
                     //                                     on tab.Field<int>("SectionID") equals sec.Field<int>("SectionID")
                     //                                    join deletesection in dtDeleteSections.AsEnumerable()
                     //                                     on tab.Field<int>("SectionID") equals deletesection.Field<int>("SectionID")
                     //                                    where tab.Field<int>("TabId") == Convert.ToInt32(dtTab.Rows[i]["TabId"])

                     //                                    select tab);
                     //            deleteSectioninTabcount = theDeletesection.Count();
                     //            if (deleteSectioninTabcount > 0)
                     //            {
                     //                if ((tabSectionlnkcount - deleteSectioninTabcount) == 0)
                     //                {
                     //                    SelectTab(dtTab.Rows[i]["TabName"].ToString());
                     //                    blnSaveData = false;
                     //                    break;
                     //                }
                     //            }
                     //        }
                     //    }
                     //}

                    }
                }
            }

            if (dtMstSection.Rows.Count > 0)
            {
                //check for duplicate section name
                //section name cannot be duplicate on one form. one section name can be same on two different forms.
                for (int iDupSec = 0; iDupSec < dtMstSection.Rows.Count; iDupSec++)
                {
                    for (int x = iDupSec + 1; x < dtMstSection.Rows.Count; x++)
                    {
                        if (dtMstSection.Rows[iDupSec]["SectionName"].ToString() == dtMstSection.Rows[x]["SectionName"].ToString() && blnSaveData != false)
                        {
                            //pass message builder message
                            MsgBuilder theBuilder = new MsgBuilder();
                            theBuilder.DataElements["Control"] = dtMstSection.Rows[iDupSec]["SectionName"].ToString();
                            IQCareWindowMsgBox.ShowWindow("PMTCTDupSectionName", theBuilder, this);
                            blnSaveData = false;
                            break;
                        }
                    }
                }
                if (blnSaveData)
                {
                    for (int iMstSecGridView = 0; iMstSecGridView < dtMstSection.Rows.Count; iMstSecGridView++)
                    {

                        if (dtMstSection.Rows[iMstSecGridView]["IsGridView"].ToString() == "1")
                        {
                            MsgBuilder theBuilder = new MsgBuilder();
                            theBuilder.DataElements["MessageText"] = "In Section - " + dtMstSection.Rows[iMstSecGridView]["SectionName"].ToString() + " has following error(s)-:";
                            DataView dvFilteredFieldGridView = new DataView();
                            dvFilteredFieldGridView = dtLnk_Form.DefaultView;
                            dvFilteredFieldGridView.RowFilter = "SectionId=" + dtMstSection.Rows[iMstSecGridView]["SectionId"].ToString();
                            if (dvFilteredFieldGridView.Count > 8)
                            {

                                theBuilder.DataElements["MessageText"] += "\r\n Should not be contain more than 8 fields.";
                                //  IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                blnSaveData = false;
                                //break;
                            }

                            for (int x = 0; x < dvFilteredFieldGridView.Count; x++)
                            {
                                if (dvFilteredFieldGridView[x]["Predefined"].ToString() == "1")
                                {

                                    // theBuilder.DataElements["MessageText"] += "\r\n Should be contain only custom field.";
                                    //  IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);

                                    theBuilder.DataElements["MessageText"] += "\r\n Field Label - " + dvFilteredFieldGridView[x]["FieldLabel"].ToString() + " is predefine field.Should be contain only custom field.";
                                    blnSaveData = false;
                                    //break;

                                }
                                DataView dvFiltercondfieldGridView;
                                DataSet dscondfieldGridView = dsFieldDetails.Copy();
                                DataTable theDT;
                                theDT = dscondfieldGridView.Tables[0];
                                dvFiltercondfieldGridView = new DataView(theDT);
                                dvFiltercondfieldGridView.RowFilter = "ConditionalField >0  and Id =" + dvFilteredFieldGridView[x]["FieldId"].ToString();
                                if (dvFiltercondfieldGridView.Count > 0)
                                {

                                    // MsgBuilder theBuilder = new MsgBuilder();
                                    theBuilder.DataElements["MessageText"] += "\r\n Field Label - " + dvFilteredFieldGridView[x]["FieldLabel"].ToString() + " has a conditional field.";
                                    //IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                    blnSaveData = false;
                                    //break;
                                }
                                dvFiltercondfieldGridView.Dispose();
                                theDT = dscondfieldGridView.Tables[0];
                                dvFiltercondfieldGridView = new DataView(theDT);
                                dvFiltercondfieldGridView.RowFilter = "controlID = 9 and Id =" + dvFilteredFieldGridView[x]["FieldId"].ToString();
                                if (dvFiltercondfieldGridView.Count > 0)
                                {

                                    theBuilder.DataElements["MessageText"] += "\r\n Field Label - " + dvFilteredFieldGridView[x]["FieldLabel"].ToString() + " is multi select control.";
                                    //IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                    blnSaveData = false;
                                    //break;
                                }

                                dscondfieldGridView.Dispose();

                                theDT = dscondfieldGridView.Tables[0];
                                dvFiltercondfieldGridView = new DataView(theDT);
                                dvFiltercondfieldGridView.RowFilter = "controlID = 15 and Id =" + dvFilteredFieldGridView[x]["FieldId"].ToString();
                                if (dvFiltercondfieldGridView.Count > 0)
                                {

                                    theBuilder.DataElements["MessageText"] += "\r\n Field Label - " + dvFilteredFieldGridView[x]["FieldLabel"].ToString() + " is Disease/Symptoms control.";
                                    //IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                    blnSaveData = false;
                                    //break;
                                }

                                dscondfieldGridView.Dispose();

                                theDT = dscondfieldGridView.Tables[0];
                                dvFiltercondfieldGridView = new DataView(theDT);
                                dvFiltercondfieldGridView.RowFilter = "controlID = 12 and Id =" + dvFilteredFieldGridView[x]["FieldId"].ToString();
                                if (dvFiltercondfieldGridView.Count > 0)
                                {

                                    theBuilder.DataElements["MessageText"] += "\r\n Field Label - " + dvFilteredFieldGridView[x]["FieldLabel"].ToString() + " is Lab Selection control.";
                                    //IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                    blnSaveData = false;
                                    //break;
                                }

                                dscondfieldGridView.Dispose();

                                theDT = dscondfieldGridView.Tables[0];
                                dvFiltercondfieldGridView = new DataView(theDT);
                                dvFiltercondfieldGridView.RowFilter = "controlID = 11 and Id =" + dvFilteredFieldGridView[x]["FieldId"].ToString();
                                if (dvFiltercondfieldGridView.Count > 0)
                                {

                                    theBuilder.DataElements["MessageText"] += "\r\n Field Label - " + dvFilteredFieldGridView[x]["FieldLabel"].ToString() + " is Drug Selection control.";
                                    //IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                    blnSaveData = false;
                                    //break;
                                }

                                dscondfieldGridView.Dispose();

                                theDT = dscondfieldGridView.Tables[0];
                                dvFiltercondfieldGridView = new DataView(theDT);
                                dvFiltercondfieldGridView.RowFilter = "controlID = 16 and Id =" + dvFilteredFieldGridView[x]["FieldId"].ToString();
                                if (dvFiltercondfieldGridView.Count > 0)
                                {

                                    theBuilder.DataElements["MessageText"] += "\r\n Field Label - " + dvFilteredFieldGridView[x]["FieldLabel"].ToString() + " is ICD10 control.";
                                    //IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                    blnSaveData = false;
                                    //break;
                                }
                                dscondfieldGridView.Dispose();
                                dscondfieldGridView = null;
                            }

                            if (!blnSaveData)
                            {
                                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                break;
                            }

                        }
                    }

                }
            }
            else if (blnSaveData != false)
            {
                //at least one section should exists
                IQCareWindowMsgBox.ShowWindow("PMTCTNoSectionCreated", this);
                blnSaveData = false;
            }

            //check for duplicate field in same form
            for (int iDupFld = 0; iDupFld < dtLnk_Form.Rows.Count; iDupFld++)
            {
                for (int x = iDupFld + 1; x < dtLnk_Form.Rows.Count; x++)
                {
                    //two field id can be same in case of one is custom field and another is predefined field,
                    //so need to check predefined check also while comparing duplicate field
                    if (dtLnk_Form.Rows[iDupFld]["FieldId"].ToString() == dtLnk_Form.Rows[x]["FieldId"].ToString() && dtLnk_Form.Rows[iDupFld]["Predefined"].ToString() == dtLnk_Form.Rows[x]["Predefined"].ToString() && blnSaveData != false)
                    {
                        //pass message builder message
                        //MsgBuilder theBuilder = new MsgBuilder();
                        //theBuilder.DataElements["Control"] = dtLnk_Form.Rows[iDupFld]["FieldId"].ToString();
                        if (dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() != "PlaceHolder")
                        {
                            MsgBuilder theBuilder = new MsgBuilder();
                            //theBuilder.DataElements["MessageText"] = dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() + " field selected twice in the form";
                            theBuilder.DataElements["MessageText"] = "Field Label " + dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() + " and " + dtLnk_Form.Rows[x]["FieldLabel"].ToString() + " belongs to same field.";
                            IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);

                            //IQCareWindowMsgBox.ShowWindow("PMTCTDupFieldName", this);
                            blnSaveData = false;
                            break;
                        }
                    }
                    //duplicate label should not be allowed
                    if (dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() == dtLnk_Form.Rows[x]["FieldLabel"].ToString() && blnSaveData != false)
                    {
                        if (dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() != "PlaceHolder")
                        {
                            IQCareWindowMsgBox.ShowWindow("PMTCTDupFieldLabel", this);
                            blnSaveData = false;
                            break;
                        }
                    }
                }
            }


            // for checking duplicate field in conditional field
            DataSet dscondfield = dsFieldDetails.Copy();
            DataView dvFilteredRowcondfield = new DataView();
            dvFilteredRowcondfield = dscondfield.Tables[5].DefaultView;
            for (int iDupFld = 0; iDupFld < dtLnk_Form.Rows.Count; iDupFld++)
            {
                for (int x = 0; x < dtLnk_Form.Rows.Count; x++)
                {
                    dvFilteredRowcondfield.RowFilter = "ConfieldId=" + dtLnk_Form.Rows[iDupFld]["FieldID"].ToString() + " and FieldId=" + dtLnk_Form.Rows[x]["FieldID"].ToString();
                    if (dvFilteredRowcondfield.ToTable().Rows.Count > 0)
                    {
                        MsgBuilder theBuilder = new MsgBuilder();
                        theBuilder.DataElements["MessageText"] = "Field Label " + dtLnk_Form.Rows[x]["FieldLabel"].ToString() + " is associate with Field Label " + dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() + ".";
                        IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                        blnSaveData = false;
                        break;
                    }

                }
            }

            DataSet dscond = dsFieldDetails.Copy();
            DataSet dscond1 = dsFieldDetails.Copy();
            if (blnSaveData)
            {
                for (int iDupFld = 0; iDupFld < dtLnk_Form.Rows.Count; iDupFld++)
                {
                    DataView dvFilteredRowcondfield1 = new DataView();
                    dvFilteredRowcondfield1 = dscondfield.Tables[5].DefaultView;
                    dvFilteredRowcondfield1.RowFilter = "ConfieldId=" + dtLnk_Form.Rows[iDupFld]["FieldID"].ToString();
                    for (int x = iDupFld + 1; x < dtLnk_Form.Rows.Count; x++)
                    {
                        DataView dvFilteredRowcondfield2 = new DataView();
                        dvFilteredRowcondfield2 = dscond1.Tables[5].DefaultView;

                        dvFilteredRowcondfield2.RowFilter = "ConfieldId=" + dtLnk_Form.Rows[x]["FieldID"].ToString();
                        for (int y = 0; y < dvFilteredRowcondfield1.ToTable().Rows.Count; y++)
                        {
                            for (int z = 0; z < dvFilteredRowcondfield2.ToTable().Rows.Count; z++)
                            {
                                if (dvFilteredRowcondfield1.ToTable().Rows[y]["FieldID"].ToString() == dvFilteredRowcondfield2.ToTable().Rows[z]["FieldID"].ToString())
                                {
                                    MsgBuilder theBuilder = new MsgBuilder();
                                    // theBuilder.DataElements["MessageText"] = "Field Label " + dtLnk_Form.Rows[x]["FieldLabel"].ToString() + " is associate with Field Label " + dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() + ".";
                                    theBuilder.DataElements["MessageText"] = "Field Label " + dtLnk_Form.Rows[iDupFld]["FieldLabel"].ToString() + " and " + dtLnk_Form.Rows[x]["FieldLabel"].ToString() + " has common associate field(s).";
                                    IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
                                    blnSaveData = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            

            //call save method
            if (blnSaveData)
            {
                Decimal vernumber;
                dtMstFeature.TableName = "dtMstFeature";
                dsCollectDataToSave.Tables.Add(dtMstFeature);
                dsCollectDataToSave.Tables.Add(dtMstSection);
                dtLnk_Form.TableName = "dtLnk_Form";
                dsCollectDataToSave.Tables.Add(dtLnk_Form);
                dsCollectDataToSave.Tables.Add(dtDeleteFields);
                if (dtLnk_Form.Rows.Count > 0)
                {
                    if (theDRButton != null)
                    {
                        foreach (DataRow theDRBtn in theDRButton)
                        {
                            foreach (DataRow theDR in dtLnk_Form.Rows)
                            {
                                if (theDRBtn.ItemArray[1].ToString() == theDR.ItemArray[3].ToString())
                                {
                                    IQCareWindowMsgBox.ShowWindow("ButtonOpenForm", this);
                                    blnSaveData = false;
                                    return;
                                }
                            }
                        }
                    }
                }

                if (dtDeleteSections != null)
                {
                   dtDeleteSections.TableName ="DeleteSection";
                    dsCollectDataToSave.Tables.Add(dtDeleteSections);
                }

                dtTab.TableName = "MstTab";
                dsCollectDataToSave.Tables.Add(dtTab);

                dtLnk_Sectiontab.TableName = "LnkSectionTab";
                dsCollectDataToSave.Tables.Add(dtLnk_Sectiontab);

                if (dtDeleteTabs != null)
                {
                    dtDeleteTabs.TableName = "DeleteTab";
                    dsCollectDataToSave.Tables.Add(dtDeleteTabs);
                }
                IFormBuilder objFormBuilder;
                objFormBuilder = (IFormBuilder)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BFormBuilder,BusinessProcess.FormBuilder");
                DataTable dtdatetime = objFormBuilder.SystemDate();
                
                if (GblIQCare.iFormBuilderId < 1)
                {
                    vernumber = Convert.ToDecimal(1.0);
                    DateTime dtchng = DateTime.Parse(dtformrel.Text.Trim()).Add(TimeSpan.Parse(dtdatetime.Rows[0]["Time"].ToString()));
                    DSFormVerTables = new DataSet();
                    dtmstformversion = clsCommon.CreateMstVersionTable();
                    DataRow drfrmver = dtmstformversion.NewRow();
                    drfrmver["VerId"] = 0;
                    drfrmver["VersionName"] = vernumber.ToString();
                    drfrmver["VersionDate"] = dtchng.ToString("dd-MMM-yyyy");
                    drfrmver["UserId"] = GblIQCare.AppUserId;
                    drfrmver["PaperVersion"] = txtpaperversion.Text.Trim();
                    dtmstformversion.Rows.Add(drfrmver);
                    dtTabchanges = clsCommon.ManageVersionTab();
                    dtSectionchanges = clsCommon.ManageVersionSection();
                    dtFieldChanges = clsCommon.ManageVersionField();
                    dtConFieldchanges = clsCommon.ManageVersionConField();
                    DSFormVerTables.Tables.Add(dtmstformversion);
                    DSFormVerTables.Tables.Add(dtTabchanges);
                    DSFormVerTables.Tables.Add(dtSectionchanges);
                    DSFormVerTables.Tables.Add(dtFieldChanges);
                }
                else
                {
                    if (!CheckFormVersion(dsCollectDataToSave))
                    {

                        int result = DateTime.Compare(GblIQCare.FormVersionDate, Convert.ToDateTime(dtformrel.Text).Add(TimeSpan.Parse(dtdatetime.Rows[0]["Time"].ToString())));
                        if ((Convert.ToDecimal(txtvernumber.Text) <= GblIQCare.FormVersionNo) && (result < 0))
                        {
                            DialogResult msgconfirm = IQCareWindowMsgBox.ShowWindow(string.Format("There are field changes on the form.{0} The electronic form version will update upon saving.", Environment.NewLine), "*", "", this);
                            if (msgconfirm == DialogResult.OK)
                            {

                                vernumber = Convert.ToDecimal(txtvernumber.Text);
                                vernumber = vernumber + Convert.ToDecimal(0.1);
                                DateTime dtchng = DateTime.Parse(dtformrel.Text.Trim()).Add(TimeSpan.Parse(dtdatetime.Rows[0]["Time"].ToString()));
                                //DateTime dt1 = DateTime.ParseExact((dtchng.Add(TimeSpan.Parse(dtdatetime.Rows[0]["Time"].ToString()))).ToString(), "dd/MM/yy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                                DSFormVerTables = new DataSet();
                                dtmstformversion = clsCommon.CreateMstVersionTable();
                                DataRow drfrmver = dtmstformversion.NewRow();
                                drfrmver["VerId"] = 0;
                                drfrmver["VersionName"] = vernumber.ToString();
                                drfrmver["VersionDate"] = dtchng.ToString("dd-MMM-yyyy");
                                drfrmver["UserId"] = GblIQCare.AppUserId;
                                drfrmver["PaperVersion"] = txtpaperversion.Text.Trim();
                                dtmstformversion.Rows.Add(drfrmver);
                                DSFormVerTables.Tables.Add(dtmstformversion);
                                DSFormVerTables.Tables.Add(dtTabchanges);
                                DSFormVerTables.Tables.Add(dtSectionchanges);
                                DSFormVerTables.Tables.Add(dtFieldChanges);
                                //return;
                            }
                            else
                                return;
                        }
                        //return;
                    }
                    else
                    {
                        DateTime dtchng = DateTime.Parse(dtformrel.Text.Trim()).Add(TimeSpan.Parse(dtdatetime.Rows[0]["Time"].ToString()));
                        DSFormVerTables = new DataSet();
                        dtmstformversion = clsCommon.CreateMstVersionTable();
                        DataRow drfrmver = dtmstformversion.NewRow();
                        drfrmver["VerId"] = GblIQCare.FormVersionId;
                        drfrmver["VersionName"] = txtvernumber.Text.ToString();
                        drfrmver["VersionDate"] = dtchng.ToString("dd-MMM-yyyy");
                        drfrmver["UserId"] = GblIQCare.AppUserId;
                        drfrmver["PaperVersion"] = txtpaperversion.Text.Trim();
                        dtmstformversion.Rows.Add(drfrmver);
                        dtTabchanges = clsCommon.ManageVersionTab();
                        dtSectionchanges = clsCommon.ManageVersionSection();
                        dtFieldChanges = clsCommon.ManageVersionField();
                        dtConFieldchanges = clsCommon.ManageVersionConField();
                        DSFormVerTables.Tables.Add(dtmstformversion);
                        DSFormVerTables.Tables.Add(dtTabchanges);
                        DSFormVerTables.Tables.Add(dtSectionchanges);
                        DSFormVerTables.Tables.Add(dtFieldChanges);
                    }
                }

               

                int res = objFormBuilder.SaveFormDetail(dsCollectDataToSave, dsFieldDetails.Tables[0], GblIQCare.dtformBusinessValues, DSFormVerTables);
                GblIQCare.iFormBuilderId = 0;
                GblIQCare.FormVersionId = 0;
                

                Form theForm;
                theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmManageForms, IQCare.FormBuilder"));
                theForm.MdiParent = this.MdiParent;
                theForm.Left = 0;
                theForm.Top = 0;
                theForm.Focus();
                theForm.Show();

                this.Close();
            }
            btnSave.Enabled = true;
        }

        private bool CheckFormVersion(DataSet NewData)
        {
            if (dsUpdateMode.Tables.Count > 0)
            {
                var qrySection1 = dsUpdateMode.Tables[1].AsEnumerable().Select(a => new { SectionId = a["SectionId"].ToString() });
                var qrySection2 = NewData.Tables["dtLnk_Form"].AsEnumerable().Select(b => new { SectionId = b["SectionId"].ToString() });

                var exceptAB = qrySection1.Except(qrySection2);
                var exceptBA = qrySection2.Except(qrySection1);
                dtTabchanges = clsCommon.ManageVersionTab();
                dtSectionchanges = clsCommon.ManageVersionSection();
                dtFieldChanges = clsCommon.ManageVersionField();
                dtConFieldchanges = clsCommon.ManageVersionConField();
                DataRow dr;
                foreach (var section in exceptAB)
                {
                    
                        var TabIdsQuery = NewData.Tables["LnkSectionTab"].AsEnumerable()
                                                .Where(c => c["SectionId"].ToString() == section.SectionId)
                                                .Select(c => new
                                                {
                                                    TabId = c["TabId"]

                                                }).FirstOrDefault();
                        if (TabIdsQuery == null)
                        {
                             TabIdsQuery = dsUpdateMode.Tables[4].AsEnumerable()
                                              .Where(c => c["SectionId"].ToString() == section.SectionId)
                                              .Select(c => new
                                              {
                                                  TabId = c["TabId"]

                                              }).FirstOrDefault();
                        dr = dtSectionchanges.NewRow();
                        dr["SectionId"] = section.SectionId;
                        dr["FunctionId"] = 3;
                        dr["TabId"] = TabIdsQuery.TabId;
                        dtSectionchanges.Rows.Add(dr);
                        }
                        else {
                        dr = dtSectionchanges.NewRow();
                        dr["SectionId"] = section.SectionId;
                        dr["FunctionId"] = 3;
                        dr["TabId"] = TabIdsQuery.TabId;
                        dtSectionchanges.Rows.Add(dr);
                        }
                }
                
                foreach (var section in exceptBA)
                {
                    dr = null;
                    var TabIdsQuery = NewData.Tables["LnkSectionTab"].AsEnumerable()
                                                 .Where(c => c["SectionId"].ToString() == section.SectionId)
                                                 .Select(c => new
                                                 {
                                                     TabId = c["TabId"]

                                                 }).FirstOrDefault();
                    if (TabIdsQuery == null)
                    {
                        TabIdsQuery = dsUpdateMode.Tables[4].AsEnumerable()
                                         .Where(c => c["SectionId"].ToString() == section.SectionId)
                                         .Select(c => new
                                         {
                                             TabId = c["TabId"]

                                         }).FirstOrDefault();
                        dr = dtSectionchanges.NewRow();
                        dr["SectionId"] = section.SectionId;
                        dr["FunctionId"] = 3;
                        dr["TabId"] = TabIdsQuery.TabId;
                        dtSectionchanges.Rows.Add(dr);
                    }
                    else
                    {
                        dr = dtSectionchanges.NewRow();
                        dr["SectionId"] = section.SectionId;
                        dr["FunctionId"] = 3;
                        dr["TabId"] = TabIdsQuery.TabId;
                        dtSectionchanges.Rows.Add(dr);
                    }

                }

                var qryTab1 = dsUpdateMode.Tables[3].AsEnumerable().Select(a => new { TabId = a["TabId"].ToString() });
                var qryTab2 = NewData.Tables["MstTab"].AsEnumerable().Select(b => new { TabId = b["TabId"].ToString() });

                var exceptTabAB = qryTab1.Except(qryTab2);
                var exceptTabBA = qryTab2.Except(qryTab1);

                foreach (var tab in exceptTabAB)
                {
                    dr = null;
                    dr = dtTabchanges.NewRow();
                    dr["TabId"] = tab.TabId;
                    dr["FunctionId"] = 3;
                    dtTabchanges.Rows.Add(dr);

                }
                foreach (var tab in exceptTabBA)
                {
                    dr = null;
                    dr = dtTabchanges.NewRow();
                    dr["TabId"] = tab.TabId;
                    dr["FunctionId"] = 4;
                    dtTabchanges.Rows.Add(dr);

                }
                
                /*Original All fields*/
                var qryField1 = dsUpdateMode.Tables[2].AsEnumerable().Select(a => new { FieldId = a["FieldId"].ToString()});
                /*Update all fields*/
                var qryField2 = NewData.Tables["dtLnk_Form"].AsEnumerable().Select(b => new { FieldId = b["FieldId"].ToString()});

                var exceptFieldAB = qryField2.Except(qryField1);
                var exceptFieldBA = qryField1.Except(qryField2);

                foreach (var field in exceptFieldAB)
                {
                    dr = null;
                    var FieldIdsQuery = NewData.Tables["dtLnk_form"].AsEnumerable()
                                            .Where(c => c["FieldId"].ToString() == field.FieldId)
                                            .Select(c => new
                                            {
                                                SectionId = c["SectionId"]

                                            }).FirstOrDefault();
                    string sectionId = FieldIdsQuery.SectionId.ToString();
                    var FieldTabsQuery = NewData.Tables["LnkSectionTab"].AsEnumerable()
                                            .Where(d => d["SectionID"].ToString() == sectionId.ToString())
                                            .Select(d => new
                                            {
                                                TabId = d["TabId"]

                                            }).FirstOrDefault();
                    dr = dtFieldChanges.NewRow();
                    dr["FieldId"] = field.FieldId;
                    dr["FunctionId"] = 4;
                    dr["TabId"] = FieldTabsQuery.TabId;
                    dr["SectionId"] = FieldIdsQuery.SectionId;
                    dtFieldChanges.Rows.Add(dr);

                }
                foreach (var field in exceptFieldBA)
                {
                    dr = null;
                    var FieldIdsQuery = dsUpdateMode.Tables[2].AsEnumerable()
                                            .Where(c => c["FieldId"].ToString() == field.FieldId)
                                            .Select(c => new
                                            {
                                                SectionId = c["SectionId"]

                                            }).FirstOrDefault();
                    var FieldTabsQuery = dsUpdateMode.Tables[4].AsEnumerable()
                                            .Where(c => c["SectionID"].ToString() == FieldIdsQuery.SectionId.ToString())
                                            .Select(c => new
                                            {
                                                TabId = c["TABID"]

                                            }).FirstOrDefault();
                    dr = dtFieldChanges.NewRow();
                    dr["FieldId"] = field.FieldId;
                    dr["FunctionId"] = 3;
                    dr["TabId"] = FieldTabsQuery.TabId;
                    dr["SectionId"] = FieldIdsQuery.SectionId;
                    dtFieldChanges.Rows.Add(dr);

                }
                
                if (dtTabchanges.Rows.Count > 0 || dtSectionchanges.Rows.Count > 0 || dtFieldChanges.Rows.Count > 0)
                {
                    return false;
                }
 
            }
            return true;
        }
        private void  SelectTab(string TabName)
        {
           
            IQCareWindowMsgBox.ShowWindow("PMTCTNoSectionCreated", this);
            foreach (TabPage tb in tabFormBuilder.TabPages)
            {
                if (tb.Text == TabName)
                {
                    tabFormBuilder.SelectedTab = tb;
                    
                    break;
                }
            }
            

        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            GblIQCare.iFormBuilderId = 0;
            GblIQCare.FormVersionId = 0;
            if (dtDeleteFields.Rows.Count > 0)
                dtDeleteFields.Clear();
            dsUpdateMode.Clear();
            dsFieldDetails.Clear();
            if (dtDeleteSections != null)
            {
                if (dtDeleteSections.Rows.Count > 0)
                    dtDeleteSections.Clear();
            }
            if (dtMstFeatureForManageField != null)
            {
                if (dtMstFeatureForManageField.Rows.Count > 0)
                    dtMstFeatureForManageField.Clear();
            }
            if (dtMstSectionForManageField != null)
            {
                if (dtMstSectionForManageField.Rows.Count > 0)
                    dtMstSectionForManageField.Clear();
            }
            if (dtLnkFormsForManageField != null)
            {
                if (dtLnkFormsForManageField.Rows.Count > 0)
                    dtLnkFormsForManageField.Clear();
            }



            Form theForm;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmManageForms, IQCare.FormBuilder"));
            //Paritosh
            //theForm.MdiParent = this.MdiParent;
          //  theForm.MdiParent = ParentForm.MdiParent;

            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();

          
             this.Close();
        }

        private void btnAddCustomField_Click(object sender, EventArgs e)
        {
            if (ValidationCheck())
            {
                return;
            }
            //store all form data in datatable to maintain form state.

            if (dtMstFeatureForManageField != null)
            {
                if (dtMstFeatureForManageField.Rows.Count > 0)
                    dtMstFeatureForManageField.Clear();
            }
            if (dtMstSectionForManageField != null)
            {
                if (dtMstSectionForManageField.Rows.Count > 0)
                    dtMstSectionForManageField.Clear();
            }
            if (dtLnkFormsForManageField != null)
            {
                if (dtLnkFormsForManageField.Rows.Count > 0)
                    dtLnkFormsForManageField.Clear();
            }

            DataRow drRow;
            dtMstFeatureForManageField = clsCommon.CreateTableMstFeature();
            drRow = dtMstFeatureForManageField.NewRow();
            drRow["FeatureId"] = GblIQCare.iFormBuilderId;
            drRow["FeatureName"] = txtFormName.Text.Trim();


            dtMstFeatureForManageField.Rows.Add(drRow);

            dtMstSectionForManageField = clsCommon.CreateTableMstSection();
            dtLnkFormsForManageField = clsCommon.CreateTableLnk_Forms();

            DataRow rowMstSection;
            DataRow rowLnk_Form;

            int iSecSequence = 0;
            int iSectionRowCounter = 0;
            #region Code need to change
            //foreach (Control ctrlControls in pnlMainPanel.Controls)
            //{
            //    if (ctrlControls is Panel && ctrlControls.Visible != false)
            //    {
            //        //for each panel execute code
            //        foreach (Control ctrl in ctrlControls.Controls)
            //        {
            //            //section name
            //            if (ctrl is TextBox)
            //            {
            //                if (ctrl.Text.ToString().Trim() != "")
            //                {
            //                    int iSecSequenceReset = 0;
            //                    string[] strNameSplit;
            //                    strNameSplit = ctrlControls.Name.Split('_');
            //                    if (strNameSplit.Length > 2)
            //                        iSecSequenceReset = System.Convert.ToInt32(strNameSplit[2]);

            //                    int iSecIdInUpdateMode = 0;
            //                    rowMstSection = dtMstSectionForManageField.NewRow();
            //                    //sectionid in update mode
            //                    if (GblIQCare.iFormBuilderId != 0)
            //                    {
            //                        iSecIdInUpdateMode = System.Convert.ToInt32(ctrl.Name.Split('_')[2]);

            //                        if (iSecIdInUpdateMode > 0)
            //                        {
            //                            rowMstSection["SectionId"] = iSecIdInUpdateMode;
            //                        }
            //                        rowMstSection["InsertUpdateStatus"] = PMTCTConstants.strUpdate;

            //                    }

            //                    rowMstSection["SectionName"] = ctrl.Text.ToString().Trim();
            //                    if (iSecSequenceReset > 0)
            //                        rowMstSection["Sequence"] = iSecSequenceReset;
            //                    else
            //                        rowMstSection["Sequence"] = iSecSequence;

            //                    rowMstSection["CustomFlag"] = 0;
            //                    rowMstSection["DeleteFlag"] = 0;
            //                    rowMstSection["UserId"] = GblIQCare.AppUserId;
            //                    rowMstSection["FeatureId"] = GblIQCare.iFormBuilderId;
            //                    dtMstSectionForManageField.Rows.Add(rowMstSection);

            //                    iSecSequence += 1;
            //                }

            //            } //End of textbox control

            //        }//end of foreach inside panel control

            //        int iSeq = 1;
            //        //for loop again to fetch datagrid details
            //        foreach (Control ctrl in ctrlControls.Controls)
            //        {

            //            if (ctrl is DataGridView)
            //            {
            //                DataGridView dgwGridData = ctrl as DataGridView;
            //                if (dgwGridData.Rows.Count > 0)
            //                {
            //                    //foreach (DataGridView dgwGridData in ctrl)
            //                    for (int iGridRow = 0; iGridRow < dgwGridData.Rows.Count; iGridRow++)
            //                    {
            //                        rowLnk_Form = dtLnkFormsForManageField.NewRow();
            //                        if (dgwGridData.Rows[iGridRow].Cells[0].Value != null && dgwGridData.Rows[iGridRow].Cells["Field Label"].Value != null)
            //                        {
            //                            //update mode of field
            //                            if (GblIQCare.iFormBuilderId != 0 && dgwGridData.Rows[iGridRow].Cells["ID"].Value != null && dgwGridData.Rows[iGridRow].Cells["ID"].Value.ToString() != "")
            //                            {
            //                                rowLnk_Form["InsertUpdateStatus"] = PMTCTConstants.strUpdate;
            //                                rowLnk_Form["Id"] = dgwGridData.Rows[iGridRow].Cells["ID"].Value;
            //                            }
            //                            else //insert mode
            //                            {
            //                                rowLnk_Form["InsertUpdateStatus"] = PMTCTConstants.strInsert;
            //                            }

            //                            //update mode
            //                            if (GblIQCare.iFormBuilderId > 0)
            //                            {

            //                                DataSet dsSetFieldDetail = dsFieldDetails.Copy();
            //                                DataView dvFilteredRow = new DataView();
            //                                dvFilteredRow = dsSetFieldDetail.Tables[0].DefaultView;
            //                                DataTable dtRow = new DataTable();

            //                                dvFilteredRow.RowFilter = "FieldName='" + dgwGridData.Rows[iGridRow].Cells["Table Field Name"].Value + "'";
            //                                dtRow = dvFilteredRow.ToTable();
            //                                rowLnk_Form["FieldId"] = dtRow.Rows[0]["Id"];
            //                            }
            //                            else //insert mode
            //                            {
            //                                rowLnk_Form["FieldId"] = dgwGridData.Rows[iGridRow].Cells["FieldId"].Value.ToString().Trim();
            //                            }
            //                            rowLnk_Form["FieldName"] = dgwGridData.Rows[iGridRow].Cells["Table Field Name"].Value.ToString().Trim();
            //                            rowLnk_Form["FeatureId"] = GblIQCare.iFormBuilderId;
            //                            rowLnk_Form["SectionId"] = dtMstSectionForManageField.Rows[iSectionRowCounter]["SectionId"];
            //                            rowLnk_Form["FieldLabel"] = dgwGridData.Rows[iGridRow].Cells["Field Label"].Value.ToString().Trim();
            //                            rowLnk_Form["Sequence"] = iSeq;
            //                            rowLnk_Form["Predefined"] = dgwGridData.Rows[iGridRow].Cells["Predefined"].Value;
            //                            //rowLnk_Form["UserId"] = GblIQCare.AppUserId;
            //                            dtLnkFormsForManageField.Rows.Add(rowLnk_Form);

            //                            iSeq += 1;
            //                        }

            //                    }//end of foreach of datagrid 
            //                    //first grid data goes in first section, and so on.
            //                }//if field exist in section i.e. dgwGridData.rows.count>0

            //                iSectionRowCounter += 1;
            //            }//endif of datagridview check
            //        }
            //    }//endif of panel control check

            //}//end of foreach panel
            #endregion
            Form theForm;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmFieldDetails, IQCare.FormBuilder"));
            theForm.MdiParent = this.MdiParent;
            theForm.Deactivate += new EventHandler(FrmManageFieldHideChildOnLostFocus);
            //theForm.LostFocus += new EventHandler(frmLostFocus);
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Focus();
            theForm.Show();

        }//end of btn_saveClick event

        /// <summary>
        /// Hide the child form opened from formbuilder only if focus is lost from that child form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmManageFieldHideChildOnLostFocus(object sender, EventArgs e)
        {
            Form theSenderForm = sender as Form;
            theSenderForm.Close();
            GblIQCare.RefillDdlFields = 1;
            frmFormBuilder_Load(sender, e);
        }

        private void FrmManageTabHideChildOnLostFocus(object sender, EventArgs e)
        {
            Form theSenderForm = sender as Form;
            theSenderForm.Close();
            RefreshTab();
        }
       
        private void RefreshTab()
        {
            List<TabPage> listTab= new List<TabPage>();
            for (int x = 0; x < dtManageTabPos.Rows.Count; x++)
                {
                    foreach (TabPage ctrl in tabFormBuilder.TabPages)
                    {
                        if (dtManageTabPos.Rows[x]["TabName"].ToString() == ctrl.Text.ToString().Trim())
                        {
                            if (dtManageTabPos.Rows[x]["DeleteFlag"].ToString() == "0")
                            {
                                listTab.Add(ctrl);
                            }
                            else if (dtManageTabPos.Rows[x]["DeleteFlag"].ToString() == "1")
                            {
                                DataRow TheDeleteTabDR = dtDeleteTabs.NewRow();
                                TheDeleteTabDR["TabId"] =dtManageTabPos.Rows[x]["TabId"];
                                TheDeleteTabDR["DeleteFlag"] = dtManageTabPos.Rows[x]["DeleteFlag"];
                                TheDeleteTabDR["TabName"] = dtManageTabPos.Rows[x]["TabName"];
                                dtDeleteTabs.Rows.Add(TheDeleteTabDR);
                            }
                        }
                    }
                }

                tabFormBuilder.TabPages.Clear();

                foreach (TabPage tpage in listTab)
            {
                tabFormBuilder.TabPages.Add(tpage);
            }
            pnlPanel.Refresh();
           

        }

        private void btnManageTab_Click(object sender, EventArgs e)
        {
            if (ValidationCheck())
            {
                return;
            }
            if (dtManageTabPos != null)
            {
                dtManageTabPos.Clear();
            }
            dtManageTabPos = clsCommon.ManageTabPos();
            foreach (Control ctrlControls in this.Controls)
            {
                if (ctrlControls is Panel && ctrlControls.Name.Contains("pnlPanel"))
                {
                    foreach (Control pnlctrl in ctrlControls.Controls)
                    {

                        if (pnlctrl is TabControl)
                        {
                            int pos = 1;
                            foreach (TabPage ctrl in pnlctrl.Controls)
                            {

                                DataRow row;
                                row = dtManageTabPos.NewRow();
                                if (GblIQCare.iFormBuilderId > 0)
                                {
                                    string[] strTabId;
                                    strTabId = ctrl.Name.Split('_');
                                    if (strTabId.Length > 1)
                                    {
                                        if (System.Convert.ToInt32(strTabId[2]) > 0)
                                        {
                                            row["TabId"] = strTabId[2];
                                        }
                                    }
                                }
                                row["TabName"] = ctrl.Text.ToString().Trim();
                                row["TopPos"] = pos;
                                row["DeleteFlag"] = 0;
                                dtManageTabPos.Rows.Add(row);
                                pos++;

                            }
                        }
                    }
                }
            }
            frmManageFBTab theForm = new frmManageFBTab();
            theForm.MdiParent = this.ParentForm;
            theForm.dtTabPos = dtManageTabPos;
            theForm.Deactivate += new EventHandler(FrmManageTabHideChildOnLostFocus);
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Focus();
            theForm.Show();
        }

        private void txtTabCaptionPlaceHolder_Leave(object sender, EventArgs e)
        {
            if (tabFormBuilder.TabPages != null)
            {
                tabFormBuilder.TabPages[tabFormBuilder.SelectedIndex].Text = txtTabCaptionPlaceHolder.Text;
                txtTabCaptionPlaceHolder.Hide();
            }
        }

        private void tabFormBuilder_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Rectangle rect = tabFormBuilder.GetTabRect(tabFormBuilder.SelectedIndex);
            txtTabCaptionPlaceHolder.Location = new Point(tabFormBuilder.Location.X + rect.X + 2, tabFormBuilder.Location.Y +
            rect.Y + 2);
            txtTabCaptionPlaceHolder.Size = new Size(rect.Size.Width - 2,rect.Height + 2);
            txtTabCaptionPlaceHolder.Text = tabFormBuilder.TabPages[tabFormBuilder.SelectedIndex].Text;
            txtTabCaptionPlaceHolder.Show();
            txtTabCaptionPlaceHolder.BringToFront();
            txtTabCaptionPlaceHolder.Focus();
            txtTabCaptionPlaceHolder.SelectAll();

        }

        private void frmFormBuilder_FormClosing(object sender, FormClosingEventArgs e)
        {
            GC.Collect();
        }

        private void btnformbusinessrules_Click(object sender, EventArgs e)
        {
            if (txtFormName.Text.ToString().Trim() == "")
            {
                IQCareWindowMsgBox.ShowWindow("PMTCTFormNameMandatory", this);
                txtFormName.Focus();
                return;
            }
            if (theForm == null)
            {
                GblIQCare.iFormMode = 0;
                theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmFormBusinessRule, IQCare.FormBuilder"));
                theForm.FormClosed += new FormClosedEventHandler(theForm_FormClosed);
                theForm.Left = 0;
                theForm.Top = 0;
                theForm.Show();
            }
        }

        void theForm_FormClosed(object sender, EventArgs e)
        {
            theForm = null;
        }

        private void dtformrel_ValueChanged(object sender, EventArgs e)
        {
            dtformrel.CustomFormat = "dd-MMM-yyyy";
        }

        private void dtformrel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                dtformrel.Format = DateTimePickerFormat.Custom;
                dtformrel.CustomFormat = "   ";
            }
        }

    }
}