﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace proyecto_BDA
{
    class BaseDeDatos
    {
        /**
         * Clase que permite manejar y coleccionar archivos en
         * un repositorio (carpeta). 
         *   ________________________
         *  /                        \
         * |                          |
         * |\________________________/|
         * |                          |
         * |\________________________/|
         * |                          |
         * |\________________________/|
         * |                          |
         * \__________________________/
         *
         **/

        private string nombreBaseDeDatos;
        // Nombre de la carpeta con los archivos de datos.
        public string NombreBaseDeDatos
        {
            // Obtiene el nombre de la base de datos.
            get => nombreBaseDeDatos;
            
            // Modifica el nombre de la base de datos.
            // Si éste cambia, se debe de renombrar el
            // directorio.
            set
            {
                Directory.Move(nombreBaseDeDatos, value);
                nombreBaseDeDatos = value;
                this.LeeTablas();
            }
        }

        // Claves de las tablas y el nombre de su archivo
        // correspondiente.
        public SortedDictionary<string, string> Tablas { get; }
    
        /**
         * Crea una nueva base de datos y se le asigna su
         * nombre correspondiente. Si no existe, crea una
         * nueva, de lo contrario, lee la base de datos
         * existente.
         **/
        public BaseDeDatos(string nombre)
        {
            nombreBaseDeDatos = nombre;
            Tablas = new SortedDictionary<string, string>();

            if (Directory.Exists(NombreBaseDeDatos))
                // Ya existe una base de datos con ese nombre.
                LeeTablas();
            else
                // No existe, por lo tanto, se crea una nueva.
                Directory.CreateDirectory(NombreBaseDeDatos);
        }

        /**
         * Lee el conjunto de tablas que contiene la base de datos. 
         **/
        private void LeeTablas()
        {
            // Obtiene un listado del nombre de los archivos de datos
            // de cada tabla en la base de datos.
            var archivosDeDatos = Directory.EnumerateFiles(NombreBaseDeDatos, "*.dat", SearchOption.AllDirectories);

            foreach (var archivoDeDatos in archivosDeDatos)
            { 
                // Obtiene el nombre de la tabla.
                string nomTabla = Path.GetFileName(archivoDeDatos).Replace(".dat", "");
                Tablas[nomTabla] = archivoDeDatos;
            }
        }

        /**
         * Agrega una tabla nueva a la base de datos, siempre y
         * cuando no exista una con el mismo nombre.
         **/
        public bool AgregaTabla(string nomTabla)
        {
            // Verifica si la tabla no existe, de no ser así,
            // no se genera un archivo de datos nuevo.
            bool res = !Tablas.ContainsKey(nomTabla);

            if (res)
            {
                // Crea el archivo de datos de la tabla nueva.
                Tablas[nomTabla] = NombreBaseDeDatos + "\\" + nomTabla + ".dat";
                GuardaArchivoDeDatos(new ArchivoDeDatos(Tablas[nomTabla], new Tabla(nomTabla)));
            }

            return res;
        }

        /**
         * Modifica el nombre de una tabla de la base de datos
         **/
        public bool ModificaNombreTabla(string nomTablaAnterior, string nomTablaNuevo)
        {
            // Verifica si la tabla existe
            bool res = Tablas.ContainsKey(nomTablaAnterior);

            if (res)
            {
                // Obtiene la ruta a modificar y la ruta nueva. Para
                // utilizar le método Move de File.
                string rutaAnterior = Tablas[nomTablaAnterior];
                Tablas.Remove(nomTablaAnterior);
                string rutaNueva = Tablas[nomTablaNuevo] = NombreBaseDeDatos + "\\" + nomTablaNuevo + ".dat";

                File.Move(rutaAnterior, rutaNueva);
            }

            return res;
        }

        /**
         * Elimina la tabla de la base de datos
         **/
        public bool EliminaTabla(string nomTabla)
        {
            // Verifica si la tabla existe
            bool res = Tablas.ContainsKey(nomTabla);

            if (res)
            {
                // Obtiene la ruta a modificar y la ruta nueva. Para
                // utilizar le método Move de File.
                string ruta = Tablas[nomTabla];
                File.Delete(ruta);
                
                Tablas.Remove(nomTabla);
            }

            return res;
        }

        /**
         * Agrega un atributo nuevo a una tabla de la base de datos,
         * siempre y cuando a la tabla no se le haya agregado ningún
         * registro.
         **/
        public bool AgregaAtributo(string nomTabla, Atributo atributo)
        {
            var archivoDeDatos = LeeArchivoDeDatos(Tablas[nomTabla]);
            var tabla = archivoDeDatos.Tabla;
            tabla.Modificable = true;

            // Verifica que no se haya insertado ningún registro a
            // este archivo de datos.
            bool res = tabla.Editable;
            res &= atributo.Llave != TipoLlave.Primaria || (atributo.Llave == TipoLlave.Primaria && !tabla.ContieneLlavePrimaria());

            if (res)
            {
                // Agrega el atributo y actualiza el archivo de
                // datos.
                tabla.Atributos.Add(atributo);
                GuardaArchivoDeDatos(archivoDeDatos);
            }

            return res;
        }

        /**
         * Modifica un atributo en la tabla actual de esta base de 
         * datos, siempre y cuando no se haya eliminado uno
         * anteriormente.
         **/
        public bool ModificaAtributo(string nomTabla, int indiceAtributo, Atributo atributoNuevo)
        {
            var archivoDeDatos = LeeArchivoDeDatos(Tablas[nomTabla]);
            var tabla = archivoDeDatos.Tabla;

            bool res = tabla.Modificable;

            if (res)
            {
                tabla.Atributos[indiceAtributo] = atributoNuevo;
                GuardaArchivoDeDatos(archivoDeDatos);
            }

            return res;
        }

        /**
         * Elimina un atributo y bloquea la posibilidad de poder modificarlo en un futuro.
         **/
        public void EliminaAtributo(string nomTabla, int indiceAtributo)
        {
            var archivoDeDatos = LeeArchivoDeDatos(Tablas[nomTabla]);
            var tabla = archivoDeDatos.Tabla;

            tabla.Atributos.RemoveAt(indiceAtributo);
            tabla.Modificable = false;

            GuardaArchivoDeDatos(archivoDeDatos);
        }

        public void AgregaRegistro(string nomTabla, IComparable[] registro)
        {
            var archivoDeDatos = LeeArchivoDeDatos(Tablas[nomTabla]);
            
            archivoDeDatos.AgregaRegistro(registro);
            GuardaArchivoDeDatos(archivoDeDatos);
        }

        /**
         * Guarda/Actualiza el archivo de datos en la base de
         * datos.
         **/
        private void GuardaArchivoDeDatos(ArchivoDeDatos archivoDeDatos)
        {
            using (var archivoDAT = new FileStream(archivoDeDatos.NombreArchivo, FileMode.OpenOrCreate))
            {
                var serializador = new BinaryFormatter();
                serializador.Serialize(archivoDAT, archivoDeDatos);
            }
        }

        /**
         * Lee el archivo de datos encontrado dentro de la base
         * de datos.
         **/
        private ArchivoDeDatos LeeArchivoDeDatos(string nombreArchivoDeDatos)
        {
            ArchivoDeDatos archivoDeDatos = null;

            using (var archivoDAT = new FileStream(nombreArchivoDeDatos, FileMode.Open))
            {
                var serializador = new BinaryFormatter();
                archivoDeDatos = (ArchivoDeDatos)serializador.Deserialize(archivoDAT);
                archivoDeDatos.NombreArchivo = nombreArchivoDeDatos;
            }

            return archivoDeDatos;
        }

        public DataTable ObtenAtributos(string nomTabla)
        {
            var archivoDeDatos = LeeArchivoDeDatos(Tablas[nomTabla]);
            var tabla = archivoDeDatos.Tabla;

            DataTable tablaDatos = new DataTable();
            string[] nomColumnas = { "Nombre", "Tipo de Dato", "Longitud", "Tipo de Llave" };

            foreach (var nomColumna in nomColumnas)
                tablaDatos.Columns.Add(nomColumna);

            foreach (var atributo in tabla.Atributos)
            {
                DataRow dataRow = tablaDatos.NewRow();

                dataRow["Nombre"] = atributo.Nombre;
                dataRow["Tipo de Dato"] = atributo.Tipo;
                dataRow["Longitud"] = atributo.Tamaño;

                switch (atributo.Llave)
                {
                    case TipoLlave.Primaria: dataRow["Tipo de Llave"] = "Primaria"; break;
                    case TipoLlave.Foranea: dataRow["Tipo de Llave"] = "Foránea"; break;
                    case TipoLlave.SinLlave: dataRow["Tipo de Llave"] = "Ninguna"; break;
                }

                tablaDatos.Rows.Add(dataRow);
            }

            return tablaDatos;
        }
    }
}