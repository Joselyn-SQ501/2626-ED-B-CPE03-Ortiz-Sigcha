using System;
using System.Collections.Generic;

namespace SistemaBiblioteca.Models
{
    // Modelo que representa cada libro registrado en la biblioteca
    // Guardamos autores y etiquetas en conjuntos para no tener datos repetidos
    public class Libro
    {
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        public HashSet<string> Autores { get; set; }
        public int AnioPublicacion { get; set; }
        public string Editorial { get; set; }
        public string CodigoCategoria { get; set; }
        public HashSet<string> PalabrasClave { get; set; }
        
        // Control de copias fisicas para saber cuales estan disponibles y cuales prestadas
        public HashSet<string> EjemplaresDisponibles { get; set; }
        public HashSet<string> EjemplaresPrestados { get; set; }

        public int TotalEjemplares => EjemplaresDisponibles.Count + EjemplaresPrestados.Count;

        public Libro()
        {
            Autores = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            PalabrasClave = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            EjemplaresDisponibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            EjemplaresPrestados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public Libro(string isbn, string titulo, IEnumerable<string> autores, int anio, string editorial, string codigoCategoria, IEnumerable<string> palabrasClave, int copiasIniciales = 1)
            : this()
        {
            ISBN = isbn;
            Titulo = titulo;
            AnioPublicacion = anio;
            Editorial = editorial;
            CodigoCategoria = codigoCategoria;

            if (autores != null)
            {
                foreach (var autor in autores)
                {
                    Autores.Add(autor.Trim());
                }
            }

            if (palabrasClave != null)
            {
                foreach (var pc in palabrasClave)
                {
                    PalabrasClave.Add(pc.Trim().ToLower());
                }
            }

            // Asignamos codigos individuales a cada copia fisica del libro
            for (int i = 1; i <= copiasIniciales; i++)
            {
                EjemplaresDisponibles.Add(string.Format("{0}-EJ{1:D2}", isbn, i));
            }
        }

        // Pasamos una copia disponible al grupo de prestadas en tiempo constante
        public bool PrestarEjemplar(out string codigoEjemplarPrestado)
        {
            codigoEjemplarPrestado = null;
            if (EjemplaresDisponibles.Count == 0)
            {
                return false;
            }

            using (var enumerator = EjemplaresDisponibles.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    codigoEjemplarPrestado = enumerator.Current;
                }
            }

            if (codigoEjemplarPrestado != null)
            {
                EjemplaresDisponibles.Remove(codigoEjemplarPrestado);
                EjemplaresPrestados.Add(codigoEjemplarPrestado);
                return true;
            }

            return false;
        }

        // Reintegramos la copia devuelta al inventario disponible de inmediato
        public bool DevolverEjemplar(string codigoEjemplar)
        {
            if (EjemplaresPrestados.Contains(codigoEjemplar))
            {
                EjemplaresPrestados.Remove(codigoEjemplar);
                EjemplaresDisponibles.Add(codigoEjemplar);
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            string autoresStr = string.Join(", ", Autores);
            string tagsStr = string.Join(", ", PalabrasClave);
            return string.Format("[ISBN: {0}] \"{1}\" por {2} ({3}) | Cat: {4} | Disp: {5}/{6} | Tags: [{7}]",
                ISBN, Titulo, autoresStr, AnioPublicacion, CodigoCategoria,
                EjemplaresDisponibles.Count, TotalEjemplares, tagsStr);
        }
    }
}
