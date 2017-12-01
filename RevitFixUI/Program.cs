using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using RevitFixUI;

namespace RevitFixUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public readonly static string appdata = @"%appdata%\Autodesk\Revit\";
        public const string directory_regex = @"^Autodesk Revit (20[0-9]{2})$";
        public const string timestamp_format = "yyMMdd_HHmmss";
        public const string registry_path = @"SOFTWARE\Autodesk\Revit\Autodesk Revit {0}\Workspace";
        public static string path_appdata = Environment.ExpandEnvironmentVariables(appdata);
        public const string title = "RevitFixUI";
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool dir_appdata_exists = Directory.Exists(path_appdata);

            if (dir_appdata_exists == false) {
                MessageBox.Show("Revit directory wasn't found\n" + path_appdata, title + " - Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<RevitVersion> versions = FindRevitVersions();

            Application.Run(new VersionSelectionForm(versions));
            
        }

        static List<RevitVersion> FindRevitVersions()
        {
            string[] directories = Directory.GetDirectories(path_appdata);
            List<RevitVersion> versions = new List<RevitVersion>();
            
            foreach (var d in directories)
            {
                Match m = Regex.Match(Path.GetFileName(d), directory_regex, RegexOptions.IgnoreCase);

                if (m.Groups.Count < 2) continue;

                versions.Add(new RevitVersion(m.Groups[1].ToString(), d) );
            }

            return versions;
        }

        public static void RegCopyTo(this RegistryKey src, RegistryKey dst)
        {
           
            // copy the values
            foreach (var name in src.GetValueNames())
            {
                dst.SetValue(name, src.GetValue(name), src.GetValueKind(name));
            }

            // copy the subkeys
            foreach (var name in src.GetSubKeyNames())
            {
                using (var srcSubKey = src.OpenSubKey(name, false))
                {
                    var dstSubKey = dst.CreateSubKey(name);
                    Program.RegCopyTo(srcSubKey, dstSubKey);
                }
            }
        }
    }

    public class RevitVersion
    {
        public string Version { get; set; }
        public string DirPath { get; set; }
        public string RegisterPath { get; set; }

        public RevitVersion(string version, string dirpath)
        {
            Version = version; DirPath = dirpath;
        }

        public RevitVersion(string version, string dirpath, string registerpath)
        {
            Version = version; DirPath = dirpath; RegisterPath = registerpath;
        }

        public override string ToString()
        {
            return Version;
        }

        public List<String> RenameUIStateFiles(List<String> uistate_files)
        { 
            List<String> renamed = new List<String>();

            foreach (string f in uistate_files)
            {
                string newfilename = Path.Combine(Path.GetDirectoryName(f), Path.GetFileNameWithoutExtension(f)
                   + "_backup_" + DateTime.Now.ToString(Program.timestamp_format) + Path.GetExtension(f));
                try
                {
                    System.IO.File.Move(f, newfilename);
                    renamed.Add(newfilename);
                } catch
                {
                    Console.WriteLine("RenameUIStateFiles exception: " + f);
                }
            }
            return renamed;
        }

        public List<string> FindUIStateFiles()
        {
            string[] directories = Directory.GetDirectories(this.DirPath);
            List<string> uistate_files = new List<string>();

            foreach (var d in directories)
            {
                bool ismatch = Regex.IsMatch(Path.GetFileName(d), @"^([A-Z]{3})$");

                if (ismatch == false)
                    continue;

                string path_uistate = Path.Combine(d, "UIState.dat");

                if (File.Exists(path_uistate) == false)
                    continue;

                uistate_files.Add(path_uistate);
            }

            return uistate_files;
        }


        public bool RenameRegWorkspace()
        {
            string key_src_string = String.Format(Program.registry_path, this.Version);
            var key_src = Registry.CurrentUser.OpenSubKey(key_src_string);

            string key_dst_string = String.Format(Program.registry_path, this.Version)
                + "_backup_" + DateTime.Now.ToString(Program.timestamp_format);
            var key_dst = Registry.CurrentUser.OpenSubKey(key_dst_string);

            if (key_src == null)
                return false;

            if (key_dst == null)
            {
                key_dst = Registry.CurrentUser.CreateSubKey(key_dst_string);
            }

            try
            {
                Program.RegCopyTo(key_src, key_dst);
                Registry.CurrentUser.DeleteSubKeyTree(key_src_string);
                return true;
            } catch
            {
                return false;
            }
        }

        
    }
}
