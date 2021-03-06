﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace proyecto_BDA
{
    public partial class Form1 : Form
    {
        private BaseDeDatos BaseDeDatos { get; set; }
        public Form1()
        {
            InitializeComponent();
            Habilita(false);
        }

        private void nuevoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog open = new SaveFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                BaseDeDatos = new BaseDeDatos(open.FileName);
                Habilita(true);
            }
            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if(BaseDeDatos != null && BaseDeDatos.NombreBaseDeDatos != null)
            {
                string s = BaseDeDatos.NombreBaseDeDatos;
                label_bd.Text = s.Substring(s.LastIndexOf("\\") + 1);

                list_tablas.Items.Clear();
                combobox_tablas_atributos.Items.Clear();

                foreach (var item in BaseDeDatos.Tablas)
                {
                    string[] subitems = new string[] {item.Key, item.Value };
                    ListViewItem list = new ListViewItem(subitems);
                    list_tablas.Items.Add(list);

                    combobox_tablas_atributos.Items.Add(item.Key);
                }
            }
            else
            {
                label_bd.Text = "";
                list_tablas.Items.Clear();
            }
        }

        private void abrirToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog open = new FolderBrowserDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                BaseDeDatos = new BaseDeDatos(open.SelectedPath);
                Habilita(true);
                combobox_indice.SelectedIndex = 0;
                combobox_tipo.SelectedIndex = 0;
                Invalidate();
            }
        }

        private void cerrarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BaseDeDatos = null;
            Invalidate();
        }

        private void boton_agregar_tabla_Click(object sender, EventArgs e)
        {
            if (BaseDeDatos != null)
            {
                if (textbox_agregar_tabla.Text != "")
                {
                    if(BaseDeDatos.AgregaTabla(textbox_agregar_tabla.Text))
                    {
                        textbox_agregar_tabla.Text = "";
                        Invalidate();
                    }
                }
            }
        }

        private void boton_modificar_tabla_Click(object sender, EventArgs e)
        {
            if(textbox_actualizar_tabla.Text != "" && list_tablas.SelectedItems.Count > 0)
            {
                string nombreAnterior = list_tablas.SelectedItems[0].Text;
                string nombreNuevo = textbox_actualizar_tabla.Text;

                BaseDeDatos.ModificaNombreTabla(nombreAnterior, nombreNuevo);

                textbox_actualizar_tabla.Text = "";
                label_eliminar_tabla.Text = "-";
                Invalidate();
            }
        }

        private void list_tablas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list_tablas.SelectedItems.Count > 0)
            {
                string nombre = list_tablas.SelectedItems[0].Text;
                label_eliminar_tabla.Text = textbox_actualizar_tabla.Text = nombre;
            }
        }

        private void modificar_bd_Click(object sender, EventArgs e)
        {
            if (textbox_modificar_bd.Text != "")
            {
                
                int index_path = BaseDeDatos.NombreBaseDeDatos.LastIndexOf("\\");
                string nombreNuevo = BaseDeDatos.NombreBaseDeDatos.Substring(0, index_path+1) + textbox_modificar_bd.Text;

                BaseDeDatos.NombreBaseDeDatos = nombreNuevo;

                textbox_modificar_bd.Text = "";
                Invalidate();
            }
        }

        private void boton_eliminar_bd_Click(object sender, EventArgs e)
        {
            if (BaseDeDatos != null && 
                MessageBox.Show("¿Estás seguro de eliminar la Base de Datos?", 
                "Confirmar eliminación", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Directory.Delete(BaseDeDatos.NombreBaseDeDatos, true);
                BaseDeDatos = null;
                Invalidate();
            }
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void boton_eliminar_tabla_Click(object sender, EventArgs e)
        {
            if (list_tablas.SelectedItems.Count > 0)
            {
                string tabla = list_tablas.SelectedItems[0].Text;
                BaseDeDatos.EliminaTabla(tabla);
                textbox_actualizar_tabla.Text = "";
                label_eliminar_tabla.Text = "-";
                Invalidate();
            }
        }

        private void combobox_tablas_atributos_SelectedIndexChanged(object sender, EventArgs e)
        {
            diccionario_atributos.Columns.Clear();
            diccionario_atributos.DataSource = BaseDeDatos.ObtenAtributos(combobox_tablas_atributos.SelectedItem.ToString());
        }

        private void boton_agregar_atributo_Click(object sender, EventArgs e)
        {
            if (!ModificaAtributos())
                return;

            TipoLlave llave = TipoLlave.SinLlave;
            string nomTabla = combobox_tablas_atributos.SelectedItem.ToString();
            switch (combobox_indice.SelectedIndex)
            {
                case 0: llave = TipoLlave.SinLlave; break;
                case 1: llave = TipoLlave.Primaria; break;
                case 2: llave = TipoLlave.Foranea; break;
            }

            Atributo atributo = new Atributo(textbox_agregar_atributo.Text, combobox_tipo.SelectedItem.ToString(), int.Parse(textbox_longitud.Text), llave);
            BaseDeDatos.AgregaAtributo(nomTabla, atributo);
            diccionario_atributos.DataSource = BaseDeDatos.ObtenAtributos(combobox_tablas_atributos.SelectedItem.ToString());
        }

        private void combobox_tipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            textbox_longitud.Enabled = combobox_tipo.SelectedIndex == 2;
            textbox_longitud.Text = !textbox_longitud.Enabled ? "4" : "";
        }

        private void diccionario_atributos_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1)
                return;

            textbox_agregar_atributo.Text = diccionario_atributos.Rows[e.RowIndex].Cells[0].Value.ToString();

            switch (diccionario_atributos.Rows[e.RowIndex].Cells[1].Value)
            {
                case "Entero": combobox_tipo.SelectedIndex = 0; break;
                case "Flotante": combobox_tipo.SelectedIndex = 1; break;
                case "Cadena": combobox_tipo.SelectedIndex = 2; break;
            }

            textbox_longitud.Text = diccionario_atributos.Rows[e.RowIndex].Cells[2].Value.ToString();

            switch (diccionario_atributos.Rows[e.RowIndex].Cells[3].Value)
            {
                case "Ninguna": combobox_indice.SelectedIndex = 0; break;
                case "Primaria": combobox_indice.SelectedIndex = 1; break;
                case "Foránea": combobox_indice.SelectedIndex = 2; break;
            }
        }

        private void boton_modificar_atributo_Click(object sender, EventArgs e)
        {
            int numTupla = diccionario_atributos.CurrentCell.RowIndex;
            bool res = ModificaAtributos() && numTupla >= 0;

            if (!res)
                return;

            TipoLlave llave = TipoLlave.SinLlave;
            string nomTabla = combobox_tablas_atributos.SelectedItem.ToString();

            switch (combobox_indice.SelectedIndex)
            {
                case 0: llave = TipoLlave.SinLlave; break;
                case 1: llave = TipoLlave.Primaria; break;
                case 2: llave = TipoLlave.Foranea; break;
            }

            Atributo atributo = new Atributo(textbox_agregar_atributo.Text, combobox_tipo.SelectedItem.ToString(), int.Parse(textbox_longitud.Text), llave);
            
            if (BaseDeDatos.ModificaAtributo(nomTabla, numTupla, atributo))
                diccionario_atributos.DataSource = BaseDeDatos.ObtenAtributos(combobox_tablas_atributos.SelectedItem.ToString());
            else
                MessageBox.Show("No puedes eliminar y modificar en cascada", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /**
         * Habilita/deshabilita los botones/controles del programa. 
         * Este método es preventivo ;v
         */
        private void Habilita(bool valor)
        {
            boton_agregar_tabla.Enabled = valor;
            boton_modificar_tabla.Enabled = valor;
            boton_eliminar_tabla.Enabled = valor;
            modificar_bd.Enabled = valor;
            boton_eliminar_bd.Enabled = valor;
            combobox_tablas_atributos.Enabled = valor;
            textbox_agregar_atributo.Enabled = valor;
            combobox_tipo.Enabled = valor;
            textbox_longitud.Enabled = valor;
            combobox_indice.Enabled = valor;
            boton_agregar_atributo.Enabled = valor;
            boton_modificar_atributo.Enabled = valor;
            boton_eliminar_atributo.Enabled = valor;
            diccionario_atributos.ReadOnly = true;
        }

        private void boton_eliminar_atributo_Click(object sender, EventArgs e)
        {
            int numTupla = diccionario_atributos.CurrentCell.RowIndex;

            bool res = combobox_tablas_atributos.SelectedIndex >= 0 && combobox_indice.SelectedIndex >= 0;
            res &= numTupla >= 0;

            if (!res)
            {
                MessageBox.Show("Por favor escoge un atributo a eliminar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string nomTabla = combobox_tablas_atributos.SelectedItem.ToString();
            BaseDeDatos.EliminaAtributo(nomTabla, numTupla);
            diccionario_atributos.DataSource = BaseDeDatos.ObtenAtributos(combobox_tablas_atributos.SelectedItem.ToString());
        }

        private bool ModificaAtributos()
        {
            bool res = true;

            if (string.IsNullOrEmpty(textbox_agregar_atributo.Text))
            {
                MessageBox.Show("No puedes insertar un atributo sin nombre. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                res = false;
            }

            if (res && string.IsNullOrEmpty(textbox_longitud.Text))
            {
                MessageBox.Show("No puedes insertar un atributo sin longitud. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                res = false;
            }

            if (res && combobox_tablas_atributos.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor selecciona una tabla", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                res = false;
            }

            if (res && combobox_indice.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor selecciona un tipo de llave", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                res = false;
            }

            return res;
        }
    }
}