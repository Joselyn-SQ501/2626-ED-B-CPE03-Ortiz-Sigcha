using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SistemaBiblioteca.Models;

namespace SistemaBiblioteca.Services
{
    // Logica principal para administrar el catalogo, categorias y prestamos
    public class BibliotecaServicio
    {
        // Catalogo principal donde la clave es el codigo del libro para buscar de inmediato
        private readonly Dictionary<string, Libro> _catalogoLibros;

        // Raiz principal del arbol jerarquico de categorias
        private readonly NodoCategoria _nodoRaiz;

        // Diccionario auxiliar para ubicar cualquier categoria sin tener que recorrer el arbol
        private readonly Dictionary<string, NodoCategoria> _mapaCategorias;

        // Indices invertidos para autores y palabras clave
        private readonly Dictionary<string, HashSet<string>> _indiceAutores;
        private readonly Dictionary<string, HashSet<string>> _indicePalabrasClave;

        // Conjunto con todos los codigos registrados para asegurar que no existan duplicados
        private readonly HashSet<string> _conjuntoIsbnsGlobal;

        // Relacion de cada usuario con los libros que tiene prestados actualmente
        private readonly Dictionary<string, HashSet<string>> _prestamosPorUsuario;

        // Historial detallado de los prestamos que siguen vigentes
        private readonly Dictionary<string, string> _registroPrestamosActivos;

        public NodoCategoria RaizCategorias => _nodoRaiz;
        public int TotalLibrosRegistrados => _catalogoLibros.Count;

        public BibliotecaServicio()
        {
            _catalogoLibros = new Dictionary<string, Libro>(StringComparer.OrdinalIgnoreCase);
            _mapaCategorias = new Dictionary<string, NodoCategoria>(StringComparer.OrdinalIgnoreCase);
            _indiceAutores = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            _indicePalabrasClave = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            _conjuntoIsbnsGlobal = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _prestamosPorUsuario = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            _registroPrestamosActivos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Creamos la raiz del arbol de clasificacion
            _nodoRaiz = new NodoCategoria("ROOT", "Catalogo General de la Biblioteca", "Raiz del arbol de clasificacion");
            _mapaCategorias[_nodoRaiz.Codigo] = _nodoRaiz;

            InicializarCategoriasPorDefecto();
            CargarDatosSemilla();
        }

        #region Arbol de Categorias

        // Inserta una nueva categoria enlazandola a su respectivo padre
        public bool RegistrarCategoria(string codigo, string nombre, string descripcion, string codigoPadre)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return false;
            if (_mapaCategorias.ContainsKey(codigo)) return false;

            NodoCategoria padre = null;
            if (string.IsNullOrWhiteSpace(codigoPadre) || !_mapaCategorias.TryGetValue(codigoPadre, out padre))
            {
                padre = _nodoRaiz;
            }

            var nuevaCategoria = new NodoCategoria(codigo, nombre, descripcion);
            padre.AgregarHijo(nuevaCategoria);
            _mapaCategorias[codigo] = nuevaCategoria;
            return true;
        }

        // Busca una categoria en el diccionario usando su codigo
        public NodoCategoria ObtenerCategoria(string codigo)
        {
            if (_mapaCategorias.TryGetValue(codigo, out var nodo))
            {
                return nodo;
            }
            return null;
        }

        public Dictionary<string, NodoCategoria> ObtenerTodasCategorias()
        {
            return _mapaCategorias;
        }

        // Categorias iniciales del arbol organizadas por areas del conocimiento
        private void InicializarCategoriasPorDefecto()
        {
            // Primer nivel que se desprende de la raiz
            RegistrarCategoria("000", "Ciencias de la Computacion e Informacion", "Obras generales de computacion y tecnologia", "ROOT");
            RegistrarCategoria("500", "Ciencias Exactas y Naturales", "Matematicas, fisica y ciencias fundamentales", "ROOT");
            RegistrarCategoria("800", "Literatura y Filologia", "Narrativa, ensayos y obras literarias", "ROOT");

            // Subcategorias del area de computacion
            RegistrarCategoria("004", "Procesamiento de Datos e Informatica", "Hardware, arquitectura de computadoras", "000");
            RegistrarCategoria("005", "Programacion, Software y Algoritmos", "Desarrollo de software y paradigmas", "000");
            RegistrarCategoria("006", "Metodos Especiales de Computacion", "Inteligencia artificial y vision artificial", "000");

            // Subcategorias de programacion
            RegistrarCategoria("005.1", "Algoritmos y Estructuras de Datos", "Estructuras complejas, listas, arboles, grafos y mapas", "005");
            RegistrarCategoria("005.2", "Lenguajes de Programacion", "C#, Java, Python, C++", "005");
            RegistrarCategoria("005.7", "Bases de Datos y Almacenamiento", "Modelos relacionales, NoSQL y persistencia", "005");

            // Subcategorias del area de ciencias exactas
            RegistrarCategoria("510", "Matematicas", "Calculo, algebra lineal y matematicas discretas", "500");
            RegistrarCategoria("511", "Matematica Discreta y Teoria de Conjuntos", "Conjuntos, logica proposicional y teoria de grafos", "510");
        }

        #endregion

        #region Registro y Busqueda de Libros

        // Valida que el libro no este repetido y lo guarda en los indices correspondientes
        public bool RegistrarLibro(string isbn, string titulo, IEnumerable<string> autores, int anio, string editorial, string codigoCategoria, IEnumerable<string> palabrasClave, int copias, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(isbn))
            {
                mensajeError = "El codigo ISBN no puede ser vacio.";
                return false;
            }

            // Verificamos si el codigo ya existe en el conjunto para evitar duplicidad
            if (_conjuntoIsbnsGlobal.Contains(isbn))
            {
                mensajeError = string.Format("Conflicto de integridad: El ISBN '{0}' ya existe en el conjunto bibliografico.", isbn);
                return false;
            }

            if (!_mapaCategorias.ContainsKey(codigoCategoria))
            {
                codigoCategoria = "000";
            }

            var nuevoLibro = new Libro(isbn, titulo, autores, anio, editorial, codigoCategoria, palabrasClave, copias);

            // Guardamos el codigo en el conjunto general
            _conjuntoIsbnsGlobal.Add(isbn);

            // Guardamos el libro en el catalogo principal
            _catalogoLibros[isbn] = nuevoLibro;

            // Enlazamos el libro con el nodo correspondiente del arbol
            _mapaCategorias[codigoCategoria].AsociarLibro(isbn);

            // Actualizamos el indice invertido por autor
            foreach (var autor in nuevoLibro.Autores)
            {
                if (!_indiceAutores.ContainsKey(autor))
                {
                    _indiceAutores[autor] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                _indiceAutores[autor].Add(isbn);
            }

            // Actualizamos el indice invertido por etiquetas
            foreach (var tag in nuevoLibro.PalabrasClave)
            {
                if (!_indicePalabrasClave.ContainsKey(tag))
                {
                    _indicePalabrasClave[tag] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                _indicePalabrasClave[tag].Add(isbn);
            }

            return true;
        }

        // Consulta directa de un libro por su codigo en tiempo constante
        public Libro BuscarPorISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return null;
            if (_catalogoLibros.TryGetValue(isbn, out var libro))
            {
                return libro;
            }
            return null;
        }

        // Obtiene los libros de un autor usando su indice de conjunto
        public List<Libro> BuscarPorAutor(string autor)
        {
            var resultado = new List<Libro>();
            if (_indiceAutores.TryGetValue(autor, out var isbns))
            {
                foreach (var isbn in isbns)
                {
                    if (_catalogoLibros.TryGetValue(isbn, out var libro))
                    {
                        resultado.Add(libro);
                    }
                }
            }
            return resultado;
        }

        public Dictionary<string, Libro> ObtenerCatalogoCompleto()
        {
            return _catalogoLibros;
        }

        #endregion

        #region Operaciones de Teoria de Conjuntos

        // Interseccion de conjuntos para encontrar libros que tengan todas las etiquetas pedidas
        public HashSet<string> BuscarPorInterseccionPalabrasClave(IEnumerable<string> tags)
        {
            var tagsList = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLower()).ToList();
            if (tagsList == null || tagsList.Count == 0) return new HashSet<string>();

            HashSet<string> conjuntoResultado = null;

            foreach (var tag in tagsList)
            {
                if (_indicePalabrasClave.TryGetValue(tag, out var isbnsConTag))
                {
                    if (conjuntoResultado == null)
                    {
                        conjuntoResultado = new HashSet<string>(isbnsConTag, StringComparer.OrdinalIgnoreCase);
                    }
                    else
                    {
                        conjuntoResultado.IntersectWith(isbnsConTag);
                    }
                }
                else
                {
                    return new HashSet<string>();
                }
            }

            return conjuntoResultado ?? new HashSet<string>();
        }

        // Union de conjuntos para traer libros que tengan al menos una de las etiquetas
        public HashSet<string> BuscarPorUnionPalabrasClave(IEnumerable<string> tags)
        {
            var conjuntoResultado = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (tags == null) return conjuntoResultado;

            foreach (var tag in tags)
            {
                string tagLimpio = tag.Trim().ToLower();
                if (_indicePalabrasClave.TryGetValue(tagLimpio, out var isbnsConTag))
                {
                    conjuntoResultado.UnionWith(isbnsConTag);
                }
            }

            return conjuntoResultado;
        }

        // Une los libros de dos ramas completas del arbol aplicando teoria de conjuntos
        public HashSet<string> UnionDeRamasCategorias(string codCat1, string codCat2)
        {
            var res1 = _mapaCategorias.TryGetValue(codCat1, out var nodo1) ? nodo1.ObtenerTodosLosLibrosSubarbol() : new HashSet<string>();
            var res2 = _mapaCategorias.TryGetValue(codCat2, out var nodo2) ? nodo2.ObtenerTodosLosLibrosSubarbol() : new HashSet<string>();

            var union = new HashSet<string>(res1, StringComparer.OrdinalIgnoreCase);
            union.UnionWith(res2);
            return union;
        }

        // Diferencia de conjuntos para ver libros de la primera rama que no estan en la segunda
        public HashSet<string> DiferenciaEntreCategorias(string codCat1, string codCat2)
        {
            var res1 = _mapaCategorias.TryGetValue(codCat1, out var nodo1) ? nodo1.ObtenerTodosLosLibrosSubarbol() : new HashSet<string>();
            var res2 = _mapaCategorias.TryGetValue(codCat2, out var nodo2) ? nodo2.ObtenerTodosLosLibrosSubarbol() : new HashSet<string>();

            var diferencia = new HashSet<string>(res1, StringComparer.OrdinalIgnoreCase);
            diferencia.ExceptWith(res2);
            return diferencia;
        }

        // Recomienda obras de una categoria restando las que el usuario ya leyo
        public HashSet<string> ObtenerLibrosNoLeidosPorUsuario(string cedulaUsuario, string codigoCategoria)
        {
            var librosCategoria = _mapaCategorias.TryGetValue(codigoCategoria, out var nodo)
                ? nodo.ObtenerTodosLosLibrosSubarbol()
                : new HashSet<string>();

            var librosUsuario = _prestamosPorUsuario.TryGetValue(cedulaUsuario, out var prestados)
                ? prestados
                : new HashSet<string>();

            var noLeidos = new HashSet<string>(librosCategoria, StringComparer.OrdinalIgnoreCase);
            noLeidos.ExceptWith(librosUsuario);
            return noLeidos;
        }

        // Interseccion para ver que libros han leido dos usuarios en comun
        public HashSet<string> LibrosComunesEntreUsuarios(string cedula1, string cedula2)
        {
            var libros1 = _prestamosPorUsuario.TryGetValue(cedula1, out var c1) ? c1 : new HashSet<string>();
            var libros2 = _prestamosPorUsuario.TryGetValue(cedula2, out var c2) ? c2 : new HashSet<string>();

            var comunes = new HashSet<string>(libros1, StringComparer.OrdinalIgnoreCase);
            comunes.IntersectWith(libros2);
            return comunes;
        }

        // Conjunto de libros que todavia tienen copias disponibles para prestar
        public HashSet<string> ObtenerLibrosConDisponibilidad()
        {
            var disponibles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in _catalogoLibros)
            {
                if (kvp.Value.EjemplaresDisponibles.Count > 0)
                {
                    disponibles.Add(kvp.Key);
                }
            }
            return disponibles;
        }

        #endregion

        #region Prestamos y Devoluciones

        // Registra la salida de una copia fisica hacia un estudiante
        public bool PrestarLibro(string isbn, string cedulaUsuario, string nombreUsuario, out string codigoCopia, out string mensaje)
        {
            codigoCopia = null;
            mensaje = string.Empty;

            if (!_catalogoLibros.TryGetValue(isbn, out var libro))
            {
                mensaje = "El libro no se encuentra registrado en el catalogo.";
                return false;
            }

            if (!libro.PrestarEjemplar(out codigoCopia))
            {
                mensaje = "No existen copias disponibles en este momento para prestamo.";
                return false;
            }

            if (!_prestamosPorUsuario.ContainsKey(cedulaUsuario))
            {
                _prestamosPorUsuario[cedulaUsuario] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            _prestamosPorUsuario[cedulaUsuario].Add(isbn);

            string idPrestamo = string.Format("PREST-{0}-{1}", cedulaUsuario, codigoCopia);
            _registroPrestamosActivos[idPrestamo] = string.Format("Usuario: {0} ({1}) | Libro: \"{2}\" | Copia: {3} | Fecha: {4:yyyy-MM-dd HH:mm}",
                nombreUsuario, cedulaUsuario, libro.Titulo, codigoCopia, DateTime.Now);

            mensaje = string.Format("Prestamo exitoso. Se asigno el ejemplar fisico [{0}].", codigoCopia);
            return true;
        }

        // Procesa la devolucion de una copia fisica al inventario disponible
        public bool DevolverLibro(string isbn, string cedulaUsuario, string codigoCopia, out string mensaje)
        {
            mensaje = string.Empty;

            if (!_catalogoLibros.TryGetValue(isbn, out var libro))
            {
                mensaje = "El libro especificado no existe.";
                return false;
            }

            if (!libro.DevolverEjemplar(codigoCopia))
            {
                mensaje = string.Format("El ejemplar [{0}] no figura como prestado.", codigoCopia);
                return false;
            }

            string idPrestamo = string.Format("PREST-{0}-{1}", cedulaUsuario, codigoCopia);
            _registroPrestamosActivos.Remove(idPrestamo);

            mensaje = string.Format("Devolucion completada. Ejemplar [{0}] reintegrado al inventario disponible.", codigoCopia);
            return true;
        }

        public Dictionary<string, string> ObtenerPrestamosActivos() => _registroPrestamosActivos;
        public Dictionary<string, HashSet<string>> ObtenerPrestamosPorUsuario() => _prestamosPorUsuario;

        #endregion

        #region Reporteria

        // Despliega en pantalla el listado ordenado de todos los libros
        public void ImprimirReporteCatalogo()
        {
            Console.WriteLine(new string('=', 110));
            Console.WriteLine(string.Format("{0, -16} | {1, -38} | {2, -22} | {3, -5} | {4, -10} | {5, -6}",
                "ISBN", "TITULO", "AUTORES", "ANIO", "CATEGORIA", "DISP"));
            Console.WriteLine(new string('-', 110));

            foreach (var kvp in _catalogoLibros.OrderBy(l => l.Value.CodigoCategoria))
            {
                var l = kvp.Value;
                string autores = string.Join(", ", l.Autores);
                if (autores.Length > 22) autores = autores.Substring(0, 19) + "...";

                string titulo = l.Titulo;
                if (titulo.Length > 38) titulo = titulo.Substring(0, 35) + "...";

                string disp = string.Format("{0}/{1}", l.EjemplaresDisponibles.Count, l.TotalEjemplares);

                Console.WriteLine(string.Format("{0, -16} | {1, -38} | {2, -22} | {3, -5} | {4, -10} | {5, -6}",
                    l.ISBN, titulo, autores, l.AnioPublicacion, l.CodigoCategoria, disp));
            }
            Console.WriteLine(new string('=', 110));
            Console.WriteLine("Total de titulos registrados: {0} | Ejemplares en inventario: {1}",
                _catalogoLibros.Count, _catalogoLibros.Values.Sum(l => l.TotalEjemplares));
        }

        // Muestra en consola el dibujo del arbol jerarquico con sus ramas
        public void ImprimirReporteArbolCategorias()
        {
            Console.WriteLine(new string('=', 85));
            Console.WriteLine("ESTRUCTURA JERARQUICA DE CATEGORIAS (ARBOL NODOS PADRE-HIJOS)");
            Console.WriteLine("Visualizacion con propagacion de conjuntos por rama:");
            Console.WriteLine(new string('=', 85));
            _nodoRaiz.ImprimirArbol();
            Console.WriteLine(new string('-', 85));
            Console.WriteLine("Leyenda: [Directos] = Libros propios del nodo | [Rama] = Union recursiva de toda la descendencia.");
        }

        // Lista los autores y la cantidad de obras asociadas en sus conjuntos
        public void ImprimirReporteAutores()
        {
            Console.WriteLine(new string('=', 85));
            Console.WriteLine(string.Format("{0, -30} | {1, -12} | {2}", "AUTOR", "CANT. OBRAS", "ISBNS ASOCIADOS (CONJUNTO)"));
            Console.WriteLine(new string('-', 85));
            foreach (var kvp in _indiceAutores.OrderByDescending(a => a.Value.Count))
            {
                string isbns = string.Join(", ", kvp.Value);
                Console.WriteLine(string.Format("{0, -30} | {1, -12} | {2}", kvp.Key, kvp.Value.Count, isbns));
            }
            Console.WriteLine(new string('=', 85));
        }

        #endregion

        #region Analisis de Rendimiento

        // Prueba practica para comparar el tiempo de respuesta entre tablas hash y listas
        public void EjecutarBenchmarkRendimiento()
        {
            Console.WriteLine(new string('=', 90));
            Console.WriteLine("BENCHMARK EXPERIMENTAL: COMPLEJIDAD TEMPORAL O(1) vs O(n)");
            Console.WriteLine("Comparativa entre Diccionario/Conjunto (Hash Table) y Lista Lineal");
            Console.WriteLine(new string('=', 90));

            int[] tamanos = new int[] { 1000, 10000, 50000 };

            Console.WriteLine(string.Format("{0, -10} | {1, -25} | {2, -25} | {3, -15}",
                "ELEMENTOS", "BUSQUEDA DICT/SET (O(1))", "BUSQUEDA LISTA (O(n))", "DIFERENCIA"));
            Console.WriteLine(new string('-', 90));

            var sw = new Stopwatch();

            foreach (int n in tamanos)
            {
                var testDict = new Dictionary<string, string>();
                var testSet = new HashSet<string>();
                var testList = new List<string>();

                for (int i = 0; i < n; i++)
                {
                    string clave = "ISBN-TEST-" + i;
                    testDict[clave] = "Libro #" + i;
                    testSet.Add(clave);
                    testList.Add(clave);
                }

                string claveABuscar = "ISBN-TEST-" + (n - 1);

                // Prueba de busqueda en diccionario
                sw.Restart();
                for (int rep = 0; rep < 500; rep++)
                {
                    bool existeD = testDict.ContainsKey(claveABuscar);
                }
                sw.Stop();
                double tiempoDictMs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000.0;

                // Prueba de busqueda secuencial en lista
                sw.Restart();
                for (int rep = 0; rep < 500; rep++)
                {
                    bool existeL = testList.Contains(claveABuscar);
                }
                sw.Stop();
                double tiempoListMs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000.0;

                double factor = tiempoDictMs > 0 ? (tiempoListMs / tiempoDictMs) : 0;

                Console.WriteLine(string.Format("{0, -10} | {1, -25:F4} ms | {2, -25:F4} ms | {3, -15:F1}x mas rapido",
                    n, tiempoDictMs, tiempoListMs, factor));
            }

            Console.WriteLine(new string('=', 90));
            Console.WriteLine("CONCLUSION TECNICA DEL BENCHMARK:");
            Console.WriteLine("- En O(1) mediante tablas hash (Dictionary/HashSet), el tiempo de acceso permanece");
            Console.WriteLine("  practicamente constante sin importar si existen 1,000 o 50,000 elementos.");
            Console.WriteLine("- En O(n) con List<T>, el tiempo crece linealmente en funcion del tamano N,");
            Console.WriteLine("  demostrando por que las tablas hash son indispensables en sistemas a gran escala.");
            Console.WriteLine(new string('=', 90));
        }

        #endregion

        #region Datos de Prueba

        // Datos iniciales para que el sistema tenga libros y prestamos de prueba al iniciar
        private void CargarDatosSemilla()
        {
            RegistrarLibro(
                "978-0131103627",
                "The C Programming Language",
                new[] { "Brian W. Kernighan", "Dennis M. Ritchie" },
                1988, "Prentice Hall", "005.2",
                new[] { "c", "programacion", "sistemas", "algoritmos" },
                3, out _);

            RegistrarLibro(
                "978-0262033848",
                "Introduction to Algorithms (CLRS)",
                new[] { "Thomas H. Cormen", "Charles E. Leiserson", "Ronald L. Rivest", "Clifford Stein" },
                2009, "MIT Press", "005.1",
                new[] { "algoritmos", "estructuras de datos", "complejidad", "computacion" },
                4, out _);

            RegistrarLibro(
                "978-0321573513",
                "Algorithms 4th Edition",
                new[] { "Robert Sedgewick", "Kevin Wayne" },
                2011, "Addison-Wesley", "005.1",
                new[] { "algoritmos", "estructuras de datos", "grafos", "busqueda" },
                2, out _);

            RegistrarLibro(
                "978-0132350884",
                "Clean Code: A Handbook of Agile Software Craftsmanship",
                new[] { "Robert C. Martin" },
                2008, "Prentice Hall", "005",
                new[] { "software", "buenas practicas", "refactorizacion", "programacion" },
                3, out _);

            RegistrarLibro(
                "978-0201896831",
                "The Art of Computer Programming: Vol 1",
                new[] { "Donald E. Knuth" },
                1997, "Addison-Wesley", "005.1",
                new[] { "algoritmos", "matematicas", "estructuras de datos", "computacion" },
                2, out _);

            RegistrarLibro(
                "978-1491957660",
                "Designing Data-Intensive Applications",
                new[] { "Martin Kleppmann" },
                2017, "O'Reilly Media", "005.7",
                new[] { "bases de datos", "sistemas distribuidos", "almacenamiento", "escalabilidad" },
                3, out _);

            RegistrarLibro(
                "978-0262035612",
                "Deep Learning",
                new[] { "Ian Goodfellow", "Yoshua Bengio", "Aaron Courville" },
                2016, "MIT Press", "006",
                new[] { "inteligencia artificial", "deep learning", "redes neuronales", "machine learning" },
                2, out _);

            RegistrarLibro(
                "978-0073383095",
                "Discrete Mathematics and Its Applications",
                new[] { "Kenneth H. Rosen" },
                2012, "McGraw-Hill", "511",
                new[] { "matematicas", "teoria de conjuntos", "logica", "grafos", "discreta" },
                3, out _);

            RegistrarLibro(
                "978-0321751041",
                "The C# Player's Guide",
                new[] { "RB Whitaker" },
                2021, "Starbound", "005.2",
                new[] { "c#", "programacion", ".net", "orientada a objetos" },
                2, out _);

            RegistrarLibro(
                "978-8420685625",
                "Cien Anos de Soledad",
                new[] { "Gabriel Garcia Marquez" },
                1967, "Editorial Sudamericana", "800",
                new[] { "literatura", "novela", "realismo magico" },
                5, out _);

            // Prestamos de ejemplo para poblar las consultas entre usuarios
            PrestarLibro("978-0262033848", "1600123456", "Marcelo Ortiz", out _, out _);
            PrestarLibro("978-0131103627", "1600123456", "Marcelo Ortiz", out _, out _);
            PrestarLibro("978-0262033848", "1600987654", "Joselyn Sigcha", out _, out _);
            PrestarLibro("978-0073383095", "1600987654", "Joselyn Sigcha", out _, out _);
        }

        #endregion
    }
}
