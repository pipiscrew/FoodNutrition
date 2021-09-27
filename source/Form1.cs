using DBManager.DBASES;
using Finisar.SQLite;
using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace FoodNutrition
{
    public partial class Form1 : Form
    {
        internal static SQLiteClass db = null;

        public Form1()
        {
            InitializeComponent();

            this.Text = Application.ProductName + " v" + Application.ProductVersion;

            SQLiteException exErrorCode;
            db = new SQLiteClass("Data Source=" + Application.StartupPath + "\\dbase.db;Version=3;", out exErrorCode);

            if (db.GetConnection() == null)
            {
                MessageBox.Show(exErrorCode.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = false;
                return;
            }

            lblCount.Text = "7944 products";
        }

        #region " Fill Combos "
        bool updating;
        internal void FillFoods()
        {
            updating = true;
            DataTable dT = db.GetDATATABLE("select fdc_id, description fname from food order by description");

            cmbFood.DisplayMember = "fname";
            cmbFood.ValueMember = "fdc_id";

            cmbFood.DataSource = dT;

            cmbFood.SelectedIndex = -1;
            updating = false;
        }

        private void cmbFood_MouseClick(object sender, MouseEventArgs e)
        {
            if (cmbFood.Items.Count == 0)
                FillFoods();
        }

        internal void FillIngredients()
        {
            updating = true;
            DataTable dT = db.GetDATATABLE("select id, name || ' (' || lower(unit_name) || ')' as iname from nutrient order by rank");


            cmbIng.DisplayMember = "iname";
            cmbIng.ValueMember = "id";

            cmbIng.DataSource = dT;

            cmbIng.SelectedIndex = -1;
            updating = false;
        }

        private void cmbIng_MouseClick(object sender, MouseEventArgs e)
        {
            if (cmbIng.Items.Count==0)
                FillIngredients();
        }
        //https://stackoverflow.com/questions/2259067/override-winforms-combobox-autocomplete-suggest-rule
        #endregion 

        private void cmbFood_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFood.SelectedValue == null || updating)
                return;

            Cursor = Cursors.WaitCursor;

            dg.Visible = false;

            string sort = chSort.Checked ? "amount desc" : "n.rank";
            string sql = string.Format(@"SELECT
                            n.name as ingredient, amount  || ' ' || unit_name as amount2
                             from 
                            food f
                            left join food_nutrient fn on fn.fdc_id = f.fdc_id
                            left join nutrient n on n.id =fn.nutrient_id

                            where f.fdc_id = {0} and amount > 0
                            order by {1}", cmbFood.SelectedValue, sort);

            dg.DataSource = db.GetDATATABLE(sql);

            dg.Visible = true;

            lblCount.Text = dg.Rows.Count.ToString();

            Cursor = Cursors.Default;
        }

        private void cmbIng_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIng.SelectedValue == null || updating)
                return;

            Cursor = Cursors.WaitCursor;

            dg.Visible = false;

            string sql = string.Format(@"SELECT
                            f.description as food, amount || ' ' || unit_name as amount2
                             from 
                            food f
                            left join food_nutrient fn on fn.fdc_id = f.fdc_id
                            left join nutrient n on n.id =fn.nutrient_id

                            where n.id = {0} and amount > 0
                            order by amount desc", cmbIng.SelectedValue);

            dg.DataSource = db.GetDATATABLE(sql);

            dg.Visible = true;

            lblCount.Text = dg.Rows.Count.ToString();

            Cursor = Cursors.Default;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://fdc.nal.usda.gov/");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (db != null)
                db.Dispose();
        }

        private void chSort_CheckedChanged(object sender, EventArgs e)
        {
            if (chSort.Checked)
                chSort.Text = "by amount";
            else
                chSort.Text = "predefined sort";

            cmbFood_SelectedIndexChanged(null, null);
        }


    }
}
