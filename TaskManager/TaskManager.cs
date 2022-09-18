using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Management;
using System.Dynamic;
using System.Diagnostics;

namespace TaskManager
{
    public partial class TaskManager : Form
    {
        public TaskManager()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);


        private void barraSuperior_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void TaskManager_Load(object sender, EventArgs e)
        {
            renderProcessesOnListView();
        }

        /// <summary>
        /// Este método representa todos los procesos de Windows en un ListView con algunos valores e iconos.
        /// </summary>
        public void renderProcessesOnListView()
        {
            Process[] processList = Process.GetProcesses();

            ImageList Imagelist = new ImageList();
            foreach (Process process in processList)
            {

                string status = (process.Responding == true ? "Responding" : "Not responding");

                dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);

                //string cpuInfo = "";
                
                //while (process.HasExited != false)

                //{
                //     cpuInfo = process.TotalProcessorTime.ToString();
                //}

                string[] row = {

                    process.ProcessName,
                    process.Id.ToString(),
                    status,
                    extraProcessInfo.Username,
                    BytesToReadableValue(process.PrivateMemorySize64),
                    extraProcessInfo.Description
                };

                      


                    try
                    {
                        Imagelist.Images.Add(
                            process.Id.ToString(),
                            Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap()
                        );
                    }
                    catch { }

                    // Crea un nuevo elemento para agregar a la vista de lista que espera la fila de información como primer argumento
                    ListViewItem item = new ListViewItem(row)

                    {
                        // Establezca el ImageIndex del elemento como el mismo definido en el try-catch anterior
                        ImageIndex = Imagelist.Images.IndexOfKey(process.Id.ToString())

                    };

                    // Agrega el artículo
                    listView1.Items.Add(item);
                }

                // Configura la lista de imágenes de su lista para ver la lista creada anteriormente :)
                listView1.LargeImageList = Imagelist;
                listView1.SmallImageList = Imagelist;

            }
        
        /// <summary>
        /// Método que convierte bytes a su valor legible por humanos
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string BytesToReadableValue(long number)
        {
            List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < suffixes.Count; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return (number / (int)Math.Pow(1024, i)) + suffixes[i];
                }
            }

            return number.ToString();
        }

        /// <summary>
        /// Devuelve un objeto Expando con la descripción y el nombre de usuario de un proceso del ID del proceso.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public ExpandoObject GetProcessExtraInformation(int processId)
        {
            // Consultar el proceso Win32
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            // Crea un objeto dinámico para almacenar algunas propiedades en él.
            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username = "Unknown";

            foreach (ManagementObject obj in processList)
            {
                // Retornar el nombre de usuario
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // Retornar Username
                    response.Username = argList[0];

                }

                // Retornar la descripción del proceso (si existe)
                if (obj["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
                        response.Description = info.FileDescription;
                    }
                    catch { }
                }
            }

            return response;
        }

        private void BTNProcesos_MouseHover(object sender, EventArgs e)
        {
            panelBTNProcesos.Visible = true;
        }

        private void BTNProcesos_MouseLeave(object sender, EventArgs e)
        {
            panelBTNProcesos.Visible = false;
        }

        private void BTNRendimiento_Click(object sender, EventArgs e)
        {
            openChildForm(new panelRendimiento());
        }

        private void BTNRendimiento_MouseHover(object sender, EventArgs e)
        {
            panelBTNRendimiento.Visible = true;
        }

        private void BTNRendimiento_MouseLeave(object sender, EventArgs e)
        {
            panelBTNRendimiento.Visible = false;
        }

        private void openChildForm(object childForm)
        {
            if (this.panelCentral.Controls.Count > 0)
                this.panelCentral.Controls.RemoveAt(0);
            Form fh = childForm as Form;
            fh.TopLevel = false;
            fh.Dock = DockStyle.Fill;
            this.panelCentral.Controls.Add(fh);
            this.panelCentral.Tag = fh;
            fh.Show();
        }

        private void BTNMenu_Click(object sender, EventArgs e)
        {
            BTNProcesos.Visible = !BTNProcesos.Visible;
            BTNRendimiento.Visible = !BTNRendimiento.Visible;
        }

        private void BTNMenu_MouseHover(object sender, EventArgs e)
        {
            panelBTNMenu.Visible = true;
        }

        private void BTNMenu_MouseLeave(object sender, EventArgs e)
        {
            panelBTNMenu.Visible = false;
        }

        public void BTNEndTask_Click(object sender, EventArgs e)
        {
            try
            {
                //Process[listView1.SelectedItems].kill();
                renderProcessesOnListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BTNRefresh_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            renderProcessesOnListView();
        }
    }
}