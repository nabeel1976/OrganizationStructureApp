using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OrganizationStructureApp
{
    public class MainForm : Form
    {
        private TreeView orgTreeView;
        private DataGridView employeesGridView;
        private Label currentDepartmentLbl;
        private string connectionString;

        public MainForm()
        {
            var dbPath = Path.Combine(Application.StartupPath, "OrganizationDB.sqlite");
            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);
            connectionString = $"Data Source={dbPath};Version=3;";
            InitializeUI();
            LoadOrganizationStructure();
        }

        private void InitializeUI()
        {
            this.Text = "نظام إدارة الهيكل التنظيمي";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            orgTreeView = new TreeView() { Location = new Point(20, 20), Size = new Size(300, 600) };
            orgTreeView.AfterSelect += OrgTreeView_AfterSelect;
            this.Controls.Add(orgTreeView);

            currentDepartmentLbl = new Label() { Location = new Point(340, 20), Size = new Size(620, 25), Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(currentDepartmentLbl);

            employeesGridView = new DataGridView() { Location = new Point(340, 60), Size = new Size(620, 560), ReadOnly = true, AllowUserToAddRows = false };
            this.Controls.Add(employeesGridView);
        }

        private void LoadOrganizationStructure()
        {
            orgTreeView.Nodes.Clear();
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var dt = new DataTable();
            SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT DepartmentId, DepartmentName, ParentDepartmentId FROM Departments", conn);
            da.Fill(dt);
            var nodes = new Dictionary<long, TreeNode>();
            foreach (DataRow row in dt.Rows)
            {
                var id = (long)row["DepartmentId"];
                nodes[id] = new TreeNode(row["DepartmentName"].ToString()) { Tag = id };
            }
            foreach (DataRow row in dt.Rows)
            {
                var id = (long)row["DepartmentId"];
                var pid = row["ParentDepartmentId"] as long?;
                if (pid.HasValue && nodes.ContainsKey(pid.Value))
                    nodes[pid.Value].Nodes.Add(nodes[id]);
                else
                    orgTreeView.Nodes.Add(nodes[id]);
            }
            orgTreeView.ExpandAll();
        }

        private void OrgTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentDepartmentLbl.Text = "القسم: " + e.Node.Text;
            LoadEmployees(e.Node.Tag as long? ?? 0);
        }

        private void LoadEmployees(long deptId)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT EmployeeName AS 'اسم الموظف', Position AS 'المنصب' FROM Employees WHERE DepartmentId = @d";
            cmd.Parameters.AddWithValue("@d", deptId);
            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            employeesGridView.DataSource = dt;
        }
    }
}
