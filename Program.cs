using System;
using System.Collections.Generic;
using System.Linq;
using SistemaBiblioteca.Models;
using SistemaBiblioteca.Services;

namespace SistemaBiblioteca
{
    class Program
    {
        private static BibliotecaServicio _servicio;

        static void LimpiarPantalla()
        {
            try
            {
                if (!Console.IsInputRedirected && !Console.IsOutputRedirected)
                {
                    Console.Clear();
                }
            }
            catch { }
        }

        static void Pausa()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nPresione cualquier tecla (o Enter) para regresar al menu...");
                Console.ResetColor();
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey();
                }
                else
                {
                    Console.ReadLine();
                }
            }
            catch { }
            LimpiarPantalla();
        }

        static void Main(string[] args)
        {
            _servicio = new BibliotecaServicio();

            if (args != null && args.Length > 0 && (args[0] == "--test" || args[0] == "-t")) { SistemaBiblioteca.Tests.BibliotecaPruebas.EjecutarTodasLasPruebas(); return; } if (args != null && args.Length > 0 && (args[0] == "--demo" || args[0] == "-d"))
            {
                EjecutarDemostracionAutomatica();
                return;
            }

            try
            {
                if (!Console.IsOutputRedirected)
                {
                    Console.Title = "UEA - Sistema de Biblioteca con Conjuntos, Mapas y Nodos Padre-Hijos";
                }
            }
            catch { }

            bool continuar = true;
            while (continuar)
            {
                MostrarEncabezado();
                MostrarMenuPrincipal();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nSeleccione una opcion [1-10]: ");
                Console.ResetColor();

                string opcion = Console.ReadLine()?.Trim();
                LimpiarPantalla();

                if (string.IsNullOrEmpty(opcion)) continue;

                switch (opcion)
                {
                    case "1":
                        OpcionVerCatalogo();
                        break;
                    case "2":
                        OpcionVerArbolCategorias();
                        break;
                    case "3":
                        OpcionRegistrarLibro();
                        break;
                    case "4":
                        OpcionCrearCategoria();
                        break;
                    case "5":
                        OpcionBuscarPorISBN();
                        break;
                    case "6":
                        OpcionBuscarPorAutor();
                        break;
                    case "7":
                        SubmenuTeoriaDeConjuntos();
                        break;
                    case "8":
                        SubmenuPrestamos();
                        break;
                    case "9":
                        OpcionEjecutarBenchmark();
                        break;
                    case "10":
                        continuar = false;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nGracias por utilizar el Sistema Bibliotecario UEA. ¡Hasta pronto!");
                        Console.ResetColor();
                        try
                        {
                            if (!Console.IsInputRedirected)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine("\nPresione cualquier tecla para salir...");
                                Console.ResetColor();
                                Console.ReadKey();
                            }
                        }
                        catch { }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[ERROR] Opcion no valida. Intente de nuevo...");
                        Console.ResetColor();
                        Pausa();
                        break;
                }
            }
        }

        private static void MostrarEncabezado()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================================================================");
            Console.WriteLine("                  UNIVERSIDAD ESTATAL AMAZONICA - SEDE PASTAZA                           ");
            Console.WriteLine("                CPE #03: IMPLEMENTACION DE CONJUNTOS, MAPAS Y ARBOLES                    ");
            Console.WriteLine("              Estudiantes: Marcelo Ivan Ortiz Gallegos & Joselyn Alexandra Sigcha         ");
            Console.WriteLine("==========================================================================================");
            Console.ResetColor();
        }

        private static void MostrarMenuPrincipal()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n                   --- MENU PRINCIPAL DE GESTION BIBLIOTECARIA ---                        ");
            Console.WriteLine(" 1. Ver Catalogo Completo de Libros (Reporteria General)");
            Console.WriteLine(" 2. Ver Arbol Jerarquico de Categorias (Estructura Nodos Padre-Hijos)");
            Console.WriteLine(" 3. Registrar Nuevo Libro (Validacion de Unicidad en O(1) con HashSet)");
            Console.WriteLine(" 4. Crear Nueva Categoria en el Arbol (Enlace Padre-Hijo)");
            Console.WriteLine(" 5. Consultar Libro por ISBN (Acceso Directo en O(1) con Dictionary)");
            Console.WriteLine(" 6. Consultar Libros por Autor (Indice Invertido con HashSet)");
            Console.WriteLine(" 7. Operaciones con Teoria de Conjuntos (Interseccion, Union, Diferencia)");
            Console.WriteLine(" 8. Gestion de Prestamos y Devoluciones (Control de Copias Fisicas)");
            Console.WriteLine(" 9. Benchmark de Rendimiento (Comparativa O(1) Hash vs O(n) List en vivo)");
            Console.WriteLine(" 10. Salir del Sistema");
            Console.ResetColor();
        }

        private static void OpcionVerCatalogo()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> REPORTE 1: CATALOGO GENERAL DE LIBROS EN BIBLIOTECA <<<\n");
            Console.ResetColor();
            _servicio.ImprimirReporteCatalogo();
            Pausa();
        }

        private static void OpcionVerArbolCategorias()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> REPORTE 2: ARBOL JERARQUICO DE CLASIFICACION (NODOS PADRE-HIJOS) <<<\n");
            Console.ResetColor();
            _servicio.ImprimirReporteArbolCategorias();
            Pausa();
        }

        private static void OpcionRegistrarLibro()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> REGISTRO DE NUEVO LIBRO (VALIDACION DE INTEGRIDAD CON CONJUNTOS) <<<\n");
            Console.ResetColor();

            Console.Write("Ingrese ISBN (ej. 978-0132350884): ");
            string isbn = Console.ReadLine()?.Trim();

            Console.Write("Ingrese Titulo: ");
            string titulo = Console.ReadLine()?.Trim();

            Console.Write("Ingrese Autores (separados por coma): ");
            string autoresInput = Console.ReadLine()?.Trim();
            var autores = autoresInput?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim());

            Console.Write("Ingrese Anio de Publicacion: ");
            int.TryParse(Console.ReadLine(), out int anio);

            Console.Write("Ingrese Editorial: ");
            string editorial = Console.ReadLine()?.Trim();

            Console.WriteLine("\nCategorias disponibles en el arbol:");
            foreach (var cat in _servicio.ObtenerTodasCategorias().Values.Where(c => !c.EsRaiz))
            {
                Console.WriteLine(" - [{0}] {1} (Padre: {2})", cat.Codigo, cat.Nombre, cat.Padre?.Codigo ?? "Raiz");
            }

            Console.Write("\nIngrese Codigo de Categoria: ");
            string codigoCat = Console.ReadLine()?.Trim();

            Console.Write("Ingrese Palabras Clave / Etiquetas (separadas por coma): ");
            string tagsInput = Console.ReadLine()?.Trim();
            var tags = tagsInput?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());

            Console.Write("Numero de copias fisicas disponibles: ");
            int.TryParse(Console.ReadLine(), out int copias);
            if (copias <= 0) copias = 1;

            bool exito = _servicio.RegistrarLibro(isbn, titulo, autores, anio, editorial, codigoCat, tags, copias, out string error);

            if (exito)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[EXITO] Libro registrado satisfactoriamente.");
                Console.WriteLine("- Incorporado al conjunto global de ISBNs en O(1).");
                Console.WriteLine("- Indexado en el mapa principal y en el arbol de categorias [{0}].", codigoCat);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR DE RECHAZO]: {0}", error);
                Console.ResetColor();
            }

            Pausa();
        }

        private static void OpcionCrearCategoria()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> CREAR NUEVA CATEGORIA EN EL ARBOL JERARQUICO (NODO PADRE-HIJO) <<<\n");
            Console.ResetColor();

            Console.WriteLine("Nodos actuales en el arbol:");
            foreach (var c in _servicio.ObtenerTodasCategorias().Values)
            {
                Console.WriteLine(" - [{0}] {1} (Ruta: {2})", c.Codigo, c.Nombre, c.ObtenerRutaJerarquica());
            }

            Console.Write("\nIngrese Codigo de la Nueva Categoria (ej. 005.13): ");
            string codigo = Console.ReadLine()?.Trim();

            Console.Write("Ingrese Nombre de la Categoria: ");
            string nombre = Console.ReadLine()?.Trim();

            Console.Write("Ingrese Descripcion: ");
            string desc = Console.ReadLine()?.Trim();

            Console.Write("Ingrese Codigo del Nodo Padre (dejar en blanco para asignar a ROOT): ");
            string codigoPadre = Console.ReadLine()?.Trim();

            bool creada = _servicio.RegistrarCategoria(codigo, nombre, desc, codigoPadre);
            if (creada)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[EXITO] Categoria [{0}] creada y enlazada como nodo hijo en el arbol.", codigo);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR] Ya existe una categoria con ese codigo o los datos no son validos.");
                Console.ResetColor();
            }

            Pausa();
        }

        private static void OpcionBuscarPorISBN()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> BUSQUEDA DIRECTA POR ISBN (COMPLEJIDAD O(1) CON DICCIONARIO) <<<\n");
            Console.ResetColor();

            Console.Write("Ingrese el ISBN a buscar: ");
            string isbn = Console.ReadLine()?.Trim();

            var libro = _servicio.BuscarPorISBN(isbn);
            if (libro != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[LIBRO ENCONTRADO EN O(1)]");
                Console.ResetColor();
                Console.WriteLine(" Titulo:               {0}", libro.Titulo);
                Console.WriteLine(" Autores:              {0}", string.Join(", ", libro.Autores));
                Console.WriteLine(" Anio / Editorial:     {0} | {1}", libro.AnioPublicacion, libro.Editorial);
                Console.WriteLine(" Categoria Jerarquica: {0}", _servicio.ObtenerCategoria(libro.CodigoCategoria)?.ObtenerRutaJerarquica() ?? libro.CodigoCategoria);
                Console.WriteLine(" Palabras Clave:       [{0}]", string.Join(", ", libro.PalabrasClave));
                Console.WriteLine(" Ejemplares Disp.:     {0} de {1} copias", libro.EjemplaresDisponibles.Count, libro.TotalEjemplares);
                Console.WriteLine(" Codigos de Copias:    {0}", string.Join(", ", libro.EjemplaresDisponibles));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[NO ENCONTRADO] No existe ningun libro con el ISBN especificado.");
                Console.ResetColor();
            }

            Pausa();
        }

        private static void OpcionBuscarPorAutor()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> BUSQUEDA POR AUTOR (INDICE INVERTIDO CON HASHSET) <<<\n");
            Console.ResetColor();

            Console.Write("Ingrese nombre o parte del autor: ");
            string autor = Console.ReadLine()?.Trim();

            var libros = _servicio.BuscarPorAutor(autor);
            if (libros.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSe encontraron {0} libro(s) del autor '{1}':", libros.Count, autor);
                Console.ResetColor();
                foreach (var l in libros)
                {
                    Console.WriteLine(" - [{0}] \"{1}\" ({2}) - Cat: {3}", l.ISBN, l.Titulo, l.AnioPublicacion, l.CodigoCategoria);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[NO ENCONTRADO] No se registran obras para el autor indicado.");
                Console.ResetColor();
            }

            Pausa();
        }

        private static void SubmenuTeoriaDeConjuntos()
        {
            bool volver = false;
            while (!volver)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("==========================================================================================");
                Console.WriteLine("                 MODULO AVANZADO DE TEORIA Y ALGEBRA DE CONJUNTOS                         ");
                Console.WriteLine("==========================================================================================");
                Console.ResetColor();
                Console.WriteLine(" 1. Interseccion de Palabras Clave (A n B): Libros que contienen TODAS las etiquetas");
                Console.WriteLine(" 2. Union de Palabras Clave (A u B): Libros que contienen AL MENOS UNA etiqueta");
                Console.WriteLine(" 3. Union de Ramas del Arbol Padre-Hijos: Consolidacion de dos areas academicas");
                Console.WriteLine(" 4. Diferencia de Categorias (A \\ B): Libros en rama A que no estan en rama B");
                Console.WriteLine(" 5. Recomendacion por Diferencia (Libros \\ Prestados): Novedades para un usuario");
                Console.WriteLine(" 6. Interseccion de Prestamos entre dos Usuarios: Libros leidos en comun");
                Console.WriteLine(" 7. Regresar al Menu Principal");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nSeleccione operacion de conjuntos [1-7]: ");
                Console.ResetColor();

                string op = Console.ReadLine()?.Trim();
                LimpiarPantalla();

                if (string.IsNullOrEmpty(op)) continue;

                switch (op)
                {
                    case "1":
                        Console.WriteLine(">>> INTERSECCION DE PALABRAS CLAVE (A n B) <<<\n");
                        Console.Write("Ingrese las etiquetas a intersectar separadas por coma (ej. algoritmos, estructuras de datos): ");
                        var tagsAnd = Console.ReadLine()?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var resInter = _servicio.BuscarPorInterseccionPalabrasClave(tagsAnd);
                        MostrarResultadoConjuntoISBNs(resInter, "Interseccion Booleana (AND)");
                        Pausa();
                        break;
                    case "2":
                        Console.WriteLine(">>> UNION DE PALABRAS CLAVE (A u B) <<<\n");
                        Console.Write("Ingrese las etiquetas a unir separadas por coma (ej. deep learning, c#): ");
                        var tagsOr = Console.ReadLine()?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var resUnion = _servicio.BuscarPorUnionPalabrasClave(tagsOr);
                        MostrarResultadoConjuntoISBNs(resUnion, "Union Booleana (OR)");
                        Pausa();
                        break;
                    case "3":
                        Console.WriteLine(">>> UNION DE RAMAS EN EL ARBOL PADRE-HIJOS <<<\n");
                        Console.Write("Ingrese Codigo Categoria Rama 1 (ej. 005.1): ");
                        string rama1 = Console.ReadLine()?.Trim();
                        Console.Write("Ingrese Codigo Categoria Rama 2 (ej. 511): ");
                        string rama2 = Console.ReadLine()?.Trim();
                        var unionRamas = _servicio.UnionDeRamasCategorias(rama1, rama2);
                        MostrarResultadoConjuntoISBNs(unionRamas, string.Format("Union de Rama [{0}] y Rama [{1}]", rama1, rama2));
                        Pausa();
                        break;
                    case "4":
                        Console.WriteLine(">>> DIFERENCIA DE CONJUNTOS ENTRE RAMAS (A \\ B) <<<\n");
                        Console.Write("Ingrese Codigo Categoria Base (A) (ej. 000): ");
                        string cA = Console.ReadLine()?.Trim();
                        Console.Write("Ingrese Codigo Categoria a Excluir (B) (ej. 005.1): ");
                        string cB = Console.ReadLine()?.Trim();
                        var dif = _servicio.DiferenciaEntreCategorias(cA, cB);
                        MostrarResultadoConjuntoISBNs(dif, string.Format("Diferencia [{0}] \\ [{1}]", cA, cB));
                        Pausa();
                        break;
                    case "5":
                        Console.WriteLine(">>> RECOMENDACION POR DIFERENCIA DE CONJUNTOS (Catalogo \\ Leidos) <<<\n");
                        Console.Write("Ingrese Cedula del Usuario (ej. 1600123456): ");
                        string cedula = Console.ReadLine()?.Trim();
                        Console.Write("Ingrese Codigo de Categoria a explorar (ej. 005): ");
                        string catExpl = Console.ReadLine()?.Trim();
                        var noLeidos = _servicio.ObtenerLibrosNoLeidosPorUsuario(cedula, catExpl);
                        MostrarResultadoConjuntoISBNs(noLeidos, string.Format("Libros no leidos por {0} en categoria [{1}]", cedula, catExpl));
                        Pausa();
                        break;
                    case "6":
                        Console.WriteLine(">>> INTERSECCION DE PRESTAMOS ENTRE DOS USUARIOS <<<\n");
                        Console.Write("Ingrese Cedula Usuario 1 (ej. 1600123456 - Marcelo Ortiz): ");
                        string u1 = Console.ReadLine()?.Trim();
                        Console.Write("Ingrese Cedula Usuario 2 (ej. 1600987654 - Joselyn Sigcha): ");
                        string u2 = Console.ReadLine()?.Trim();
                        var comunes = _servicio.LibrosComunesEntreUsuarios(u1, u2);
                        MostrarResultadoConjuntoISBNs(comunes, string.Format("Libros leidos en comun por {0} y {1}", u1, u2));
                        Pausa();
                        break;
                    case "7":
                        volver = true;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida.");
                        Pausa();
                        break;
                }
            }
        }

        private static void MostrarResultadoConjuntoISBNs(HashSet<string> conjuntoIsbns, string descripcionOperacion)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[RESULTADO TEORIA DE CONJUNTOS - {0}]", descripcionOperacion);
            Console.WriteLine("Cardinalidad del conjunto (|S|): {0} elemento(s)\n", conjuntoIsbns.Count);
            Console.ResetColor();

            if (conjuntoIsbns.Count == 0)
            {
                Console.WriteLine("El conjunto resultante es VACIO (\u2205).");
                return;
            }

            foreach (var isbn in conjuntoIsbns)
            {
                var libro = _servicio.BuscarPorISBN(isbn);
                if (libro != null)
                {
                    Console.WriteLine(" - [ISBN: {0}] \"{1}\" ({2}) | Cat: {3}",
                        libro.ISBN, libro.Titulo, libro.AnioPublicacion, libro.CodigoCategoria);
                }
            }
        }

        private static void SubmenuPrestamos()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> GESTION DE PRESTAMOS Y DEVOLUCIONES DE EJEMPLARES FISICOS <<<\n");
            Console.ResetColor();

            Console.WriteLine(" 1. Registrar Prestamo de Ejemplar");
            Console.WriteLine(" 2. Registrar Devolucion de Ejemplar");
            Console.WriteLine(" 3. Listar Prestamos Activos");
            Console.Write("\nSeleccione opcion: ");
            string op = Console.ReadLine()?.Trim();

            if (op == "1")
            {
                Console.Write("\nIngrese ISBN del libro a prestar: ");
                string isbn = Console.ReadLine()?.Trim();
                Console.Write("Ingrese Cedula del Estudiante/Usuario: ");
                string cedula = Console.ReadLine()?.Trim();
                Console.Write("Ingrese Nombre del Estudiante/Usuario: ");
                string nombre = Console.ReadLine()?.Trim();

                bool exito = _servicio.PrestarLibro(isbn, cedula, nombre, out string copia, out string msj);
                if (exito)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[EXITO]: {0}", msj);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[FALLO]: {0}", msj);
                }
            }
            else if (op == "2")
            {
                Console.Write("\nIngrese ISBN del libro a devolver: ");
                string isbn = Console.ReadLine()?.Trim();
                Console.Write("Ingrese Cedula del Estudiante/Usuario: ");
                string cedula = Console.ReadLine()?.Trim();
                Console.Write("Ingrese Codigo del Ejemplar Fisico (ej. 978-0131103627-EJ01): ");
                string copia = Console.ReadLine()?.Trim();

                bool devuelto = _servicio.DevolverLibro(isbn, cedula, copia, out string msj);
                if (devuelto)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[EXITO]: {0}", msj);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[FALLO]: {0}", msj);
                }
            }
            else if (op == "3")
            {
                Console.WriteLine("\n--- REGISTRO DE PRESTAMOS ACTIVOS ---");
                var prestamos = _servicio.ObtenerPrestamosActivos();
                if (prestamos.Count == 0)
                {
                    Console.WriteLine("No hay prestamos activos actualmente.");
                }
                else
                {
                    foreach (var p in prestamos)
                    {
                        Console.WriteLine(" - [{0}] -> {1}", p.Key, p.Value);
                    }
                }
            }
            Pausa();
        }

        private static void OpcionEjecutarBenchmark()
        {
            _servicio.EjecutarBenchmarkRendimiento();
            Pausa();
        }

        public static void EjecutarDemostracionAutomatica()
        {
            MostrarEncabezado();
            Console.WriteLine("\n==========================================================================================");
            Console.WriteLine("           MODO DEMOSTRACION COMPLETA AUTOMATIZADA PARA EVALUACION INSTITUCIONAL           ");
            Console.WriteLine("==========================================================================================\n");

            Console.WriteLine("\n>>> CASO 1: REPORTE DEL CATALOGO GENERAL (DICCIONARIOS Y MAPAS)");
            _servicio.ImprimirReporteCatalogo();

            Console.WriteLine("\n>>> CASO 2: ESTRUCTURA JERARQUICA DE CATEGORIAS (NODOS PADRE-HIJOS)");
            _servicio.ImprimirReporteArbolCategorias();

            Console.WriteLine("\n>>> CASO 3: VALIDACION DE UNICIDAD EN O(1) CON CONJUNTOS (HASHSET)");
            Console.WriteLine("Intentando registrar libro duplicado (ISBN ya existente: 978-0131103627)...");
            bool exitoDup = _servicio.RegistrarLibro("978-0131103627", "Duplicado Invalido", new[] { "Autor" }, 2020, "Edit", "000", new[] { "tag" }, 1, out string errDup);
            if (!exitoDup)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[VALIDACION CORRECTA - RECHAZADO EN O(1)]: " + errDup);
                Console.ResetColor();
            }

            Console.WriteLine("\n>>> CASO 4: BUSQUEDA DIRECTA POR ISBN EN O(1) (DICCIONARIO)");
            var libro = _servicio.BuscarPorISBN("978-0262033848");
            if (libro != null)
            {
                Console.WriteLine("Libro encontrado: \"{0}\" | Editorial: {1} | Cat: {2} | Copias: {3}/{4}",
                    libro.Titulo, libro.Editorial, libro.CodigoCategoria, libro.EjemplaresDisponibles.Count, libro.TotalEjemplares);
            }

            Console.WriteLine("\n>>> CASO 5: BUSQUEDA POR AUTOR CON INDICE INVERTIDO (HASHSET)");
            var librosAutor = _servicio.BuscarPorAutor("Donald E. Knuth");
            Console.WriteLine("Libros de Donald E. Knuth encontrados: " + librosAutor.Count);
            foreach (var l in librosAutor)
            {
                Console.WriteLine(" - [{0}] {1} ({2})", l.ISBN, l.Titulo, l.AnioPublicacion);
            }

            Console.WriteLine("\n>>> CASO 6: TEORIA DE CONJUNTOS - INTERSECCION DE PALABRAS CLAVE (AND)");
            Console.WriteLine("Intersectando: {'algoritmos', 'estructuras de datos'}");
            var inter = _servicio.BuscarPorInterseccionPalabrasClave(new[] { "algoritmos", "estructuras de datos" });
            MostrarResultadoConjuntoISBNs(inter, "Interseccion de Etiquetas");

            Console.WriteLine("\n>>> CASO 7: TEORIA DE CONJUNTOS - UNION DE RAMAS EN EL ARBOL (005.1 y 511)");
            var unionRamas = _servicio.UnionDeRamasCategorias("005.1", "511");
            MostrarResultadoConjuntoISBNs(unionRamas, "Union de Ramas [005.1] y [511]");

            Console.WriteLine("\n>>> CASO 8: TEORIA DE CONJUNTOS - DIFERENCIA DE RAMAS ([000] \\ [005.1])");
            var difRamas = _servicio.DiferenciaEntreCategorias("000", "005.1");
            MostrarResultadoConjuntoISBNs(difRamas, "Diferencia de Libros en Computacion excluyendo Algoritmos");

            Console.WriteLine("\n>>> CASO 9: TEORIA DE CONJUNTOS - INTERSECCION DE PRESTAMOS ENTRE USUARIOS");
            Console.WriteLine("Usuarios: Marcelo Ortiz (1600123456) y Joselyn Sigcha (1600987654)");
            var comunes = _servicio.LibrosComunesEntreUsuarios("1600123456", "1600987654");
            MostrarResultadoConjuntoISBNs(comunes, "Libros leidos por ambos estudiantes");

            Console.WriteLine("\n>>> CASO 10: GESTION DE PRESTAMOS ACTIVOS");
            var prestamos = _servicio.ObtenerPrestamosActivos();
            foreach (var p in prestamos)
            {
                Console.WriteLine(" - [{0}] -> {1}", p.Key, p.Value);
            }

            Console.WriteLine("\n>>> CASO 11: BENCHMARK DE RENDIMIENTO (O(1) vs O(n))");
            _servicio.EjecutarBenchmarkRendimiento();

            Console.WriteLine("\n==========================================================================================");
            Console.WriteLine("                     FIN DE LA DEMOSTRACION - EVALUACION EXITOSA                          ");
            Console.WriteLine("==========================================================================================");
        }
    }
}

