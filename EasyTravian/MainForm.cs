using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EasyTravian.Properties;
using System.Deployment.Application;
using System.Diagnostics;
using EasyTravian.Types;

namespace EasyTravian
{
    public partial class MainForm : Form
    {
        TraviController controller = null;
        MapPainterProps mapPainterProps = new MapPainterProps();
        //List<MapElement> map = null;

        bool FirstActivate = true;
        ToolTip MapToolTip = new ToolTip();
        //DateTime AutobuildStarted;

        private DateTime LastAutoBuild = DateTime.MinValue;
        private DateTime LastAutoTrade = DateTime.MinValue;
        private DateTime LastAutoFarm = DateTime.MinValue;

        public MainForm()
        {
            InitializeComponent();
            
            Globals.Web = webBrowser;
            Globals.Web.ScriptErrorsSuppressed = true;

            controller = new TraviController();

            MapToolTip.ShowAlways = true;
            MapToolTip.ToolTipTitle = Globals.Translator["Information"];

            Text = Application.ProductName;

            if (ApplicationDeployment.IsNetworkDeployed)
                Text += " " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();

            tsmiOff.Enabled = false;

            if (!Globals.Cfg.Debugging)
                while (tcMain.TabPages.Count > 2)
                {
                    tcMain.TabPages.RemoveAt(2);
                }
        }

        private void LoadAll()
        {
            LoadVillages();
            gridResources.DataSource = controller.bsResources;
            gridProduction.DataSource = controller.bsProductions;
            gridBuildings.DataSource = controller.bsBuildings;
            gridResourceOveralls.DataSource = controller.bsResourceOverall;
            gridConstructions.DataSource = controller.bsConstruction;
            gridCanBuild.DataSource = controller.bsCanBuild;

            gridMarket.DataSource = controller.bsTraderItems;
            gridArmies.DataSource = controller.bsRallyPoints;
            gridFarmlists.DataSource = controller.bsFarmLists;

            controller.CheckTrials();

            LoadTroopList();
        }


        private void LoadVillages()
        {
            //string[] vs = controller.GetVillageNames();
            //Array.Sort(vs);

            bVillages.DropDownItems.Clear();
            foreach (VillageData v in controller.GetVillages())
            {
                ToolStripItem i = bVillages.DropDownItems.Add(v.ToString());
                i.Tag = v;
                lstbxVillagesSettings.Items.Add(v);
            }
            if (bVillages.DropDownItems.Count > 0)
                SelectVillage(bVillages.DropDownItems[0].Text);

            BuildingCopyFromComboBox.Items.Clear();
            VillageData[] villages = controller.GetVillages();
            foreach (VillageData v in villages)
                BuildingCopyFromComboBox.Items.Add(v);
            
            RefreshVillageDataOutside();
        }

        private void RefreshVillageDataOutside()
        {
            lstbxVillagesOutside.Items.Clear();
            foreach (VillageDataOutside vo in controller.GetVillagesOutside())
            {
                lstbxVillagesOutside.Items.Add(vo);
            }
        }

        private void bVillages_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SelectVillage( (VillageData)e.ClickedItem.Tag );
        }

        private void SelectVillage(VillageData village)
        {
            bVillages.Text = village.ToString();
            controller.SetActiveVillage(village.Id);
        }

        private void SelectVillage( string VillageName )
        {
            bVillages.Text = VillageName;
            controller.SetActiveVillageByName(VillageName);
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.SaveData();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Width = 980; Height = 760;

            // tcMain.TabIndex = 1;
            tcMain.SelectTab(1);

            Globals.Translator.TranslateForm(this);
        }

        private void tmrAutoBuild_Tick(object sender, EventArgs e)
        {
            //controller.Refresh();
            //LoadAll();

            DateTime now = DateTime.Now;
            tmrAuto.Enabled = false;
            try
            {
                try
                {
                    if (bAutoBuild.Checked
                        &&
                        now.Subtract(LastAutoBuild).TotalMinutes > Globals.Cfg.AutoBuildInterval)
                    {
                        LastAutoBuild = now;
                        controller.Build();
                    }

                    if (bAutoSend.Checked
                        &&
                        now.Subtract(LastAutoTrade).TotalMinutes > Globals.Cfg.AutoTradeInterval)
                    {
                        LastAutoTrade = now;
                        controller.DoTradeAll();
                    }

                    if (bAutoFarm.Checked
                        &&
                        now.Subtract(LastAutoFarm).TotalMinutes > Globals.Cfg.AutoFarmInterval)
                    {
                        LastAutoFarm = now;
                        controller.DoFarmAll();
                    }

                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        throw;
                    Globals.Logger.Log(ex.Message, LogType.ltDebug);
                }

            }
            finally
            {
                /*
                if (!Globals.Register.IsRegistered(TraviModule.Builder)
                    &&
                    DateTime.Now.Subtract(AutobuildStarted).Hours > 0)
                    bAutoBuild.Checked = false;
                else
                    tmrAuto.Enabled = true;
                 */

                Random rnd = new Random(DateTime.Now.Millisecond);
                tmrAuto.Interval = rnd.Next(180000);
                tmrAuto.Enabled = true;

                RefreshTimeDisplay(now);
            }

        }

        private void pnlMap_Paint(object sender, PaintEventArgs e)
        {
            mapPainterProps.zoom = trackBar1.Value;

            controller.DrawActMap(e.Graphics, new Point(1000, 1000), mapPainterProps);
            //e.Graphics.DrawEllipse(new Pen(Color.Red), new Rectangle(10, 10, 100, 100));
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            RedrawMap();
        }

        private void pnlMap_MouseMove(object sender, MouseEventArgs e)
        {
            MapElement me = controller.GetMapInfo(((Panel)sender).Bounds.Size, e.Location, trackBar1.Value);
            if (me != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Globals.Translator["Village"]);
                sb.Append(":");
                sb.Append(me.Village);
                sb.AppendFormat("({0}|{1})", me.X, me.Y);
                sb.Append("/n");
                sb.Append(Globals.Translator["Player"]);
                sb.Append(":");
                sb.Append(me.Player);
                sb.Append("/n");
                sb.Append(Globals.Translator["Alliance"]);
                sb.Append(":");
                sb.Append(me.Alliance);
                sb.Append("/n");
                sb.Append(Globals.Translator["Population"]);
                sb.Append(":");
                sb.Append(me.Population);
                sb.Append("/n");
                sb.Append(Globals.Translator["Tribe"]);
                sb.Append(":");
                sb.Append(Globals.Translator[((TribeType)me.Tid - 1).ToString()]);

                string s = "";
                //s += Globals.Translator["Village"] + ':' + me.Village + "(" + me.X.ToString() + "|" + me.Y.ToString() + ")" + '\n';
                //s += Globals.Translator["Player"] + ':' + me.Player + '\n';
                //s += Globals.Translator["Alliance"] + ':' + me.Alliance + '\n';
                //s += Globals.Translator["Population"] + ':' + me.Population + '\n';
                //s += Globals.Translator["Tribe"] + ':' + Globals.Translator[((TribeType)me.Tid - 1).ToString()];
                if (me.Terrain != null)
                {
                    sb.Append("/n");
                    sb.Append(Globals.Translator["Terrain"]);
                    sb.Append(":");
                    sb.Append(Globals.Translator[me.Terrain]);
                }
                    //s += '\n' + Globals.Translator["Terrain"] + ':' + Globals.Translator[me.Terrain];

                MapToolTip.SetToolTip(pnlMap, sb.ToString()); // s
            }
           
        }

        private void pnlMap_MouseClick(object sender, MouseEventArgs e)
        {
            controller.MapClicked(((Panel)sender).Bounds.Size, e.Location, trackBar1.Value);
            RedrawMap();
        }

        private void RedrawMap()
        {
            pnlMap.Invalidate();
        }

        private void Login()
        {
            using (LoginForm f = new LoginForm())
            {
                bool succlogin = false;
                while (!succlogin)
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        if (controller.Login())
                        {
                            succlogin = true;
                            LoadAll();
                        }
                    }
                    else
                    {
                        succlogin = true;
                        Close();
                    }
                }
            }

            Text = Globals.Cfg.UserName + '@' + Globals.Cfg.Server + ' ' + Text;

        }

        private void MapPainAllies()
        {
            if (controller.ActiveClans() != null)
            {
                cbMapAllies.Items.AddRange(controller.ActiveClans().ToArray());
            }
            mapPainterProps.Coloring = MapColoring.Ally;
        }

        private void tcMapProps_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tcMapProps.SelectedIndex)
            {
                case 0: //tribe
                    mapPainterProps.Coloring = MapColoring.Tribe;
                    break;
                case 1: //ally
                    MapPainAllies();
                    break;
                case 2: //popu
                    mapPainterProps.Coloring = MapColoring.Population;
                    break;
                default:
                    break;
            }
            RedrawMap();
        }

        private void bMapAlliesColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                    ((Button)sender).BackColor = cd.Color;
            }
        }

        private void bMapAlliesAdd_Click(object sender, EventArgs e)
        {
            if (cbMapAllies.Text != "")
            {
                mapPainterProps.Alliances[cbMapAllies.Text] = bMapAlliesColor.BackColor;
                DrawMapAlliesList();
                RedrawMap();
            }
        }

        private void DrawMapAlliesList()
        {
            lvMapAllies.Items.Clear();
            foreach (KeyValuePair<string, Color> item in mapPainterProps.Alliances)
            {
                ListViewItem li = new ListViewItem(item.Key);
                li.BackColor = item.Value;
                lvMapAllies.Items.Add(li);
            }
        }

        private void lvMapAllies_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void bMapAlliesRemove_Click(object sender, EventArgs e)
        {
            if (lvMapAllies.SelectedItems.Count > 0)
            {
                mapPainterProps.Alliances.Remove(lvMapAllies.SelectedItems[0].Text);
                DrawMapAlliesList();
                RedrawMap();
            }
        }

        private void gridResources_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            InitGridColumns();
        }

        private void InitGridColumns()
        {
            gridResources.Columns.Clear();

            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = Globals.Translator["Name"];
            col.ReadOnly = true;
            col.FillWeight = 60;
            gridResources.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.FillWeight = 20;
            col.DefaultCellStyle.ForeColor = Globals.Cfg.Colors("ReadOnly");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResources.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Target";
            col.HeaderText = Globals.Translator["Target level"];
            col.FillWeight = 20;
            col.DefaultCellStyle.BackColor = Globals.Cfg.Colors("Input"); // Globals.Cfg.clrInput;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResources.Columns.Add(col);

            gridResources.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void gridBuildings_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridBuildings.Columns.Clear();

            DataGridViewComboBoxColumn ccol = new DataGridViewComboBoxColumn();
            ccol.DataPropertyName = "Name";
            ccol.HeaderText = Globals.Translator["Name"];
            ccol.ReadOnly = false;
            ccol.FillWeight = 60;
            ccol.Items.Add(Globals.Translator[BuildingType.None.ToString()]);
            for (int i = 4; i < 40; i++)
                ccol.Items.Add( Globals.Translator[((BuildingType)i).ToString()]);
            ccol.DefaultCellStyle.BackColor = Globals.Cfg.Colors("Input");
            gridBuildings.Columns.Add(ccol);
            
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.FillWeight = 20;
            col.DefaultCellStyle.ForeColor = Globals.Cfg.Colors("ReadOnly");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridBuildings.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Target";
            col.HeaderText = Globals.Translator["Target level"];
            col.FillWeight = 20;
            col.DefaultCellStyle.BackColor = Globals.Cfg.Colors("Input");
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridBuildings.Columns.Add(col);

            gridBuildings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void gridBuildings_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (gridBuildings.Columns[e.ColumnIndex].DataPropertyName == "Name")
            {
                if ((int)gridBuildings.Rows[e.RowIndex].Cells[1].Value != 0)
                    e.Cancel = true;
            }
            if (gridBuildings.Columns[e.ColumnIndex].DataPropertyName == "Target")
            {
                if ((string)gridBuildings.Rows[e.RowIndex].Cells[0].Value == Globals.Translator[BuildingType.None.ToString()])
                    e.Cancel = true;
            }
             
        }

        private void gridBuildings_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (FirstActivate)
            {
                FirstActivate = false;
                Login();
            }
            /*
            for (int i = 1; i < tcMain.TabPages.Count; i++)
			{
                tcMain.TabPages[i].Text = ((TraviModule)i-1).ToString();
                if (!Globals.Register.IsRegistered((TraviModule)i-1))
                    tcMain.TabPages[i].Text += " (" + Globals.Translator["Trial"] + ")";
			}
            */
        }

        private void gridProduction_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridProduction.Columns.Clear();

            DataGridViewTextBoxColumn col;
            
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "TypeName";
            col.HeaderText = Globals.Translator["Resource"];
            col.ReadOnly = true;
            gridProduction.Columns.Add(col);
            
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Capacity";
            col.HeaderText = Globals.Translator["Storage capacity"];
            col.ReadOnly = true;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Stock";
            col.HeaderText = Globals.Translator["Available"];
            col.ReadOnly = true;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Producing";
            col.HeaderText = Globals.Translator["Producing"];
            col.ReadOnly = true;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "TargetPercent";
            col.HeaderText = Globals.Translator["Target %"];
            col.DefaultCellStyle.BackColor = Globals.Cfg.Colors("Input");
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ActPercent";
            col.HeaderText = Globals.Translator["Actual %"];
            col.ReadOnly = true;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);
            
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "FullnessPercent";
            col.HeaderText = Globals.Translator["Full %"];
            col.ReadOnly = true;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            gridProduction.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void gridResourceOveralls_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridResourceOveralls.Columns.Clear();

            DataGridViewTextBoxColumn col;
            /*
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Type";
            col.HeaderText = Globals.Translator["Type"];
            col.ReadOnly = true;
            gridResourceOveralls.Columns.Add(col);
             */ 

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Stock";
            col.HeaderText = Globals.Translator["Available"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResourceOveralls.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Producing";
            col.HeaderText = Globals.Translator["Producing"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResourceOveralls.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Capacity";
            col.HeaderText = Globals.Translator["Storage capacity"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResourceOveralls.Columns.Add(col);

            gridResourceOveralls.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void gridConstructions_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridConstructions.Columns.Clear();

            DataGridViewTextBoxColumn col;
            
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = Globals.Translator["Name"];
            col.ReadOnly = true;
            gridConstructions.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridConstructions.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ends";
            col.HeaderText = Globals.Translator["Ends"];
            col.ReadOnly = true;
            gridConstructions.Columns.Add(col);

            gridConstructions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void gridConstructions_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void gridCanBuild_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridCanBuild.Columns.Clear();

            DataGridViewTextBoxColumn col;

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = Globals.Translator["Name"];
            col.ReadOnly = true;
            col.FillWeight = 50;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 15;
            gridCanBuild.Columns.Add(col);

            /*
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Cost";
            col.HeaderText = Globals.Translator["Cost"];
            col.ReadOnly = true;
            gridCanBuild.Columns.Add(col);
            */

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Lumber";
            col.HeaderText = Globals.Translator["Lumber"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Clay";
            col.HeaderText = Globals.Translator["Clay"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Iron";
            col.HeaderText = Globals.Translator["Iron"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Crop";
            col.HeaderText = Globals.Translator["Crop"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            gridCanBuild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void tcMain_Selecting(object sender, TabControlCancelEventArgs e)
        {
            /*
            e.Cancel =
                !(
                e.TabPageIndex == 0
                ||
                (Globals.Register.IsRegistered((TraviModule)e.TabPageIndex-1))
                );
             */
        }


        private void btnBuildRefresh_Click(object sender, EventArgs e)
        {
            pnlBrowser.Enabled = false;
            try
            {
                controller.Refresh();
            }
            finally
            {
                pnlBrowser.Enabled = true;
            }
        }

        private void btnAutoBuild_Click(object sender, EventArgs e)
        {
            /*
            if (!Globals.Register.IsRegistered(TraviModule.Builder))
                MessageBox.Show(Globals.Translator["In the trial version the autobuild works for 1 hour only!"]);

            AutobuildStarted = DateTime.Now;

            tmrAuto.Enabled = bAutoBuild.Checked;
            //tabControl1.Enabled = !bAutoBuild.Checked;

             */
 
            if (bAutoBuild.Checked)
                MessageBox.Show(Globals.Translator["Please don't log on to this account elsewhere while autobuild is in use!"]);
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            pnlBrowser.Enabled = false;
            try
            {
                controller.Build();
            }
            finally
            {
                pnlBrowser.Enabled = true;
            }
        }

        private void registrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegisterForm f = new RegisterForm();
            f.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SettingsForm f = new SettingsForm())
            {
                f.ShowDialog();
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void gridCanBuild_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
        }

        private void gridCanBuild_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                if (!((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Buildable)
                    e.CellStyle.BackColor = Color.LightPink;
            }

            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Lumber > 
                    ((Production)controller.bsProductions[0]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }
            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Clay >
                    ((Production)controller.bsProductions[1]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }
            if (e.ColumnIndex == 4 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Iron >
                    ((Production)controller.bsProductions[2]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }
            if (e.ColumnIndex == 5 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Crop >
                    ((Production)controller.bsProductions[3]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }

            e.Handled = false;

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                controller.RefreshMap();
                RedrawMap();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                controller.ReadMap();
                RedrawMap();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSendMail_Click(object sender, EventArgs e)
        {
            controller.SendMail2CSRecipients(txtRecipients.Text,
                                             txtSubject.Text,
                                             txtBody.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RegisterForm f = new RegisterForm();
            f.ShowDialog();
        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            txtbxUri.Text = webBrowser.Url.AbsoluteUri;
        }

        private void btnClipboardUrl_Click(object sender, EventArgs e)
        {
            if (txtbxUri.Text != string.Empty)
                Clipboard.SetDataObject(txtbxUri.Text);
        }

        private void txtbxUri_Click(object sender, EventArgs e)
        {
            txtbxUri.SelectAll();
        }

        private void gridMarket_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                gridMarket.Columns.Clear();

                DataGridViewColumn col;

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "Active";
                col.HeaderText = Globals.Translator["Active"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                // col.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridMarket.Columns.Add(col);

                DataGridViewComboBoxColumn ccol = new DataGridViewComboBoxColumn();
                ccol.DataPropertyName = "SourceVillageId";
                ccol.HeaderText = Globals.Translator["Source"];
                ccol.ReadOnly = false;
                ccol.FillWeight = 60;
                foreach (VillageData v in controller.GetVillages())
                    ccol.Items.Add(v);
                // ccol.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                ccol.ValueMember = "Id";
                ccol.DisplayMember = "Nev";
                gridMarket.Columns.Add(ccol);

                ccol = new DataGridViewComboBoxColumn();
                ccol.DataPropertyName = "DestinationVillageId";
                ccol.HeaderText = Globals.Translator["Destination"];
                ccol.ReadOnly = false;
                ccol.FillWeight = 60;
                foreach (VillageData v in controller.GetVillages())
                    ccol.Items.Add(v);
                foreach (VillageDataOutside v in controller.GetVillagesOutside())
                    ccol.Items.Add(v);
                // ccol.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                ccol.ValueMember = "Id";
                ccol.DisplayMember = "Nev";
                gridMarket.Columns.Add(ccol);

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "ResourcesTypeLumber";
                col.HeaderText = Globals.Translator["Lumber"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                // col.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridMarket.Columns.Add(col);

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "ResourcesTypeClay";
                col.HeaderText = Globals.Translator["Clay"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                // col.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridMarket.Columns.Add(col);

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "ResourcesTypeIron";
                col.HeaderText = Globals.Translator["Iron"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                // col.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridMarket.Columns.Add(col);

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "ResourcesTypeCrop";
                col.HeaderText = Globals.Translator["Crop"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                // col.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridMarket.Columns.Add(col);

                ccol = new DataGridViewComboBoxColumn();
                ccol.DataPropertyName = "Type";
                ccol.HeaderText = Globals.Translator["Type"];
                ccol.ReadOnly = false;
                ccol.FillWeight = 60;
                ccol.DataSource = Enum.GetValues(typeof(TraderType));
                // ccol.ValueMember = "Id";
                // ccol.DisplayMember = "Nev";

                // for (int i = 0; i < 3; i++)
                    //ccol.Items.Add(Globals.Translator[((TraderType)i).ToString()]);
                // ccol.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridMarket.Columns.Add(ccol);

                DataGridViewTextBoxColumn tcol = new DataGridViewTextBoxColumn();
                tcol.DataPropertyName = "Value";
                tcol.HeaderText = Globals.Translator["Value (%)"];
                tcol.ReadOnly = false;
                tcol.FillWeight = 20;
                tcol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                tcol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                // tcol.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridMarket.Columns.Add(tcol);

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "AllowFullMerchantOnly";
                col.HeaderText = Globals.Translator["Full.M."];
                col.HeaderCell.ToolTipText = Globals.Translator["Allow full merchant only"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                gridMarket.Columns.Add(col);

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "MaxDestinationCapacity";
                col.HeaderText = Globals.Translator["Max.D."];
                col.HeaderCell.ToolTipText = Globals.Translator["Maximum to destination capacity"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                gridMarket.Columns.Add(col);

                gridMarket.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch
            { }
        }

        private void FormHide()
        {
            Hide();
            if (string.IsNullOrEmpty(notifyIcon1.Text))
                notifyIcon1.Text = Globals.Cfg.UserName + '@' + Globals.Cfg.Server;
            notifyIcon1.Visible = true;
        }
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                FormHide();
        }

        private void FormShow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            FormShow();
        }

        private void tSMIExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tSMIShow_Click(object sender, EventArgs e)
        {
            FormShow();
        }

        private void BuildingCopyFromComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            VillageData from = (VillageData)BuildingCopyFromComboBox.SelectedItem;
            controller.CopyBuildingsFrom(from, controller.GetVillage(controller.BuilderGUIActiveVillageId));
            gridBuildings.Refresh();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            TraderItem ti = controller.AddSendResource();
            if (ti != null)
            {
                controller.bsTraderItems.Position = controller.bsTraderItems.IndexOf(ti);
            }
            controller.bsTraderItems.ResetBindings(false);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            controller.bsTraderItems.RemoveCurrent();
            controller.bsTraderItems.ResetBindings(false);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            controller.DoTrade((TraderItem)controller.bsTraderItems.Current);
        }

        private void toolStripButton6_Click_1(object sender, EventArgs e)
        {
            controller.DoTradeAll();
        }

        private void tsbRallyRefresh_Click(object sender, EventArgs e)
        {
            controller.RefreshRallyPoints();
            controller.bsRallyPoints.ResetBindings(false);
        }

        private void gridArmies_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                gridArmies.Columns.Clear();

                DataGridViewTextBoxColumn tcol = new DataGridViewTextBoxColumn();
                tcol.DataPropertyName = "Section";
                tcol.HeaderText = Globals.Translator["Section"];
                tcol.FillWeight = 30;
                gridArmies.Columns.Add(tcol);

                tcol = new DataGridViewTextBoxColumn();
                tcol.DataPropertyName = "State";
                tcol.HeaderText = Globals.Translator["State"];
                tcol.FillWeight = 30;
                gridArmies.Columns.Add(tcol);

                tcol = new DataGridViewTextBoxColumn();
                tcol.DataPropertyName = "SourceVillageName";
                tcol.HeaderText = Globals.Translator["Source"];
                tcol.FillWeight = 60;
                gridArmies.Columns.Add(tcol);

                tcol = new DataGridViewTextBoxColumn();
                tcol.DataPropertyName = "DestinationVillageName";
                tcol.HeaderText = Globals.Translator["Destination"];
                tcol.FillWeight = 60;
                gridArmies.Columns.Add(tcol);

                tcol = new DataGridViewTextBoxColumn();
                tcol.DataPropertyName = "Arrival";
                tcol.HeaderText = Globals.Translator["Arrival"];
                tcol.FillWeight = 40;
                gridArmies.Columns.Add(tcol);

                gridArmies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch
            { }
        }

        private void demolitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Globals.Translator["Are you sure?"]) == DialogResult.OK)
            {
                controller.BuildingDemolition();
                gridBuildings.Refresh();
            }
        }

        private void demolitionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Globals.Translator["Are you sure?"]) == DialogResult.OK)
            {
                controller.ResourceDemolition();
                gridResources.Refresh();
            }
        }

        private void all10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.AllResourcesTo10();
            gridResources.Refresh();
        }

        private void gridResources_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Building bldng = (controller.bsResources[e.RowIndex] as Building);
                if (bldng. Level == 0)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Empty");
                }
                if (bldng.Level > bldng.Target)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Higher");
                }
                if (bldng.Target > bldng.Level)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Lower");
                }
                if (bldng.Demolition)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Demolish");
                }
            }
        }

        private void gridBuilder_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Building bldng = (controller.bsBuildings[e.RowIndex] as Building);
                if (bldng.Level == 0)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Empty");
                }
                if (bldng.Level > bldng.Target)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Higher");
                }
                if (bldng.Target > bldng.Level)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Lower");
                }
                if (bldng.Demolition)
                {
                    e.CellStyle.BackColor = Globals.Cfg.Colors("Demolish");
                }
                if (e.ColumnIndex == 1)
                {
                    if ((int)gridBuildings.Rows[e.RowIndex].Cells[e.ColumnIndex].Value > 0)
                        gridBuildings.Rows[e.RowIndex].Cells[0].Style.BackColor =  Globals.Cfg.Colors("DefaultBackground");
                }
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            controller.SendTroopsAll(controller.BuilderGUIActiveVillageId,
                ((VillageData)(bVillages.DropDownItems[0].Tag)).Props.Origin);
        }

        private void lstbxVillagesSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ((VillageData)(lstbxVillagesSettings.SelectedItem))...
        }

        private void LoadTroopList()
        {
            Troop[] troops = controller.GetTroops();
            foreach (Troop trup in troops)
	        {
                lstbxArmyTypeTroopSave.Items.Add(trup.Name);
        	}
            
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            // txtbxNewWWVillage.Text
            // VillageDataOutside 
            controller.AddVillageOutside(txtbxNewWWVillage.Text);
            RefreshVillageDataOutside();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (lstbxVillagesOutside.SelectedItem != null)
                if (lstbxVillagesOutside.SelectedItem is VillageDataOutside)
                {
                    controller.DeleteVillageOutside((VillageDataOutside)(lstbxVillagesOutside.SelectedItem));
                    RefreshVillageDataOutside();
                }
        }

        private void gridMarket_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if ((bool)gridMarket.Rows[e.RowIndex].Cells[0].Value == false)
                    gridMarket.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Globals.Cfg.Colors("Inactive");

                switch ((TraderType)gridMarket.Rows[e.RowIndex].Cells[7].Value)
                {
                    case TraderType.Feltöltés:
                        // e.CellStyle.BackColor = Color.FromArgb(255, 243, 151);
                        gridMarket.Rows[e.RowIndex].DefaultCellStyle.BackColor = Globals.Cfg.Colors("Feltoles");
                        break;
                    case TraderType.Fölözés:
                        gridMarket.Rows[e.RowIndex].DefaultCellStyle.BackColor = Globals.Cfg.Colors("Folozes");
                        break;
                }
            }
        }

        private void gridFarmlists_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                gridFarmlists.Columns.Clear();

                DataGridViewColumn col;

                col = new DataGridViewCheckBoxColumn();
                col.DataPropertyName = "Active";
                col.HeaderText = Globals.Translator["Active"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                // col.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridFarmlists.Columns.Add(col);

                DataGridViewComboBoxColumn ccol = new DataGridViewComboBoxColumn();
                ccol.DataPropertyName = "VillageId";
                ccol.HeaderText = Globals.Translator["Village"];
                ccol.ReadOnly = false;
                ccol.FillWeight = 60;
                foreach (VillageData v in controller.GetVillages())
                    ccol.Items.Add(v);
                ccol.ValueMember = "Id";
                ccol.DisplayMember = "Nev";
                gridFarmlists.Columns.Add(ccol);

                col = new DataGridViewTextBoxColumn();
                col.DataPropertyName = "FarmlistName";
                col.HeaderText = Globals.Translator["FarmlistName"];
                col.ReadOnly = false;
                col.FillWeight = 60;
                // col.DefaultCellStyle.BackColor = Globals.Cfg.clrInput;
                gridFarmlists.Columns.Add(col);

                col = new DataGridViewTextBoxColumn();
                col.DataPropertyName = "Id";
                col.HeaderText = Globals.Translator["Id"];
                col.HeaderCell.ToolTipText = Globals.Translator["a számot írd be a mintából pl: id('list158'), tehát ez esetben 158"];
                col.ReadOnly = false;
                col.FillWeight = 10;
                gridFarmlists.Columns.Add(col);

                // példa: id('raidListMarkAll158')
                col = new DataGridViewTextBoxColumn();
                col.DataPropertyName = "XPathAll";
                col.HeaderText = Globals.Translator["XPath SelectAll"];
                //col.HeaderCell.ToolTipText = Globals.Translator["példa: id('raidListMarkAll158'"];
                //col.ReadOnly = false;
                col.FillWeight = 60;
                gridFarmlists.Columns.Add(col);

                col = new DataGridViewTextBoxColumn();
                col.DataPropertyName = "XPathGo";
                col.HeaderText = Globals.Translator["XPath Go"];
                //col.ReadOnly = false;
                col.FillWeight = 60;
                gridFarmlists.Columns.Add(col);

                gridFarmlists.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch
            { }

        }

        private void gridFarmlists_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if ((e.RowIndex >= 0) && (gridFarmlists.Rows.Count > 0))
            {
                if (gridFarmlists.Rows[e.RowIndex].Cells.Count > 0)
                    if (gridFarmlists.Rows[e.RowIndex].Cells[0].Value != null)
                        if ((bool)gridFarmlists.Rows[e.RowIndex].Cells[0].Value == false)
                            gridFarmlists.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Globals.Cfg.Colors("Inactive");
            }
        }

        private void btnNewFarmlist_Click(object sender, EventArgs e)
        {
            FarmList fl = controller.AddFarmlist();
            if (fl != null)
            {
                controller.bsFarmLists.Position = controller.bsFarmLists.IndexOf(fl);
            }
            controller.bsFarmLists.ResetBindings(false);
        }

        private void btnDeleteFarmList_Click(object sender, EventArgs e)
        {
            controller.bsFarmLists.RemoveCurrent();
            controller.bsFarmLists.ResetBindings(false);
        }

        private void btnGoFarm1_Click(object sender, EventArgs e)
        {
            controller.DoFarm((FarmList)controller.bsFarmLists.Current);
        }

        private void btnGoFarmAll_Click(object sender, EventArgs e)
        {
            controller.DoFarmAll();
        }

        private void btnTradeDown_Click(object sender, EventArgs e)
        {
            controller.DoTradeDown();
        }

        private void btnTradeUp_Click(object sender, EventArgs e)
        {
            controller.DoTadeItemUp();
        }

        private void btnFarmDown_Click(object sender, EventArgs e)
        {
            controller.DoFarmItemDown();
        }

        private void btnFarmUp_Click(object sender, EventArgs e)
        {
            controller.DoFarmItemUp();
        }

        private void SetAutoOn()
        {
            bAutoBuild.Checked = true;
            bAutoSend.Checked = true;
            bAutoFarm.Checked = true;
            tsmiOn.Enabled = false;
            tsmiOff.Enabled = true;
        }

        private void SetAutoOff()
        {
            bAutoBuild.Checked = false;
            bAutoSend.Checked = false;
            bAutoFarm.Checked = false;
            tsmiOn.Enabled = true;
            tsmiOff.Enabled = false;
        }

        private void btnAutoStartStop_Click(object sender, EventArgs e)
        {
            SetAutoOn();
        }

        private void btnAutoStop_Click(object sender, EventArgs e)
        {
            SetAutoOff();
        }

        private void RefreshTimeDisplay(DateTime now)
        {
            TimeSpan ts;

            ts = TimeSpan.FromMinutes(Globals.Cfg.AutoBuildInterval) - 
                now.Subtract(LastAutoBuild);
            lblTimeRemainingBuild.Text =
                        string.Format("{0:D2}:{1:D2}:{2:D2}",
                                      ts.Hours, ts.Minutes, ts.Seconds);
            ts = TimeSpan.FromMinutes(Globals.Cfg.AutoTradeInterval) -
                now.Subtract(LastAutoTrade);
            lblTimeRemainingMarket.Text =
                        string.Format("{0:D2}:{1:D2}:{2:D2}", 
                                      ts.Hours, ts.Minutes, ts.Seconds);
            ts = TimeSpan.FromMinutes(Globals.Cfg.AutoFarmInterval) -
                now.Subtract(LastAutoFarm);
            lblTimeRemainingFarm.Text =
                        string.Format("{0:D2}:{1:D2}:{2:D2}",
                                      ts.Hours, ts.Minutes, ts.Seconds);
        }

        private void tsmiOn_Click(object sender, EventArgs e)
        {
            SetAutoOn();
        }

        private void tsmiOff_Click(object sender, EventArgs e)
        {
            SetAutoOff();
        }

    }
}