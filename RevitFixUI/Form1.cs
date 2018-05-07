using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitFixUI
{
    public partial class VersionSelectionForm : Form
    {
        public VersionSelectionForm(List<RevitVersion> revitVersions)
        {
            InitializeComponent();
            foreach (RevitVersion v in revitVersions)
            {
                comboBox1.Items.Add(v);
            }
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            
        }

        private void RevitSelectionForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonFix_Click(object sender, EventArgs e)
        {
            string errors = "";
            int error_count = 0;
            RevitVersion selectedVersion = (RevitVersion)comboBox1.SelectedItem;
            List<String> uistate_files = selectedVersion.FindUIStateFiles();
            if (uistate_files.Count != 0)
            {
                List<String> renamed = selectedVersion.RenameUIStateFiles(uistate_files);
            } else
            {
                errors += "- UIState files not found\n";
                error_count ++;
            }

            string registry_path = Program.registry_path;

            if (checkBoxHard.Checked)
                registry_path = Program.registry_path_hard;

            bool reg_renamed = selectedVersion.RenameRegWorkspace(registry_path);
            if (reg_renamed == false)
            {
                errors += "- Registry key not found\n";
                error_count++;
            }

            MessageBoxIcon message_icon;
            string message_text = "";
            bool exit = false;

            if (error_count == 0)
            {
                message_icon = MessageBoxIcon.None;
                message_text = "Success!\nRevit " + selectedVersion.Version + " UI fixed";
                exit = true;
            } else if (error_count == 2)
            {
                message_icon = MessageBoxIcon.Error;
                message_text = "Error:\n" + errors;
            } else
            {
                
                message_icon = MessageBoxIcon.Warning;
                message_text = "Completed with errors:\n" + errors;
            }
            MessageBox.Show(message_text, Program.title, MessageBoxButtons.OK, message_icon);
            if (exit == true)
                Application.Exit();

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
