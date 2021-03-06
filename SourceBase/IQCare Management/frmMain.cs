﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Application.Common;
using Application.Presentation;
using System.IO;
using System.Diagnostics;
using Interface.FormBuilder;
using Interface.Security;


namespace IQCare_Management
{
    public partial class frmMain : Form
    {
        private int childFormNumber = 0;
        Form theForm;

        public frmMain()
        {

            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            clsCssStyle theStyle = new clsCssStyle();
            theStyle.setStyle(this);
            this.Text = PMTCTConstants.strIQCareTitle + " [" + GblIQCare.AppUserName + "] - " + GblIQCare.AppLocation;
            lblStatus.Text = GblIQCare.AppVersion + "     Release Date: " + GblIQCare.ReleaseDate;
            lblCopyRight.Text = "©" + System.DateTime.Now.Year.ToString() + " Palladium Inc.";

            #region "Module Validation"
            DataTable theModTable = GblIQCare.dtModules;
            //DataView theDV = new DataView(theModTable);
            //theDV.RowFilter = "ModuleId = 201";
            //if (theDV.Count < 1)
            if (GblIQCare.dtFacility.Rows[0]["PMSCM"].ToString() != "1")
            {
                mnuPMSCM.Visible = false;
            }
            if (GblIQCare.dtFacility.Rows[0]["Billing"].ToString() != "1")
            {
                mnuBilling.Visible = false;
            }

            #endregion

            #region "User Authentication"
            if (GblIQCare.HasFeatureRight(ApplicationAccess.FormBuilder, GblIQCare.dtUserRight) == false)
            {
                mnuFormBuilder.Visible = false;
                //mnuDBMerge.Visible = false;
                //mnuDBMigration.Visible = false;
                //mnuUpsize.Visible = false;
                //toolStripSeparator1.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.ManageFields, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuManageFields.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.FBManageCareEndedField, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuManageCareEndedFields.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.FBManageRegField, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                manageRegistrationFieldsToolStripMenuItem.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.ConfigureHomePages, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuConfigureHomePageForms.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.ConfigureCareTermination, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuConfigCareTermination.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.ManageForms, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuManageForms.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.DatabaseMigration, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuDBMigration.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.Upsize, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuUpsize.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.DatabaseMerge, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
               // mnuDBMerge.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.SpecialFormLinking, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuSplFormLinking.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.ManageTechnicalArea, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuManageModule.Visible = false;
            }
            
            if (GblIQCare.HasFunctionRight(ApplicationAccess.DrugDispense, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                //mnuPatientDrugDispense.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.PurchaseOrder, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.PurchaseOrder, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuPurchaseOrder.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.GoodReceiveNotes, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.GoodReceiveNotes, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuGoodReceivedNote.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.OpeningStock, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.AdjustStocklevel, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuOpeningStock.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.AdjustStocklevel, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.AdjustStocklevel, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuAdjustStock.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.DisposeItem, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.DisposeItem, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuDisposeItem.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.BatchSummary, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuBatchSummary.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.StockSummary, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuStockSummary.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.ExpiryReport, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuExpiryReport.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.BudgetConfiguration, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.BudgetConfiguration, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuConfigureBudget.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.PatientVisitConfiguration, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.PatientVisitConfiguration, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuVisitConfiguration.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.FBDBOperation, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuDBOperations.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.QueryBuilderReports, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuQueryBuilder.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.FBDataBaseSyncronisation, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuDBMerge.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.FBSCMConfiguration, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                sCMToolStripMenuItem.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.Backuprestore, FunctionAccess.View, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.Backupsetup, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                backupDatabaseToolStripMenuItem.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.FBFieldAssociation, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuViewFieldAsscociation.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.FBImportExportForm, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                mnuImportExportForms.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.PatientRegistration, FunctionAccess.View, GblIQCare.dtUserRight) == false)
            {
                configureToolStripMenuItem.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.RequistionVoucher, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.RequistionVoucher, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                counterRequisitionToolStripMenuItem.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.BudgetView, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.BudgetView, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                mnuBudgetView.Visible = false;
            }
            if (GblIQCare.HasFunctionRight(ApplicationAccess.IssueVoucher, FunctionAccess.Add, GblIQCare.dtUserRight) == false && GblIQCare.HasFunctionRight(ApplicationAccess.IssueVoucher, FunctionAccess.Update, GblIQCare.dtUserRight) == false)
            {
                issueVoucherToolStripMenuItem.Visible = false;
            }
            #endregion
        }

        private void mnuUpsize_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.Service.frmDataUpsizing, IQCare.Service"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();

        }

        private void mnuManageForms_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 0;
            GblIQCare.iConditionalbtn = 1;
            Form theForm;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmManageForms, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void mnuManageFields_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 0;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmFieldDetails, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void mnuViewFieldAssociation_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmViewAssociation, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void mnuCalculatortool_Click(object sender, EventArgs e)
        {
            Process.Start("Calc.exe");
        }

        private void mnuNotepad_Click(object sender, EventArgs e)
        {
            Process.Start("Notepad.exe");
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmAbout, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void mnuServiceAdmin_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.Service.frmService, IQCare.Service"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void mnuDBMigration_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.Service.frmMigration, IQCare.Service"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();

        }

        private void mnuDBMerge_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmImportExportData, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }


        private void mnuRefereshSystemCache_Click(object sender, EventArgs e)
        {
            #region Commented Old Code for GenerateCache
            //string strGetXMLPath = GblIQCare.GetXMLPath();
            //System.IO.FileInfo theFileInfo1 = new System.IO.FileInfo(strGetXMLPath + "\\AllMasters.con");
            //System.IO.FileInfo theFileInfo2 = new System.IO.FileInfo(strGetXMLPath + "\\DrugMasters.con");
            //System.IO.FileInfo theFileInfo3 = new System.IO.FileInfo(strGetXMLPath + "\\LabMasters.con");

            //theFileInfo1.Delete();
            //theFileInfo2.Delete();
            //theFileInfo3.Delete();

            //IIQCareSystem theCacheManager = (IIQCareSystem)ObjectFactory.CreateInstance("BusinessProcess.Security.BIQCareSystem,BusinessProcess.Security");
            //DataSet theMainDS = theCacheManager.GetSystemCache();
            //DataSet WriteXMLDS = new DataSet();

            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_CouncellingType"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_CouncellingTopic"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Provider"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Division"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Ward"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_District"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Reason"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Education"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Designation"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Employee"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Occupation"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Province"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Village"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Code"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_HIVAIDSCareTypes"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_ARTSponsor"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_HivDisease"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Assessment"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Symptom"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Decode"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Feature"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Function"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_HivDisclosure"].Copy());
            ////WriteXMLDS.Tables.Add(theMainDS.Tables["mst_Satellite"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_LPTF"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["mst_StoppedReason"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["mst_facility"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_HIVCareStatus"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_RelationshipType"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_TBStatus"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_ARVStatus"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_LostFollowreason"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Regimen"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Store"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Supplier"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["mst_Donor"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Program"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Batch"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["VWDiseaseSymptom"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["mst_RegimenLine"].Copy());
            //WriteXMLDS.WriteXml(strGetXMLPath + "\\AllMasters.con", XmlWriteMode.WriteSchema);

            //WriteXMLDS.Tables.Clear();
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Strength"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_FrequencyUnits"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Drug"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Generic"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_DrugType"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_Frequency"].Copy());
            //WriteXMLDS.WriteXml(strGetXMLPath + "\\DrugMasters.con", XmlWriteMode.WriteSchema);

            //WriteXMLDS.Tables.Clear();
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Mst_LabTest"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Lnk_TestParameter"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Lnk_LabValue"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["Lnk_ParameterResult"].Copy());
            //WriteXMLDS.Tables.Add(theMainDS.Tables["LabTestOrder"].Copy());

            //WriteXMLDS.WriteXml(strGetXMLPath + "\\LabMasters.con", XmlWriteMode.WriteSchema);
            #endregion

            try
            {
                /*
                 * Calling generate cache from common location
                 * Update By: Gaurav 
                 * Update Date: 8 July 2014
                 */
                IQCareUtils.GenerateCache(true);
                IQCareWindowMsgBox.ShowWindow("SysCacheRefresh", this);
            }
            catch (System.UnauthorizedAccessException uex)
            {
                IQCareWindowMsgBox.ShowWindow("Error - Access to the path is denied.", "", "", this);
            }
            catch (Exception ex)
            {
                IQCareWindowMsgBox.ShowWindow("Error - unable to refresh cache.", "", "", this);
            }
        }

        private void mnuRebuildCustomReportDB_Click(object sender, EventArgs e)
        {
            IDBMaintenance objDBMaintenance;
            objDBMaintenance = (IDBMaintenance)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BDBMaintenance,BusinessProcess.FormBuilder");
            objDBMaintenance.RebuildCustomRptDB();
        }

        private void mnuIQCareDBMaintenance_Click(object sender, EventArgs e)
        {
            //rebuild indexes and truncate log
            //pr_SystemAdmin_DBMaintenance_Constella

            IDBMaintenance objDBMaintenance;
            objDBMaintenance = (IDBMaintenance)ObjectFactory.CreateInstance("BusinessProcess.FormBuilder.BDBMaintenance,BusinessProcess.FormBuilder");
            objDBMaintenance.DBMaintenance();
        }

        private void btnService_Click(object sender, EventArgs e)
        {
            mnuServiceAdmin_Click(sender, e);
        }

        private void mnuImportExportForms_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmImportExportForms, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void mnuLogOut_Click(object sender, EventArgs e)
        {
            frmLogin objForm1 = new frmLogin();
            objForm1.Show();
            this.Hide();
            GblIQCare.AppUserId = 0;
            GblIQCare.AppUserName = "";
            GblIQCare.EnrollFlag = 0;
            //GblIQCare.UserRight = theDs.Tables[1].ToString();

            GblIQCare.SystemId = 0;
            GblIQCare.AppCountryId = "";
            GblIQCare.AppDateFormat = "";
            GblIQCare.AppGracePeriod = "";
            GblIQCare.AppLocationId = 0;
            GblIQCare.AppLocation = "";
            GblIQCare.AppPosID = "";
            GblIQCare.AppSatelliteId = "";
            GblIQCare.BackupDrive = "";


        }

        private void mnuManageModule_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmModule, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void mnuQueryBuilder_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmQueryBuilder, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();


        }

        private void mnuConfigureHomePageForms_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmHomePageList, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void mnuConfigCareTermination_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmCareEndedList, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void mnuManageCareEndedFields_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 1;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmFieldDetails, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void mnuSplFormLinking_Click(object sender, EventArgs e)
        {
            //theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmSplFormLink, IQCare.FormBuilder"));
            //theForm.MdiParent = this;
            //theForm.Left = 0;
            //theForm.Top = 2;
            //theForm.Show();
        }

        private void mnuReportFieldValidation_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmRptFieldValidations, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }


        private void listViewFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.FrmListView, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();


        }

        private void gridviewFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.Gridviewform, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();

        }

        private void mnuPatientDrugDispense_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmSetUserStore, IQCare.SCM"));
            theForm.MdiParent = this;
            GblIQCare.theArea = "Dispense";
            theForm.Show();
        }

        private void itemMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmItemMaster, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        //private void donorToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmConfigureDonermaster, IQCare.SCM"));
        //    theForm.MdiParent = this;
        //    theForm.Left = 0;
        //    theForm.Top = 2;
        //    theForm.Show();
        //}

        private void masterListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmMasterList, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();

        }

        //private void supplyToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmSCMitemList, IQCare.SCM"));
        //    theForm.MdiParent = this;
        //    theForm.Left = 0;
        //    theForm.Top = 2;
        //    theForm.Show();
        //}

        private void labItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmLabItemDetails, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void configureBudgetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmConfigureBudget, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void frmDisposeItemDrugsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmDisposeItemDrugs, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void expiryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmExpiryReport, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }



        private void purchaseOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmSetUserStore, IQCare.SCM"));
            theForm.MdiParent = this;
            GblIQCare.theArea = "PO";
            theForm.StartPosition = FormStartPosition.CenterScreen;
            theForm.Show();
        }

        //private void configureLabTestToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmConfigureLabTest, IQCare.SCM"));
        //    theForm.MdiParent = this;
        //    theForm.Left = 0;
        //    theForm.Top = 2;
        //    theForm.Show();
        //}

        private void goodsRecievedNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmSetUserStore, IQCare.SCM"));
            theForm.MdiParent = this;
            GblIQCare.theArea = "GRN";
            theForm.StartPosition = FormStartPosition.CenterScreen;
            theForm.Show();

        }

        private void programItemListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmProgramItemLinking, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void itemSubItemLinkingToolStripMenuItem_Click(object sender, EventArgs e)
        {

            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmItemTypeSubTypeLinking, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void supplierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmSupplierItem, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void configurePatientVisitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmPatientVisitsPerMonth, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Show();
        }

        private void adjustStockLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmAdjustStockLevel, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void openingStoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmOpeningStock, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();

        }

        private void holisticBudgetViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmHolisticBudgetView, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void stockSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmStockSummary, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void batchSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmBatchSummaryByStore, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void ReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmReports, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void manageRegistrationFieldsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 2;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmFieldDetails, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 0;
            Form theForm;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmPatientRegistrationManageForms, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void sCMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 0;
            Form theForm;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmSCM_PharmacyMasterImportExport, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void counterRequisitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmSetUserStore, IQCare.SCM"));
            theForm.MdiParent = this;
            GblIQCare.theArea = "CR";
            theForm.StartPosition = FormStartPosition.CenterScreen;
            theForm.Show();

        }

        private void issueVoucherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmSetUserStore, IQCare.SCM"));
            theForm.MdiParent = this;
            GblIQCare.theArea = "IV";
            theForm.StartPosition = FormStartPosition.CenterScreen;
            theForm.Show();

        }

        private void managePharmacyFieldsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 3;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmFieldDetails, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void configurePharmacyFieldsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GblIQCare.iManageCareEnded = 3;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmPatientPharmacyManageForm, IQCare.FormBuilder"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void backupDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.Service.frmDatabaseBackup, IQCare.Service"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }

        private void iQCareHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.FormBuilder.frmIQCarehelp, IQCare.FormBuilder"));
            //theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.WindowState = FormWindowState.Maximized;
            theForm.Show();
        }

        private void mnuPriceList_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmItemCostConfiguration, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void billingDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmBillingDetails, IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void billablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GblIQCare.ItemLabel = "Billables";
            GblIQCare.ItemCategoryId = "213";
            GblIQCare.ItemTableName = "Decode";
            GblIQCare.ItemFeatureId = 173;
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmCommonItemMaster,IQCare.SCM"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();
        }

        private void nDRXMLCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.Service.frmNDRXMLCreation, IQCare.Service"));
            theForm.MdiParent = this;
            theForm.Left = 0;
            theForm.Top = 0;
            theForm.Show();
        }
    }
}
