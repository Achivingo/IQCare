﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Interface.SCM;
using Application.Common;
using Application.Presentation;
using System.Xml;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;


namespace IQCare.SCM
{
    public partial class frmStockSummary : Form
    {
        #region "Variables"
        DataTable theDTItems = new DataTable();
        #endregion

        public frmStockSummary()
        {
            InitializeComponent();
        }

        private void frmStockSummary_Load(object sender, EventArgs e)
        {
            try
            {
                clsCssStyle theStyle = new clsCssStyle();
                theStyle.setStyle(this);
                BindCombo();
                //ShowGrid();
                //Init_Form();
                //SetRights();
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }
        }

        private void BindCombo()
        {
            try
            {
                DataSet XMLDS = new DataSet();
                XMLDS.ReadXml(GblIQCare.GetXMLPath() + "\\AllMasters.con");
                BindFunctions theBindManager = new BindFunctions();

                DataView theDV = new DataView(XMLDS.Tables["Mst_Store"]);
                theDV.RowFilter = "(DeleteFlag =0 or DeleteFlag is null)";
                theDV.Sort = "Name ASC";
                DataTable theStoreDT = theDV.ToTable();
                theBindManager.Win_BindCombo(ddlStore, theStoreDT, "Name", "Id");
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }
        }

        private void ShowGrid(DataTable theDT)
        {
            try
            {
                dgwStockSummary.Columns.Clear();
                dgwStockSummary.DataSource = null;
                DataGridViewTextBoxColumn ItemCode = new DataGridViewTextBoxColumn();
                ItemCode.HeaderText = "Item Code";
                ItemCode.DataPropertyName = "ItemId";
                ItemCode.Name = "ItemId";
                ItemCode.Width = 100;
                ItemCode.ReadOnly = true;
                ItemCode.Visible = false;

                DataGridViewTextBoxColumn ItemName = new DataGridViewTextBoxColumn();
                ItemName.HeaderText = "Item Description";
                ItemName.DataPropertyName = "ItemName";
                ItemName.Width = 280;
                ItemName.ReadOnly = true;

                DataGridViewTextBoxColumn Unit = new DataGridViewTextBoxColumn();
                Unit.HeaderText = "Unit";
                Unit.DataPropertyName = "DispensingUnit";
                Unit.Width = 86;
                Unit.ReadOnly = true;

                DataGridViewTextBoxColumn OpeningQty = new DataGridViewTextBoxColumn();
                OpeningQty.HeaderText = "Opening Stock";
                OpeningQty.DataPropertyName = "OpeningStock";
                OpeningQty.Width = 90;
                OpeningQty.ReadOnly = true;

                DataGridViewTextBoxColumn QtyRecieved = new DataGridViewTextBoxColumn();
                QtyRecieved.HeaderText = "Recieved Quantity";
                QtyRecieved.DataPropertyName = "QtyRecieved";
                QtyRecieved.Width = 90;
                QtyRecieved.ReadOnly = true;


                DataGridViewTextBoxColumn InterStoreIss = new DataGridViewTextBoxColumn();
                InterStoreIss.HeaderText = "Inter Store Issue";
                InterStoreIss.DataPropertyName = "InterStoreIssueQty";
                InterStoreIss.Width = 90;
                InterStoreIss.ReadOnly = true;

                DataGridViewTextBoxColumn QtyDispensed = new DataGridViewTextBoxColumn();
                QtyDispensed.HeaderText = "Quantity Dispensed";
                QtyDispensed.DataPropertyName = "QtyDispensed";
                QtyDispensed.Width = 80;
                QtyDispensed.ReadOnly = true;

                DataGridViewTextBoxColumn AdjustedQty = new DataGridViewTextBoxColumn();
                AdjustedQty.HeaderText = "Adjusted Quantity";
                AdjustedQty.DataPropertyName = "AdjustmentQuantity";
                AdjustedQty.Width = 75;
                AdjustedQty.ReadOnly = true;

                DataGridViewTextBoxColumn ClosingQty = new DataGridViewTextBoxColumn();
                ClosingQty.HeaderText = "Closing Quantity";
                ClosingQty.DataPropertyName = "ClosingQty";
                ClosingQty.Width = 75;
                ClosingQty.ReadOnly = true;

                DataGridViewImageColumn BinCard = new DataGridViewImageColumn();
                BinCard.HeaderText = "Bin Card";
                BinCard.DataPropertyName = "BinCard";
                BinCard.Name = "BinCard";
                BinCard.Width = 40;
                BinCard.Image = Image.FromFile(GblIQCare.GetPath() + "\\bincard.jpg");
                //BinCard.Visible = false;

                dgwStockSummary.Columns.Add(ItemCode);
                dgwStockSummary.Columns.Add(ItemName);
                dgwStockSummary.Columns.Add(Unit);
                dgwStockSummary.Columns.Add(OpeningQty);
                dgwStockSummary.Columns.Add(QtyRecieved);
                dgwStockSummary.Columns.Add(InterStoreIss);
                dgwStockSummary.Columns.Add(QtyDispensed);
                dgwStockSummary.Columns.Add(AdjustedQty);
                dgwStockSummary.Columns.Add(ClosingQty);
                dgwStockSummary.Columns.Add(BinCard);
                dgwStockSummary.AutoGenerateColumns = false;
                dgwStockSummary.DataSource = theDT;
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }

        }

        private DataSet GetItems(int StoreId, int ItemsId, DateTime FromDate, DateTime ToDate)
        {
            BindFunctions theBindManager = new BindFunctions();
            //IMasterList objOpenStock = (IMasterList)ObjectFactory.CreateInstance("BusinessProcess.SCM.BMasterList,BusinessProcess.SCM");
            ISCMReport objOpenStock = (ISCMReport)ObjectFactory.CreateInstance("BusinessProcess.SCM.BSCMReport,BusinessProcess.SCM");
            return objOpenStock.GetStockSummary(StoreId, ItemsId, FromDate, ToDate);
        }

        private void ddlStore_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(ddlStore.SelectedValue) != 0)
                {
                    BindFunctions theBindManager = new BindFunctions();
                    DataSet theDS = GetItems(Convert.ToInt32(ddlStore.SelectedValue), Convert.ToInt32(lstSearch.SelectedValue), Convert.ToDateTime(dtpFrom.Text), Convert.ToDateTime(dtpTo.Text));
                    //theBindManager.Win_BindListBox(lstSearch, theDS.Tables[0], "DrugName", "Drug_pk");
                    theDTItems = theDS.Tables[1];
                    //ShowGrid(theDS.Tables[1]);
                }
                else
                {
                    txtItemName.Text = "";
                    dgwStockSummary.Columns.Clear();
                    dgwStockSummary.DataSource = null;
                }

            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }
        }

        private void txtItemName_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Convert.ToInt32(ddlStore.SelectedValue) > 0)
                {
                    if (txtItemName.Text != "")
                    {
                        lstSearch.Visible = true;
                        lstSearch.Width = txtItemName.Width;
                        lstSearch.Left = txtItemName.Left;
                        lstSearch.Top = txtItemName.Top + txtItemName.Height;
                        lstSearch.Height = 300;
                        lstSearch.BringToFront();
                        DataView theDV = new DataView(theDTItems);
                        theDV.RowFilter = "ItemName like '" + txtItemName.Text + "*'";
                        //theDV.RowFilter = "ItemName like '" + txtItemName.Text + "*'";
                        if (theDV.Count > 0)
                        {
                            DataTable theDTdata = theDV.ToTable();
                            BindFunctions theBindManager = new BindFunctions();
                            //theBindManager.Win_BindListBox(lstSearch, theDTdata, "DrugName", "Drug_pk");
                            theBindManager.Win_BindListBox(lstSearch, theDTdata, "ItemName", "ItemId");
                        }
                        else
                        {
                            lstSearch.DataSource = null;
                            lstSearch.Visible = false;
                        }
                    }
                    else
                    {
                        lstSearch.Visible = false;
                    }
                    if (e.KeyCode == Keys.Down)
                        lstSearch.Select();
                }
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }


        }

        private void lstSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lstSearch_DoubleClick(sender, e);
            }
        }

        private void lstSearch_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Int32 theItemId = Convert.ToInt32(lstSearch.SelectedValue);
                txtItemName.Text = lstSearch.Text;
                DataView theDV = new DataView(theDTItems);
                //theDV.RowFilter = "Drug_Pk=" + theItemId.ToString();
                theDV.RowFilter = "ItemId=" + theItemId.ToString();
                if (theDV.ToTable().Rows.Count > 0)
                {
                    lstSearch.Visible = false;
                }
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(ddlStore.SelectedValue) == 0)
            {
                IQCareWindowMsgBox.ShowWindow("StoreName", this);
                ddlStore.Focus();
                return;
            }

            TimeSpan ts = Convert.ToDateTime(dtpTo.Text) - Convert.ToDateTime(dtpFrom.Text);
            Int32 days = ts.Days;
            if (days < 0)
            {
                IQCareWindowMsgBox.ShowWindow("FromToDate", this);
                return;
            }
            TimeSpan difference = Convert.ToDateTime(dtpTo.Text) - Convert.ToDateTime(GblIQCare.CurrentDate);
            double Totaldays = difference.TotalDays;

            if (Totaldays > 0)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = "Cannot generate Stock Summary for Future Date.";
                IQCareWindowMsgBox.ShowWindow("#C1", theBuilder, this);
                return;
            }
            try
            {

                //int itemsId = lstSearch.SelectedValue == null ? 0 : Convert.ToInt32(lstSearch.SelectedValue);
                int itemsId = 0;
                if (txtItemName.Text == "")
                {
                    itemsId = 0;
                }
                DataSet theDS = GetItems(Convert.ToInt32(ddlStore.SelectedValue), itemsId, Convert.ToDateTime(dtpFrom.Text), Convert.ToDateTime(dtpTo.Text));
                DataView theDV = new DataView(theDS.Tables[1]);
                theDV.RowFilter = "ItemName like '" + txtItemName.Text + "*'";
                DataTable theDT = theDV.ToTable();
                if (itemsId == 0 && txtItemName.Text != "")
                {
                    ShowGrid(theDT);
                    //ShowGrid(theDS.Tables[2]);
                }
                else
                {
                    ShowGrid(theDS.Tables[1]);
                    //ShowGrid(theDT);
                }
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgwStockSummary.RowCount > 0)
                {
                    IQCareUtils theUtil = new IQCareUtils();
                    DataTable theDT = (DataTable)dgwStockSummary.DataSource;
                    theDT.Columns.Remove("StoreId");
                    //theDT.Columns.Remove("StoreName");
                    theDT.Columns.Remove("ItemId");
                    string theFilePath = System.Configuration.ConfigurationSettings.AppSettings["ExcelFilesPath"];
                    theFilePath = theFilePath + "StockSummary.xls";
                    theUtil.ExportToExcel_Windows(theDT, theFilePath, "");
                    btnExport.Enabled = false;
                }
                else
                {
                    IQCareWindowMsgBox.ShowWindow("GridData", this);
                    return;
                }
            }
            catch (Exception err)
            {
                MsgBuilder theBuilder = new MsgBuilder();
                theBuilder.DataElements["MessageText"] = err.Message.ToString();
                IQCareWindowMsgBox.ShowWindowConfirm("#C1", theBuilder, this);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Form theForm = new Form();
            theForm = (Form)Activator.CreateInstance(Type.GetType("IQCare.SCM.frmMasterList, IQCare.SCM"));
            theForm.MdiParent = this.MdiParent;
            theForm.Left = 0;
            theForm.Top = 2;
            theForm.Show();

            this.Close();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgwStockSummary_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgwStockSummary.Columns["BinCard"].Index)
            {
                int storeid = Convert.ToInt32(ddlStore.SelectedValue.ToString());
                int itemid = Convert.ToInt32(dgwStockSummary.Rows[e.RowIndex].Cells["ItemId"].Value.ToString());
                DateTime dateFrom = Convert.ToDateTime(dtpFrom.Value.ToString("yyyy-MM-dd"));
                DateTime dateTo = Convert.ToDateTime(dtpTo.Value.ToString("yyyy-MM-dd"));
                
                ReportDocument objRptDoc = new ReportDocument();
                                
                DataSet theDS = GetBINCard(storeid, itemid, dateFrom, dateTo);
                ////////////////////////////////////////////////////////////////
                //Image Streaming
                DataTable dtFacility = new DataTable();
                // object of data row
                DataRow drow = null;
                // add the column in table to store the image of Byte array type
                dtFacility.Columns.Add("FacilityImage", System.Type.GetType("System.Byte[]"));
                drow = dtFacility.NewRow();
                // define the filestream object to read the image
                FileStream fs = default(FileStream);
                // define te binary reader to read the bytes of image
                BinaryReader br = default(BinaryReader);
                int ImageFlag = 0;

                // check the existance of image
                if (File.Exists(GblIQCare.PresentationImagePath() + theDS.Tables[3].Rows[0]["FacilityLogo"].ToString()))
                {
                    // open image in file stream
                    fs = new FileStream(GblIQCare.PresentationImagePath() + theDS.Tables[3].Rows[0]["FacilityLogo"].ToString(), FileMode.Open);

                    // initialise the binary reader from file streamobject
                    br = new BinaryReader(fs);
                    // define the byte array of filelength
                    byte[] imgbyte = new byte[fs.Length + 1];
                    // read the bytes from the binary reader
                    imgbyte = br.ReadBytes(Convert.ToInt32((fs.Length)));
                    drow[0] = imgbyte;
                    // add the image in bytearray
                    dtFacility.Rows.Add(drow);
                    ImageFlag = 1;
                    // add row into the datatable
                    br.Close();
                    // close the binary reader
                    fs.Close();
                    // close the file stream
                }

                theDS.Tables.Add(dtFacility);
                ////////////////////////////////////////
                
                theDS.WriteXmlSchema(GblIQCare.GetXMLPath() + "\\BinCard.xml");

                rptBinCard rep = new rptBinCard();
                rep.SetDataSource(theDS);
                //  rep.ParameterFields["FormDate","1"];
                rep.SetParameterValue("facilityname", GblIQCare.AppLocation);
                                
                frmReportViewer theRepViewer = new frmReportViewer();
                theRepViewer.MdiParent = this.MdiParent;
                theRepViewer.Location = new Point(0, 0);
                theRepViewer.crViewer.ReportSource = rep;
                theRepViewer.Show();
                this.Close();

            }
        }

        private DataSet GetBINCard(int StoreId, int ItemsId, DateTime FromDate, DateTime ToDate)
        {
            
            ISCMReport binCard = (ISCMReport)ObjectFactory.CreateInstance("BusinessProcess.SCM.BSCMReport,BusinessProcess.SCM");
            return binCard.GetBINCard(StoreId, ItemsId, FromDate, ToDate, GblIQCare.AppLocationId);
        }

       

    }
}
