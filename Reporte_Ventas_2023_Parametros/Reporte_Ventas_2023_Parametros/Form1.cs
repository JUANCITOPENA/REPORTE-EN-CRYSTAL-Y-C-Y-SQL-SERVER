using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Reporte_Ventas_2023_Parametros
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }
        SqlConnection conexion = new SqlConnection("Data Source=ADVISERTECNOLOG;Initial Catalog=VENTAS_VEHICULOS_TOYOTA_2023_JPV_0;User ID=JUANCITO;Password=123456;");

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnVer_Click(object sender, EventArgs e)
        {
            DateTime fechainicio = DateTime.Parse(dtpFecha_Inicial.Value.ToString());
            DateTime fechafin = DateTime.Parse(dtpFecha_Final.Value.Date.ToString());

            ReporteVentas_fechas reporte = new ReporteVentas_fechas();
            reporte.fechainicial = fechainicio;
            reporte.fechafinal = fechafin;

            reporte.ShowDialog();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Estás seguro de que deseas salir?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Establecer el DataGridView como de solo lectura
                dataGridView1.ReadOnly = true;

                // Consulta SQL para obtener datos de una tabla de la base de datos
                string consulta = "SELECT ID_Venta, Fecha_Venta, Pais, Cliente, Vendedor, Categoria, Modelo, Precio_ventas, Cantidad, Total FROM VW_Ventas_PorFecha_v_2023_2";
                SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);

                // Crear un nuevo DataTable y llenarlo con los datos de la base de datos
                DataTable dt = new DataTable();
                adaptador.Fill(dt);

                // Establecer el DataTable como origen de datos para el DataGridView
                dataGridView1.DataSource = dt;

                // Calcular el monto total y la cantidad basados en los datos del DataTable
                var totals = CalculateTotalAmountAndQuantity(dt);
                decimal totalAmount = totals.Item1; // Obtener totalAmount del Tuple
                lblTotalAmount.Text = $"Monto Total: {totalAmount:C}";
                lblTotalInvoices.Text = $"Total Facturas: {dt.Rows.Count}";
                lblTotalQuantity.Text = $"Cantidad Vendidos: {totals.Item2}";

                // Centrar el formulario en la pantalla
                this.CenterToScreen();
            }
            catch (Exception ex)
            {
                // Mostrar un mensaje de error si ocurre una excepción durante la carga del formulario
                MessageBox.Show($"Se produjo un error en Form_Load: {ex.Message}\n\nTrace: {ex.StackTrace}");
            }
        }


        private Tuple<decimal, int> CalculateTotalAmountAndQuantity(DataTable dataTable)
        {
            decimal totalAmount = 0;
            int totalQuantity = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                totalAmount += Convert.ToDecimal(row["Total"]);

                // Asegúrate de que la columna "Cantidad" existe y no es nula
                if (row["Cantidad"] != DBNull.Value)
                {
                    totalQuantity += Convert.ToInt32(row["Cantidad"]);
                }
            }

            return Tuple.Create(totalAmount, totalQuantity);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text;
            DataTable dt = (DataTable)dataGridView1.DataSource;

            if (!string.IsNullOrEmpty(filtro))
            {
                string expresion = string.Format("Cliente LIKE '%{0}%' OR Vendedor LIKE '%{0}%' OR Modelo LIKE '%{0}%'", filtro);
                dt.DefaultView.RowFilter = expresion;
            }
            else
            {
                dt.DefaultView.RowFilter = "";
            }

            UpdateLabels(); // Call the method to update labels after applying the filter
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Limpiar el contenido del cuadro de texto de búsqueda
            txtBuscar.Text = "";
            // Remove the filter by setting RowFilter to an empty string
            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = "";

            // Update labels after clearing the filter
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            // Calculate and display the total amount and total quantity based on the displayed rows
            decimal totalAmount = 0;
            int totalQuantity = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // Check if the row is visible (filtered in) and not a new row (if applicable)
                if (row.Visible && !row.IsNewRow)
                {
                    totalAmount += Convert.ToDecimal(row.Cells["Total"].Value);

                    // Ensure the "Cantidad" column exists and is not null
                    if (row.Cells["Cantidad"].Value != null)
                    {
                        totalQuantity += Convert.ToInt32(row.Cells["Cantidad"].Value);
                    }
                }
            }

            lblTotalAmount.Text = $"Monto Total: {totalAmount:C}";
            lblTotalInvoices.Text = $"Total Facturas: {dataGridView1.Rows.Cast<DataGridViewRow>().Count(row => row.Visible && !row.IsNewRow)}";
            lblTotalQuantity.Text = $"Cantidad Vendidos: {totalQuantity}";
        }
    }
}
