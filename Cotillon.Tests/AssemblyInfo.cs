using Xunit;

// Cada prueba usa su propia base temporal, pero al liberarla hay que vaciar el pool
// de conexiones de SQLite, que es global al proceso. Si las clases corrieran en
// paralelo, una prueba cerraría las conexiones que otra está usando.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
