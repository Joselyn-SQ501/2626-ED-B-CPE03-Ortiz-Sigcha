using System;
using System.Collections.Generic;

namespace SistemaBiblioteca.Models
{
    // Nodo para armar el arbol de categorias con relaciones padre e hijos
    // Permite juntar los libros de toda una rama usando operaciones de conjuntos
    public class NodoCategoria
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        // Puntero al nodo padre que sera nulo unicamente en la raiz del arbol
        public NodoCategoria Padre { get; set; }

        // Diccionario con los hijos directos para buscarlos al instante por su codigo
        public Dictionary<string, NodoCategoria> Hijos { get; set; }

        // Conjunto con los codigos de libros asignados directamente a esta categoria
        public HashSet<string> LibrosISBNs { get; set; }

        public bool EsRaiz => Padre == null;
        public bool EsHoja => Hijos.Count == 0;

        public NodoCategoria(string codigo, string nombre, string descripcion = "")
        {
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Padre = null;
            Hijos = new Dictionary<string, NodoCategoria>(StringComparer.OrdinalIgnoreCase);
            LibrosISBNs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        // Enlazamos un nuevo hijo asociando la referencia hacia este padre
        public void AgregarHijo(NodoCategoria hijo)
        {
            if (hijo == null) throw new ArgumentNullException(nameof(hijo));
            hijo.Padre = this;
            Hijos[hijo.Codigo] = hijo;
        }

        // Quita un nodo hijo buscando directamente por su codigo
        public bool EliminarHijo(string codigoHijo)
        {
            if (Hijos.ContainsKey(codigoHijo))
            {
                Hijos[codigoHijo].Padre = null;
                return Hijos.Remove(codigoHijo);
            }
            return false;
        }

        // Registra un libro dentro del conjunto de esta categoria
        public bool AsociarLibro(string isbn)
        {
            return LibrosISBNs.Add(isbn);
        }

        // Retira un libro del conjunto de esta categoria
        public bool DesasociarLibro(string isbn)
        {
            return LibrosISBNs.Remove(isbn);
        }

        // Calcula que tan profundo esta este nodo dentro del arbol
        public int ObtenerNivel()
        {
            int nivel = 0;
            NodoCategoria actual = Padre;
            while (actual != null)
            {
                nivel++;
                actual = actual.Padre;
            }
            return nivel;
        }

        // Arma el texto con la ruta completa desde la raiz hasta este punto
        public string ObtenerRutaJerarquica()
        {
            var ruta = new List<string>();
            NodoCategoria actual = this;
            while (actual != null)
            {
                ruta.Add(string.Format("[{0}] {1}", actual.Codigo, actual.Nombre));
                actual = actual.Padre;
            }
            ruta.Reverse();
            return string.Join(" > ", ruta);
        }

        // Recorre toda la rama sumando los libros de los hijos con una union de conjuntos
        public HashSet<string> ObtenerTodosLosLibrosSubarbol()
        {
            var resultado = new HashSet<string>(LibrosISBNs, StringComparer.OrdinalIgnoreCase);

            foreach (var parHijo in Hijos)
            {
                var librosDescendientes = parHijo.Value.ObtenerTodosLosLibrosSubarbol();
                resultado.UnionWith(librosDescendientes);
            }

            return resultado;
        }

        // Imprime la estructura jerarquica en la consola con lineas y sangrias
        public void ImprimirArbol(string indent = "", bool esUltimo = true)
        {
            string marcador = esUltimo ? "+-- " : "|-- ";
            int librosPropios = LibrosISBNs.Count;
            int librosRama = ObtenerTodosLosLibrosSubarbol().Count;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(indent + marcador);
            Console.ForegroundColor = EsRaiz ? ConsoleColor.Yellow : (EsHoja ? ConsoleColor.Green : ConsoleColor.White);
            Console.WriteLine("[{0}] {1} (Directos: {2} | Rama: {3})", Codigo, Nombre, librosPropios, librosRama);
            Console.ResetColor();

            indent += esUltimo ? "    " : "|   ";

            int totalHijos = Hijos.Count;
            int indice = 0;
            foreach (var hijo in Hijos.Values)
            {
                indice++;
                hijo.ImprimirArbol(indent, indice == totalHijos);
            }
        }
    }
}
